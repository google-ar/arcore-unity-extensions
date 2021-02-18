//-----------------------------------------------------------------------
// <copyright file="DependentModulesManager.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;

    /// <summary>
    /// Manager for available modules.
    /// </summary>
    public class DependentModulesManager : IPreprocessBuildWithReport
    {
        private static List<IDependentModule> _modules;

        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Overriden property.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
            Justification = "Internal")]
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Get Feature Dependent Modules.
        /// </summary>
        /// <returns>The list of available modules.</returns>
        public static List<IDependentModule> GetModules()
        {
            if (_modules == null)
            {
                _modules = new List<IDependentModule>()
                {
                    new KeylessModule(),
                };
            }

            return _modules;
        }

        /// <summary>
        /// Callback before the build is started.</para>
        /// </summary>
        /// <param name="report">A report containing information about the build,
        /// such as its target platform and output path.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                if (GetModules().Count == 0)
                {
                    return;
                }

                CheckCompatibilityWithAllSesssionConfigs(
                    ARCoreExtensionsProjectSettings.Instance,
                    AndroidDependenciesHelper.GetAllSessionConfigs());
            }
        }

        private static void CheckCompatibilityWithAllSesssionConfigs(
            ARCoreExtensionsProjectSettings settings,
            Dictionary<ARCoreExtensionsConfig, string> sessionToSceneMap)
        {
            List<IDependentModule> featureModules = GetModules();
            foreach (IDependentModule module in featureModules)
            {
                foreach (var entry in sessionToSceneMap)
                {
                    if (!module.IsCompatibleWithSessionConfig(
                            settings, entry.Key))
                    {
                        throw new BuildFailedException(
                            string.Format(
                                "{0} isn't compatible with the ARCoreExtensionsConfig in {1}",
                                module.GetType().Name, entry.Value));
                    }
                }
            }
        }
    }
}
