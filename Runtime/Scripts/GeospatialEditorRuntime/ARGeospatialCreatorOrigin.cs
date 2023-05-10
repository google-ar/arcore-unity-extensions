//-----------------------------------------------------------------------
// <copyright file="ARGeospatialCreatorOrigin.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Internal
{
    using System;
#if ARCORE_INTERNAL_USE_CESIUM
    using CesiumForUnity;
#endif
    using Google.XR.ARCoreExtensions.Internal;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

    /// <summary>
    /// Provides a Geospatial Creator Origin that has both a lat/lon and gamespace coordinates. This is
    /// the reference point used by AR Anchors made in the Geospatial Creator to resolve their
    /// location in gamespace.
    /// </summary>
#if ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
    [AddComponentMenu("XR/Geospatial Creator/AR Geospatial Creator Origin")]
#endif
    [ExecuteInEditMode]
    public class ARGeospatialCreatorOrigin : MonoBehaviour
    {
        // Helper that extracts the API key from a Google Map Tiles API call URL
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
            return "";
        }

        // Returns the URL for the tiles API for the given key
        private static string TilesApiUrl(string apiKey) {
            return String.Format("https://tile.googleapis.com/v1/3dtiles/root?key={0}", apiKey);
        }

        public bool HasGeoreference()
        {
#if ARCORE_INTERNAL_USE_CESIUM
            return (gameObject.GetComponent(typeof(CesiumGeoreference)) != null);
#endif
            return false;
        }

        public void AddNewGeoreferenceComponent()
        {
            if (HasGeoreference())
            {
                throw new Exception(
                    "Geospatial Creator georeference already exists, could not create another.");
            }

#if !ARCORE_INTERNAL_USE_CESIUM
            throw new Exception("Cesium dependency is missing.");
#else // need to use #else block to avoid unreachable code failures

            CesiumGeoreference georeference =
                gameObject.AddComponent(typeof(CesiumGeoreference)) as CesiumGeoreference;

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
#if UNITY_EDITOR
            tilesetObject.tag = "EditorOnly";
            Undo.RegisterCreatedObjectUndo(georeference, "Create Cesium Georeference");
#endif
#endif
        }

        public string Get3DTilesApiKey()
        {
#if !ARCORE_INTERNAL_USE_CESIUM
            throw new Exception("Cannot get Map Tiles API key; Cesium dependency is missing.");
#else // need to use #else block to avoid unreachable code failures
            Cesium3DTileset tileset =
                gameObject.GetComponentInChildren(typeof(Cesium3DTileset)) as Cesium3DTileset;
            if (tileset == null)
            {
                return "";
            }
            return ApiKeyFromTilesetUrl(tileset.url);
#endif
        }

        public void Set3DTileApiKey(string key)
        {
#if !ARCORE_INTERNAL_USE_CESIUM
            throw new Exception("Cannot set Map Tiles API key; Cesium dependency is missing.");
#else // need to use #else block to avoid unreachable code failures
            Cesium3DTileset tileset =
                gameObject.GetComponentInChildren(typeof(Cesium3DTileset)) as Cesium3DTileset;
            if (tileset == null)
            {
                Debug.LogError(
                    "Attempted to set Map Tiles API key on a missing Cesium3DTileset component.");
                return;
            }
            String url = String.IsNullOrEmpty(key) ? "" : TilesApiUrl(key);
            if (url != tileset.url)
            {
                Debug.Log("Setting new URL for Map Tiles API: " + url);
                tileset.url = url;
            }
#endif
        }

    }
}

#endif // UNITY_X_OR_NEWER
