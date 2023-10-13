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
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
#if ARCORE_INTERNAL_USE_UNITY_MATH
    using Unity.Mathematics;
#endif
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Networking;

    internal class PlaceSearchHelper
    {
        private const string _searchFieldDefaultText =
            "Type the name of a place and press \"Enter\"";

        private static readonly string[] _waitAnimation = new string[] { "/", "-", "\\", "|" };

        private int _selectedPlaceIndex = 0;
        private UnityWebRequest _placeApiRequest = null;
        private int _frameCount = 0;
        private PlaceSearchRequest _request = new PlaceSearchRequest();
        private PlaceSearchResponse _response = new PlaceSearchResponse();

        public delegate void SetLatLongAltDelegate(double lat, double lng, double Alt);

        public delegate void SetPreviewPinLocationDelegate(GeoCoordinate location);

        public void GUIForSearch(UnityEngine.Object targetObject, string targetTypeName,
            string apiKey, GeoCoordinate originPoint,
            SetPreviewPinLocationDelegate setPreviewPinFunc,
            SetLatLongAltDelegate setLatLongAltFunc)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            // Search Bar, which shows gray prompt text until the user starts typing
            GUIStyle searchFieldStyle = new GUIStyle(EditorStyles.textField);
            string searchFieldText;
            if (_request == null || string.IsNullOrEmpty(_request.SearchText))
            {
                searchFieldStyle.normal.textColor = Color.gray;
                searchFieldStyle.focused.textColor = EditorStyles.textField.normal.textColor;
                searchFieldText = _searchFieldDefaultText;
            }
            else
            {
                searchFieldText = _request.SearchText;
            }

            // Send request if a new search text is entered.
            string newSearchText = EditorGUILayout.DelayedTextField(
                "Search for a place", searchFieldText, searchFieldStyle).Trim();
            if (newSearchText != _searchFieldDefaultText &&
                (_request == null || newSearchText != _request.SearchText))
            {
                StartNewRequest(newSearchText, originPoint, apiKey);
            }

            // show an animated "Searching..." label only when the search is active
            string status = string.Empty;
            if (_placeApiRequest != null && !_placeApiRequest.isDone)
            {
                // force OnInspectorGUI to be called next frame faking a change to the target
                // we need this as we are polling using _placeApiRequest.SendWebRequest with
                // _placeApiRequest.isDone
                if (targetObject != null)
                {
                    EditorUtility.SetDirty(targetObject);
                }

                status = ("Searching... " +
                    _waitAnimation[(_frameCount / 10) % _waitAnimation.Length]);
                _frameCount++;
            }

            EditorGUILayout.LabelField(status);

            // Parse the result of place api response.
            if (_placeApiRequest != null && _placeApiRequest.isDone)
            {
                if (!string.IsNullOrWhiteSpace(_placeApiRequest.error))
                {
                    Debug.LogError(_placeApiRequest.error);
                }
                else
                {
                    _response = PlaceSearchResponse.ParseJson(
                        _placeApiRequest.downloadHandler.text);

                    // copy NextPageToken from the _response to the _request
                    // this will make the next page button work
                    _request.NextPageToken = _response.NextPageToken;
                    PlaceSearchResponse.Location loc = _response.GetLocation(_selectedPlaceIndex);
                    if (loc != null)
                    {
                        SetSceneViewPreview(loc.lat, loc.lng, _request.CameraAltitude,
                            _request.CameraAltitude - 3500.0f, animate: true, setPreviewPinFunc);
                    }
                }

                // Clean up the request
                _placeApiRequest.Dispose();
                _placeApiRequest = null;
            }

            // If the result is not empty then show those places.
            // Anchors' list is also only available when places' list isn't empty.
            EditorGUI.BeginDisabledGroup(!_response.HasPages());
            int oldSelectedPlaceIndex = _selectedPlaceIndex;
            _selectedPlaceIndex =
                EditorGUILayout.Popup("Results", _selectedPlaceIndex, _response.ResultsAddress);
            if (oldSelectedPlaceIndex != _selectedPlaceIndex)
            {
                PlaceSearchResponse.Location loc = _response.GetLocation(_selectedPlaceIndex);
                if (loc != null)
                {
                    SetSceneViewPreview(loc.lat, loc.lng, _request.CameraAltitude,
                    _request.CameraAltitude - 3500.0f, animate: true, setPreviewPinFunc);
                }
            }

            // Attach the selected place latitude and longitude to the anchor or origin
            string s = $"Attach the selected place {targetTypeName}.";
            GUIContent attachPlaceToAnchor = new GUIContent("Apply to objects", s);
            if (GUILayout.Button(attachPlaceToAnchor))
            {
                PlaceSearchResponse.Location loc = _response.GetLocation(_selectedPlaceIndex);
                if (loc != null)
                {
                    setLatLongAltFunc(loc.lat, loc.lng, 0.0);
                    SetSceneViewPreview(loc.lat, loc.lng, _request.CameraAltitude,
                        _request.CameraAltitude - 3500.0f, animate: false, setPreviewPinFunc);

                    // remove the preview pin since the object is there now
                    setPreviewPinFunc(null);
                }
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }

        private void StartNewRequest(string searchString, GeoCoordinate originPoint, string apiKey)
        {
            // cancel any in-progress request
            if (_placeApiRequest != null)
            {
                _placeApiRequest.Abort(); // Abort is a no-op if the request already finished
                _placeApiRequest.Dispose();
            }

            _request = new PlaceSearchRequest();
            _request.SearchText = searchString;
            _request.ApiKey = apiKey;
            _request.NextPageToken = string.Empty;
            _request.Latitude = originPoint.Latitude;
            _request.Longitude = originPoint.Longitude;

            _placeApiRequest = UnityWebRequest.Get(_request.GetUrl());
            _placeApiRequest.SendWebRequest();

            _selectedPlaceIndex = 0;
        }

        private void SetSceneViewPreview(double lat, double lng, double cameraAlt,
            double objectAlt, bool animate, SetPreviewPinLocationDelegate setPreviewPinFunc)
        {
            // hack the alt so it is above the point we place other things
#if ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
            GeoCoordinate originPoint =
                ARGeospatialCreatorOrigin.FindDefaultOrigin()?._originPoint;
#else // ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
            // just quick out if we creator is not enabled so can't get lat lng.
            GeoCoordinate originPoint = null;
#endif // ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED

            if (originPoint == null)
            {
                // An error message was already printed (if needed) in FindOriginPoint()
                return;
            }

            double4x4 ECEFToENU = GeoMath.CalculateEcefToEnuTransform(originPoint);

            // Make a high point where the camera will be
            GeoCoordinate coor = new GeoCoordinate(lat, lng, cameraAlt);
            double3 localInECEF = GeoMath.GeoCoordinateToECEF(coor);
            double3 ENU = MatrixStack.MultPoint(ECEFToENU, localInECEF);

            // Unity is EUN not ENU so swap z and y
            Vector3 EUNHigh = new Vector3((float)ENU.x, (float)ENU.z, (float)ENU.y);

            // Make a low point where the object is
            coor = new GeoCoordinate(lat, lng, objectAlt);
            localInECEF = GeoMath.GeoCoordinateToECEF(coor);
            ENU = MatrixStack.MultPoint(ECEFToENU, localInECEF);

            // Unity is EUN not ENU so swap z and y
            Vector3 EUNLow = new Vector3((float)ENU.x, (float)ENU.z, (float)ENU.y);

            // make a vector from that points from high to low.
            // So the vector that points down to earth
            Vector3 earthDownVec = Vector3.Normalize(EUNLow - EUNHigh);
            if (animate)
            {
                // position the camera at EUNHigh
                // Rotate from forward to earth down
                SceneView.lastActiveSceneView.LookAt(EUNLow,
                    Quaternion.FromToRotation(Vector3.forward, earthDownVec));
                SceneView.lastActiveSceneView.pivot = EUNHigh;
            }
            else
            {
                // position the camera at EUNHigh
                // Rotate from forward to earth down
                SceneView.lastActiveSceneView.LookAtDirect(EUNLow,
                    Quaternion.FromToRotation(Vector3.forward, earthDownVec));
                SceneView.lastActiveSceneView.pivot = EUNHigh;
            }

            // The altitude value can be zero, because we're using the location for a 2D icon that
            // will be drawn over the scene view.
            setPreviewPinFunc(new GeoCoordinate(lat, lng, 0.0));
        }
    }

    internal class PlaceSearchRequest
    {
        public string ApiKey;
        public bool LocationBias = false;
        public double Latitude;
        public double Longitude;
        public float CameraAltitude = 3500.0f;
        public int Radius = 50000;
        public string NextPageToken = string.Empty;
        public string SearchText = string.Empty;

        public string GetUrl()
        {
            string location = LocationBias ? Latitude + "," + Longitude : string.Empty;

            string url = CreatePlaceApiUrl(ApiKey, SearchText, NextPageToken,
                                  location, Radius);

            Debug.Log("GeospatialCreator Places Api: " + url);
            return url;
        }

        public bool HasNextPage()
        {
            return !string.IsNullOrEmpty(NextPageToken);
        }

        internal static string CreatePlaceApiUrl(string apiKey, string query, string pageToken,
            string location, int radius)
        {
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append("https://maps.googleapis.com/maps/api/place/textsearch/json?");
            urlBuilder.Append("key=" + apiKey);
            urlBuilder.Append("&query=" + query);

            if (!string.IsNullOrEmpty(pageToken))
            {
                urlBuilder.Append("&pagetoken=" + pageToken);
            }

            // Radius is defined by meters and 50,000 is the maximum. And it comes together
            // with location parameter
            // (http://shortn/_W8iiioFbXV)
            // And location is specified as latitude,longitude
            // (http://shortn/_EaVtW19qdg)
            if (!string.IsNullOrEmpty(location))
            {
                urlBuilder.Append("&location=" + location);
                urlBuilder.Append("&radius=" + radius);
            }

            string url = urlBuilder.ToString();
            return url;
        }
    }

    internal class PlaceSearchResponse
    {
        public string[] ResultsAddress = new string[0];
        public string NextPageToken = string.Empty;
        internal PlaceApiResponse _placeApiResponse = null;

        public static PlaceSearchResponse ParseJson(string responseText)
        {
            PlaceSearchResponse r = new PlaceSearchResponse();

            // Parse the JSON string into a custom class using JsonUtility
            r._placeApiResponse = JsonUtility.FromJson<PlaceApiResponse>(responseText);

            // can't parse json or no results
            if (r._placeApiResponse == null || r._placeApiResponse.results == null
                || r._placeApiResponse.results.Length == 0)
            {
                r.ResultsAddress = new string[0];
                return r;
            }

            r.NextPageToken = r._placeApiResponse.next_page_token;
            r.ResultsAddress = new string[r._placeApiResponse.results.Length];
            for (int i = 0; i < r._placeApiResponse.results.Length; i++)
            {
                r.ResultsAddress[i] = r._placeApiResponse.results[i].formatted_address;
            }

            return r;
        }

        public Location GetLocation(int selectedPlaceIndex)
        {
            if (_placeApiResponse != null &&
               selectedPlaceIndex >= 0 &&
               selectedPlaceIndex < _placeApiResponse.results.Length)
            {
                return _placeApiResponse.results[selectedPlaceIndex].geometry.location;
            }

            return null;
        }

        public bool HasPages()
        {
            return _placeApiResponse != null && ResultsAddress.Length != 0;
        }

        // The following structures are used to parse the json in the Places API responses.
        // Feel free to add more fields in PlacesTextSearchResponse if needed.
        // (http://shortn/_HWb8TmynQi)
        // But do NOT modify their names or types.
        [System.Serializable]
        [SuppressMessage("UnityRules.UnityStyleRules", "US1107:PublicFieldsMustBeUpperCamelCase",
          Justification = "Properties must match naming convention of the backend web response.")]
        [SuppressMessage("UnityRules.UnityStyleRules", "US1103:PublicFieldsMustHaveNoPrefix",
          Justification = "Properties must match naming convention of the backend web response.")]
        internal class PlaceApiResponse
        {
            public Result[] results = null;
            public string next_page_token = null;
            public string status = null;
        }

        [System.Serializable]
        [SuppressMessage("UnityRules.UnityStyleRules", "US1107:PublicFieldsMustBeUpperCamelCase",
          Justification = "Properties must match naming convention of the backend web response.")]
        [SuppressMessage("UnityRules.UnityStyleRules", "US1103:PublicFieldsMustHaveNoPrefix",
          Justification = "Properties must match naming convention of the backend web response.")]
        internal class Result
        {
            public string formatted_address = null;
            public Geometry geometry = null;
        }

        [System.Serializable]
        [SuppressMessage("UnityRules.UnityStyleRules", "US1107:PublicFieldsMustBeUpperCamelCase",
          Justification = "Properties must match naming convention of the backend web response.")]
        internal class Geometry
        {
            public Location location = null;
        }

        [System.Serializable]
        [SuppressMessage("UnityRules.UnityStyleRules", "US1107:PublicFieldsMustBeUpperCamelCase",
          Justification = "Properties must match naming convention of the backend web response.")]
        internal class Location
        {
            public double lat = 0.0;
            public double lng = 0.0;

            public Location(double latitude, double longitude)
            {
                lat = latitude;
                lng = longitude;
            }
        }
    }
}
