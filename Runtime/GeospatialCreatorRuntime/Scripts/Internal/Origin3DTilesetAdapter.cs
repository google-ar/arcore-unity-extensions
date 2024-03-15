//-----------------------------------------------------------------------
// <copyright file="Origin3DTilesetAdapter.cs" company="Google LLC">
//
// Copyright 2023 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Internal
{
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif // UNITY_EDITOR

    /// <summary>
    /// Used when an ARGeospatialCreatorOrigin's 3D Tileset is determined by or maintained by
    /// a child object. For example, the default way to configure an origin is to add a
    /// Cesium3D Tileset as a child object. The adapter pattern allows us to avoid
    /// exposing the implementation details of the Cesium3D Tileset to the
    /// ARGeospatialCreatorOrigin class.
    /// </summary>
    internal abstract class Origin3DTilesetAdapter
    {
        // How many milliseconds to wait for the tiles to load before attempting a hit test
        private const int _tileLoadingWaitInterval = 100;

        // How many seconds to try loading tiles before timing out
        private readonly TimeSpan _tileLoadingWaitTimeout = TimeSpan.FromSeconds(5);

        /// <summary>Gets the parent ARGeospatialCreatorOrigin object.</summary>
        public abstract ARGeospatialCreatorOrigin Origin { get; }

        /// <summary>
        /// Whether or not physics meshes are enabled for the tileset to allow collision testing.
        /// </summary>
        /// <returns> True if physics meshes are enabled for the tileset.</returns>
        public abstract bool GetPhysicsMeshesEnabled();

        /// <summary>
        /// Used to enable or disable physics meshes on the tileset.
        /// </summary>
        /// <param name="enable"> If true, tileset meshes will allow collision testing.</param>
        public abstract void SetPhysicsMeshesEnabled(bool enable);

        /// <summary>
        /// Returns what percentage of Tiles have loaded in the current view.
        /// </summary>
        /// <returns> Percentage (0.0 to 100.0) of tiles loaded in the current view.</returns>
        public abstract float ComputeTilesetLoadProgress();

        /// <summary>
        /// Verifies whether a collider belongs to a tile in the tileset.
        /// </summary>
        /// <param name="collider"> The collider to be checked.</param>
        /// <returns> True if the collider belongs to a tile.</returns>
        public abstract bool IsTileCollider(Collider collider);

#if UNITY_EDITOR
        /// <summary>
        /// Asynchronously gets the terrain or rooftop altitude at the given lat and lng.
        /// This method ensures that physics are enabled on the tileset and waits for the
        /// tiles to load before computing the altitude.
        /// </summary>
        /// <param name="lat"> The latitude at which to sample the altitude.</param>
        /// <param name="lng"> The longitude at which to sample the altitude.</param>
        /// <returns>
        /// Tuple with "success" and "terrainAltitude" named elements.
        /// "success": True if the altitude was successfully calculated.
        /// "terrainAltitude": Altitude of the terrain at lat/lng or 0.0 if calculations failed.
        /// </returns>
        /// <remarks>
        /// If disabled, this method will enable physics meshes on the tiles during its execution,
        /// and it will revert the setting back to its previous value before returning.
        /// The tileset will reload all its tiles each time this setting is changed.
        /// </remarks>
        public virtual async Task<(bool success, double terrainAltitude)>
            CalcTileAltitudeWGS84Async(double lat, double lng)
        {
            if (Origin?._originPoint == null)
            {
                return (false, 0.0);
            }

            // Enable physics meshes on the tileset to allow the raycast to hit
            bool previousPhysicsSetting = GetPhysicsMeshesEnabled();
            SetPhysicsMeshesEnabled(true);

            // Wait an initial interval of at least one frame before computing tile load progress
            // Otherwise, you will get 100% loaded because previous tiles haven't unloaded
            await Task.Delay(_tileLoadingWaitInterval);

            // Wait until all tiles have loaded or we've hit a timeout
            DateTime waitStartTime = DateTime.UtcNow;
            while ((ComputeTilesetLoadProgress() < 99.9f)
                && (DateTime.UtcNow - waitStartTime < _tileLoadingWaitTimeout))
            {
                await Task.Delay(_tileLoadingWaitInterval);
            }

            // TODO: (b/319861940) improve accuracy of sampled altitude.
            (bool hitSucceeded, double hitAltitude) = CalcTileAltitudeWGS84(lat, lng);

            // Revert the physics meshes on the tileset to the previous setting
            SetPhysicsMeshesEnabled(previousPhysicsSetting);

            return (hitSucceeded, hitAltitude);
        }

        /// <summary> Gets the terrain or rooftop altitude at the given lat and lng.</summary>
        /// <param name="lat"> The latitude at which to sample the altitude.</param>
        /// <param name="lng"> The longitude at which to sample the altitude.</param>
        /// <returns>
        /// Tuple with "success" and "terrainAltitude" named elements.
        /// "success": True if the altitude was successfully calculated.
        /// "terrainAltitude": Altitude of the terrain at lat/lng or 0.0 if calculations failed.
        /// </returns>
        /// <remarks>
        /// Calculation can fail if the tiles haven't had time to load, if the tiles don't have
        /// physics meshes enabled, or if the target tile is out of the camera's view while
        /// Frustum Culling is enabled. Sampled altitude can be inaccurate if high precision
        /// LODs haven't loaded for the tiles (ex: because the tile is far from the camera).
        /// For accurate results, have the desired anchor position well within the camera view.
        /// </remarks>
        public virtual (bool success, double terrainAltitude)
            CalcTileAltitudeWGS84(double lat, double lng)
        {
            GeoCoordinate originPoint = Origin?._originPoint;
            Vector3 originUnityCoord = Origin ? Origin.gameObject.transform.position: Vector3.zero;
            if (originPoint == null)
            {
                return (false, 0.0);
            }

            // Make a high point where the ray will originate (above any possible terrain)
            GeoCoordinate highGeoCoord =
                new GeoCoordinate(lat, lng, GeoMath.HighestElevationOnEarth);
            Vector3 rayOrigin = GeoMath.GeoCoordinateToUnityWorld(
                highGeoCoord, originPoint, originUnityCoord);

            // Make a low point at zero altitude where the ray will be cast towards
            GeoCoordinate lowGeoCoord = new GeoCoordinate(lat, lng, 0.0);
            Vector3 rayTarget = GeoMath.GeoCoordinateToUnityWorld(
                lowGeoCoord, originPoint, originUnityCoord);

            // Make a vector that points from the high point to the low point
            Vector3 rayDirection = Vector3.Normalize(rayTarget - rayOrigin);

            // Retrieve the highest altitude tile hit
            bool hitSucceeded = false;
            double hitAltitude = double.MinValue;
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDirection);
            foreach (RaycastHit hit in hits)
            {
                if (IsTileCollider(hit.collider))
                {
                    GeoCoordinate hitPoint = GeoMath.UnityWorldToGeoCoordinate(
                        hit.point, originPoint, originUnityCoord);
                    hitSucceeded = true;
                    hitAltitude = Math.Max(hitPoint.Altitude, hitAltitude);
                }
            }

            // If there was no hit, return an altitude of zero
            hitAltitude = hitSucceeded ? hitAltitude : 0.0;

            return (hitSucceeded, hitAltitude);
        }
#endif // UNITY_EDITOR
    }
}
