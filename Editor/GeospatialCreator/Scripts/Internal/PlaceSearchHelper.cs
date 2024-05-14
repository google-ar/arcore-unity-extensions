//-----------------------------------------------------------------------
// <copyright file="PlaceSearchHelper.cs" company="Google LLC">
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
namespace Google.XR.ARCoreExtensions.GeospatialCreator.Editor.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
    using UnityEditor;
    using UnityEngine;

    internal class PlaceSearchHelper
    {
        private const string _searchFieldDefaultText =
            "Type the name of a place and press \"Enter\"";

        // Desired relative height (meters) from SceneView camera to target lat/lng's highest point
        // (ie. Rooftop, or Terrain if no building)
        private const double _sceneViewHeightAboveTile = 200;

        private static readonly string[] _waitAnimation = new string[] { "/", "-", "\\", "|" };

        private PlacesApi _placesApi;

        // Array of results, with a corresponding array of display names. We don't use a
        // Map<string, Place>, because we want to maintain the ordering returned by the API and
        // we want random access to the elements.
        private PlacesApi.Place[] _places = new PlacesApi.Place[0];
        private string[] _placeNames = new string[0];
        private int _selectedPlaceIndex = 0;

        private int _frameCount = 0;

        public delegate void SetLatLongAltDelegate(double lat, double lng, double Alt);

        public delegate void SetPreviewPinLocationDelegate(GeoCoordinate location);

        public void GUIForSearch(UnityEngine.Object targetObject, string targetTypeName,
            string apiKey, ARGeospatialCreatorOrigin origin,
            SetPreviewPinLocationDelegate setPreviewPinFunc,
            SetLatLongAltDelegate setLatLongAltFunc)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            // Search Bar, which shows gray prompt text until the user starts typing
            GUIStyle searchFieldStyle = new GUIStyle(EditorStyles.textField);
            string searchFieldText;
            if (string.IsNullOrEmpty(_placesApi?.SearchText))
            {
                searchFieldStyle.normal.textColor = Color.gray;
                searchFieldStyle.focused.textColor = EditorStyles.textField.normal.textColor;
                searchFieldText = _searchFieldDefaultText;
            }
            else
            {
                searchFieldText = _placesApi.SearchText;
            }

            // Send request if a new search text is entered.
            string newSearchText = EditorGUILayout.DelayedTextField(
                "Search for a place", searchFieldText, searchFieldStyle).Trim();
            if (newSearchText != _searchFieldDefaultText &&
                (_placesApi == null || newSearchText != _placesApi.SearchText))
            {
                // Abort a running request, if there is one.
                if (_placesApi != null)
                {
                    _placesApi.AbortRequest();
                }

                GeoCoordinate center = origin._originPoint;
                _placesApi =
                    new PlacesApi(newSearchText, center.Latitude, center.Longitude, apiKey);
                origin.StartCoroutine(_placesApi.SendRequest());
            }

            // show an animated "Searching..." label only when the search is active
            string status = string.Empty;
            if (_placesApi?.Status == PlacesApi.RequestStatus.InProgress)
            {
                // force OnInspectorGUI to be called next frame faking a change to the target
                // we need this as we are polling using SendWebRequest
                if (targetObject != null)
                {
                    EditorUtility.SetDirty(targetObject);
                }

                status = ("Searching... " +
                    _waitAnimation[(_frameCount / 10) % _waitAnimation.Length]);
                _frameCount++;
            }

            EditorGUILayout.LabelField(status);

            // There are three possible "completed" states: Failed, Succeeded, or Aborted:
            if (_placesApi?.Status == PlacesApi.RequestStatus.Failed)
            {
                Debug.LogError("Places search failed. Check that the API key property for your " +
                    "ARGeospatialCreatorOrigin is configured for the Places API (New), and that " +
                    "the Places API (new) is enabled in your Google Cloud project. See the " +
                    "following message for more information:\n" +
                    _placesApi.ErrorText);
                _placesApi = null;
            }
            else if (_placesApi?.Status == PlacesApi.RequestStatus.Succeeded)
            {
                _places = _placesApi.SearchResults.ToArray();
                _placeNames = _places.Select(p => p.Name + "; " + p.Address).ToArray();
                _selectedPlaceIndex = 0;
                if (_places.Length > 0)
                {
                    SetSceneViewPreview(_places[0].Latitude, _places[0].Longitude,
                        animate: true, setPreviewPinFunc);
                }

                _placesApi = null;
            }
            else if (_placesApi?.Status == PlacesApi.RequestStatus.Aborted)
            {
                // Nothing to log, the user must have initiated a new search.
                _placesApi = null;
            }

            // If the result is not empty then show those places.
            // Anchors' list is also only available when places' list isn't empty.
            EditorGUI.BeginDisabledGroup(_places.Length == 0);
            int oldSelectedPlaceIndex = _selectedPlaceIndex;
            _selectedPlaceIndex =
                EditorGUILayout.Popup("Results", _selectedPlaceIndex, _placeNames);

            if (oldSelectedPlaceIndex != _selectedPlaceIndex)
            {
                PlacesApi.Place place = _places[_selectedPlaceIndex];
                SetSceneViewPreview(
                    place.Latitude, place.Longitude, animate: true, setPreviewPinFunc);
            }

            // Attach the selected place latitude and longitude to the anchor or origin
            string s = $"Attach the selected place {targetTypeName}.";
            GUIContent attachPlaceToAnchor = new GUIContent("Apply to objects", s);
            if (GUILayout.Button(attachPlaceToAnchor))
            {
                PlacesApi.Place place = _places[_selectedPlaceIndex];
                setLatLongAltFunc(place.Latitude, place.Longitude, 0.0);
                SetSceneViewPreview(
                    place.Latitude, place.Longitude, animate: false, setPreviewPinFunc);

                // remove the preview pin since the object is there now
                setPreviewPinFunc(null);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }

        // Move the SceneView camera to a given lat/lng/alt and have it face the tiles.
        internal static void MoveSceneViewCamera(
            double lat,
            double lng,
            double cameraAltitude,
            bool animate)
        {
            ARGeospatialCreatorOrigin origin = ARGeospatialCreatorOrigin.FindDefaultOrigin();
            GeoCoordinate originPoint = origin?._originPoint;
            Vector3 originUnityCoord =
                origin ? origin.gameObject.transform.position : Vector3.zero;
            if (originPoint == null)
            {
                return;
            }

            // Make a high point where the camera will be
            GeoCoordinate cameraGeoCoord = new GeoCoordinate(lat, lng, cameraAltitude);
            Vector3 cameraViewPosition = GeoMath.GeoCoordinateToUnityWorld(
                cameraGeoCoord, originPoint, originUnityCoord);

            // Make a low point at a low altitude where the camera will pivot around
            // This altitude should be lower than any possible terrain since the camera
            // will not be able to easily zoom down to terrain below the pivot point
            GeoCoordinate targetGeoCoord =
                new GeoCoordinate(lat, lng, GeoMath.LowestElevationOnEarth);
            Vector3 targetTerrainPosition = GeoMath.GeoCoordinateToUnityWorld(
                targetGeoCoord, originPoint, originUnityCoord);

            // Get the direction vector
            Vector3 viewDirection = Vector3.Normalize(targetTerrainPosition - cameraViewPosition);

            SceneView sceneView = SceneView.lastActiveSceneView;

            // Compute the size parameter based on the desired distance from the camera.
            float desiredDistanceCameraToTarget =
                Vector3.Distance(cameraViewPosition, targetTerrainPosition);
            float size = desiredDistanceCameraToTarget *
                Mathf.Sin(sceneView.camera.fieldOfView * 0.5f * Mathf.Deg2Rad);

            if (animate)
            {
                sceneView.LookAt(targetTerrainPosition,
                    Quaternion.LookRotation(viewDirection), size);
            }
            else
            {
                sceneView.LookAtDirect(targetTerrainPosition,
                    Quaternion.LookRotation(viewDirection), size);
            }

            sceneView.Repaint();
        }

        private void SetSceneViewPreview(
            double lat, double lng, bool animate, SetPreviewPinLocationDelegate setPreviewPinFunc)
        {
#if ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
            ARGeospatialCreatorOrigin origin = ARGeospatialCreatorOrigin.FindDefaultOrigin();
            if (origin == null)
            {
                return;
            }

            MoveSceneViewCameraAboveTerrainAsync(
                origin._origin3DTilesetAdapter, lat, lng, animate);
#endif // ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED

            // The altitude value can be zero, because we're using the location for a 2D icon that
            // will be drawn over the scene view.
            setPreviewPinFunc(new GeoCoordinate(lat, lng, 0.0));
        }

        // Calculates the terrain altitude at the given lat/lng and moves the SceneView camera
        // at a reasonable height above it.
        private async void MoveSceneViewCameraAboveTerrainAsync(
            Origin3DTilesetAdapter tileset, double lat, double lng, bool animate)
        {
            // Move the camera high above the target location to begin loading the tiles
            MoveSceneViewCamera(lat, lng, GeoMath.HighestElevationOnEarth, animate);

            var (success, terrainAltitude) = await tileset.CalcTileAltitudeWGS84Async(lat, lng);
            double cameraAltitude;
            if (success)
            {
                cameraAltitude = terrainAltitude + _sceneViewHeightAboveTile;
            }
            else
            {
                // If terrain sampling failed then set the camera to an altitude above all terrain.
                Debug.Log("Could not get Terrain height. Setting camera at an altitude of " +
                GeoMath.HighestElevationOnEarth + " meters.");
                return;
            }

            MoveSceneViewCamera(lat, lng, cameraAltitude, animate);
        }
    }
}
