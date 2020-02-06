//-----------------------------------------------------------------------
// <copyright file="RuntimeConfig.cs" company="Google">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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

namespace Google.XR.ARCoreExtensions.Internal
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Stores all ARCore Extensions runtime configuration which is used for a cross-platform
    /// ARCore session.
    /// Note: it can only be used as singleton.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class RuntimeConfig : ScriptableObject
    {
        public static RuntimeConfig Instance;

        public string IOSCloudServicesApiKey;

        // GUID to folder [ARCore Extensions]/Runtime
        private const string k_RuntimeFolderGUID = "df6f7c8173aef4ce18044d1392042d34";

        private const string k_RuntimeConfigFolder = "/Configurations/RuntimeSettings";

        private const string k_RuntimeConfigAsset = "RuntimeConfig.asset";

#if UNITY_EDITOR
        public static void LoadInstance()
        {
            if (Instance != null)
            {
                return;
            }

            string folderPath = UnityEditor.AssetDatabase.GUIDToAssetPath(k_RuntimeFolderGUID) +
                k_RuntimeConfigFolder;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string assetPath = folderPath + "/" + k_RuntimeConfigAsset;
            if (!File.Exists(assetPath))
            {
                Debug.Log("Created ARCore Extensions RuntimeConfig for Preloaded Assets.");
                var config = CreateInstance<RuntimeConfig>();
                UnityEditor.AssetDatabase.CreateAsset(config, assetPath);
                Instance = config;
            }
            else
            {
                Instance = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeConfig>(assetPath);
            }
        }

        public static void UploadInstance()
        {
            var preloadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.RemoveAll(x => x.GetType() == typeof(RuntimeConfig));

            if (Instance == null)
            {
                Debug.Log("Cleared ARCore Extensions RuntimeConfig in Preloaded Assets.");
                return;
            }

            preloadedAssets.Add(Instance);
            UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            Debug.Log("Updated ARCore Extensions RuntimeConfig in Preloaded Assets.");
        }

        public static void SetIOSApiKey(string apiKey)
        {
            LoadInstance();
            Instance.IOSCloudServicesApiKey = apiKey;
            UploadInstance();
        }
#endif

        public void OnEnable()
        {
            Instance = this;
        }
    }
}
