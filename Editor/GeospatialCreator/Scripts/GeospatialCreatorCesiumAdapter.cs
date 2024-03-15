//-----------------------------------------------------------------------
// <copyright file="GeospatialCreatorCesiumAdapter.cs" company="Google LLC">
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

#if ARCORE_INTERNAL_USE_CESIUM
namespace Google.XR.ARCoreExtensions.GeospatialCreator.Editor
{
    using System;

    using CesiumForUnity;
    using Google.XR.ARCoreExtensions.Editor.Internal;
    using Google.XR.ARCoreExtensions.GeospatialCreator;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Editor.Internal;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
    using UnityEditor;
    using UnityEngine;

    public class GeospatialCreatorCesiumAdapter
    {
        /// <summary>
        /// Static factory method to create an ARGeospatialCreatorOrigin at the given
        /// latitude/longitude/altitude in the current Unity Scene. The Unity world coordinates
        /// will be (0, 0, 0) and can be modified by accessing the returned object's transform.
        /// </summary>
        /// <remarks>
        // This method is available only if the Cesium dependency is present in the project.
        /// </remarks>
        /// <param name = "latitude">The latitude at which to create the new origin, in decimal
        /// degrees.</param>
        /// <param name = "longitude">The longitude at which to create the new origin, in decimal
        /// degrees.</param>
        /// <param name = "altitude">The altitude at which to create the new origin, in meters
        /// according to WGS84. </param>
        public static ARGeospatialCreatorOrigin CreateOriginWithCesiumGeoreference(
            double latitude, double longitude, double altitude, string mapTilesApiKey)
        {
            ARGeospatialCreatorOrigin origin =
                new GameObject("GeospatialOrigin").AddComponent<ARGeospatialCreatorOrigin>();
            origin.gameObject.tag = "EditorOnly";
            origin.SetOriginPoint(latitude, longitude, altitude);

            AddGeoreferenceAndTileset(origin);
            Cesium3DTileset tileset = GetTilesetComponent(origin);
            SetMapTileApiKeyForCesium3DTileset(tileset, mapTilesApiKey);

            return origin;
        }

        internal static Cesium3DTileset GetTilesetComponent(ARGeospatialCreatorOrigin origin)
        {
            return origin.gameObject.GetComponentInChildren(typeof(Cesium3DTileset))
                as Cesium3DTileset;
        }

        internal static CesiumGeoreference AddGeoreferenceAndTileset(
            ARGeospatialCreatorOrigin origin)
        {
            CesiumGeoreference georeference =
                origin.gameObject.AddComponent(typeof(CesiumGeoreference)) as CesiumGeoreference;

            georeference.latitude = origin._originPoint.Latitude;
            georeference.longitude = origin._originPoint.Longitude;
            georeference.height = origin._originPoint.Altitude;

            GameObject tilesetObject = new GameObject("Cesium3DTileset");
            tilesetObject.transform.SetParent(georeference.gameObject.transform);

            // Since this is an AR app, it is likely using the camera instead of a scene
            // so default to the tiles only being visible in the Editor. Developers can
            // manually change the tag in the Inspector, if desired.
            Cesium3DTileset tileset =
                tilesetObject.AddComponent(typeof(Cesium3DTileset)) as Cesium3DTileset;
            tileset.name = tilesetObject.name;
            tileset.tilesetSource = CesiumDataSource.FromUrl;
            tileset.showCreditsOnScreen = true;
            tileset.createPhysicsMeshes = false;

            georeference.tag = "EditorOnly";
            tilesetObject.tag = "EditorOnly";

            return georeference;
        }

        /// <summary>
        /// Returns the Map Tiles API Key from the Cesium3DTileset in the Origin.
        /// </summary>
        /// <param name="origin"> A ARGeospatialCreatorOrigin that has a Cesium3DTileset child.
        /// </param>
        /// <returns> The Map Tiles API Key if the origin and its tileset aren't null,
        ///  else returns null. </returns>
        internal static string GetMapTilesApiKey(ARGeospatialCreatorOrigin origin)
        {
            if (origin == null)
            {
                return null;
            }
            return GetMapTilesApiKey(GetTilesetComponent(origin));
        }

        /// <summary> Returns the Map Tiles API Key from the Cesium3DTileset.</summary>
        /// <param name="tileset"> The Cesium3DTileset with a Map tiles API Key.</param>
        /// <returns> The Map Tiles API Key if the tileset isn't null, else returns null. </returns>
        internal static string GetMapTilesApiKey(Cesium3DTileset tileset)
        {
            if (tileset == null)
            {
                return null;
            }
            return MapTilesUtils.ExtractMapTilesApiKey(tileset.url);
        }

        /// <summary>Sets the Map Tiles API Key on the Cesium3DTileset for a origin.</summary>
        /// <param name="origin"> A ARGeospatialCreatorOrigin that has a Cesium3DTileset child.
        /// </param>
        /// <param name="key"> The Map Tiles API key that should be used.</param>
        /// <returns> True if the key was modified. This is useful for GUI clients to record
        /// Undoable operations. </returns>
        internal static bool SetMapTileApiKeyForCesium3DTileset(
            Cesium3DTileset tileset, string key)
        {
            String url = String.IsNullOrEmpty(key) ? "" : MapTilesUtils.CreateMapTilesUrl(key);
            if (url == tileset.url)
            {
                // The URL is unchanged, nothing to modify
                return false;
            }
            tileset.url = url;
            EditorUtility.SetDirty(tileset);
            return true;
        }

        internal class Origin3DTilesetCesiumAdapter : Origin3DTilesetAdapter
        {
            // The parent ARGeospatialCreatorOrigin object that owns this instance of the Adapter
            private readonly ARGeospatialCreatorOrigin _origin;

            internal Origin3DTilesetCesiumAdapter(ARGeospatialCreatorOrigin origin)
            {
                _origin = origin;
            }

            /// <summary> Gets the parent ARGeospatialCreatorOrigin object.</summary>
            public override ARGeospatialCreatorOrigin Origin
            {
                get
                {
                    return _origin;
                }
            }

            /// <summary> Sets the Cesium tileset's "Create Physics Meshes" setting.</summary>
            /// <param name="enable"> Specifies whether to enable the setting or not.</param>
            public override void SetPhysicsMeshesEnabled(bool enable)
            {
                Cesium3DTileset tileset = GetTilesetComponent(Origin);
                if (tileset == null)
                {
                    Debug.LogError(
                        "No Cesium3DTileset component associated with Origin " + Origin.name);
                        return;
                }

                if (tileset.createPhysicsMeshes != enable)
                {
                    tileset.createPhysicsMeshes = enable;
                }
            }

            /// <summary> Gets the Cesium tileset's "Create Physics Meshes" setting.</summary>
            public override bool GetPhysicsMeshesEnabled()
            {
                Cesium3DTileset tileset = GetTilesetComponent(Origin);
                if (tileset == null)
                {
                    Debug.LogError(
                        "No Cesium3DTileset component associated with Origin " + Origin.name);
                        return false;
                }

                return tileset.createPhysicsMeshes;
            }

            /// <summary> Retrieves percentage of tiles loaded for current view.</summary>
            /// <returns> Percentage (0.0 to 100.0) of tiles loaded for current view.</returns>
            public override float ComputeTilesetLoadProgress()
            {
                Cesium3DTileset tileset = GetTilesetComponent(Origin);
                if (tileset == null)
                {
                    Debug.LogError(
                        "No Cesium3DTileset component associated with Origin " + Origin.name);
                    return 0;
                }
                return tileset.ComputeLoadProgress();
            }

            /// <summary> Returns true if the collider belongs to a tile in this tileset.</summary>
            /// <param name="collider"> The collider to be checked.</param>
            public override bool IsTileCollider(Collider collider)
            {
                if ((collider != null) &&
                    (collider.gameObject.GetComponent<CesiumGlobeAnchor>() != null))
                {
                    return true;
                }
                return false;
            }
        }

        internal class OriginComponentCesiumAdapter : IOriginComponentAdapter
        {
            // The parent ARGeospatialCreatorOrigin object that owns this instance of the Adapter
            private readonly ARGeospatialCreatorOrigin _origin;

            public OriginComponentCesiumAdapter(ARGeospatialCreatorOrigin origin)
            {
                _origin = origin;
            }

            public GeoCoordinate GetOriginFromComponent()
            {
                CesiumForUnity.CesiumGeoreference geoRef = GetCesiumGeoreference();
                return (geoRef == null) ? null :
                    new GeoCoordinate(geoRef.latitude, geoRef.longitude, geoRef.height);
            }

            public void SetComponentOrigin(GeoCoordinate newOrigin)
            {
                CesiumForUnity.CesiumGeoreference geoRef = GetCesiumGeoreference();
                if (geoRef == null)
                {
                    Debug.LogWarning("Origin location updated for " + _origin.gameObject.name +
                        ", but there is no Cesium Georeference subcomponent.");
                    return;
                }

                geoRef.latitude = newOrigin.Latitude;
                geoRef.longitude = newOrigin.Longitude;
                geoRef.height = newOrigin.Altitude;
            }

            private CesiumForUnity.CesiumGeoreference GetCesiumGeoreference()
            {
                return _origin.gameObject.GetComponent<CesiumForUnity.CesiumGeoreference>();
            }
        }
    }
}
#endif // ARCORE_INTERNAL_USE_CESIUM
