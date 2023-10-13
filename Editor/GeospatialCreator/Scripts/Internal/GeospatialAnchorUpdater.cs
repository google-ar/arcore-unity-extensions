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
        private GeoCoordinate _previousCoor = new GeoCoordinate(Mathf.Infinity, Mathf.Infinity, 0);
        private Vector3 _previousScale = Vector3.one;

        // Use a static initializer, plus the InitializeOnLoad attribute, to ensure objects in the
        // scene are always being tracked.
        static GeospatialAnchorUpdater()
        {
            Func<ARGeospatialCreatorAnchor, Action> actionFactory = anchor =>
                (new GeospatialAnchorUpdater(anchor)).EditorUpdate;

            var tracker = new GeospatialObjectTracker<ARGeospatialCreatorAnchor>(actionFactory);
            tracker.StartTracking();
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
            else if (
                !GeoMath.ApproximatelyEqualsDegrees(_previousCoor.Latitude, _anchor.Latitude) ||
                !GeoMath.ApproximatelyEqualsDegrees(_previousCoor.Longitude, _anchor.Longitude) ||
                !GeoMath.ApproximatelyEqualsMeters(_previousCoor.Altitude, _anchor.Altitude))
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
            _anchor._unityPositionUpdateRequired = false;
        }

        // returns the GeoCoordinate of the Origin assigned to this Anchor. If the Anchor does not
        // have an Origin assigned, the method logs a message and returns a suitable default, or
        // null if no Origins exist in the scene.
        private GeoCoordinate FindOriginPoint()
        {
            ARGeospatialCreatorOrigin origin = ARGeospatialCreatorOrigin.FindDefaultOrigin();
            if (origin == null)
            {
                Debug.LogError("Cannot update the location for " + _anchor.gameObject.name + ": " +
                    "The Origin field is unassigned.");
            }
            else if(origin._originPoint == null)
            {
                Debug.LogError("Cannot update the location for " + _anchor.gameObject.name + ": " +
                    "The Origin " + origin.gameObject.name + " has no Georeference.");
            }

            return origin?._originPoint;
        }

        // :TODO: b/295547447 Automated testing for these two methods
        private void UpdateGeospatialCoordinate()
        {
            GeoCoordinate originPoint = FindOriginPoint();
            if (originPoint == null)
            {
                // An error message was already printed (if needed) in FindOriginPoint()
                return;
            }

            double4x4 ENUToECEF = GeoMath.CalculateEnuToEcefTransform(originPoint);
            double3 EUN = new double3(
                _anchor.transform.position.x,
                _anchor.transform.position.y,
                _anchor.transform.position.z);
            double3 ENU = new double3(EUN.x, EUN.z, EUN.y);
            double3 ECEF = MatrixStack.MultPoint(ENUToECEF, ENU);
            double3 llh = GeoMath.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ECEF);

            _anchor.Longitude = llh.x;
            _anchor.Latitude = llh.y;

            _anchor.Altitude = llh.z;
        }

        private void UpdateUnityPosition()
        {
            GeoCoordinate originPoint = FindOriginPoint();
            if (originPoint == null)
            {
                // An error message was already printed (if needed) in FindOriginPoint()
                return;
            }

            double alt = _anchor.Altitude;

            double4x4 ECEFToENU = GeoMath.CalculateEcefToEnuTransform(originPoint);
            GeoCoordinate coor = new GeoCoordinate(
                _anchor.Latitude,
                _anchor.Longitude,
                alt);
            double3 localInECEF = GeoMath.GeoCoordinateToECEF(coor);
            double3 ENU = MatrixStack.MultPoint(ECEFToENU, localInECEF);
            // Unity is EUN not ENU so swap z and y
            Vector3 EUN = new Vector3((float)ENU.x, (float)ENU.z, (float)ENU.y);

            _anchor.transform.position = EUN;
        }
    }
}

#endif // ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
