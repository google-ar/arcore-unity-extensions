//-----------------------------------------------------------------------
// <copyright file="CompatibilityCheckPreprocessBuild.cs" company="Google LLC">
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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    /// <summary>
    /// Manage the dependencies for compatibility and requirements check.
    /// </summary>
    public class CompatibilityCheckPreprocessBuild : IPreprocessBuildWithReport
    {
        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Overridden property.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
            Justification = "Internal")]
        public int callbackOrder
        {
            get
            {
                // This preprocess would mark which modules are required.
                // It should execute before all other Module Framework workflow.
                // Skip 0 so other developers could let their workflow run first.
                return 1;
            }
        }

        /// <summary>
        /// Callback before the build is started.</para>
        /// </summary>
        /// <param name="report">A report containing information about the build,
        /// such as its target platform and output path.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            UnityEditor.BuildTarget buildTarget = report.summary.platform;
            if (DependentModulesManager.GetModules().Count == 0)
            {
                return;
            }

            CheckCompatibilityWithAllSessionConfigs(
                ARCoreExtensionsProjectSettings.Instance,
                AndroidDependenciesHelper.GetAllSessionConfigs(),
                buildTarget);
        }

        private static void CheckCompatibilityWithAllSessionConfigs(
            ARCoreExtensionsProjectSettings settings,
            Dictionary<ARCoreExtensionsConfig, string> sessionToSceneMap,
            UnityEditor.BuildTarget buildTarget)
        {
            List<IDependentModule> featureModules =
                DependentModulesManager.GetModules();
            foreach (IDependentModule module in featureModules)
            {
                ModuleNecessity moduleNecessity = ModuleNecessity.NotRequired;
                foreach (var entry in sessionToSceneMap)
                {
                    ARCoreExtensionsConfig sessionConfig = entry.Key;
                    if (!module.IsCompatible(settings, sessionConfig, buildTarget))
                    {
                        throw new BuildFailedException(
                            string.Format(
                                "{0} isn't compatible with the ARCoreExtensionsConfig in {1}.",
                                module.GetType().Name, entry.Value));
                    }

                    moduleNecessity = (ModuleNecessity)Math.Max(
                        (int)moduleNecessity,
                        (int)module.GetModuleNecessity(sessionConfig));
                }

                if (moduleNecessity == ModuleNecessity.NotRequired &&
                    module.IsEnabled(settings, buildTarget))
                {
                    Debug.LogWarning(module.GetEnabledNotRequiredWarning(settings, buildTarget));
                }
            }
        }
    }
}
