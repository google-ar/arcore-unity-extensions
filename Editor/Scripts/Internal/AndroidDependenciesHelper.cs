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
                    bool removeFromEditor = false;
                    if (!scene.isLoaded)
                    {
                        removeFromEditor = true;
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

                    if (removeFromEditor)
                    {
                        // Unload scenes from the Editor after iterated all game objects
                        // if it's not loaded at build time.
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }

            return sessionToPathMap;
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
    }
}
