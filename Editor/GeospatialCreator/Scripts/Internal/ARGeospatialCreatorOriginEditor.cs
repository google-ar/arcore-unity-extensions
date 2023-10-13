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

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Editor.Internal
{
    using System;
#if ARCORE_INTERNAL_USE_CESIUM
    using CesiumForUnity;
#endif
    using Google.XR.ARCoreExtensions.Editor.Internal;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom editor for ARGeospatialCreatorOrigin.
    /// </summary>
    [CustomEditor(typeof(ARGeospatialCreatorOrigin))]
    internal class ARGeospatialCreatorOriginEditor : Editor
    {
        /// <summary>
        /// Function that is called every GUI update when the target object get updated.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var origin = serializedObject.targetObject as ARGeospatialCreatorOrigin;

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 20;
            GUILayout.Label("Geospatial Creator Origin", titleStyle);

            GUIForQuickstartButton();
            EditorGUILayout.Space();

            if (HasGeoreference(origin))
            {
                // Always draw the CesiumGeoreference-specific GUI, since we only support Cesium
                // references currently.
                GUIForCesiumGeoreference(origin);
            }
            else
            {
                GUIForMissingReference(origin);
            }

            if (GUILayout.Button("Search for Location"))
            {
                PlaceSearchWindow.ShowPlaceSearchWindow();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private bool HasGeoreference(ARGeospatialCreatorOrigin origin)
        {
#if ARCORE_INTERNAL_USE_CESIUM
            return (origin.gameObject.GetComponent<CesiumGeoreference>() != null);
#else
            return false;
#endif
        }

        // Draw the GUI element for opening the Quickstart guide.
        private void GUIForQuickstartButton()
        {
            GUIContent openQuickstartContent = new GUIContent(
                "Open Geospatial Creator Quickstart",
                "Open the Quickstart webpage for Geospatial Creator in a browser.");
            if (GUILayout.Button(openQuickstartContent))
            {
                Application.OpenURL(GeospatialCreatorHelper.QuickstartUrl);
            }
        }

        // Draw the GUI when there's no Georeference attached to the target Origin.
        private void GUIForMissingReference(ARGeospatialCreatorOrigin origin)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            GUIContent addGeoreferenceContent = new GUIContent(
                "Add Cesium Georeference Component",
                "Add a CesiumGeoreference to locate Geospatial Anchors in the Unity scene.");
            if (GUILayout.Button(addGeoreferenceContent))
            {
                // Only Cesium anchors are supported so far; this method will throw an exception
                // if the Cesium dependency is unavailable.
                AddGeoreference(origin);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void GUIForCesiumGeoreference(ARGeospatialCreatorOrigin origin)
        {
#if ARCORE_INTERNAL_USE_CESIUM
            EditorGUILayout.BeginVertical();
            Cesium3DTileset tileset = GeospatialCreatorCesiumAdapter.GetTilesetComponent(origin);
            if (tileset == null)
            {
                Debug.LogError(
                    "There is no Cesium3DTileset component associated with Origin " + origin.name);
                return;
            }

            // We don't persist the API Key directly in this component. Instead, it is always read
            // from and written to the tiles child.
            string oldApiKey = string.IsNullOrEmpty(tileset.url) ? "" :
                GeospatialCreatorCesiumAdapter.GetMapTilesApiKey(tileset);
            string newApiKey = EditorGUILayout.DelayedTextField(
                "Google Map Tiles API Key", oldApiKey);
            if (String.IsNullOrEmpty(newApiKey))
            {
                EditorGUILayout.HelpBox(
                    "An API key is required to use Google Map Tiles. Follow the Quickstart "
                        + "Guide for additional instructions.",
                    MessageType.Warning);
            }

            if (newApiKey != oldApiKey)
            {
                Undo.RecordObject(tileset, "Update Map Tiles API key");
                tileset.url = MapTilesUtils.CreateMapTilesUrl(newApiKey);
                EditorUtility.SetDirty(tileset);
            }

            EditorGUILayout.EndVertical();
#else // !ARCORE_INTERNAL_USE_CESIUM
            throw new Exception("Cannot get Map Tiles API key; Cesium dependency is missing.");
#endif
        }

        private void AddGeoreference(ARGeospatialCreatorOrigin origin)
        {
            // Only Cesium anchors are supported so far
#if !ARCORE_INTERNAL_USE_CESIUM
            throw new Exception("Cesium dependency is missing.");
#else
            CesiumGeoreference georeference =
                GeospatialCreatorCesiumAdapter.AddGeoreferenceAndTileset(origin);
            Undo.RegisterCreatedObjectUndo(georeference, "Create Cesium Georeference");
#endif
        }
    }
}
