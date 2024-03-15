//-----------------------------------------------------------------------
// <copyright file="ARGeospatialCreatorAnchorEditor.cs" company="Google LLC">
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
    using Google.XR.ARCoreExtensions.GeospatialCreator;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ARGeospatialCreatorAnchor))]
    [CanEditMultipleObjects]
    internal class ARGeospatialCreatorAnchorEditor : Editor
    {
        private SerializedProperty _anchorManager;
        private SerializedProperty _origin;
        private SerializedProperty _useEditorAltitudeOverride;
        private SerializedProperty _editorAltitudeOverride;

        private SerializedProperty _altitudeType;
        private SerializedProperty _latitude;
        private SerializedProperty _longitude;
        private SerializedProperty _altitude;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // don't use targetObject just use SerializedProperty
            // SerializedProperty will use the 0th in the array of targets in some cases
            // and can then support multi object edit with undo
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 20;
            GUILayout.Label("Geospatial Creator Anchor", titleStyle);

            // Start a code block to check for GUI changes
            EditorGUI.BeginChangeCheck();

            GUIContent originLabel = new GUIContent("Geospatial Origin");
            originLabel.tooltip  = "The Origin is the reference point for converting real-world" +
                " latitude, longitude, and altitude values to & from Unity game coordinates." +
                " There should be exactly one ARGeospatialCreatorOrigin in your scene.";
            EditorGUILayout.PropertyField(_origin, originLabel);

            GUIContent anchorManagerLabel = new GUIContent("Anchor Manager",
                "The ARAnchorManager used to resolve this anchor at runtime.");
            EditorGUILayout.PropertyField(_anchorManager, anchorManagerLabel);

            EditorGUILayout.Space();

            _latitude.doubleValue =
                EditorGUILayout.DoubleField("Latitude", _latitude.doubleValue);
            _longitude.doubleValue =
                EditorGUILayout.DoubleField("Longitude", _longitude.doubleValue);

            GUIContent altitudeTypeLabel = new GUIContent("Altitude Type");
            EditorGUILayout.PropertyField(_altitudeType, altitudeTypeLabel);
            AnchorAltitudeType altitudeType;

            Enum.TryParse(
                _altitudeType.enumNames[_altitudeType.enumValueIndex], out altitudeType);

            GUIForAltitude(altitudeType);

            if (GUILayout.Button("Search for Location"))
            {
                PlaceSearchWindow.ShowPlaceSearchWindow();
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void GUIForSnapToTile()
        {
            string snapToTileTooltip = "Visually position the anchor within the editor so it " +
                "aligns with the top of the tile. This override in the editor does not impact " +
                "the position at runtime.";

            if (GUILayout.Button(new GUIContent("Snap to Tile", snapToTileTooltip)))
            {
                ARGeospatialCreatorOrigin origin =
                    _origin.objectReferenceValue as ARGeospatialCreatorOrigin;
                if (origin == null)
                {
                    Debug.LogError("An ARGeospatialCreatorOrigin object must be present in the " +
                        "scene and assigned to the anchor to use the Snap to Tile feature.");
                    return;
                }

                var tileset = origin._origin3DTilesetAdapter;
                if (tileset.GetPhysicsMeshesEnabled() == false)
                {
#if ARCORE_INTERNAL_USE_CESIUM
                    Debug.LogError("Physics Meshes must be enabled on the tileset to use " +
                        "the Snap to Tile feature. Please enable the \"Create Physics Meshes\" " +
                        "setting in the Cesium3DTileset component owned by the Origin.",
                    GeospatialCreatorCesiumAdapter.GetTilesetComponent(origin));
#else
                    Debug.LogError("Physics Meshes must be enabled on the tileset to use " +
                        "the Snap to Tile feature.");
#endif
                    return;
                }

                (bool success, double tileAltitude) =
                    tileset.CalcTileAltitudeWGS84(_latitude.doubleValue, _longitude.doubleValue);
                if (success)
                {
                    _editorAltitudeOverride.doubleValue = tileAltitude;
                }
                else
                {
                    Debug.LogWarning("Could not Snap to Tile at Latitude: " +
                    _latitude.doubleValue + " and Longitude: " + _longitude.doubleValue);
                }
            }
        }

        private void GUIForAltitude(AnchorAltitudeType altitudeType)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                string altitudeLabel = string.Empty;
                switch (altitudeType)
                {
                    case AnchorAltitudeType.WGS84:
                        altitudeLabel = "WGS84 altitude";
                        break;
                    case AnchorAltitudeType.Terrain:
                        altitudeLabel = "Altitude relative to terrain";
                        break;
                    case AnchorAltitudeType.Rooftop:
                        altitudeLabel = "Altitude relative to rooftop";
                        break;
                }

                _altitude.doubleValue =
                    EditorGUILayout.DoubleField(altitudeLabel, _altitude.doubleValue);

                GUILayout.BeginHorizontal();

                string overrideAltitudeLabel = "Override altitude in Editor Scene View";
                _useEditorAltitudeOverride.boolValue = EditorGUILayout.Toggle(
                        overrideAltitudeLabel,
                        _useEditorAltitudeOverride.boolValue);

                // Allow the override value to be edited only if the flag is enabled.
                EditorGUI.BeginDisabledGroup(!_useEditorAltitudeOverride.boolValue);
                _editorAltitudeOverride.doubleValue = EditorGUILayout.DoubleField(
                    _editorAltitudeOverride.doubleValue);
                GUIForSnapToTile();
                EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();

                if (_useEditorAltitudeOverride.boolValue)
                {
                    EditorGUILayout.HelpBox(
                        "\"" + overrideAltitudeLabel + "\" sets the altitude used in the Scene " +
                        "View to position the anchor, in meters according to WGS84. This is " +
                        "useful to vizualize the anchor relative to the scene geometry in cases " +
                        "where the scene geometry altitude is not fully aligned with the real " +
                        "world. This is an Editor-only property; the " + altitudeLabel + " is " +
                        "always used at runtime.",
                        MessageType.Info,
                        wide: true);
                }
            }
        }

        private void OnEnable()
        {
            // Fetch the objects from the GameObject script to display in the inspector
            _altitudeType = serializedObject.FindProperty("_altitudeType");
            _anchorManager = serializedObject.FindProperty("_anchorManager");
            _origin = serializedObject.FindProperty("Origin");
            _useEditorAltitudeOverride =
                serializedObject.FindProperty("_useEditorAltitudeOverride");
            _editorAltitudeOverride = serializedObject.FindProperty("_editorAltitudeOverride");
            _latitude = serializedObject.FindProperty("_latitude");
            _longitude = serializedObject.FindProperty("_longitude");
            _altitude = serializedObject.FindProperty("_altitude");
        }
    }
}
