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
    /// ARGeospatialCreatorOriginEditor
    ///
    /// The GUI for ARGeospatialCreatorOrigin,
    /// It looks like this go/prototype-geospatial-creator-gui-design.
    /// </summary>
    [CustomEditor(typeof(ARGeospatialCreatorOrigin))]
    internal class ARGeospatialCreatorOriginEditor : Editor
    {
        /// <summary>Helper that extracts the API key from a Google Map Tiles API URL.</summary>
        /// <param name = "url">A URL containing a "key=" parameter.</param>
        /// <returns>The key extracted from the "key" parameter.</returns>
        public static string ApiKeyFromTilesetUrl(string url)
        {
            char[] delimeters = { '&', '?' };
            foreach (string urlPart in url.Split(delimeters))
            {
                if (urlPart.StartsWith("key="))
                {
                    return urlPart.Substring(4);
                }
            }

            return string.Empty;
        }

        /// <summary>
        ///  OnInspectorGUI()
        ///
        ///  function that is called every GUI update when the target object get updated.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var origin = serializedObject.targetObject as ARGeospatialCreatorOrigin;

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 20;
            GUILayout.Label("Geospatial Creator Origin", titleStyle);

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


            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>Finds the 3d tiles Application Key from the origin.</summary>
        /// <param name = "origin">A ARGeospatialCreatorOrigin that has a Cesium3DTileset child.
        /// </param>
        /// <returns>The key extracted.</returns>
        internal static string Get3DTilesApiKey(ARGeospatialCreatorOrigin origin)
        {
#if !ARCORE_INTERNAL_USE_CESIUM
            throw new Exception("Cannot get Map Tiles API key; Cesium dependency is missing.");
#else  // need to use #else block to avoid unreachable code failures
            Cesium3DTileset tileset =
                origin.gameObject.GetComponentInChildren(typeof(Cesium3DTileset))
                as Cesium3DTileset;
            if (tileset == null)
            {
                return "";
            }
            return ApiKeyFromTilesetUrl(tileset.url);
#endif
        }

        private static void Set3DTileApiKey(ARGeospatialCreatorOrigin origin, string key)
        {
#if !ARCORE_INTERNAL_USE_CESIUM
            throw new Exception("Cannot set Map Tiles API key; Cesium dependency is missing.");
#else  // need to use #else block to avoid unreachable code failures
            Cesium3DTileset tileset =
                origin.gameObject.GetComponentInChildren(typeof(Cesium3DTileset))
                as Cesium3DTileset;
            if (tileset == null)
            {
                Debug.LogError(
                    "Attempted to set Map Tiles API key on a missing Cesium3DTileset component.");
                return;
            }
            String url = String.IsNullOrEmpty(key) ? "" : TilesApiUrl(key);
            if (url != tileset.url)
            {
                Undo.RecordObject(tileset, "Update Map Tiles API key ");
                tileset.url = url;
                EditorUtility.SetDirty(tileset);
            }
#endif
        }

        // Helper that returns the URL for the tiles API for the given key
        private static string TilesApiUrl(string apiKey)
        {
            return String.Format(
                "https://tile.googleapis.com/v1/3dtiles/root.json?key={0}",
                apiKey);
        }

        private bool HasGeoreference(ARGeospatialCreatorOrigin origin)
        {
#if ARCORE_INTERNAL_USE_CESIUM
            return (origin.gameObject.GetComponent<CesiumGeoreference>() != null);
#else
            return false;
#endif
        }

        // Draw the GUI when there's no Georeference attached to the target Origin.
        private void GUIForMissingReference(ARGeospatialCreatorOrigin origin)
        {
            GUILayout.BeginVertical();
            EditorGUILayout.Space();
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
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            GUIContent openQuickstartContent = new GUIContent(
                "Open Geospatial Creator Quickstart",
                "Open the Quickstart webpage for Geospatial Creator in a browser.");
            if (GUILayout.Button(openQuickstartContent))
            {
                Application.OpenURL(GeospatialCreatorHelper.QuickstartUrl);
            }

            // We don't persist the API Key directly in this component. Instead, it is always read
            // from and written to the tiles child.
            string oldApiKey = Get3DTilesApiKey(origin);
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
                Set3DTileApiKey(origin, newApiKey);
            }

            EditorGUILayout.EndVertical();
        }

        private void AddGeoreference(ARGeospatialCreatorOrigin origin)
        {
            // Only Cesium anchors are supported so far
#if !ARCORE_INTERNAL_USE_CESIUM
            throw new Exception("Cesium dependency is missing.");
#else // need to use #else block to avoid unreachable code failures

            CesiumGeoreference georeference =
                origin.gameObject.AddComponent(typeof(CesiumGeoreference)) as CesiumGeoreference;

            GameObject tilesetObject = new GameObject("Cesium3DTileset");
            tilesetObject.transform.SetParent(georeference.gameObject.transform);

            // Since this is an AR app, it is likely using the camera instead of a scene
            // so default to the tiles only being visible in the Editor. Developers can
            // manually change the tag in the Inspector, if desired.
            Cesium3DTileset tileset =
                tilesetObject.AddComponent(typeof(Cesium3DTileset)) as Cesium3DTileset;
            tileset.name = tilesetObject.name;
            tileset.tilesetSource = CesiumDataSource.FromUrl;
            tileset.showCreditsOnScreen = true;
            tileset.createPhysicsMeshes = false;

            georeference.tag = "EditorOnly";
            tilesetObject.tag = "EditorOnly";
            Undo.RegisterCreatedObjectUndo(georeference, "Create Cesium Georeference");
#endif
        }
    }
}
