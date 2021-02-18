//-----------------------------------------------------------------------
// <copyright file="CustomizedManifestInjection.cs" company="Google LLC">
//
// Copyright 2020 Unity Technologies All Rights Reserved.
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
    using System.IO;
    using System.Xml.Linq;
    using UnityEditor.Android;

    /// <summary>
    /// Inject customized manifest after the Android Gradle project is generated.
    /// </summary>
    public class CustomizedManifestInjection : IPostGenerateGradleAndroidProject
    {
        private const string _androidManifestPath = "/src/main/AndroidManifest.xml";

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
        /// Generate the AndroidManifest XDocument based on the ARCoreProjectSettings.
        /// This function would be used in Tests.
        /// </summary>
        /// <param name="manifestDoc">Original AndroidManifest XML Document.</param>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>The XDocument of the final AndroidManifest.</returns>
        public static XDocument GenerateCustomizedAndroidManifest(
            XDocument manifestDoc,
            ARCoreExtensionsProjectSettings settings)
        {
            XElement mergedRoot = manifestDoc.Root;
            List<IDependentModule> featureModules = DependentModulesManager.GetModules();
            foreach (IDependentModule module in featureModules)
            {
                if (module.IsEnabled(settings))
                {
                    XDocument xDocument =
                        AndroidManifestMerger.TransferToXDocument(
                            module.GetAndroidManifestSnippet(settings));
                    mergedRoot = AndroidManifestMerger.MergeXElement(
                        mergedRoot, xDocument.Root);
                }
            }

            return new XDocument(mergedRoot);
        }

        /// <summary>
        /// Callback event after the Android Gradle project is generated.
        /// This ensures the Android Manifest corresponds to
        /// https://developers.google.com/ar/develop/java/enable-arcore.
        /// </summary>
        /// <param name="path">The path to the root of the Unity library Gradle project.</param>
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string manifestPath = path + _androidManifestPath;
            XDocument manifestDoc = XDocument.Parse(File.ReadAllText(manifestPath));
            manifestDoc = GenerateCustomizedAndroidManifest(
                manifestDoc, ARCoreExtensionsProjectSettings.Instance);
            File.WriteAllText(manifestPath, manifestDoc.ToString());
        }
    }
}
