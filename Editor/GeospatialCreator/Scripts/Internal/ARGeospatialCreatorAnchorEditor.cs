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
    [CanEditMultipleObjects]
    internal class ARGeospatialCreatorAnchorEditor : Editor
    {
        private SerializedProperty _altitudeOffset;

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

        private void GUIForAltitude(AnchorAltitudeType altitudeType)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                // Draw the custom GUI for the legacy _altitudeOffset field
                if (altitudeType == AnchorAltitudeType.Terrain ||
                    altitudeType == AnchorAltitudeType.Rooftop)
                {
                    _altitudeOffset.doubleValue = EditorGUILayout.DoubleField(
                        "Altitude Offset",
                        _altitudeOffset.doubleValue);
                }

                _altitude.doubleValue =
                    EditorGUILayout.DoubleField("WGS84 Altitude", _altitude.doubleValue);
                if (altitudeType == AnchorAltitudeType.Terrain)
                {
                    EditorGUILayout.HelpBox("WGS84 Altitude is only used in the editor to " +
                        "display altitude of the anchored object. At runtime Altitude Offset is " +
                        "used to position the anchor relative to the terrain.",
                        MessageType.Info,
                        wide: true);
                }
                else if (altitudeType == AnchorAltitudeType.Rooftop)
                {
                    EditorGUILayout.HelpBox("WGS84 Altitude is only used in the editor to " +
                        "display altitude of the anchored object. At runtime Altitude Offset is " +
                        "used to position the anchor relative to rooftops.",
                        MessageType.Info,
                        wide: true);
                }
            }
        }

        private void OnEnable()
        {
            // Fetch the objects from the GameObject script to display in the inspector
            _altitudeType = serializedObject.FindProperty("_altitudeType");
            _altitudeOffset = serializedObject.FindProperty("_altitudeOffset");
            _latitude = serializedObject.FindProperty("_latitude");
            _longitude = serializedObject.FindProperty("_longitude");
            _altitude = serializedObject.FindProperty("_altitude");
        }
    }
}
