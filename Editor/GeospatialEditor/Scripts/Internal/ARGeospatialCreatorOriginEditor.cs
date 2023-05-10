//-----------------------------------------------------------------------
// <copyright file="ARGeospatialCreatorOriginEditor.cs" company="Google LLC">
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

#if UNITY_2021_3_OR_NEWER

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Editor.Internal
{
    using System;
    using Google.XR.ARCoreExtensions.Editor.Internal;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ARGeospatialCreatorOrigin))]
    public class ARGeospatialCreatorOriginEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var origin = serializedObject.targetObject as ARGeospatialCreatorOrigin;

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 20;
            GUILayout.Label("Geospatial Creator Origin", titleStyle);

            if (origin.HasGeoreference())
            {
                // Always draw the CesiumGeoreference-specific GUI, since we only support Cesium
                // references currently.
                GUIForCesiumGeoreference(origin);
            }
            else
            {
                GUIForMissingReference(origin);
            }
        }

        // Draw the GUI when there's no Georeference attached to the target Origin.
        private void GUIForMissingReference(ARGeospatialCreatorOrigin origin)
        {
            GUILayout.BeginVertical();
            EditorGUILayout.PrefixLabel(""); // vertical whitespace for readability.
            GUILayout.BeginHorizontal();

            GUIContent addGeoreferenceContent = new GUIContent(
                "Add Cesium Georeference Component",
                "Add a CesiumGeoreference to locate Geospatial Anchors in the Unity scene."
            );
            if (GUILayout.Button(addGeoreferenceContent))
            {
                AddGeoreference(origin);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void AddGeoreference(ARGeospatialCreatorOrigin origin)
        {
            // Only Cesium anchors are supported so far; the underlying origin object
            // will throw an exception if the Cesium dependency is unavailable.
            origin.AddNewGeoreferenceComponent();
        }

        private void GUIForCesiumGeoreference(ARGeospatialCreatorOrigin origin)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PrefixLabel(""); // vertical whitespace for readability.
            GUIContent openQuickstartContent = new GUIContent(
                "Open Geospatial Creator Quickstart",
                "Open the Quickstart webpage for Geospatial Creator in a browser.");
            if (GUILayout.Button(openQuickstartContent))
            {
                Application.OpenURL(GeospatialEditorHelper.QuickstartUrl);
            }

            // We don't persist the API Key directly in this component. Instead, it is always read from
            // and written to the tiles child.
            string oldApiKey = origin.Get3DTilesApiKey();
            string newApiKey = EditorGUILayout.DelayedTextField(
                "Google Map Tiles API Key",
                oldApiKey);
            if (String.IsNullOrEmpty(newApiKey))
            {
                EditorGUILayout.HelpBox(
                    "An API key is required to use Google Map Tiles. Follow the Quickstart "
                        + "Guide for additional instructions.",
                    MessageType.Warning);
            }
            if (newApiKey != oldApiKey)
            {
                origin.Set3DTileApiKey(newApiKey);
            }

            EditorGUILayout.EndVertical();
        }
    }
}
#endif // UNITY_X_OR_LATER
