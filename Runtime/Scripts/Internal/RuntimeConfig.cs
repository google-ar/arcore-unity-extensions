//-----------------------------------------------------------------------
// <copyright file="RuntimeConfig.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    using UnityEditor;
    using UnityEngine;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
        Justification = "Internal")]
    public class RuntimeConfig : ScriptableObject
    {
        public static RuntimeConfig Instance;

        public string IOSCloudServicesApiKey;

        // GUID to folder [ARCore Extensions]/Runtime
        private const string _runtimeFolderGUID = "df6f7c8173aef4ce18044d1392042d34";

        private const string _runtimeConfigFolder = "/Configurations/RuntimeSettings";

        private const string _runtimeConfigAsset = "RuntimeConfig.asset";

#if UNITY_EDITOR
        public static void LoadInstance()
        {
            if (Instance != null)
            {
                return;
            }

            string folderPath =
                AssetDatabase.GUIDToAssetPath(_runtimeFolderGUID) + _runtimeConfigFolder;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string assetPath = folderPath + "/" + _runtimeConfigAsset;
            if (!File.Exists(assetPath))
            {
                Debug.Log("Created ARCore Extensions RuntimeConfig for Preloaded Assets.");
                var config = CreateInstance<RuntimeConfig>();
                AssetDatabase.CreateAsset(config, assetPath);
                Instance = config;
            }
            else
            {
                Instance = AssetDatabase.LoadAssetAtPath<RuntimeConfig>(assetPath);
            }
        }

        public static void UploadInstance()
        {
            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.RemoveAll(x => x.GetType() == typeof(RuntimeConfig));

            if (Instance == null)
            {
                Debug.Log("Cleared ARCore Extensions RuntimeConfig in Preloaded Assets.");
                return;
            }

            preloadedAssets.Add(Instance);
            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
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
