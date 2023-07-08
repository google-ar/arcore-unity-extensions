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
    using System.Collections.Generic;
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

        private bool shouldPosition = true;

        private Vector3 _previousPosition = Vector3.zero;
        private Quaternion _previousRotation = Quaternion.identity;
        private Vector3 _previousScale = Vector3.one;
        private GeoCoordinate _previousCoor = new GeoCoordinate(Mathf.Infinity, Mathf.Infinity, 0);

        // Use a static initializer, plus the InitializeOnLoad attribute, to ensure objects in the
        // scene are always being tracked.
        static GeospatialAnchorUpdater()
        {
            Func<ARGeospatialCreatorAnchor, Action> actionFactory = anchor =>
                (new GeospatialAnchorUpdater(anchor)).EditorUpdate;

            var tracker = new GeospatialObjectTracker<ARGeospatialCreatorAnchor>(actionFactory);
            tracker.StartTracking();
        }

        // :TODO b/278071434: Make the Origin a property of the anchor instead of finding it. This
        // implementation is inefficient to do on each Editor Update, but will be replaced soon.
        private static GeoCoordinate FindOriginPoint()
        {
            ARGeospatialCreatorOrigin[] origins =
                GameObject.FindObjectsOfType<ARGeospatialCreatorOrigin>();
            if (origins.Length == 0)
            {
                Debug.LogError("No valid ARGeospatialCreatorOrigin found in scene");
                return null;
            }
            if (origins.Length > 1)
            {
                Debug.LogWarning("Multiple ARGeospatialCreatorOrigin objects found in scene.");
            }
            return origins[0].OriginPoint;
        }

        public GeospatialAnchorUpdater(ARGeospatialCreatorAnchor anchor)
        {
            _anchor = anchor;
        }

        private void EditorUpdate()
        {
            if (_previousPosition != _anchor.transform.position ||
                _previousRotation != _anchor.transform.rotation ||
                _previousScale != _anchor.transform.localScale)
            {
                // Update lat/lon/alt to match the changed game coordinates
                UpdateGeospatialCoordinate();
            }
            else if (_previousCoor.Latitude != _anchor.Latitude ||
                _previousCoor.Longitude != _anchor.Longitude ||
                _previousCoor.Altitude != _anchor.Altitude)
            {
                // update the game coordinates to match the changed lat/lon/alt
                UpdateUnityPosition();
            } else
            {
                return;
            }

            _previousPosition = _anchor.transform.position;
            _previousRotation = _anchor.transform.rotation;
            _previousScale = _anchor.transform.localScale;
            _previousCoor = new GeoCoordinate(
                _anchor.Latitude,
                _anchor.Longitude,
                _anchor.Altitude);
        }

        // :TODO: b/277365140 Automated testing for these two methods
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

            double4x4 ECEFToENU = GeoMath.CalculateEcefToEnuTransform(originPoint);
            GeoCoordinate coor = new GeoCoordinate(
                _anchor.Latitude,
                _anchor.Longitude,
                _anchor.Altitude);
            double3 localInECEF = GeoMath.GeoCoordinateToECEF(coor);
            double3 ENU = MatrixStack.MultPoint(ECEFToENU, localInECEF);
            // Unity is EUN not ENU so swap z and y
            Vector3 EUN = new Vector3((float)ENU.x, (float)ENU.z, (float)ENU.y);

            _anchor.transform.position = EUN;
        }
    }
}

#endif // ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
