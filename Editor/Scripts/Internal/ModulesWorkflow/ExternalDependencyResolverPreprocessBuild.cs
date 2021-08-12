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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEditor.Callbacks;
    using UnityEditor.XR.Management;
    using UnityEngine;
    using UnityEngine.XR.ARCore;
    using UnityEngine.XR.Management;

    /// <summary>
    /// Manage the dependencies for play service resolver.
    /// </summary>
    public class ExternalDependencyResolverPreprocessBuild : IPreprocessBuildWithReport
    {
        private const string _dependenciesDirectory =
            "/ExtensionsAssets/Editor/DependenciesTempFolder";

        private const string _androidDependenciesFileSuffix = "Dependencies.xml";
        private const string _androidDependenciesFormat =
            @"<dependencies>
                <androidPackages>
                {0}
                </androidPackages>
            </dependencies>";

        private static HashSet<string> _enabledIOSTemplate = new HashSet<string>();

        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Overriden property.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
            Justification = "Overriden property.")]
        public int callbackOrder
        {
            get
            {
                // This preprocess build might need to check whether the module is required.
                // So it should be executed after CompatiblityCheckPreprocessBuild.
                return 2;
            }
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
            if (target == UnityEditor.BuildTarget.Android)
            {
                Debug.Log("ARCoreExtensions: Cleaning up Android library dependencies.");
                string folderPath = Application.dataPath + _dependenciesDirectory;
                Directory.Delete(folderPath, true);
                AssetDatabase.Refresh();
                AndroidDependenciesHelper.DoPlayServicesResolve();
            }
            else if (target == UnityEditor.BuildTarget.iOS)
            {
                foreach (string enabledTemplateFile in _enabledIOSTemplate)
                {
                    Debug.LogFormat("ARCoreExtensions: Cleaning up {0} in PostprocessBuild.",
                            enabledTemplateFile);
                    IOSSupportHelper.UpdateIOSPodDependencies(false, enabledTemplateFile);
                }
            }
        }

        /// <summary>
        /// A callback before the build is started to manage the dependecies.
        /// </summary>
        /// <param name="report">A report containing information about the build.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            UnityEditor.BuildTarget buildTarget = report.summary.platform;
            if (buildTarget == UnityEditor.BuildTarget.Android)
            {
                ManageAndroidDependencies(ARCoreExtensionsProjectSettings.Instance);
            }
            else if (buildTarget == UnityEditor.BuildTarget.iOS)
            {
                ManageIOSDependencies(ARCoreExtensionsProjectSettings.Instance);
            }
        }

        /// <summary>
        /// Manage the ios dependecies based on the project settings.
        /// </summary>
        /// <param name="settings">
        /// The <see cref="ARCoreExtensionsProjectSettings"/> used by current build.
        /// </param>
        public void ManageIOSDependencies(ARCoreExtensionsProjectSettings settings)
        {
            _enabledIOSTemplate.Clear();
            List<IDependentModule> featureModules = DependentModulesManager.GetModules();
            foreach (IDependentModule module in featureModules)
            {
                string iOSDependenciesTemplateFile = module.GetIOSDependenciesTemplateFileName();
                if (!string.IsNullOrEmpty(iOSDependenciesTemplateFile))
                {
                    bool isModuleEnabled = module.IsEnabled(
                        settings, UnityEditor.BuildTarget.iOS);
                    if (isModuleEnabled)
                    {
                        Debug.LogFormat("ARCoreExtensions: Include {0} for {1}.",
                            iOSDependenciesTemplateFile, module.GetType().Name);
                        _enabledIOSTemplate.Add(iOSDependenciesTemplateFile);
                        IOSSupportHelper.UpdateIOSPodDependencies(
                            true, iOSDependenciesTemplateFile);
                    }
                    else
                    {
                        Debug.LogFormat("ARCoreExtensions: Exclude {0} for {1}.",
                            iOSDependenciesTemplateFile, module.GetType().Name);
                        IOSSupportHelper.UpdateIOSPodDependencies(
                            false, iOSDependenciesTemplateFile);
                    }
                }
            }
        }

        /// <summary>
        /// Manage the android dependecies based on the project settings.
        /// </summary>
        /// <param name="settings">The <see cref="ARCoreExtensionsProjectSettings"/> used by
        /// current build.</param>
        public void ManageAndroidDependencies(ARCoreExtensionsProjectSettings settings)
        {
            List<IDependentModule> featureModules = DependentModulesManager.GetModules();

            // Use Assets/ExtensionsAssets for all building generated resources.
            string folderPath = Application.dataPath + _dependenciesDirectory;
            Directory.CreateDirectory(folderPath);
            foreach (IDependentModule module in featureModules)
            {
                if (module.IsEnabled(settings, UnityEditor.BuildTarget.Android))
                {
                    string dependenciesSnippet = module.GetAndroidDependenciesSnippet(settings);
                    if (!string.IsNullOrEmpty(dependenciesSnippet))
                    {
                        string fileName = module.GetType().Name + _androidDependenciesFileSuffix;
                        string fullPath = Path.Combine(folderPath, fileName);
                        string finalSnippet = XDocument.Parse(string.Format(
                            _androidDependenciesFormat, dependenciesSnippet)).ToString();
                        File.WriteAllText(fullPath, finalSnippet);
                        Debug.LogFormat("Module {0} added Android library dependencies:\n{1}",
                            module.GetType().Name, finalSnippet);
                        AssetDatabase.Refresh();
                        AndroidDependenciesHelper.DoPlayServicesResolve();
                    }
                }
            }
        }
    }
}
