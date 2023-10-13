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
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// A representation of a Geospatial Anchor that was created using the Geospatial Creator tool.
    /// This object is responsible for creating a proper ARGeospatialAnchor at runtime at the
    /// latitude, longitude, and altitude specified.
    /// </summary>
    [AddComponentMenu("XR/AR Geospatial Creator Anchor")]
    [ExecuteInEditMode]
#if !UNITY_EDITOR
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1624:PropertySummaryDocumentationMustOmitSetAccessorWithRestrictedAccess",
        Justification = "Many of the properties have setters that are only available in Editor " +
            "mode, so the documentation should read \"Gets and sets...\". Because the setters " +
            "are excluded from Runtime, the SA1624 style check will fail for non-Editor builds.",
        Scope = "type")]
#endif // !UNITY_EDITOR
    public class ARGeospatialCreatorAnchor : ARGeospatialCreatorObject
    {
        // Message logged when no AnchorManagers are present. This is internal so it can be checked
        // during testing.
        internal const string _noAnchorManagersMessage =
            "No ARAnchorManagers were found in the scene.";

        // TODO (b/298042491) This can be private & editor-only when the GEOSPATIAL_CREATOR_API
        // flag is permanently enabled. It cannot be removed entirely because we need to be able to
        // migrate the value for customers upgrading from older versions of ARCore Extensions.
        [SerializeField]
        internal double _altitudeOffset;

#if !UNITY_EDITOR
#pragma warning disable CS0649
        // Disable "Field 'field' is never assigned to..." error. The following fields are all
        // read-only at runtime, so disable the error about them not being written except for in
        // Editor mode.
#endif
        [SerializeField]
        private double _latitude;

        [SerializeField]
        private double _longitude;

        [SerializeField]
        private double _altitude;

        [SerializeField]
        [field: FormerlySerializedAs("AltType")]
        private AnchorAltitudeType _altitudeType = AnchorAltitudeType.WGS84;

#if UNITY_EDITOR

        internal bool _unityPositionUpdateRequired = false;
#endif // UNITY_EDITOR

#if !UNITY_EDITOR
#pragma warning restore CS0649
#endif

#if UNITY_EDITOR
        // The _anchorManager is never accessed in Editor mode when the API feature flag is off. It
        // is still used at runtime so we'll suppress this error rather than exclude the field.
        // This warning can be re-enabled for all builds once the feature flag is permanently true.
#pragma warning disable CS0414
#endif // UNITY_EDITOR
        [SerializeField]
        private ARAnchorManager _anchorManager = null;
#if UNITY_EDITOR
#pragma warning restore CS0414
#endif // UNITY_EDITOR

#if !UNITY_EDITOR
        private AnchorResolutionState _anchorResolution = AnchorResolutionState.NotStarted;

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
#if UNITY_EDITOR
            set
            {
                if (_latitude != value)
                {
                    _latitude = value;
                    _unityPositionUpdateRequired = true;
                }
            }
#endif
        }

        /// <summary>Gets or sets the longitude of this anchor.</summary>
        public double Longitude
        {
            get => this._longitude;
#if UNITY_EDITOR
            set
            {
                if (_longitude != value)
                {
                    _longitude = value;
                    _unityPositionUpdateRequired = true;
                }
            }
#endif
        }

        /// <summary>
        /// Gets or sets the altitude. When AltitudeType is WSG84, this value is the altitude of
        /// the anchor, in meters according to WGS84. When AltitudeType is Terrain or Rooftop, this
        /// value is ONLY used in Editor mode, to determine the altitude at which to render the
        /// anchor in the Editor's Scene View. If AltitudeType is Terrain or Rooftop, this value is
        /// ignored in the Player.</summary>
        public double Altitude
        {
            get => this._altitude;
#if UNITY_EDITOR
            set
            {
                if (_altitude != value)
                {
                    _altitude = value;
                    _unityPositionUpdateRequired = true;
                }
            }
#endif
        }

        /// <summary>Gets or sets the AltitudeType used for resolution of this anchor.</summary>
        public AnchorAltitudeType AltitudeType
        {
            get => _altitudeType;
#if UNITY_EDITOR
            set => _altitudeType = value;
#endif
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

            _anchorManager =
                ARCoreExtensions._instance.SessionOrigin?.GetComponent<ARAnchorManager>();

            if (_anchorManager == null)
            {
                string errorReason =
                    "The Session Origin's AnchorManager is null.";
                Debug.LogError("Unable to place ARGeospatialCreatorAnchor " + name + ": " +
                    errorReason);
                _anchorResolution = AnchorResolutionState.Complete;
                return;
            }

            _anchorResolution = AnchorResolutionState.InProgress;
            switch (this.AltitudeType)
            {
                case AnchorAltitudeType.Terrain:
                    StartCoroutine(ResolveTerrainAnchor());
                    break;
                case AnchorAltitudeType.Rooftop:
                    StartCoroutine(ResolveRooftopAnchor());
                    break;
                case AnchorAltitudeType.WGS84:
                    // WGS84 altitude anchors don't use async APIs, so there's no coroutine
                    ResolveWGS84AltitudeAnchor();
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

        // Synchronously resolves this anchor at (Latitude, Longitude, Altitude). Assumes
        // _anchorManager is not null and configured properly for creating geospatial anchors.
        private void ResolveWGS84AltitudeAnchor()
        {
            ARGeospatialAnchor anchor = _anchorManager.AddAnchor(
                Latitude, Longitude, Altitude, transform.rotation);
            FinishAnchor(anchor);
        }

        // Initiates asynchronous resolution of this anchor at (Latitude, Longitude) on the surface
        // of the local terrain. Assumes _anchorManager is not null and configured properly for
        // creating geospatial anchors.
        private IEnumerator ResolveTerrainAnchor()
        {
            double altitudeAboveTerrain = _altitudeOffset;
            ARGeospatialAnchor anchor = null;
            ResolveAnchorOnTerrainPromise promise =
                _anchorManager.ResolveAnchorOnTerrainAsync(
                    Latitude, Longitude, altitudeAboveTerrain, transform.rotation);

            yield return promise;
            var result = promise.Result;
            if (result.TerrainAnchorState == TerrainAnchorState.Success)
            {
                anchor = result.Anchor;
            }

            FinishAnchor(anchor);
            yield break;
        }

       // Initiates asynchronous resolution of this anchor at (Latitude, Longitude) on the surface
        // of the local skyline. Assumes _anchorManager is not null and configured properly for
        // creating geospatial anchors.
        private IEnumerator ResolveRooftopAnchor()
        {
            double altitudeAboveRooftop = _altitudeOffset;
            ARGeospatialAnchor anchor = null;
            ResolveAnchorOnRooftopPromise promise =
                _anchorManager.ResolveAnchorOnRooftopAsync(
                    Latitude, Longitude, altitudeAboveRooftop, transform.rotation);

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
