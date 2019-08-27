//-----------------------------------------------------------------------
// <copyright file="ARCoreExtensionsProjectSettings.cs" company="Google">
//
// Copyright 2019 Google LLC All Rights Reserved.
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

namespace Google.XR.ARCoreExtensions.Editor.Internal
{
    using System;
    using System.IO;
    using Google.XR.ARCoreExtensions;
    using UnityEngine;

    [Serializable]
    public class ARCoreExtensionsProjectSettings
    {
        public string Version;
        public string AndroidCloudServicesApiKey;

        private const string k_ProjectSettingsPath =
            "ProjectSettings/ARCoreExtensionsProjectSettings.json";
        private static ARCoreExtensionsProjectSettings s_Instance = null;

        public static ARCoreExtensionsProjectSettings Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ARCoreExtensionsProjectSettings();
                    s_Instance.Load();
                }

                return s_Instance;
            }
        }

        public void Load()
        {
            // If a settings file exists, load it now.
            if (File.Exists(k_ProjectSettingsPath))
            {
                ARCoreExtensionsProjectSettings settings =
                    JsonUtility.FromJson<ARCoreExtensionsProjectSettings>(
                        File.ReadAllText(k_ProjectSettingsPath));

                AndroidCloudServicesApiKey = settings.AndroidCloudServicesApiKey;
            }
            else
            {
                // Default settings.
                AndroidCloudServicesApiKey = string.Empty;
            }

            // Update the settings version as needed.
            Version = Google.XR.ARCoreExtensions.VersionInfo.Version;
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(k_ProjectSettingsPath, JsonUtility.ToJson(this));
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Unable to save ARCoreExtensionsProjectSettings, '{0}'", e);
            }
        }
    }
}
