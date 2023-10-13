//-----------------------------------------------------------------------
// <copyright file="PlaceSearchWindow.cs" company="Google LLC">
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
    using System.Collections;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
#if ARCORE_INTERNAL_USE_UNITY_MATH
    using Unity.Mathematics;
#endif
    using UnityEditor;
    using UnityEngine;

    // Create a window with a place search diolog with the currently using the currently
    // selected objects
    internal class PlaceSearchWindow : EditorWindow
    {
        // GUID for the map pin icon file, relative to ARCore Extensions package:
        // Editor/GeospatialCreator/Resources/red-dot.png
        private const string _mapPinIconGuid = "38eb796b1a05b44dba0b8af24fbf1392";

        private static PlaceSearchWindow _window;

        private PlaceSearchHelper _searchHelper = new PlaceSearchHelper();
        private Texture2D _mapPinIcon = null;

        // The location to place the search preview pin icon, in Unity world coordinates.
        // If null, there is no search location to preview.
        private Vector3? _previewPinLocation = null;

        public static void ShowPlaceSearchWindow()
        {
            if (_window == null)
            {
                _window = ScriptableObject.CreateInstance(typeof(PlaceSearchWindow))
                    as PlaceSearchWindow;

                // the window can't be changed to be smaller than minSize by user.
                _window.minSize = new Vector2(450, 180);
                _window.titleContent = new GUIContent("Geospatial Creator Search");

                // pre-load the map pin icon
                _window._mapPinIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    AssetDatabase.GUIDToAssetPath(_mapPinIconGuid));
            }

            // The scene view doesn't redraw if the window is modal.
            // window.ShowModalUtility();
            // non-Modal
            _window.ShowUtility();
        }

        // Sets the lat/lon location at which to place the search preview pin icon. The altitude
        // parameter is ignored as the pin is drawn on top of the scene view, not rendered as a
        // GameObject.
        public void SetPreviewPinLocation(GeoCoordinate location)
        {
            if (location == null)
            {
                _previewPinLocation = null;
                return;
            }

            GeoCoordinate originPoint = GetOrigin()?._originPoint;
            if (originPoint == null)
            {
                Debug.LogWarning(
                    "Could not preview search result: Invalid ARGeospatialCreatorOrigin");

                _previewPinLocation = null;
                return;
            }

            // TODO (b/300087328) simplify and make this testable
            double4x4 ECEFToENU = GeoMath.CalculateEcefToEnuTransform(originPoint);
            double3 localInECEF = GeoMath.GeoCoordinateToECEF(location);
            double3 ENU = MatrixStack.MultPoint(ECEFToENU, localInECEF);

            // Unity is EUN not ENU so swap z and y
            _previewPinLocation = new Vector3((float)ENU.x, (float)ENU.z, (float)ENU.y);
        }

        // :TODO b/278071434: Make the Origin a property of the anchor instead of finding it. This
        // implementation is inefficient to do on each Editor Update, but will be replaced soon.
        private static ARGeospatialCreatorOrigin GetOrigin()
        {
            ARGeospatialCreatorOrigin[] origins =
                GameObject.FindObjectsOfType<ARGeospatialCreatorOrigin>();
            if (origins.Length == 0)
            {
                Debug.LogError("No valid ARGeospatialCreatorOrigin found in scene");
                return null;
            }

            return origins[0];
        }

        private void Awake()
        {
            SceneView.duringSceneGui += DrawPreviewPin;
        }

        private void OnDestroy()
        {
            _window = null;
            SceneView.duringSceneGui -= DrawPreviewPin;
        }

        // Delegate for SceneView.duringSceneGui. This draws a pin for the search result on the
        // scene view, if a search result is being previewed.
        private void DrawPreviewPin(SceneView sceneView)
        {
            if (_window == null || !_previewPinLocation.HasValue)
            {
                return;
            }

            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(_previewPinLocation.Value);

            // The bottom center of the pin icon should be at the guiPoint, but the bounding rect
            // rect for it is defined by the top-left corner:
            Rect rect = new Rect(
                guiPoint.x - (_mapPinIcon.width / 2),
                guiPoint.y - _mapPinIcon.height,
                _mapPinIcon.width,
                _mapPinIcon.height);
            Handles.BeginGUI();
            GUILayout.BeginArea(rect, _mapPinIcon);
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        private void SetLatLongAlt(double lat, double lng, double alt)
        {
            foreach (var obj in Selection.gameObjects)
            {
                var originTarget = obj.GetComponent<ARGeospatialCreatorOrigin>();
                var anchorTarget = obj.GetComponent<ARGeospatialCreatorAnchor>();
                if (originTarget != null)
                {
                    SetLatLongAltOrigin(originTarget, lat, lng, alt);
                }
                else if (anchorTarget != null)
                {
                    SetLatLongAltAnchor(anchorTarget, lat, lng, alt);
                }
            }
        }

        private void SetLatLongAltAnchor(ARGeospatialCreatorAnchor anchor, double lat, double lng,
            double alt)
        {
            // use SerializedObject so undo works
            var so = new SerializedObject(anchor);

            // you can Shift+Right Click on property names in the Inspector to see their paths
            so.FindProperty("_latitude").doubleValue = lat;
            so.FindProperty("_longitude").doubleValue = lng;
            so.ApplyModifiedProperties();
            anchor._unityPositionUpdateRequired = true;
        }

        private void SetLatLongAltOrigin(ARGeospatialCreatorOrigin origin, double lat, double lng,
            double alt)
        {
#if ARCORE_INTERNAL_USE_CESIUM
            CesiumForUnity.CesiumGeoreference geoRef =
                origin.gameObject.GetComponent<CesiumForUnity.CesiumGeoreference>();
            if (geoRef != null) {
                var so = new SerializedObject(geoRef);
                // you can Shift+Right Click on property names in the Inspector to see their paths
                so.FindProperty("_originAuthority").intValue =
                    (int)CesiumForUnity.CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;
                so.FindProperty("_latitude").doubleValue = lat;
                so.FindProperty("_longitude").doubleValue = lng;
                so.ApplyModifiedProperties();
                origin.SetOriginPoint(geoRef.latitude, geoRef.longitude, geoRef.height);
            }
#else // ARCORE_INTERNAL_USE_CESIUM
            throw new Exception("Cesium dependency is missing SetLatLong");
#endif // ARCORE_INTERNAL_USE_CESIUM
        }

        private int CountObjectWithComponent<T>()
        {
            int count = 0;
            foreach (var obj in Selection.gameObjects)
            {
                var targetType = obj.GetComponent<T>();
                if (targetType != null)
                {
                    count++;
                }
            }

            return count;
        }

        private void OnGUI()
        {
            ARGeospatialCreatorOrigin origin = GetOrigin();
            string apiKey = GetPlacesApiKey(origin);

            int anchorCount = CountObjectWithComponent<ARGeospatialCreatorAnchor>();
            int originCount = CountObjectWithComponent<ARGeospatialCreatorOrigin>();
            bool typeError = false;
            if (anchorCount != 0 && originCount == 0)
            {
                _searchHelper.GUIForSearch(targetObject: null, "Anchor",
                    apiKey, origin._originPoint, SetPreviewPinLocation, SetLatLongAlt);
            }
            else if (anchorCount == 0 && originCount != 0)
            {
                _searchHelper.GUIForSearch(targetObject: null, "Origin",
                    apiKey, origin._originPoint, SetPreviewPinLocation, SetLatLongAlt);
            }
            else
            {
                typeError = true;
            }

            GUIStyle labelWrapStyle = new GUIStyle(EditorStyles.label);
            labelWrapStyle.wordWrap = true;

            if (typeError)
            {
                GUILayout.Label($"Select an Anchor or Origin.", labelWrapStyle);
            }
            else
            {
                string selected = string.Empty;
                foreach (var t in Selection.transforms)
                {
                    selected += "   " + t.name + "\n";
                }

                string label =
                    Selection.transforms.Length < 2 ? "Selected Object:" : "Selected Objects:";
                GUILayout.Label($"{label}\n{selected}", labelWrapStyle);
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private string GetPlacesApiKey(ARGeospatialCreatorOrigin origin)
        {
            string apiKey = null;
#if ARCORE_INTERNAL_USE_CESIUM
            // The same API key is used for both Map Tiles and Places search.
            apiKey = GeospatialCreatorCesiumAdapter.GetMapTilesApiKey(origin);
#endif
            if (apiKey == null)
            {
                throw new Exception("No valid Places API key found. Please refer to the " +
                    "Geospatial Creator documentation for more information.");
            }

            return apiKey;
        }
    }
}
