//-----------------------------------------------------------------------
// <copyright file="IOSSupportHelper.cs" company="Google">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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

    internal static class IOSSupportHelper
    {
        // GUID of folder [ARCore Extensions Package]/Editor/BuildResources/
        private const string k_ARCoreIOSDependencyFolderGUID = "117437286c43f4eeb845c3257f2a8546";

        private const string k_ARCoreIOSDependencyFileName = "ARCoreiOSDependencies";
        private const string k_ARCoreExtensionIOSSupportSymbol = "ARCORE_EXTENSIONS_IOS_SUPPORT";

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

            _UpdateIOSScriptingDefineSymbols(arcoreIOSEnabled);
            _UpdateIOSPodDependencies(arcoreIOSEnabled);
        }

        private static void _UpdateIOSScriptingDefineSymbols(bool arcoreIOSEnabled)
        {
            string iOSScriptingDefineSymbols =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
            bool iOSSupportDefined = iOSScriptingDefineSymbols.Contains(
                k_ARCoreExtensionIOSSupportSymbol);

            if (arcoreIOSEnabled && !iOSSupportDefined)
            {
                Debug.LogFormat("Adding {0} define symbol.", k_ARCoreExtensionIOSSupportSymbol);
                iOSScriptingDefineSymbols += ";" + k_ARCoreExtensionIOSSupportSymbol;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    BuildTargetGroup.iOS, iOSScriptingDefineSymbols);
            }
            else if (!arcoreIOSEnabled && iOSSupportDefined)
            {
                Debug.LogFormat("Removing {0} define symbol.", k_ARCoreExtensionIOSSupportSymbol);
                iOSScriptingDefineSymbols = iOSScriptingDefineSymbols.Replace(
                        k_ARCoreExtensionIOSSupportSymbol, string.Empty);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    BuildTargetGroup.iOS, iOSScriptingDefineSymbols);
            }
        }

        private static void _UpdateIOSPodDependencies(bool arcoreIOSEnabled)
        {
            string dependencyFolderFullPath = Path.GetFullPath(
                AssetDatabase.GUIDToAssetPath(k_ARCoreIOSDependencyFolderGUID));
            string iOSPodDependencyTemplatePath =
                Path.Combine(dependencyFolderFullPath, k_ARCoreIOSDependencyFileName + ".template");
            string iOSPodDependencyXMLPath =
                Path.Combine(dependencyFolderFullPath, k_ARCoreIOSDependencyFileName + ".xml");

            if (arcoreIOSEnabled && !File.Exists(iOSPodDependencyXMLPath))
            {
                Debug.Log("Adding ARCoreiOSDependencies.");

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
                Debug.Log("Removing ARCoreiOSDependencies.");

                File.Delete(iOSPodDependencyXMLPath);
                File.Delete(iOSPodDependencyXMLPath + ".meta");

                AssetDatabase.Refresh();
            }
        }
    }
}
