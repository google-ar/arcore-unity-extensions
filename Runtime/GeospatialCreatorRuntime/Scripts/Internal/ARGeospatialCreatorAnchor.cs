//-----------------------------------------------------------------------
// <copyright file="ARGeospatialCreatorAnchor.cs" company="Google LLC">
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
    using System.Collections;
    using Google.XR.ARCoreExtensions;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// A representation of a Geospatial Anchor that was created using the Geospatial Creator tool.
    /// This object is responsible for creating a proper ARGeospatialAnchor at runtime at the
    /// latitude, longitude, and altitude specified.
    /// </summary>
    [AddComponentMenu("XR/AR Geospatial Creator Anchor")]
    [ExecuteInEditMode]
    public class ARGeospatialCreatorAnchor : ARGeospatialCreatorObject
    {
        /// <summary>Gets or sets the AltitudeType used for resolution of this anchor.</summary>
        public AltitudeType AltType = AltitudeType.ManualAltitude;

        [SerializeField]
        private double _latitude;

        [SerializeField]
        private double _longitude;

        [SerializeField]
        private double _altitude;

        [SerializeField]
        private double _altitudeOffset;

#if !UNITY_EDITOR
        private AnchorResolutionState _anchorResolution = AnchorResolutionState.NotStarted;
#endif

        /// <summary>
        /// Determines how the Altitude and AlttudeOffset fields are interpreted.
        /// </summary>
        public enum AltitudeType
        {
            /// <summary>
            /// Altitude specifies the altitude of the anchor in meters for WGS84. AltitudeOffset
            /// is not used.
            /// </summary>
            ManualAltitude,

            /// <summary>
            /// AltitudeOffset specifies the relative altitude above/below the terrain, in meters.
            /// Altitude is used only in the Editor for visualizing the anchor in the scene view.
            /// </summary>
            Terrain,

            /// <summary>
            /// AltitudeOffset specifies the relative altitude above/below the rooftop, in meters.
            /// Altitude is used only in the Editor for visualizing the anchor in the scene view.
            /// </summary>
            Rooftop
        }

#if !UNITY_EDITOR
        private enum AnchorResolutionState
        {
            NotStarted,
            InProgress,
            Complete
        }
#endif

        /// <summary>Gets or sets the latitude of this anchor.</summary>
        public double Latitude
        {
            get => this._latitude;
            set { this._latitude = value; }
        }

        /// <summary>Gets or sets the longitude of this anchor.</summary>
        public double Longitude
        {
            get => this._longitude;
            set { this._longitude = value; }
        }

        /// <summary>
        /// Gets or sets the altitude. When AltType is ManualAltitude, this value is the altitude
        ///  of the anchor, in meters according to WGS84. When AltType is Terrain or Rooftop, this
        /// value is ONLY used in Editor mode, to determine the altitude at which to render the
        /// anchor in the Editor's Scene View. If AltType is Terrain or Rooftop, this value is
        /// ignored in the Player.
        /// </summary>
        public double Altitude
        {
            get => this._altitude;
            set { this._altitude = value; }
        }

        /// <summary>
        /// Gets or sets the altitude offset of the anchor, in meters relative to the resolved
        /// terrain or rooftop. This value is ignored when AltType is ManualAltitude.
        /// </summary>
        public double AltitudeOffset
        {
            get => this._altitudeOffset;
            set { this._altitudeOffset = value; }
        }

#if !UNITY_EDITOR
        private void Update()
        {
            // Create the geospatial anchor in Player mode only
            if (!Application.isPlaying)
            {
                return;
            }

            // Only attempt to create the geospatial anchor once
            if (_anchorResolution == AnchorResolutionState.NotStarted)
            {
                AddGeoAnchorAtRuntime();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "UnityRules.UnityStyleRules",
            "US1300:LinesMustBe100CharactersOrShorter",
            Justification = "Unity issue URL length > 100")]
        private void AddGeoAnchorAtRuntime()
        {
            IntPtr sessionHandle = ARCoreExtensions._instance.currentARCoreSessionHandle;

            // During boot this will return false a few times.
            if (sessionHandle == IntPtr.Zero)
            {
                return;
            }

            // Geospatial anchors cannot be created until the AR Session is stable and tracking:
            // https://developers.google.com/ar/develop/unity-arf/geospatial/geospatial-anchors#place_a_geospatial_anchor_in_the_real_world
            if (EarthApi.GetEarthTrackingState(sessionHandle) != TrackingState.Tracking)
            {
                Debug.Log("Waiting for AR Session to become stable.");
                return;
            }

            // :TODO (b/278071434): Make the anchor manager a settable property
            ARAnchorManager anchorManager =
                ARCoreExtensions._instance.SessionOrigin.GetComponent<ARAnchorManager>();

            if (anchorManager == null)
            {
                Debug.LogError(
                    "The Session Origin has no Anchor Manager. " +
                    "Unable to place Geospatial Creator Anchor " +
                    name);
                _anchorResolution = AnchorResolutionState.Complete;
                return;
            }

            _anchorResolution = AnchorResolutionState.InProgress;
            switch (this.AltType)
            {
                case AltitudeType.Terrain:
                    StartCoroutine(ResolveTerrainAnchor(anchorManager));
                    break;
                case AltitudeType.Rooftop:
                    StartCoroutine(ResolveRooftopAnchor(anchorManager));
                    break;
                case AltitudeType.ManualAltitude:
                    // Manual altitude anchors don't use async APIs, so there's no coroutine
                    ResolveManualAltitudeAnchor(anchorManager);
                    break;
            }
        }

        private void FinishAnchor(ARGeospatialAnchor resolvedAnchor)
        {
            if (resolvedAnchor == null)
            {
                // If we failed once, resolution is likley to keep failing. Don't retry endlessly.
                Debug.LogError("Failed to make Geospatial Anchor for " + name);
                _anchorResolution = AnchorResolutionState.Complete;
                return;
            }

            // Maintain an association between the ARGeospatialCreatorAnchor and the resolved
            // ARGeospatialAnchor by making the creator anchor a child of the runtime anchor.
            // We zero out the pose & rotation on the creator anchor, since the runtime
            // anchor will handle that from now on.
            transform.position = new Vector3(0, 0, 0);
            transform.rotation = Quaternion.identity;
            transform.SetParent(resolvedAnchor.transform, false);

            _anchorResolution = AnchorResolutionState.Complete;
            Debug.Log("Geospatial Anchor resolved: " + name);
        }

        private void ResolveManualAltitudeAnchor(ARAnchorManager anchorManager)
        {
            ARGeospatialAnchor anchor = anchorManager.AddAnchor(
                Latitude, Longitude, Altitude, transform.rotation);
            FinishAnchor(anchor);
        }

        private IEnumerator ResolveTerrainAnchor(ARAnchorManager anchorManager)
        {
            ARGeospatialAnchor anchor = null;
            ResolveAnchorOnTerrainPromise promise =
                        anchorManager.ResolveAnchorOnTerrainAsync(
                            Latitude, Longitude, AltitudeOffset, transform.rotation);

            yield return promise;
            var result = promise.Result;
            if (result.TerrainAnchorState == TerrainAnchorState.Success)
            {
                anchor = result.Anchor;
            }

            FinishAnchor(anchor);
            yield break;
        }

        private IEnumerator ResolveRooftopAnchor(ARAnchorManager anchorManager)
        {
            ARGeospatialAnchor anchor = null;
            ResolveAnchorOnRooftopPromise promise =
                        anchorManager.ResolveAnchorOnRooftopAsync(
                            Latitude, Longitude, AltitudeOffset, transform.rotation);

            yield return promise;
            var result = promise.Result;
            if (result.RooftopAnchorState == RooftopAnchorState.Success)
            {
                anchor = result.Anchor;
            }

            FinishAnchor(anchor);
            yield break;
        }
#endif // !UNITY_EDITOR
    }
}

