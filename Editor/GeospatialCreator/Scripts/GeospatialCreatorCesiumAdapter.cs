//-----------------------------------------------------------------------
// <copyright file="GeospatialCreatorCesiumAdapter.cs" company="Google LLC">
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

#if ARCORE_INTERNAL_USE_CESIUM
namespace Google.XR.ARCoreExtensions.GeospatialCreator.Editor
{
    using System;

    using CesiumForUnity;
    using Google.XR.ARCoreExtensions.Editor.Internal;
    using Google.XR.ARCoreExtensions.GeospatialCreator;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Editor.Internal;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
    using UnityEditor;
    using UnityEngine;

    internal class GeospatialCreatorCesiumAdapter
    {

        internal static Cesium3DTileset GetTilesetComponent(ARGeospatialCreatorOrigin origin)
        {
            return origin.gameObject.GetComponentInChildren(typeof(Cesium3DTileset))
                as Cesium3DTileset;
        }

        internal static CesiumGeoreference AddGeoreferenceAndTileset(
            ARGeospatialCreatorOrigin origin)
        {
            CesiumGeoreference georeference =
                origin.gameObject.AddComponent(typeof(CesiumGeoreference)) as CesiumGeoreference;

            georeference.latitude = origin._originPoint.Latitude;
            georeference.longitude = origin._originPoint.Longitude;
            georeference.height = origin._originPoint.Altitude;

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

            return georeference;
        }

        internal static string GetMapTilesApiKey(ARGeospatialCreatorOrigin origin)
        {
            return GetMapTilesApiKey(GetTilesetComponent(origin));
        }

        internal static string GetMapTilesApiKey(Cesium3DTileset tileset)
        {
            return MapTilesUtils.ExtractMapTilesApiKey(tileset.url);
        }

        /// <summary>Sets the Map Tiles API Key on the Cesium3DTileset for a origin.</summary>
        /// <param name="origin"> A ARGeospatialCreatorOrigin that has a Cesium3DTileset child.
        /// </param>
        /// <param name="key"> The Map Tiles API key that should be used.</param>
        /// <returns> True if the key was modified. This is useful for GUI clients to record
        /// Undoable operations. </returns>
        internal static bool SetMapTileApiKeyForCesium3DTileset(
            Cesium3DTileset tileset, string key)
        {
            String url = String.IsNullOrEmpty(key) ? "" : MapTilesUtils.CreateMapTilesUrl(key);
            if (url == tileset.url)
            {
                // The URL is unchanged, nothing to modify
                return false;
            }
            tileset.url = url;
            EditorUtility.SetDirty(tileset);
            return true;
        }
    }
}
#endif // ARCORE_INTERNAL_USE_CESIUM
