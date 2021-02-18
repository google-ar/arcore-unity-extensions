//-----------------------------------------------------------------------
// <copyright file="ProguardInjectionPreprocessBuild.cs" company="Google LLC">
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
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEditor.XR.Management;
    using UnityEngine;
    using UnityEngine.XR.ARCore;
    using UnityEngine.XR.Management;

    /// <summary>
    /// Inject customized proguard before building Android target.
    /// </summary>
    public class ProguardInjectionPreprocessBuild : IPreprocessBuildWithReport
    {
        private const string _userProguardDirectory = "Plugins/Android/";
        private const string _userProguardFile = "proguard-user.txt";
        private const string _userProguardDisabledFile = "proguard-user.txt.DISABLED";
        private const string _moduleProguardRuleEnds = "\n### Module Progurad Rules ends ###\n";
        private const string _moduleProguardRuleStarts =
            "\n### Module Progurad Rules starts ###\n";

        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Overriden property.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
            Justification = "Overriden property.")]
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// A callback before the build is started to inject customized proguard rules.
        /// </summary>
        /// <param name="report">A report containing information about the build.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                InjectModuleProguardRules(ARCoreExtensionsProjectSettings.Instance);
            }
        }

        /// <summary>
        /// Inject module proguard rules based on the project settings.
        /// </summary>
        /// <param name="settings">The <see cref="ARCoreExtensionsProjectSettings"/> used by
        /// current build.</param>
        public void InjectModuleProguardRules(ARCoreExtensionsProjectSettings settings)
        {
            bool needToEnableProguardInBuild = false;
            StringBuilder moduleProguardSnippets = new StringBuilder();
            List<IDependentModule> featureModules = DependentModulesManager.GetModules();
            foreach (IDependentModule module in featureModules)
            {
                if (module.IsEnabled(settings))
                {
                    string moduleProguardSnippet = module.GetProguardSnippet(settings);
                    if (!string.IsNullOrEmpty(moduleProguardSnippet))
                    {
                        needToEnableProguardInBuild = true;
                        moduleProguardSnippets.AppendFormat("\n## {0} ##\n{1}\n",
                            module.GetType().Name, moduleProguardSnippet);
                    }
                }
            }

            if (needToEnableProguardInBuild)
            {
                string previousProguardRule = LoadPreviousProguardRule();
                string userProguardFullPath = Path.Combine(
                    Application.dataPath, _userProguardDirectory + _userProguardFile);
                string curModuleProguardRule = moduleProguardSnippets.ToString();
                Directory.CreateDirectory(
                    Path.Combine(Application.dataPath, _userProguardDirectory));
                File.WriteAllText(userProguardFullPath,
                    ReplaceModuleProguardRule(previousProguardRule, curModuleProguardRule));
            }
        }

        /// <summary>
        /// Load existing proguard rules before injection.
        /// </summary>
        /// <returns>The proguard rule used by previous build.</returns>
        public string LoadPreviousProguardRule()
        {
            string previousProguardRule = string.Empty;
            string userProguardFullPath = Path.Combine(
                Application.dataPath, _userProguardDirectory + _userProguardFile);
            if (File.Exists(userProguardFullPath))
            {
                previousProguardRule = File.ReadAllText(userProguardFullPath);
            }
            else
            {
                string userProguardDisabledFullPath = Path.Combine(
                    Application.dataPath, _userProguardDirectory + _userProguardDisabledFile);
                if (File.Exists(userProguardDisabledFullPath))
                {
                    previousProguardRule = File.ReadAllText(userProguardDisabledFullPath);
                    File.Delete(userProguardDisabledFullPath);
                }
            }

            return previousProguardRule;
        }

        /// <summary>
        /// Replace proguard injection result with current module proguard rules.
        /// </summary>
        /// <param name="previousProguardRule">Previous proguard rule.</param>
        /// <param name="curModuleProguardRule">Current proguard rule.</param>
        /// <returns>Proguard rules contains existing rules and ARCore Extensions injected rules.
        /// </returns>
        private string ReplaceModuleProguardRule(
            string previousProguardRule, string curModuleProguardRule)
        {
            int previousProguardRuleStartIndex =
                previousProguardRule.IndexOf(_moduleProguardRuleStarts);
            if (previousProguardRuleStartIndex != -1)
            {
                int previousProguardRuleEndsIndex =
                    previousProguardRule.LastIndexOf(_moduleProguardRuleEnds) +
                    _moduleProguardRuleEnds.Length;
                previousProguardRule = previousProguardRule.Remove(
                    previousProguardRuleStartIndex,
                    previousProguardRuleEndsIndex - previousProguardRuleStartIndex);
            }

            return previousProguardRule + _moduleProguardRuleStarts +
                curModuleProguardRule + _moduleProguardRuleEnds;
        }
    }
}
