//-----------------------------------------------------------------------
// <copyright file="AndroidDependenciesHelper.cs" company="Google LLC">
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
    using System.IO;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// This handles the addition and removal android dependencies, and run
    /// ExternalDependencyManager plugin.
    /// </summary>
    public static class AndroidDependenciesHelper
    {
        // GUID of plugin [ARCore Extensions Package]/Editor/ExternalDependencyManager/
        //     Editor/Google.JarResolver_{version}.dll.meta
        private const string _jarResolverGuid = "a8f371f579f2426d93a8c958438275b7";

        private static readonly string _templateFileExtension = ".template";
        private static readonly string _playServiceDependencyFileExtension = ".xml";

        /// <summary>
        /// Gets all session configs from active scenes.
        /// </summary>
        /// <returns>A dictionary contains session config to scene path mapping.</returns>
        public static Dictionary<ARCoreExtensionsConfig, string> GetAllSessionConfigs()
        {
            Dictionary<ARCoreExtensionsConfig, string> sessionToPathMap =
                new Dictionary<ARCoreExtensionsConfig, string>();
            foreach (EditorBuildSettingsScene editorScene in EditorBuildSettings.scenes)
            {
                if (editorScene.enabled)
                {
                    Scene scene = SceneManager.GetSceneByPath(editorScene.path);
                    if (!scene.isLoaded)
                    {
                        scene = EditorSceneManager.OpenScene(
                            editorScene.path, OpenSceneMode.Additive);
                    }

                    foreach (GameObject gameObject in scene.GetRootGameObjects())
                    {
                        ARCoreExtensions extensionsComponent =
                            (ARCoreExtensions)gameObject.GetComponentInChildren(
                                typeof(ARCoreExtensions));
                        if (extensionsComponent != null)
                        {
                            if (!sessionToPathMap.ContainsKey(
                                    extensionsComponent.ARCoreExtensionsConfig))
                            {
                                sessionToPathMap.Add(
                                    extensionsComponent.ARCoreExtensionsConfig, editorScene.path);
                            }

                            break;
                        }
                    }
                }
            }

            return sessionToPathMap;
        }

        /// <summary>
        /// Handle the updating of the AndroidManifest tags by enabling/disabling the dependencies
        /// manifest AAR as necessary.
        /// </summary>
        /// <param name="enabledDependencies">If set to <c>true</c> enabled dependencies.</param>
        /// <param name="dependenciesManifestGuid">Dependencies manifest GUID.</param>
        public static void SetAndroidPluginEnabled(bool enabledDependencies,
            string dependenciesManifestGuid)
        {
            string manifestAssetPath = AssetDatabase.GUIDToAssetPath(dependenciesManifestGuid);
            if (manifestAssetPath == null)
            {
                Debug.LogError("ARCoreExtensions: Could not locate dependencies manifest plugin.");
                return;
            }

            PluginImporter pluginImporter =
                AssetImporter.GetAtPath(manifestAssetPath) as PluginImporter;
            if (pluginImporter == null)
            {
                Debug.LogErrorFormat(
                    "ARCoreExtensions: Could not locate dependencies manifest plugin {0}.",
                    Path.GetFileName(manifestAssetPath));
                return;
            }

            pluginImporter.SetCompatibleWithPlatform(BuildTarget.Android, enabledDependencies);
        }

        /// <summary>
        /// Handle the addition or removal Android dependencies using the ExternalDependencyManager.
        /// Adding the dependencies is done by renaming the dependencies .template file to a .xml
        /// file so that it will be picked up by the ExternalDependencyManager plugin.
        /// </summary>
        /// <param name="enabledDependencies">If set to <c>true</c> enabled dependencies.</param>
        /// <param name="dependenciesTemplateGuid">Dependencies template GUID.</param>
        public static void UpdateAndroidDependencies(bool enabledDependencies,
            string dependenciesTemplateGuid)
        {
            string dependenciesTemplatePath =
                AssetDatabase.GUIDToAssetPath(dependenciesTemplateGuid);
            if (dependenciesTemplatePath == null)
            {
                Debug.LogError(
                    "ARCoreExtensions: Failed to enable Android dependencies xml. " +
                    "Template file is missing.");
                return;
            }

            string dependenciesXMLPath = dependenciesTemplatePath.Replace(
                _templateFileExtension, _playServiceDependencyFileExtension);

            if (enabledDependencies && !File.Exists(dependenciesXMLPath))
            {
                Debug.LogFormat(
                    "Adding {0}.",
                    System.IO.Path.GetFileNameWithoutExtension(dependenciesTemplatePath));

                File.Copy(dependenciesTemplatePath, dependenciesXMLPath);
                AssetDatabase.Refresh();
            }
            else if (!enabledDependencies && File.Exists(dependenciesXMLPath))
            {
                Debug.LogFormat(
                    "Removing {0}.",
                    System.IO.Path.GetFileNameWithoutExtension(dependenciesTemplatePath));

                File.Delete(dependenciesXMLPath);
                File.Delete(dependenciesXMLPath + ".meta");
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Uses reflection to find the GooglePlayServices.PlayServicesResolver class and invoke
        /// the public static method, MenuResolve() in order to resolve dependencies change.
        /// </summary>
        public static void DoPlayServicesResolve()
        {
            EnableJarResolver();

            const string namespaceName = "GooglePlayServices";
            const string className = "PlayServicesResolver";
            const string methodName = "MenuResolve";
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    if (assembly.GetTypes() == null)
                    {
                        continue;
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Could not get the Assembly types; skip it.
                    continue;
                }

                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.Namespace != namespaceName)
                    {
                        continue;
                    }

                    if (type.Name == className)
                    {
                        // We found the class we're looking for. Attempt to call the method and
                        // then return.
                        var menuResolveMethod = type.GetMethod(methodName,
                            BindingFlags.Public | BindingFlags.Static);
                        if (menuResolveMethod == null)
                        {
                            Debug.LogErrorFormat("ARCoreExtensions: Error finding public " +
                                                 "static method {0} on {1}.{2}.",
                                                 methodName, namespaceName, className);
                            return;
                        }

                        Debug.LogFormat("ARCoreExtensions: Invoking {0}.{1}.{2}()",
                            namespaceName, className, methodName);
                        menuResolveMethod.Invoke(null, null);
                        return;
                    }
                }
            }

            Debug.LogFormat("ARCoreExtensions: " +
                            "Could not find class {0}.{1} for dependency resolution.",
                            namespaceName, className);
        }

        /// <summary>
        /// Gets the JDK path used by this project.
        /// </summary>
        /// <returns>If found, returns the JDK path used by this project. Otherwise, returns null.
        /// </returns>
        public static string GetJdkPath()
        {
            string jdkPath = null;

            if (EditorPrefs.GetBool("JdkUseEmbedded"))
            {
                // Use OpenJDK that is bundled with Unity. JAVA_HOME will be set when
                // 'Preferences > External Tools > Android > JDK installed with Unity' is checked.
                jdkPath = Environment.GetEnvironmentVariable("JAVA_HOME");
                if (string.IsNullOrEmpty(jdkPath))
                {
                    Debug.LogError(
                        "'Preferences > External Tools > Android > JDK installed with Unity' is " +
                        "checked, but JAVA_HOME is unset or empty. Try unchecking this setting " +
                        "and configuring a valid JDK path under " +
                        "'Preferences > External Tools > Android > JDK'.");
                }
            }
            else
            {
                // Use JDK path specified by 'Preferences > External Tools > Android > JDK'.
                jdkPath = EditorPrefs.GetString("JdkPath");
                if (string.IsNullOrEmpty(jdkPath))
                {
                    // Use JAVA_HOME from the O/S environment.
                    jdkPath = Environment.GetEnvironmentVariable("JAVA_HOME");
                    if (string.IsNullOrEmpty(jdkPath))
                    {
                        Debug.LogError(
                            "'Preferences > External Tools > Android > JDK installed with Unity' " +
                            "is unchecked, but 'Preferences > External Tools > Android > JDK' " +
                            "path is empty and JAVA_HOME environment variable is unset or empty.");
                    }
                }
            }

            if (!string.IsNullOrEmpty(jdkPath) &&
                (File.GetAttributes(jdkPath) & System.IO.FileAttributes.Directory) == 0)
            {
                Debug.LogErrorFormat("Invalid JDK path '{0}'", jdkPath);
                jdkPath = null;
            }

            return jdkPath;
        }

        private static void EnableJarResolver()
        {
            string jarResolverPath = AssetDatabase.GUIDToAssetPath(_jarResolverGuid);
            if (jarResolverPath == null)
            {
                Debug.LogError("ARCoreExtensions: Could not locate Google.JarResolver plugin.");
                return;
            }

            PluginImporter pluginImporter =
                AssetImporter.GetAtPath(jarResolverPath) as PluginImporter;
            if (!pluginImporter.GetCompatibleWithEditor())
            {
                pluginImporter.SetCompatibleWithEditor(true);
            }
        }
    }
}
