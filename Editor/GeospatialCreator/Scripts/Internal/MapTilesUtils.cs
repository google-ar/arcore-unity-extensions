//-----------------------------------------------------------------------
// <copyright file="MapTilesUtils.cs" company="Google LLC">
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
    using UnityEngine;

    internal class MapTilesUtils
    {
        /// <summary>Extracts the API key from a Google Map Tiles API URL.</summary>
        /// <param name = "url">A URL containing a "key=" parameter.</param>
        /// <returns>The key extracted from the "key" parameter.</returns>
        public static string ExtractMapTilesApiKey(string url)
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

        /// <summary> Returns a Map Tiles API URL for a given API key.</summary>
        /// <param name="apiKey">The API key to be used with the URL.</param>
        /// <returns>A valid URL for the Map Tiles API.</returns>
        public static string CreateMapTilesUrl(string apiKey)
        {
            return String.Format(
                "https://tile.googleapis.com/v1/3dtiles/root.json?key={0}",
                apiKey);
        }
    }
}
