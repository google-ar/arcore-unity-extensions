//-----------------------------------------------------------------------
// <copyright file="IOSSupportHelper.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    using System.IO;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// IOS support helper class.
    /// </summary>
    public static class IOSSupportHelper
    {
        /// <summary>
        /// Scripting define symbol to inject when iOS support is enabled on the project.
        /// </summary>
        public static string ARCoreExtensionIOSSupportSymbol = "ARCORE_EXTENSIONS_IOS_SUPPORT";

        /// <summary>
        /// IOS dependency to import when iOS support is enabled on this project.
        /// </summary>
        public static string ARCoreIOSDependencyFileName = "ARCoreiOSDependencies";

        // GUID of folder [ARCore Extensions Package]/Editor/BuildResources/
        private const string _arCoreIOSDependencyFolderGUID = "117437286c43f4eeb845c3257f2a8546";

        // Use Assets/ExtensionsAssets/Editor for generated iOS pod dependency.
        private const string _extensionAssetsEditorFolder = "/ExtensionsAssets/Editor";

        /// <summary>
        /// Updates the iOS pod dependency based on iOS support state.
        /// </summary>
        /// <param name="arcoreIOSEnabled">Enable or disable the dependency.
        /// </param>
        /// <param name="dependencyFileName">The file name of the dependency template.</param>
        public static void UpdateIOSPodDependencies(bool arcoreIOSEnabled,
            string dependencyFileName)
        {
            string templateFolderFullPath = Path.GetFullPath(
                AssetDatabase.GUIDToAssetPath(_arCoreIOSDependencyFolderGUID));
            string dependencyFolderFullPath = Application.dataPath + _extensionAssetsEditorFolder;
            Directory.CreateDirectory(dependencyFolderFullPath);
            string iOSPodDependencyTemplatePath =
                Path.Combine(templateFolderFullPath, dependencyFileName + ".template");
            string iOSPodDependencyXMLPath =
                Path.Combine(dependencyFolderFullPath, dependencyFileName + ".xml");

            if (arcoreIOSEnabled && !File.Exists(iOSPodDependencyXMLPath))
            {
                if (!File.Exists(iOSPodDependencyTemplatePath))
                {
                    Debug.LogError(
                        "Failed to enable ARCore iOS dependency xml. Template file is missing.");
                    return;
                }

                Debug.LogFormat("Adding {0}:\n{1}",
                    dependencyFileName, File.ReadAllText(iOSPodDependencyTemplatePath));

                File.Copy(iOSPodDependencyTemplatePath, iOSPodDependencyXMLPath);
                AssetDatabase.Refresh();
            }
            else if (!arcoreIOSEnabled && File.Exists(iOSPodDependencyXMLPath))
            {
                Debug.LogFormat("Removing {0}.", dependencyFileName);

                File.Delete(iOSPodDependencyXMLPath);
                File.Delete(iOSPodDependencyXMLPath + ".meta");

                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Updates the Scripting Define Symbols on iOS platforms based on the given project
        /// settings.
        /// </summary>
        /// <param name="projectSettings">The project settings for update.</param>
        public static void UpdateIOSScriptingDefineSymbols(
            ARCoreExtensionsProjectSettings projectSettings)
        {
            bool iosEnabled = projectSettings.IsIOSSupportEnabled;
            UpdateIOSScriptingDefineSymbols(ARCoreExtensionIOSSupportSymbol, iosEnabled);
            Dictionary<string, bool> iOSFeatureEnabled = projectSettings.GetIOSSymbolsStatus();
            foreach (var keyvalue in iOSFeatureEnabled)
            {
                UpdateIOSScriptingDefineSymbols(keyvalue.Key, iosEnabled && keyvalue.Value);
            }
        }

        private static void UpdateIOSScriptingDefineSymbols(string symbol, bool enabled)
        {
            HashSet<string> symbolSet = new HashSet<string>(
                PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS)
                .Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            bool iOSSupportDefined = symbolSet.Contains(symbol);

            if (enabled && !iOSSupportDefined)
            {
                Debug.LogFormat("Adding {0} define symbol.", symbol);
                symbolSet.Add(symbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    BuildTargetGroup.iOS, string.Join(";", symbolSet));
            }
            else if (!enabled && iOSSupportDefined)
            {
                Debug.LogFormat("Removing {0} define symbol.", symbol);
                symbolSet.Remove(symbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    BuildTargetGroup.iOS, string.Join(";", symbolSet));
            }
        }
    }
}
