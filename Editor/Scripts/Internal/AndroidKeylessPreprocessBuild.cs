//-----------------------------------------------------------------------
// <copyright file="AndroidKeylessPreprocessBuild.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
    using System.Diagnostics.CodeAnalysis;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEditor.Callbacks;
    using UnityEngine;

    /// <summary>
    /// This handles the addition and removal of dependencies into the App's build.
    /// For BatchMode builds, perform clean after a build is complete.
    /// </summary>
    public class AndroidKeylessPreprocessBuild : IPreprocessBuildWithReport
    {
        private const string _androidKeylessDependenciesGuid = "1fc346056f53a42949a3dcadaae39d67";

        /// <summary>
        /// Gets Callback order.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
         "US1109:PublicPropertiesMustBeUpperCamelCase", Justification = "Overriden property.")]
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Callback after the build is done.
        /// </summary>
        /// <param name="target">Build target platform.</param>
        /// <param name="pathToBuiltProject">Path to build project.</param>
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.Android && IsKeylessAuthenticationEnabled())
            {
                PostprocessAndroidBuild();
            }
        }

        /// <summary>
        /// Preprocess step for Android Build.
        /// </summary>
        /// <param name="enabledKeyless">Whether to enable or disable keyless.</param>
        public static void PreprocessAndroidBuild(bool enabledKeyless)
        {
            AndroidDependenciesHelper.UpdateAndroidDependencies(
                enabledKeyless, _androidKeylessDependenciesGuid);

            if (enabledKeyless)
            {
                Debug.Log("ARCoreExtensions: Including Keyless dependencies in this build.");
                AndroidDependenciesHelper.DoPlayServicesResolve();
            }
        }

        /// <summary>
        /// Callback before the build is started.
        /// </summary>
        /// <param name="report">A report containing information about the build,
        /// such as its target platform and output path.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                if (IsKeylessAuthenticationEnabled())
                {
                    Debug.Log("ARCoreExtensions: " +
                        "Enabling Cloud Anchors with Keyless Authentication in this build.");
                }

                PreprocessAndroidBuild(IsKeylessAuthenticationEnabled());
            }
        }

        private static void PostprocessAndroidBuild()
        {
            Debug.Log("ARCoreExtensions: Cleaning up Keyless dependencies.");

            // Run the pre-process step with <c>Keyless</c> disabled so project files get reset.
            // Then run the ExternalDependencyManager dependency resolution which will remove
            // the Keyless dependencies.
            PreprocessAndroidBuild(false);
            AndroidDependenciesHelper.DoPlayServicesResolve();
        }

        private static bool IsKeylessAuthenticationEnabled()
        {
            return ARCoreExtensionsProjectSettings.Instance.AndroidAuthenticationStrategySetting ==
                AndroidAuthenticationStrategy.Keyless && IsCloudAnchorModeEnabled();
        }

        private static bool IsCloudAnchorModeEnabled()
        {
            foreach (ARCoreExtensionsConfig config in
                AndroidDependenciesHelper.GetAllSessionConfigs().Keys)
            {
                if (config.CloudAnchorMode != CloudAnchorMode.Disabled)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
