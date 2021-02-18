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
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// IOS support helper class.
    /// </summary>
    public static class IOSSupportHelper
    {
        // GUID of plugin [ARCore Extensions Package]/Editor/ExternalDependencyManager/
        //     Editor/Google.IOSResolver_{version}.dll.meta
        private const string _iosResolverGuid = "f7ad2228faf74a398ae7d46a32a25174";

        // GUID of folder [ARCore Extensions Package]/Editor/BuildResources/
        private const string _arCoreIOSDependencyFolderGUID = "117437286c43f4eeb845c3257f2a8546";

        private const string _arCoreIOSDependencyFileName = "ARCoreiOSDependencies";
        private const string _arCoreExtensionIOSSupportSymbol = "ARCORE_EXTENSIONS_IOS_SUPPORT";

        /// <summary>
        /// Enables ARCore iOS Support in Extensions.
        /// </summary>
        /// <param name="arcoreIOSEnabled">Indicates whether to enable or disable iOS support.
        /// </param>
        public static void SetARCoreIOSSupportEnabled(bool arcoreIOSEnabled)
        {
            if (arcoreIOSEnabled)
            {
                Debug.Log(
                    "Enabling iOS Support for ARCore Extensions for AR Foundation. " +
                    "Note that you will need to add ARKit XR Plugin " +
                    "to your project to make ARCore Extensions work on iOS.");
            }
            else
            {
                Debug.Log("Disabling ARCore Extensions iOS support.");
            }

            UpdateIOSScriptingDefineSymbols(arcoreIOSEnabled);
            UpdateIOSPodDependencies(arcoreIOSEnabled, _arCoreIOSDependencyFileName);
        }

        /// <summary>
        /// Updates the iOS pod dependency based on iOS support state.
        /// </summary>
        /// <param name="arcoreIOSEnabled">Enable or disable the dependency.
        /// </param>
        /// <param name="dependencyFileName">The file name of the dependency template.</param>
        public static void UpdateIOSPodDependencies(bool arcoreIOSEnabled,
            string dependencyFileName)
        {
            EnableIOSResolver();

            string dependencyFolderFullPath = Path.GetFullPath(
                AssetDatabase.GUIDToAssetPath(_arCoreIOSDependencyFolderGUID));
            string iOSPodDependencyTemplatePath =
                Path.Combine(dependencyFolderFullPath, dependencyFileName + ".template");
            string iOSPodDependencyXMLPath =
                Path.Combine(dependencyFolderFullPath, dependencyFileName + ".xml");

            if (arcoreIOSEnabled && !File.Exists(iOSPodDependencyXMLPath))
            {
                Debug.LogFormat("Adding {0}.", dependencyFileName);

                if (!File.Exists(iOSPodDependencyTemplatePath))
                {
                    Debug.LogError(
                        "Failed to enable ARCore iOS dependency xml. Template file is missing.");
                    return;
                }

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

        private static void UpdateIOSScriptingDefineSymbols(bool arcoreIOSEnabled)
        {
            string iOSScriptingDefineSymbols =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
            bool iOSSupportDefined = iOSScriptingDefineSymbols.Contains(
                _arCoreExtensionIOSSupportSymbol);

            if (arcoreIOSEnabled && !iOSSupportDefined)
            {
                Debug.LogFormat("Adding {0} define symbol.", _arCoreExtensionIOSSupportSymbol);
                iOSScriptingDefineSymbols += ";" + _arCoreExtensionIOSSupportSymbol;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    BuildTargetGroup.iOS, iOSScriptingDefineSymbols);
            }
            else if (!arcoreIOSEnabled && iOSSupportDefined)
            {
                Debug.LogFormat("Removing {0} define symbol.", _arCoreExtensionIOSSupportSymbol);
                iOSScriptingDefineSymbols = iOSScriptingDefineSymbols.Replace(
                    _arCoreExtensionIOSSupportSymbol, string.Empty);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    BuildTargetGroup.iOS, iOSScriptingDefineSymbols);
            }
        }

        private static void EnableIOSResolver()
        {
            string iosResolverPath = AssetDatabase.GUIDToAssetPath(_iosResolverGuid);
            if (iosResolverPath == null)
            {
                Debug.LogError("ARCoreExtensions: Could not locate Google.IOSResolver plugin.");
                return;
            }

            PluginImporter pluginImporter =
                AssetImporter.GetAtPath(iosResolverPath) as PluginImporter;
            if (!pluginImporter.GetCompatibleWithEditor())
            {
                pluginImporter.SetCompatibleWithEditor(true);
            }
        }
    }
}
