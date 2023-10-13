//-----------------------------------------------------------------------
// <copyright file="ARGeospatialCreatorOrigin.cs" company="Google LLC">
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

    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;

    /// <summary>
    /// Provides a Geospatial Creator Origin that has both a lat/lon and gamespace coordinates. This is
    /// the reference point used by AR Anchors made in the Geospatial Creator to resolve their
    /// location in gamespace.
    /// </summary>
#if ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
    [AddComponentMenu("XR/Geospatial Creator/AR Geospatial Creator Origin")]
#endif
    public class ARGeospatialCreatorOrigin : ARGeospatialCreatorObject
    {
        // Message logged when multiple origins are present. The parameter is the name of the
        // GameObject selected to be the default origin. Internal so it can be checked during
        // testing.
        internal const string _multipleOriginsMessageFormat =
            "Multiple ARGeospatialCreatorOrigins found in scene. Using {0} as the default origin.";

        // Message logged when no origins are present. Internal so it can be checked during
        // testing.
        internal const string _noOriginsMessage =
            "No valid ARGeospatialCreatorOrigin found in scene.";

        // Default location for all new Origins, at Google HQ
        internal static readonly GeoCoordinate _defaultOriginPoint =
            new GeoCoordinate(37.422098, -122.08286, 11.5);


        // Read-only access to the origin location, for convenience of internal classes which use
        // the GeoCoordinate class.
        //
        // IMPORTANT: This setter is private because it sets the field directly without updating
        // any subcomponents. To update this field and any coupled subcomponent, use the
        // SetOriginPoint() method.
        [SerializeField]
        internal GeoCoordinate _originPoint
        {
            get;
#if UNITY_EDITOR
            private set;
#endif
        }
        = _defaultOriginPoint;

#if UNITY_EDITOR
        // Manages the coupling between _originPoint and this GameObject's subcomponent that also
        // maintains a lat/lon/alt. This property is set in the Editor assembly from the
        // GeospatialOriginUpdater when the updater for this Origin is initialized. See the
        // comments in IOriginComponentAdapter for more details.
        internal IOriginComponentAdapter _originComponentAdapter;

        /// <summary>
        /// Sets the geospatial location for this origin, including updating the location for any
        /// dependent subcomponents. This method is only available in the Unity Editor; moving the
        /// origin at runtime is not supported.
        /// </summary>
        /// <param name="latitude">Latitude for the origin, in decimal degrees.</param>
        /// <param name="longitude">Longitude for the origin, in decimal degrees.</param>
        /// <param name="altitude">Altitude for the origin, meters according to WGS84.</param>
        internal void SetOriginPoint(double latitude, double longitude, double altitude)
        {
            _originPoint = new GeoCoordinate(latitude, longitude, altitude);
            if (_originComponentAdapter != null)
            {
                _originComponentAdapter.SetComponentOrigin(_originPoint);
            }
        }

        // Updates the internal origin location using the value in the subcomponent, if available.
        // See the documentation for IOriginComponentAdapater for more information.
        internal void UpdateOriginFromComponent()
        {
            if (_originComponentAdapter == null)
            {
                Debug.LogWarning("Unable to update Origin subcomponent for " + gameObject.name +
                    ", the component adapter is null.");
            }
            else
            {
                // GetOriginFromComponent() will return null if there's no subcomponent that
                // tracks the origin. This is not an error state, for example if the origin was
                // recently created, the subcomponent may not have been added yet. In that case,
                // this is a no-op and _originPoint is not updated.
                GeoCoordinate newOriginPoint =
                    _originComponentAdapter.GetOriginFromComponent();
                if (newOriginPoint != null)
                {
                    _originPoint = newOriginPoint;
                }
            }
        }
#endif // UNITY_EDITOR

        /// <summary>
        /// Finds a suitable default origin, if one exists in the scene. If exactly one origin
        /// exists in scene, that origin is returned. If no origin exists, returns null and an
        /// error is logged. If more than one origins exist, returns the first origin according to
        /// GameObject.FindObjectsOfType() and a warning is logged. FindObjectsOfType is non-stable
        /// so the returned origin will not necessarily be the same across executions.
        /// </summary>
        /// <returns>A suitable default origin, or null if none exist.</returns>
        internal static ARGeospatialCreatorOrigin FindDefaultOrigin()
        {
            ARGeospatialCreatorOrigin[] origins =
                GameObject.FindObjectsOfType<ARGeospatialCreatorOrigin>();

            if (origins.Length == 0)
            {
                Debug.LogError(_noOriginsMessage);
                return null;
            }

            if (origins.Length > 1)
            {
                Debug.LogWarning(
                    string.Format(_multipleOriginsMessageFormat, origins[0].gameObject.name));
            }

            return origins[0];
        }
    }
}
