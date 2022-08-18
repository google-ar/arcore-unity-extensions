//-----------------------------------------------------------------------
// <copyright file="ExternalDependencyResolverPreprocessBuild.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEditor.Callbacks;
    using UnityEngine;

    /// <summary>
    /// Manage the dependencies for play service resolver.
    /// </summary>
    public class ExternalDependencyResolverPreprocessBuild : IPreprocessBuildWithReport
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
        /// Callback before build starts, but after PreprocessBuild.
        /// </summary>
        [PostProcessScene(0)]
        public static void OnPostProcessScene()
        {
#if UNITY_2022_1_OR_NEWER

            // To avoid network connection errors in Editor on Unity 2022.1+,
            // disable analytics reporting in External Dependency Manager.
            // After disabled, this option will be hidden from
            // Assets > External Dependency Manager > Version Handler > Settings.
            ExternalDependencyResolverHelper.EnableAnalyticsReporting(false);
#endif
        }

        /// <summary>
        /// Callback after the build is done.
        /// </summary>
        /// <param name="target">Build target platform.</param>
        /// <param name="pathToBuiltProject">Path to build project.</param>
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(
            UnityEditor.BuildTarget target, string pathToBuiltProject)
        {
            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                // Only clean up resolver in Batch Mode.
                return;
            }

            if (target == UnityEditor.BuildTarget.Android)
            {
                Debug.Log("ARCoreExtensions: Cleaning up Android library dependencies.");
                ExternalDependencyResolverHelper.ClearDependencies();
            }
        }

        /// <summary>
        /// A callback before the build is started to manage the dependencies.
        /// </summary>
        /// <param name="report">A report containing information about the build.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            UnityEditor.BuildTarget buildTarget = report.summary.platform;
            if (buildTarget == UnityEditor.BuildTarget.Android)
            {
                ExternalDependencyResolverHelper.ClearDependencies();
                ManageAndroidDependencies(ARCoreExtensionsProjectSettings.Instance);
            }
            else if (buildTarget == UnityEditor.BuildTarget.iOS &&
                ARCoreExtensionsProjectSettings.Instance.IsIOSSupportEnabled)
            {
                ExternalDependencyResolverHelper.EnableDependencyResolver(
                    ExternalDependencyResolverHelper.IOSResolverName);
            }
        }

        private void ManageAndroidDependencies(ARCoreExtensionsProjectSettings settings)
        {
            List<IDependentModule> featureModules = DependentModulesManager.GetModules();
            foreach (IDependentModule module in featureModules)
            {
                if (!module.IsEnabled(settings, UnityEditor.BuildTarget.Android))
                {
                    continue;
                }

                JarArtifact[] dependencies = module.GetAndroidDependencies(settings);
                if (dependencies != null && dependencies.Length > 0)
                {
                    ExternalDependencyResolverHelper.RegisterAndroidDependencies(dependencies);
                }
            }
        }
    }
}
