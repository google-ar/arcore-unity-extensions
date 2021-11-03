//-----------------------------------------------------------------------
// <copyright file="RuntimeConfigPreprocessBuild.cs" company="Google LLC">
//
// Copyright 2021 Google LLC
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    /// <summary>
    /// Mark modules enabled in RuntimeConfig.
    /// </summary>
    public class RuntimeConfigPreprocessBuild : IPreprocessBuildWithReport
    {
        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Overridden property.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
            Justification = "Overridden property.")]
        public int callbackOrder
        {
            get
            {
                // This preprocess build might need to check whether the module is required.
                // So it should be executed after CompatibilityCheckPreprocessBuild.
                return 2;
            }
        }

        /// <summary>
        /// A callback before the build is started to mark modules enabled.
        /// </summary>
        /// <param name="report">A report containing information about the build.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            UnityEditor.BuildTarget buildTarget = report.summary.platform;

            if (buildTarget == BuildTarget.Android)
            {
                SetEnabledModules(ARCoreExtensionsProjectSettings.Instance);
            }
            else if (buildTarget == BuildTarget.iOS)
            {
                SetApiKeyOnIOS(ARCoreExtensionsProjectSettings.Instance);
            }
        }

        private void SetApiKeyOnIOS(ARCoreExtensionsProjectSettings settings)
        {
            if (string.IsNullOrEmpty(settings.IOSCloudServicesApiKey))
            {
                RuntimeConfig.SetIOSApiKey(string.Empty);
                return;
            }

            if (!settings.IsIOSSupportEnabled)
            {
                Debug.LogWarning("Failed to enable Cloud Anchor on iOS because iOS Support " +
                    "is not enabled. Go to 'Project Settings > XR > ARCore Extensionts' " +
                    "to change the settings.");
                RuntimeConfig.SetIOSApiKey(string.Empty);
                return;
            }

            RuntimeConfig.SetIOSApiKey(settings.IOSCloudServicesApiKey);
        }

        private void SetEnabledModules(ARCoreExtensionsProjectSettings settings)
        {
            List<string> modulesEnabled = new List<string>();
            foreach (IDependentModule module in DependentModulesManager.GetModules())
            {
                if (module.IsEnabled(settings, UnityEditor.BuildTarget.Android))
                {
                    modulesEnabled.Add(module.GetType().Name);
                }
            }

            RuntimeConfig.SetEnabledModules(modulesEnabled);
        }
    }
}
