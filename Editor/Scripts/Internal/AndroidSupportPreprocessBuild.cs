//-----------------------------------------------------------------------
// <copyright file="AndroidSupportPreprocessBuild.cs" company="Google LLC">
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
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEditor.XR.Management;
    using UnityEngine;
    using UnityEngine.XR.ARCore;
    using UnityEngine.XR.Management;

    /// <summary>
    /// Preprocess build to check android support.
    /// </summary>
    public class AndroidSupportPreprocessBuild : IPreprocessBuildWithReport
    {
        private const string _mainTemplatePath = "Plugins/Android/mainTemplate.gradle";
        private const string _launcherTemplatePath = "Plugins/Android/launcherTemplate.gradle";

        /// <summary>
        /// Gets the relative callback order for callbacks. Callbacks with lower values are called
        /// before ones with higher values.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
         Justification = "Overriden property.")]
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// A callback received before the build is started.
        /// </summary>
        /// <param name="report">A report containing information about the build,
        /// such as its target platform and output path.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
#if !ARCORE_FEATURE_UNIT_TEST // BuildReport is unavailable on Test Welder.
            if (report.summary.platform == BuildTarget.Android)
#endif
            {
                if (!CheckARCoreLoader())
                {
                    Debug.LogError("ARCoreLoader is not enabled! " +
                        "To ensure Extensions SDK can work properly on Android Platform, " +
                        "navigate to 'Project Settings > XR Plug-in Management', " +
                        "switch to Android tab and check 'ARCore' as the Plug-in Provider.");
                }

                Check64BitArch();

#if UNITY_2019_4
                CheckGradleTemplate();
#endif
            }
        }

        private bool CheckARCoreLoader()
        {
            XRGeneralSettings generalSettings =
                XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(
                    BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            if (generalSettings == null)
            {
                return false;
            }

            foreach (var loader in generalSettings.Manager.loaders)
            {
                if (loader is ARCoreLoader)
                {
                    return true;
                }
            }

            return false;
        }

        private void Check64BitArch()
        {
            bool includes64Bit =
                    (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) != 0;
            if (!includes64Bit)
            {
                Debug.LogWarning("ARCoreExtensions: Missing ARM64 architecture which is " +
                "required for Android 64-bit devices. See https://developers.google.com/ar/64bit." +
                "\nSelect IL2CPP  in 'Project Settings > Player > Other Settings > Scripting " +
                "Backend' and select ARM64 in 'Project Settings > Player > Other Settings > " +
                "Target Architectures.'");
            }
        }

        private void CheckGradleTemplate()
        {
            // Need to set gradle version >= 5.6.4 by
            // 'Preferences > External Tools > Android > Gradle'.
            var gradlePath = EditorPrefs.GetString("GradlePath");
            if (string.IsNullOrEmpty(gradlePath))
            {
                throw new BuildFailedException(
                    "'Preferences > External Tools > Android > Gradle' is empty. " +
                    "ARCore Extensions for AR Foundation requires a customized Gradle with " +
                    "version >= 5.6.4.");
            }

            // Need to use gradle plugin version >= 3.6.0 in main gradle by editing
            // 'Assets/Plugins/Android/mainTemplate.gradle'.
            if (!File.Exists(Path.Combine(Application.dataPath, _mainTemplatePath)))
            {
                throw new BuildFailedException(
                    "Main Gradle template is not used in this build. " +
                    "ARCore Extensions for AR Foundation requires " +
                    "gradle plugin version >= 3.6.0. Nevigate to " +
                    "'Project Settings > Player > Android Tab > Publish Settings > Build', " +
                    "check 'Custom Main Gradle Template'. Then edit the generated file " +
                    "'Assets/Plugins/Android/mainTemplate.gradle' by adding dependency " +
                    "'com.android.tools.build:gradle:3.6.0.'.");
            }

            // Need to use gradle plugin version >= 3.6.0 in launcher gradle by editing
            // 'Assets/Plugins/Android/launcherTemplate.gradle'.
            if (!File.Exists(Path.Combine(Application.dataPath, _launcherTemplatePath)))
            {
                throw new BuildFailedException(
                    "Launcher Gradle Template is not used in this build. " +
                    "ARCore Extensions for AR Foundation requires " +
                    "gradle plugin version >= 3.6.0. Nevigate to " +
                    "'Project Settings > Player > Android Tab > Publish Settings > Build', " +
                    "check 'Custom Launcher Gradle Template'. Then edit the generated file " +
                    "'Assets/Plugins/Android/launcherTemplate.gradle' by adding dependency " +
                    "'com.android.tools.build:gradle:3.6.0.'.");
            }
        }
    }
}
