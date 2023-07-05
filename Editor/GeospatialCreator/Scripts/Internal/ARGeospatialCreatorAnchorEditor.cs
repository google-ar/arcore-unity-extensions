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
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ARGeospatialCreatorAnchor))]
    internal class ARGeospatialCreatorAnchorEditor : Editor
    {
        private SerializedProperty _altitudeType;
        private SerializedProperty _latitude;
        private SerializedProperty _longitude;
        private SerializedProperty _altitude;
        private SerializedProperty _altitudeOffset;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var anchor = serializedObject.targetObject as ARGeospatialCreatorAnchor;
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 20;
            GUILayout.Label("Geospatial Creator Anchor", titleStyle);

            // Start a code block to check for GUI changes
            EditorGUI.BeginChangeCheck();
            _latitude.doubleValue =
                EditorGUILayout.DoubleField("Latitude", _latitude.doubleValue);
            _longitude.doubleValue =
                EditorGUILayout.DoubleField("Longitude", _longitude.doubleValue);

            GUIContent altitudeTypeLabel = new GUIContent("Altitude Type");
            EditorGUILayout.PropertyField(_altitudeType, altitudeTypeLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                if (anchor.AltType == ARGeospatialCreatorAnchor.AltitudeType.Terrain ||
                    anchor.AltType == ARGeospatialCreatorAnchor.AltitudeType.Rooftop)
                {
                    _altitudeOffset.doubleValue = EditorGUILayout.DoubleField(
                        "Altitude Offset",
                        _altitudeOffset.doubleValue);
                }

                _altitude.doubleValue =
                    EditorGUILayout.DoubleField("WGS84 Altitude", _altitude.doubleValue);
                if (anchor.AltType == ARGeospatialCreatorAnchor.AltitudeType.Terrain)
                {
                    EditorGUILayout.HelpBox("WGS84 Altitude is only used in the editor to " +
                        "display altitude of the anchored object. At runtime Altitude Offset is " +
                        "used to position the anchor relative to the terrain.",
                        MessageType.Info,
                        wide: true);
                }
                else if (anchor.AltType == ARGeospatialCreatorAnchor.AltitudeType.Rooftop)
                {
                    EditorGUILayout.HelpBox("WGS84 Altitude is only used in the editor to " +
                        "display altitude of the anchored object. At runtime Altitude Offset is " +
                        "used to position the anchor relative to rooftops.",
                        MessageType.Info,
                        wide: true);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnEnable()
        {
            // Fetch the objects from the GameObject script to display in the inspector
            _altitudeType = serializedObject.FindProperty("AltType");
            _latitude = serializedObject.FindProperty("_latitude");
            _longitude = serializedObject.FindProperty("_longitude");
            _altitude = serializedObject.FindProperty("_altitude");
            _altitudeOffset = serializedObject.FindProperty("_altitudeOffset");
        }
    }
}

