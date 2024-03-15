//-----------------------------------------------------------------------
// <copyright file="GeospatialAnchorUpdater.cs" company="Google LLC">
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

#if ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Editor.Internal
{
    using System;
    using Google.XR.ARCoreExtensions.Editor.Internal;
    using Google.XR.ARCoreExtensions.GeospatialCreator;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
#if ARCORE_INTERNAL_USE_UNITY_MATH
    using Unity.Mathematics;
#endif
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Handles Editor Updates for ARGeospatialCreatorAnchor objects. Specifically, it updates the
    /// anchor's lat/lon/alt properties whenever the transform is updated, and vice versa. This
    /// class is used instead of implementing ARGeospatialCreatorAnchor's Update() method to avoid
    /// requiring the Cesium dependency in the GeospatialCreator's runtime assembly references.
    /// </summary>
    [InitializeOnLoad]
    internal class GeospatialAnchorUpdater
    {
        private static GeospatialObjectTracker<ARGeospatialCreatorAnchor> tracker;

        private ARGeospatialCreatorAnchor _anchor;

        // The default is positiveInfinity so it won't match the default transform.position value
        // for a new object. This forces the geospatial coordinate to update on the next frame.
        private Vector3 _previousPosition = Vector3.positiveInfinity;
        private Quaternion _previousRotation = Quaternion.identity;
        private Vector3 _previousScale = Vector3.one;
        private GeoCoordinate _previousCoor = new GeoCoordinate(Mathf.Infinity, Mathf.Infinity, 0);
        private double _previousEditorAltitudeOverride = Mathf.Infinity;
        private bool? _previousUseEditorAltitudeOverride = null;

        // Use a static initializer, plus the InitializeOnLoad attribute, to ensure objects in the
        // scene are always being tracked.
        static GeospatialAnchorUpdater()
        {
            Func<ARGeospatialCreatorAnchor, Action> actionFactory = anchor =>
                (new GeospatialAnchorUpdater(anchor)).EditorUpdate;

            var tracker = new GeospatialObjectTracker<ARGeospatialCreatorAnchor>(actionFactory);
            tracker.StartTracking();
            GeospatialCreatorHelper.CountGeospatialCreatorAnchorsDelegate =
                tracker.GetTrackedObjectsCount;
        }

        public GeospatialAnchorUpdater(ARGeospatialCreatorAnchor anchor)
        {
            _anchor = anchor;
        }

        private void EditorUpdate()
        {
            // The anchor maintains a flag to indicate if the Unity world coordinates (i.e., the
            // GameObject's transform.position values) need to be updated to match the anchor's
            // recently-changed geodetic coordinate properties. If the geodetic coordinate
            // properties have not changed, then check if the Unity world coordinates were modified
            // and update the geodetic coordinates to match, if necessary.
            // TODO: b/306151548 - Unity position needs update when Origin's Unity coords change.
            if (_anchor._unityPositionUpdateRequired)
            {
                UpdateUnityPosition();
            }
            else if (
                _previousPosition != _anchor.transform.position ||
                _previousRotation != _anchor.transform.rotation ||
                _previousScale != _anchor.transform.localScale)
            {
                UpdateGeospatialCoordinate();
            }
            else if (HasGeodeticPositionChanged())
            {
                // This is a special lower-priority check for the explicit geodetic values, in case
                // the Anchor's lat/lon/alt fields were updated directly (not via the property
                // setters) which prevents the _unityPositionUpdateRequired flag from being set.
                // This will happen when serialization-based state modifications occur, such as
                // SerializedObject.ApplyModifiedProperties or the Undo/Redo system. We check this
                // last because any change to the transform has a higher priority. See b/302310301
                // for additional context.
                UpdateUnityPosition();
            } else
            {
                // No change since either position was last updated.
                return;
            }

            _previousPosition = _anchor.transform.position;
            _previousRotation = _anchor.transform.rotation;
            _previousScale = _anchor.transform.localScale;
            _previousCoor = new GeoCoordinate(
                _anchor.Latitude,
                _anchor.Longitude,
                _anchor.Altitude);
            _previousEditorAltitudeOverride = _anchor.EditorAltitudeOverride;
            _previousUseEditorAltitudeOverride = _anchor.UseEditorAltitudeOverride;
            _anchor._unityPositionUpdateRequired = false;
        }

        private bool HasGeodeticPositionChanged()
        {
            if (!GeoMath.ApproximatelyEqualsDegrees(_previousCoor.Latitude, _anchor.Latitude) ||
                !GeoMath.ApproximatelyEqualsDegrees(_previousCoor.Longitude, _anchor.Longitude) ||
                !GeoMath.ApproximatelyEqualsMeters(_previousCoor.Altitude, _anchor.Altitude))
            {
                return true;
            }

            if (!_previousUseEditorAltitudeOverride.HasValue ||
                _previousUseEditorAltitudeOverride.Value != _anchor.UseEditorAltitudeOverride)
            {
                return true;
            }

            // Only check the override values if the flag is enabled. When the flag is disabled,
            // we care about the Altitude value instead, which was already checked.
            if (_anchor.UseEditorAltitudeOverride &&
                !GeoMath.ApproximatelyEqualsMeters(
                    _previousEditorAltitudeOverride, _anchor.EditorAltitudeOverride))
            {
                return true;
            }

            return false;
        }

        // returns the GeoCoordinate of the Origin assigned to this Anchor. If the Anchor does not
        // have an Origin assigned, the method logs a message and returns a suitable default, or
        // null if no Origins exist in the scene.
        private GeoCoordinate FindOriginPoint(out Vector3 originUnityCoords)
        {
            ARGeospatialCreatorOrigin origin = _anchor.Origin;
            if (origin == null)
            {
                originUnityCoords = Vector3.zero;
                Debug.LogError("Cannot update the location for " + _anchor.gameObject.name + ": " +
                    "The Origin field is unassigned.");
            }
            else
            {
                originUnityCoords = origin.gameObject.transform.position;
                if (origin._originPoint == null)
                {
                    Debug.LogError("Cannot update the location for " + _anchor.gameObject.name +
                        ": " + "The Origin " + origin.gameObject.name + " has no Georeference.");
                }
            }

            return origin?._originPoint;
        }

        // :TODO: b/295547447 Automated testing for these two methods
        private void UpdateGeospatialCoordinate()
        {
            Vector3 originUnityCoords;
            GeoCoordinate originPoint = FindOriginPoint(out originUnityCoords);
            if (originPoint == null)
            {
                // An error message was already printed (if needed) in FindOriginPoint()
                return;
            }

            GeoCoordinate anchorGeoCoord = GeoMath.UnityWorldToGeoCoordinate(
                _anchor.transform.position,
                originPoint,
                originUnityCoords);

            _anchor.Longitude = anchorGeoCoord.Longitude;
            _anchor.Latitude = anchorGeoCoord.Latitude;

            if (_anchor.UseEditorAltitudeOverride)
            {
                _anchor.EditorAltitudeOverride = anchorGeoCoord.Altitude;
            } else
            {
                _anchor.Altitude = anchorGeoCoord.Altitude;
            }
        }

        private void UpdateUnityPosition()
        {
            Vector3 originUnityCoords;
            GeoCoordinate originPoint = FindOriginPoint(out originUnityCoords);
            if (originPoint == null)
            {
                // An error message was already printed (if needed) in FindOriginPoint()
                return;
            }

            // At runtime, an anchor's position is resolved exclusively using the geodetic
            // coordinates; the unity world position doesn't matter. In Editor mode, the unity
            // world position determies where it actually appears in the SceneView, so we honor the
            // EditorAltitudeOverride value, if it is used.
            double alt = _anchor.UseEditorAltitudeOverride ?
                _anchor.EditorAltitudeOverride : _anchor.Altitude;
            GeoCoordinate coor = new GeoCoordinate(
                _anchor.Latitude,
                _anchor.Longitude,
                alt);
            _anchor.transform.position = GeoMath.GeoCoordinateToUnityWorld(
                coor,
                originPoint,
                originUnityCoords);
        }
    }
}

#endif // ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
