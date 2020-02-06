//-----------------------------------------------------------------------
// <copyright file="CloudAnchorPreprocessBuild.cs" company="Google">
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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Xml;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    internal class CloudAnchorPreprocessBuild : IPreprocessBuildWithReport
    {
        private const string k_CloudAnchorManifestFileName = "cloud_anchor_manifest.aar";

        // GUID of folder [ARCore Extensions Package]/Editor/BuildResources/
        private const string k_ManifestTemplateFolderGUID = "117437286c43f4eeb845c3257f2a8546";

        // GUID of folder [ARCore Extensions Package]/Runtime/Plugins/
        private const string k_PluginsFolderGUID = "7503bf199a08d47b681b4e1496fae841";

        [SuppressMessage("UnityRules.UnityStyleRules", "US1000:FieldsMustBeUpperCamelCase",
         Justification = "Overriden property.")]
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                _PreprocessAndroidBuild();
            }
            else if (report.summary.platform == BuildTarget.iOS)
            {
                _PreprocessIOSBuild();
            }
        }

        private string _getJdkPath()
        {
            string jdkPath = null;
            if (UnityEditor.EditorPrefs.GetBool("JdkUseEmbedded"))
            {
                // Use OpenJDK that is bundled with Unity. JAVA_HOME will be set when
                // 'Preferences > External Tools > Android > JDK installed with Unity' is checked.
                jdkPath = System.Environment.GetEnvironmentVariable("JAVA_HOME");
                if (string.IsNullOrEmpty(jdkPath))
                {
                    throw new BuildFailedException(
                        "'Preferences > External Tools > Android > JDK installed with Unity' is " +
                        "checked, but JAVA_HOME is unset or empty. Try unchecking this setting " +
                        "and configuring a valid JDK path under " +
                        "'Preferences > External Tools > Android > JDK'.");
                }
            }
            else
            {
                // Use JDK path specified by 'Preferences > External Tools > Android > JDK'.
                jdkPath = EditorPrefs.GetString("JdkPath");
                if (string.IsNullOrEmpty(jdkPath))
                {
                    // Use JAVA_HOME from the O/S environment.
                    jdkPath = System.Environment.GetEnvironmentVariable("JAVA_HOME");
                    if (string.IsNullOrEmpty(jdkPath))
                    {
                        throw new BuildFailedException(
                            "'Preferences > External Tools > Android > JDK installed with Unity' " +
                            "is unchecked, but 'Preferences > External Tools > Android > JDK' " +
                            "path is empty and JAVA_HOME environment variable is unset or empty.");
                    }
                }
            }

            if (!File.GetAttributes(jdkPath).HasFlag(FileAttributes.Directory))
            {
                throw new BuildFailedException(string.Format("Invalid JDK path '{0}'", jdkPath));
            }

            return jdkPath;
        }

        private void _PreprocessAndroidBuild()
        {
            bool cloudAnchorsEnabled = !string.IsNullOrEmpty(
                ARCoreExtensionsProjectSettings.Instance.AndroidCloudServicesApiKey);

            var cachedCurrentDirectory = Directory.GetCurrentDirectory();
            var cloudAnchorsManifestAARPath = Path.GetFullPath(
                Path.Combine(AssetDatabase.GUIDToAssetPath(k_PluginsFolderGUID),
                    k_CloudAnchorManifestFileName));

            if (cloudAnchorsEnabled)
            {
                string jarPath = Path.Combine(_getJdkPath(), "bin/jar");

                // If the API Key didn't change then do nothing.
                if (!_IsApiKeyDirty(jarPath, cloudAnchorsManifestAARPath,
                    ARCoreExtensionsProjectSettings.Instance.AndroidCloudServicesApiKey))
                {
                    return;
                }

                // Replace the project's Cloud Anchor AAR with the newly generated AAR.
                Debug.Log("Enabling Cloud Anchors in this build.");

                var tempDirectoryPath =
                    Path.Combine(cachedCurrentDirectory, FileUtil.GetUniqueTempPathInProject());

                try
                {
                    // Locate cloud_anchor_manifest.aartemplate from the package cache.
                    var manifestTemplatePath = Path.GetFullPath(
                        Path.Combine(AssetDatabase.GUIDToAssetPath(k_ManifestTemplateFolderGUID),
                            k_CloudAnchorManifestFileName + "template"));

                    // Move to a temp directory.
                    Directory.CreateDirectory(tempDirectoryPath);
                    Directory.SetCurrentDirectory(tempDirectoryPath);

                    // Extract the "template AAR" and remove it.
                    string output;
                    string errors;
                    ShellHelper.RunCommand(
                        jarPath, string.Format("xf \"{0}\"", manifestTemplatePath), out output,
                        out errors);

                    // Replace API key template parameter in manifest file.
                    var manifestPath = Path.Combine(tempDirectoryPath, "AndroidManifest.xml");
                    var manifestText = File.ReadAllText(manifestPath);
                    manifestText = manifestText.Replace(
                        "{{CLOUD_ANCHOR_API_KEY}}",
                        ARCoreExtensionsProjectSettings.Instance.AndroidCloudServicesApiKey);
                    File.WriteAllText(manifestPath, manifestText);

                    // Compress the new AAR.
                    var fileListBuilder = new StringBuilder();
                    foreach (var filePath in Directory.GetFiles(tempDirectoryPath))
                    {
                        fileListBuilder.AppendFormat(" {0}", Path.GetFileName(filePath));
                    }

                    string command = string.Format(
                        "cf {0} {1}", k_CloudAnchorManifestFileName, fileListBuilder);

                    ShellHelper.RunCommand(
                        jarPath,
                        command,
                        out output,
                        out errors);

                    if (!string.IsNullOrEmpty(errors))
                    {
                        throw new BuildFailedException(
                            string.Format(
                                "Error creating jar for Cloud Anchor manifest: {0}", errors));
                    }

                    File.Copy(Path.Combine(tempDirectoryPath, k_CloudAnchorManifestFileName),
                        cloudAnchorsManifestAARPath, true);
                }
                finally
                {
                    // Cleanup.
                    Directory.SetCurrentDirectory(cachedCurrentDirectory);
                    Directory.Delete(tempDirectoryPath, true);

                    AssetDatabase.Refresh();
                }

                AssetHelper.GetPluginImporterByName(k_CloudAnchorManifestFileName)
                    .SetCompatibleWithPlatform(BuildTarget.Android, true);
            }
            else
            {
                Debug.Log(
                    "Cloud Anchor API key has not been set. Cloud Anchors will be disabled in " +
                    "this build.");
                if (File.Exists(cloudAnchorsManifestAARPath))
                {
                    File.Delete(cloudAnchorsManifestAARPath);
                }

                AssetDatabase.Refresh();
            }
        }

        private void _PreprocessIOSBuild()
        {
            if (string.IsNullOrEmpty(
                    ARCoreExtensionsProjectSettings.Instance.IOSCloudServicesApiKey))
            {
                RuntimeConfig.SetIOSApiKey(string.Empty);
                return;
            }

            if (!ARCoreExtensionsProjectSettings.Instance.IsIOSSupportEnabled)
            {
                Debug.LogWarning("Failed to enable Cloud Anchor on iOS because iOS Support " +
                    "is not enabled. Go to 'Project Settings > XR > ARCore Extensionts' " +
                    "to change the settings.");
                RuntimeConfig.SetIOSApiKey(string.Empty);
                return;
            }

            RuntimeConfig.SetIOSApiKey(
                ARCoreExtensionsProjectSettings.Instance.IOSCloudServicesApiKey);
        }

        private bool _IsApiKeyDirty(string jarPath, string aarPath, string apiKey)
        {
            bool isApiKeyDirty = true;
            var cachedCurrentDirectory = Directory.GetCurrentDirectory();
            var tempDirectoryPath =
                Path.Combine(cachedCurrentDirectory, FileUtil.GetUniqueTempPathInProject());

            if (!File.Exists(aarPath))
            {
                return isApiKeyDirty;
            }

            try
            {
                // Move to a temp directory.
                Directory.CreateDirectory(tempDirectoryPath);
                Directory.SetCurrentDirectory(tempDirectoryPath);
                var tempAarPath = Path.Combine(tempDirectoryPath, k_CloudAnchorManifestFileName);
                File.Copy(aarPath, tempAarPath, true);

                // Extract the aar.
                string output;
                string errors;
                ShellHelper.RunCommand(jarPath, string.Format("xf \"{0}\"", tempAarPath),
                    out output, out errors);

                // Read API key parameter in manifest file.
                var manifestPath = Path.Combine(tempDirectoryPath, "AndroidManifest.xml");
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(manifestPath);
                XmlNode metaDataNode =
                    xmlDocument.SelectSingleNode("/manifest/application/meta-data");
                string oldApiKey = metaDataNode.Attributes["android:value"].Value;
                isApiKeyDirty = !apiKey.Equals(oldApiKey);
            }
            finally
            {
                // Cleanup.
                Directory.SetCurrentDirectory(cachedCurrentDirectory);
                Directory.Delete(tempDirectoryPath, true);
            }

            return isApiKeyDirty;
        }
    }
}
