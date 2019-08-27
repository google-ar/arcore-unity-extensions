//-----------------------------------------------------------------------
// <copyright file="CloudAnchorPreprocessBuild.cs" company="Google">
//
// Copyright 2019 Google LLC All Rights Reserved.
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
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    internal class CloudAnchorPreprocessBuild : IPreprocessBuildWithReport
    {
        private const string k_CloudAnchorManifest = "cloud_anchor_manifest.aar";

        private const string k_ManifestTemplatePath =
            "Packages/com.google.ar.core.arfoundation.extensions/Editor/BuildResources/" +
            "cloud_anchor_manifest.aartemplate";

        private const string k_PluginsFolderPath =
            "Packages/com.google.ar.core.arfoundation.extensions/Runtime/Plugins/" +
            k_CloudAnchorManifest;

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
        }

        private void _PreprocessAndroidBuild()
        {
            // Get the JDK path.
            var jdkPath = UnityEditor.EditorPrefs.GetString("JdkPath");
            if (string.IsNullOrEmpty(jdkPath))
            {
                Debug.Log(
                    "Unity 'Preferences > External Tools > Android JDK' path is not set. " +
                    "Falling back to JAVA_HOME environment variable.");
                jdkPath = System.Environment.GetEnvironmentVariable("JAVA_HOME");
            }

            if (string.IsNullOrEmpty(jdkPath))
            {
                throw new BuildFailedException(
                    "A JDK path needs to be specified for the Android build.");
            }

            bool cloudAnchorsEnabled = !string.IsNullOrEmpty(
                ARCoreExtensionsProjectSettings.Instance.AndroidCloudServicesApiKey);

            var cachedCurrentDirectory = Directory.GetCurrentDirectory();
            var jarPath = Path.Combine(jdkPath, "bin/jar");
            var cloudAnchorsManifestAarPath = Path.GetFullPath(k_PluginsFolderPath);

            if (cloudAnchorsEnabled)
            {
                // If the API Key didn't change then do nothing.
                if (!_IsApiKeyDirty(jarPath, cloudAnchorsManifestAarPath,
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
                    var manifestTemplatePath = Path.GetFullPath(k_ManifestTemplatePath);

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
                        "cf {0} {1}", k_CloudAnchorManifest, fileListBuilder.ToString());

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

                    File.Copy(Path.Combine(tempDirectoryPath, k_CloudAnchorManifest),
                        cloudAnchorsManifestAarPath, true);
                }
                finally
                {
                    // Cleanup.
                    Directory.SetCurrentDirectory(cachedCurrentDirectory);
                    Directory.Delete(tempDirectoryPath, true);

                    AssetDatabase.Refresh();
                }

                AssetHelper.GetPluginImporterByName(k_CloudAnchorManifest)
                    .SetCompatibleWithPlatform(BuildTarget.Android, true);
            }
            else
            {
                Debug.Log(
                    "Cloud Anchor API key has not been set. Cloud Anchors will be disabled in " +
                    "this build.");
                if (File.Exists(cloudAnchorsManifestAarPath))
                {
                    File.Delete(cloudAnchorsManifestAarPath);
                }
                AssetDatabase.Refresh();
            }
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
                var tempAarPath = Path.Combine(tempDirectoryPath, k_CloudAnchorManifest);
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
