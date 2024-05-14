//-----------------------------------------------------------------------
// <copyright file="PlacesApi.cs" company="Google LLC">
//
// Copyright 2024 Google LLC
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
    using UnityEngine;
    using UnityEngine.Networking;

    // Implements the "New" Places API:
    // https://developers.google.com/maps/documentation/places/web-service/op-overview
    internal class PlacesApi
    {
        private const string _placesUrl = "https://places.googleapis.com/v1/places:searchText";

        // 50km preferred search radius, which is the maximum for both the New and legacy APIs
        private const int _searchRadius = 50000;

        private readonly string _apiKey;
        private readonly double _latitude;
        private readonly double _longitude;

        private UnityWebRequest _request;

        // TODO (b/333882315) This ordering does not match our internal style guides, but our
        // checks are enforcing it inconsistently. The ordering in this file should be fixed
        // once the proper checks are restored.
        public PlacesApi(string searchText, double latitude, double longitude, string apiKey)
        {
            SearchText = searchText;
            _apiKey = apiKey;
            _latitude = latitude;
            _longitude = longitude;
        }

        internal enum RequestStatus
        {
            NotStarted,
            InProgress,
            Aborted,
            Succeeded,
            Failed
        }

        public string ErrorText
        {
            get;
            private set;
        }
        = null;

        public RequestStatus Status
        {
            get;
            private set;
        }
        = RequestStatus.NotStarted;

        // The returned List will be null if the request has not completed successfully.
        public List<Place> SearchResults
        {
            get;
            private set;
        }

        public string SearchText
        {
            get;
            private set;
        }

        public IEnumerator SendRequest()
        {
            if (Status != RequestStatus.NotStarted)
            {
                Debug.LogWarning("PlacesApi.SendRequest() was called on a request that was " +
                    " already sent. This call is ignored.");
                yield break;
            }

            Status = RequestStatus.InProgress;
            _request = CreateNewRequest();

            using (_request)
            {
                yield return _request.SendWebRequest();

                if (Status == RequestStatus.Aborted)
                {
                    // No-op; the caller knows the request was aborted because it must have called
                    // AbortRequest()
                    yield break;
                }

                if (string.IsNullOrEmpty(_request.error))
                {
                    SearchResults = ParseSearchResults(_request);
                    Status = RequestStatus.Succeeded;
                    if (SearchResults.Count == 0)
                    {
                        ErrorText = "No results found. Response content:\n" +
                            _request.downloadHandler.text;
                        Status = RequestStatus.Failed;
                    }
                }
                else
                {
                    // Depending on the error, either the error field or the response content will
                    // have additional important information
                    ErrorText = $"Response code: {_request.responseCode}\n" +
                        $"Error message: \"{_request.error}\"\n" +
                        $"Response content:\n{_request.downloadHandler.text}";
                    Status = RequestStatus.Failed;
                }
            }
        }

        public void AbortRequest()
        {
            if (_request != null)
            {
                // Multiple calls to abort, or calls when the request has completed, will be
                // ignored:
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.Abort.html
                _request.Abort();

                // We don't explicitly call Dispose(); the `using` statement in SendRequest() will
                // ensure the request is disposed of properly
            }

            Status = RequestStatus.Aborted;
        }

        internal UnityWebRequest CreateNewRequest()
        {
            PlacesApiPostBody body =
                new PlacesApiPostBody(SearchText, _latitude, _longitude);
            string postBodyString = JsonUtility.ToJson(body);

            UnityWebRequest request = new UnityWebRequest(_placesUrl, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(postBodyString));
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("X-Goog-Api-Key", _apiKey);
            request.SetRequestHeader("X-Goog-FieldMask",
                "places.shortFormattedAddress,places.displayName,places.location");
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("accept", "application/json");

            return request;
        }

        // Extracts results from the request. Assumes the request has completed successfully.
        private List<Place> ParseSearchResults(UnityWebRequest request)
        {
            PlacesApiResponse response =
                JsonUtility.FromJson<PlacesApiResponse>(request.downloadHandler.text);

            List<Place> places = new List<Place>();
            foreach (PlacesApiResponse.Place p in response.places)
            {
                places.Add(
                    new Place(p.displayName.text,
                              p.shortFormattedAddress,
                              p.location.latitude,
                              p.location.longitude));
            }

            return places;
        }

        public struct Place
        {
            public readonly double Latitude;
            public readonly double Longitude;
            public readonly string Name;
            public readonly string Address;

            public Place(string name, string address, double lat, double lon)
            {
                Latitude = lat;
                Longitude = lon;
                Name = name;
                Address = address;
            }
        }

        // The following structures are used to parse the json in the Places API responses.
        [System.Serializable]
        [SuppressMessage("UnityRules.UnityStyleRules", "US1107:PublicFieldsMustBeUpperCamelCase",
          Justification = "Properties must match naming convention of the backend web response.")]
        [SuppressMessage("UnityRules.UnityStyleRules", "US1103:PublicFieldsMustHaveNoPrefix",
          Justification = "Properties must match naming convention of the backend web response.")]
        internal class PlacesApiPostBody
        {
            public string textQuery = null;
            public LocationBias locationBias = null;

            public PlacesApiPostBody(string textQuery, double latitude, double longitude)
            {
                this.textQuery = textQuery;
                locationBias = new LocationBias();
                locationBias.circle = new LocationBias.Circle();
                locationBias.circle.radius = _searchRadius;
                locationBias.circle.center = new LocationBias.Circle.Center();
                locationBias.circle.center.latitude = latitude;
                locationBias.circle.center.longitude = longitude;
            }

            [System.Serializable]
            internal class LocationBias
            {
                public Circle circle = null;

                [System.Serializable]
                internal class Circle
                {
                    public Center center = null;
                    public int radius;

                    [System.Serializable]
                    internal class Center
                    {
                        public double latitude = 0;
                        public double longitude = 0;
                    }
                }
            }
        }

        [System.Serializable]
        [SuppressMessage("UnityRules.UnityStyleRules", "US1107:PublicFieldsMustBeUpperCamelCase",
          Justification = "Properties must match naming convention of the backend web response.")]
        [SuppressMessage("UnityRules.UnityStyleRules", "US1103:PublicFieldsMustHaveNoPrefix",
          Justification = "Properties must match naming convention of the backend web response.")]
        internal class PlacesApiResponse
        {
            public Place[] places = null;
            public string status = null;

            [System.Serializable]
            internal class Place
            {
                public DisplayName displayName = null;
                public Location location = null;
                public string shortFormattedAddress = null;
            }

            [System.Serializable]
            internal class DisplayName
            {
                public string text = null;
                public string languageCode = null;
            }

            [System.Serializable]
            internal class Location
            {
                public double latitude = 0.0;
                public double longitude = 0.0;
            }
        }
    }
}

