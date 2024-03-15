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

namespace Google.XR.ARCoreExtensions.GeospatialCreator
{
    using System;
    using System.Collections;

    using Google.XR.ARCoreExtensions;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
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
    /// This object is responsible for creating a proper <c>ARGeospatialAnchor</c> at runtime at the
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

#if UNITY_EDITOR
        // This field is deprecated, but cannot be removed entirely because we need to be able to
        // migrate the value for customers upgrading from older versions of ARCore Extensions.
        [SerializeField]
        [Obsolete("Superseded by EditorAltitudeOverride. See MigrateAltitudeOffset() comments.",
            false)]
        internal double _altitudeOffset;
#endif // UNITY_EDITOR

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
        [SerializeField]
        private Boolean _useEditorAltitudeOverride;

        [SerializeField]
        private double _editorAltitudeOverride;

        internal bool _unityPositionUpdateRequired = false;
#endif // UNITY_EDITOR

#if !UNITY_EDITOR
#pragma warning restore CS0649
#endif

        [SerializeField]
        private ARAnchorManager _anchorManager = null;

#if !UNITY_EDITOR
        private AnchorResolutionState _anchorResolution = AnchorResolutionState.NotStarted;

        private enum AnchorResolutionState
        {
            NotStarted,
            InProgress,
            Complete
        }
#endif

        /// <summary>
        /// Gets and sets the <c><see cref="ARAnchorManager"/></c> used for resolving this anchor
        /// at runtime. The property is read-only at runtime.
        ///
        /// If <c>null</c>, this property will be given a default value during the
        /// <c>Awake()</c> message execution, as follows:
        /// <list type="bullet">
        /// <item>
#if ARCORE_USE_ARF_5 // use ARF 5
        /// If the <c><see cref="XROrigin"/></c> has an <c><see cref="AnchorManager"/></c>
#elif ARCORE_USE_ARF_4 // use ARF 4
        /// If the <c><see cref="ARSessionOrigin"/></c> has an <c><see cref="AnchorManager"/></c>
#else // ARF error
#error error must define ARCORE_USE_ARF_5 or ARCORE_USE_ARF_4
#endif
        /// subcomponent, that <c><see cref="ARAnchorManager"/></c> will be used;
        /// </item>
        /// <item>
        /// otherwise the first active and enabled <c><see cref="ARAnchorManager"/></c> object
        /// returned by <c><see cref="Resources.FindObjectsOfTypeAll()"/></c> will be used.
        /// </item>
        /// <item>
        /// If there are no
        /// active and enabled <c><see cref="ARAnchorManager"/></c> components in the scene, the
        /// first inactive / disabled <c><see cref="ARAnchorManager"/></c> is used.
        /// </item>
        /// <item>
        /// If there are no <c><see cref="ARAnchorManager"/></c> objects in the scene, an error will
        /// be logged and this property will remain <c>null</c>.
        /// </item>
        /// </list>
        /// <c><see cref="ARGeospatialCreatorAnchor"/></c> objects will not be resolved at runtime
        /// if the property remains <c>null</c>.
        /// </summary>
        public ARAnchorManager AnchorManager
        {
            get => _anchorManager;
#if UNITY_EDITOR
            set => _anchorManager = value;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Gets or sets the Geospatial Creator Origin used to resolve the location of this anchor.
        /// This property only exists in Editor mode.
        ///
        /// This property will be given a default value in the Editor's <c>Awake()</c> message
        /// execution, as follows:
        /// <list type="bullet">
        /// <item>
        /// If there are no objects of type <c><see cref="ARGeospatialCreatorOrigin"/></c> in the
        /// scene, it will remain <c>null</c> and a warning will be logged.
        /// </item>
        /// <item>
        /// If there is exactly one object of type <c><see cref="ARGeospatialCreatorOrigin"/></c>
        /// in the scene, that origin will be assigned to this property.
        /// </item>
        /// <item>
        /// If more than one object of type <c><see cref="ARGeospatialCreatorOrigin"/></c> are in
        /// the scene, a warning is logged and the property will remain <c>null</c>. A default
        /// origin in the scene will be used to resolve the Anchor's location.
        /// </item>
        /// </list>
        /// </summary>
        public ARGeospatialCreatorOrigin Origin;

        /// <summary>
        /// Indicates if the anchor should be rendered in the Editor's Scene view at the default
        /// <c><see cref="ARGeospatialCreatorAnchor.Altitude"/></c>, or at the altitude specified by
        /// <c><see cref="EditorAltitudeOverride"/></c>. If <c>false</c>,
        /// <c><see cref="EditorAltitudeOverride"/></c> is ignored.
        /// <c>UseEditorAltitudeOverride</c> is not available at runtime.
        /// </summary>
        public bool UseEditorAltitudeOverride
        {
            get => _useEditorAltitudeOverride;
            set
            {
                if (_useEditorAltitudeOverride != value)
                {
                    _useEditorAltitudeOverride = value;
                    _unityPositionUpdateRequired = true;
                }
            }
        }

        /// <summary>
        /// Gets and sets the altitude (in WGS84 meters) at which the Anchor should be rendered in
        /// the Editor's scene view. This value is ignored when
        /// <c><see cref="UseEditorAltitudeOverride"/></c> is <c>true</c>.
        ///
        /// <c><see cref="EditorAltitudeOverride"/></c> is useful if the default altitude rooftop or
        /// terrain anchor is inaccurate, or if using WGS84 altitude and the scene geometry does not
        /// line up exactly with the real world. <c><see cref="EditorAltitudeOverride"/></c> is not
        /// used at runtime.
        /// </summary>
        public double EditorAltitudeOverride
        {
            get => _editorAltitudeOverride;
            set
            {
                if (_editorAltitudeOverride != value)
                {
                    _editorAltitudeOverride = value;

                    if (_useEditorAltitudeOverride)
                    {
                        _unityPositionUpdateRequired = true;
                    }
                }
            }
        }
#endif // UNITY_EDITOR

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
        /// Gets or sets the altitude. When <c><see cref="AltitudeType"/></c> is
        /// <c><see cref="AltitudeType.WGS84"/></c>, this value is the altitude of
        /// the anchor, in meters according to WGS84.
        ///
        /// When <c><see cref="AltitudeType"/></c> is <c><see cref="AltitudeType.Terrain"/></c> or
        /// <c><see cref="AltitudeType.Rooftop"/></c>, this value is ONLY used in Editor mode, to
        /// determine the altitude at which to render the anchor in the Editor's Scene View, and it
        /// is ignored in the Player.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the <c><see cref="AnchorAltitudeType"/></c> used for resolution of this
        /// anchor.
        /// </summary>
        public AnchorAltitudeType AltitudeType
        {
            get => _altitudeType;
#if UNITY_EDITOR
            set => _altitudeType = value;
#endif
        }

        // Helper function to find a default anchor manager in the scene, to be used if the
        // _anchorManager field is null.
        private static ARAnchorManager FindDefaultAnchorManager()
        {
            // Use the AnchorManager assigned to the AR Session Origin, if it exists.
#if ARCORE_USE_ARF_5 // use ARF 5
            ARAnchorManager sessionAnchorManager =
                ARCoreExtensions._instance?.Origin?.GetComponent<ARAnchorManager>();
#elif ARCORE_USE_ARF_4 // use ARF 4
            ARAnchorManager sessionAnchorManager =
                ARCoreExtensions._instance?.SessionOrigin?.GetComponent<ARAnchorManager>();
#else // ARF error
#error error must define ARCORE_USE_ARF_5 or ARCORE_USE_ARF_4
#endif
            if (sessionAnchorManager != null)
            {
                return sessionAnchorManager;
            }

            ARAnchorManager[] anchorManagers = Resources.FindObjectsOfTypeAll<ARAnchorManager>();

            // If none were found with the session origin, use the first active and enabled
            // AnchorManager in the scene as the default. Having multiple AnchorManagers is
            // supported, it is NOT cause for an error or warning.
            foreach (ARAnchorManager am in anchorManagers)
            {
                if (am.isActiveAndEnabled)
                {
                    return am;
                }
            }

            // Anchor managers are often disabled or inactive in the Editor, so we can return an
            // inactive one by default if there are none active / enabled.
            if (anchorManagers.Length > 0)
            {
                return anchorManagers[0];
            }

            // None found.
            Debug.LogError(_noAnchorManagersMessage);
            return null;
        }

        // Awake() and the helper function FindDefaultAnchorManager above are only used when the
        // API flag is enabled. When it is disabled:
        // 1) the default origin is resolved in the GeospatialAnchorUpdater class;
        // 2) the default ARAnchorManager is resolved at runtime in this class's Update() method.
        private void Awake()
        {
#if UNITY_EDITOR
            if (MigrateAltitudeOffset())
            {
                Debug.Log("_altitudeOffset field migrated to EditorAltitudeOverride for " + name);
                EditorUtility.SetDirty(this);
            }

            // The Origin is only used for updating the Anchor's position in Editor mode. If null,
            // assign it a default value as described in the Origin property's doc comments
            if (Origin == null)
            {
                // FindDefaultOrigin() will log an appropriate error or warning about a missing
                // origin or more than one origin, if needed.
                ARGeospatialCreatorOrigin defaultOrigin =
                    ARGeospatialCreatorOrigin.FindDefaultOrigin();
                if (defaultOrigin != null)
                {
                    Origin = defaultOrigin;
                    Debug.Log("The Origin property for " + gameObject.name + " should not be " +
                    "null. " + defaultOrigin.gameObject.name + " was assigned as default.");
                }
            }

#endif // UNITY_EDITOR
            // If the AnchorManager is null, assign it a default value as described in the
            // property's doc comments
            if (AnchorManager == null)
            {
                ARAnchorManager defaultAnchorManager = FindDefaultAnchorManager();
                if (defaultAnchorManager != null)
                {
                    _anchorManager = defaultAnchorManager;
                    Debug.Log("The AnchorManager property for " + gameObject.name + " should " +
                        "not be null. " + defaultAnchorManager.gameObject.name + " was assigned " +
                        "as default.");
                }
            }
        }

#if UNITY_EDITOR
        // Migrates the deprecated _altitudeOffset field that was used prior to the introduction of
        // of the public Geospatial Creator API.
        //
        // For rooftop & terrain anchors, the old _altitudeOffset field was used at runtime to
        // position terrain and rooftop anchors, while the _altitude field specified where to
        // render rooftop & terrain anchors in the Editor's SceneView. For WGS84 anchors,
        // _altitude was used for both runtime & Editor mode, and _altitudeOffset was ignored
        // completely. This created an inconsistency: _altitude was the relevant runtime value for
        // WGS84 anchors, while _altitudeOffset was the relevant runtime value for rooftop &
        // terrain anchors.
        //
        // With the introduction of the public API, the semantics have changed to remove that
        // inconsistency. _altitude is now the only value used at runtime, regardless of anchor
        // type. It specifies the absolute altitude for WGS84 anchors, or the relative offset from
        // the surface for rooftop & terrain anchors. The new field _editorAltitudeOverride can be
        // used by any anchor type to specify where the anchor is rendered (in WGS84 meters) in the
        // Editor SceneView. The override value is only used if _useEditorAltitudeOverride is true,
        // and it not relevant (and therefore omitted) at runtime. For new terrain & rooftop
        // anchors, the override should be manually set, since we don't currently attempt to
        // predict an appropriate override in the SceneView. This will be addressed in b/300498502.
        //
        // This method migrates the old _altitude & _altitudeOffset values to the new semantics,
        // and then sets the deprecated _altitudeOffset field to NaN to indicate the migration
        // is complete.
        //
        // Returns true if either _altitude or _editorAltitudeOffset has been modified, false
        // otherwise.
#pragma warning disable CS0618 // ignore access to the deprecated _altitudeOffset field
        internal bool MigrateAltitudeOffset()
        {
            if (Double.IsNaN(_altitudeOffset))
            {
                // The value will be NaN if & only if the anchor was previously migrated
                return false;
            }

            // There was no editor-specific offset for WGS84 anchors prior to the introduction of
            // the public Geospatial Creator API, and the semantics of _altitude didn't change for
            // WGS84 anchors, so there's nothing to migrate.
            if (_altitudeType == AnchorAltitudeType.WGS84)
            {
                _altitudeOffset = Double.NaN;
                return false;
            }

            if (_altitudeType == AnchorAltitudeType.Terrain ||
                _altitudeType == AnchorAltitudeType.Rooftop)
            {
                // If either the altitude or the offset were set to non-zero values previously,
                // migrate the values to match the new semantics. If both pre-migration values are
                // zero, migration is a no-op so pass through to return false.
                if (!Mathf.Approximately((float)_altitude, 0.0f) ||
                    !Mathf.Approximately((float)_altitudeOffset, 0.0f))
                {
                    _useEditorAltitudeOverride = true;
                    _editorAltitudeOverride = _altitude;
                    _altitude = _altitudeOffset;
                    _altitudeOffset = Double.NaN;
                    return true;
                }
            }

            // If the anchor type is not WGS84, Terrain, or Rooftop, it must be a new type that
            // was introduced after the API feature, so there's nothing to migrate.
            _useEditorAltitudeOverride = false;
            _editorAltitudeOverride = 0.0d;
            _altitudeOffset = Double.NaN;
            return false;

        }
#pragma warning restore CS0618
#endif // UNITY_EDITOR

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
            if (!ARCoreExtensions._instance)
            {
                // A null instance indicates there was some error initializing ARCore, which was
                // already logged elsewhere.
                return;
            }

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

            if (_anchorManager == null)
            {
                Debug.LogError("Unable to place ARGeospatialCreatorAnchor " + name + ": The " +
                    "AnchorManager property is null");
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

            _anchorManager.ReportCreateGeospatialCreatorAnchor(ApiGeospatialAnchorType.WGS84);
            FinishAnchor(anchor);
        }

        // Initiates asynchronous resolution of this anchor at (Latitude, Longitude) on the surface
        // of the local terrain. Assumes _anchorManager is not null and configured properly for
        // creating geospatial anchors.
        private IEnumerator ResolveTerrainAnchor()
        {
            ARGeospatialAnchor anchor = null;
            ResolveAnchorOnTerrainPromise promise =
                _anchorManager.ResolveAnchorOnTerrainAsync(
                    Latitude, Longitude, Altitude, transform.rotation);

            yield return promise;
            var result = promise.Result;
            if (result.TerrainAnchorState == TerrainAnchorState.Success)
            {
                anchor = result.Anchor;
            }

            _anchorManager.ReportCreateGeospatialCreatorAnchor(ApiGeospatialAnchorType.Terrain);
            FinishAnchor(anchor);
            yield break;
        }

#if ARCORE_USE_ARF_5 // use ARF 5
        // Initiates asynchronous resolution of this anchor at (Latitude, Longitude) on the surface
#elif ARCORE_USE_ARF_4 // use ARF 4
       // Initiates asynchronous resolution of this anchor at (Latitude, Longitude) on the surface
#else // ARF error
#error error must define ARCORE_USE_ARF_5 or ARCORE_USE_ARF_4
#endif
        // of the local skyline. Assumes _anchorManager is not null and configured properly for
        // creating geospatial anchors.
        private IEnumerator ResolveRooftopAnchor()
        {
            ARGeospatialAnchor anchor = null;
            ResolveAnchorOnRooftopPromise promise =
                _anchorManager.ResolveAnchorOnRooftopAsync(
                    Latitude, Longitude, Altitude, transform.rotation);

            yield return promise;
            var result = promise.Result;
            if (result.RooftopAnchorState == RooftopAnchorState.Success)
            {
                anchor = result.Anchor;
            }

            _anchorManager.ReportCreateGeospatialCreatorAnchor(ApiGeospatialAnchorType.Rooftop);
            FinishAnchor(anchor);
            yield break;
        }
#endif // !UNITY_EDITOR
    }
}
