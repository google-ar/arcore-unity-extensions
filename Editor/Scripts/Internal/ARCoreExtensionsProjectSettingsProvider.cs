//-----------------------------------------------------------------------
// <copyright file="ARCoreExtensionsProjectSettingsProvider.cs" company="Google">
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

namespace Google.XR.ARCoreExtensions.Editor.Internal
{
    using UnityEditor;
    using UnityEngine;

    internal class ARCoreExtensionsProjectSettingsProvider : SettingsProvider
    {
        public ARCoreExtensionsProjectSettingsProvider(
            string path,
            SettingsScope scope)
            : base(path, scope)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateARCoreExtensionsProjectSettingsProvider()
        {
            var provider =
                new ARCoreExtensionsProjectSettingsProvider(
                    "Project/XR/ARCore Extensions", SettingsScope.Project);

            // Automatically extract all keywords from public static GUI content.
            provider.keywords =
                GetSearchKeywordsFromGUIContentProperties<ARCoreExtensionsProjectSettingsGUI>();

            return provider;
        }

        public override void OnGUI(string searchContext)
        {
            ARCoreExtensionsProjectSettingsGUI.OnGUI(false);

            if (GUI.changed)
            {
                ARCoreExtensionsProjectSettings.Instance.Save();
            }
        }
    }
}
