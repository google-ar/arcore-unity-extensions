//-----------------------------------------------------------------------
// <copyright file="ExternalDependencyResolverHelper.cs" company="Google LLC">
//
// Copyright 2022 Google LLC
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
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// EDM helper class.
    /// </summary>
    public static class ExternalDependencyResolverHelper
    {
        /// <summary>
        /// Filename of IOS Resolver plugin.
        /// </summary>
        public static string IOSResolverName = "Google.IOSResolver";

        /// <summary>
        /// Filename of Android Resolver plugin.
        /// </summary>
        public static string JarResolverName = "Google.JarResolver";

        /// <summary>
        /// Filename where External Dependency Resolver stores state.
        /// </summary>
        public static string DependencyStateFile = Path.Combine(
            "ProjectSettings", "AndroidResolverDependencies.xml");

        private static object _playServicesSupport = null;

        /// <summary>
        /// Registers Android Jar depdencies.
        /// </summary>
        /// <param name="artifacts">JarArtifacts for resgistration.</param>
        public static void RegisterAndroidDependencies(JarArtifact[] artifacts)
        {
            EnableDependencyResolver(JarResolverName);
            if (artifacts == null || artifacts.Length == 0)
            {
                return;
            }

            CreatePlayServicesSupportInstance();
            if (_playServicesSupport == null)
            {
                return;
            }

            foreach (JarArtifact artifact in artifacts)
            {
                Dictionary<string, object> namedArgs = new Dictionary<string, object>();
                if (artifact.PackageIds != null)
                {
                    namedArgs["packageIds"] = artifact.PackageIds;
                }

                object[] args = new object[]
                {
                    artifact.Group, artifact.Artifact, artifact.Version
                };
                Google.VersionHandler.InvokeInstanceMethod(
                    _playServicesSupport, "DependOn", args, namedArgs: namedArgs);
                Debug.Log("Registered dependency: " + artifact);
            }
        }

        /// <summary>
        /// Clear all dependencies.
        /// </summary>
        public static void ClearDependencies()
        {
            string fullpath = Path.Combine(Application.dataPath + "/..", DependencyStateFile);
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }

            if (_playServicesSupport == null)
            {
                return;
            }

            Google.VersionHandler.InvokeInstanceMethod(
                _playServicesSupport, "ClearDependencies", null);
        }

        /// <summary>
        /// Enable a dependency resolver.
        /// </summary>
        /// <param name="resolverName">Resolver plugin's filename.</param>
        public static void EnableDependencyResolver(string resolverName)
        {
            string resolverPath = null;
            string[] guids = AssetDatabase.FindAssets(resolverName);
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (path.EndsWith(".dll"))
                {
                    if (resolverPath != null)
                    {
                        Debug.LogErrorFormat("ARCoreExtensions: " +
                            "There are multiple {0} plugins detected. " +
                            "One is {1}, another is {2}. Please remove one of them.",
                            resolverName, resolverPath, path);
                        return;
                    }

                    resolverPath = path;
                }
            }

            if (resolverPath == null)
            {
                Debug.LogErrorFormat(
                    "ARCoreExtensions: Could not locate {0} plugin.", resolverName);
                return;
            }

            PluginImporter pluginImporter =
                AssetImporter.GetAtPath(resolverPath) as PluginImporter;
            if (!pluginImporter.GetCompatibleWithEditor())
            {
                pluginImporter.SetCompatibleWithEditor(true);
            }
        }

        private static void CreatePlayServicesSupportInstance()
        {
            EnableDependencyResolver(JarResolverName);
            if (_playServicesSupport != null)
            {
                return;
            }

            Type playServicesSupportType = Google.VersionHandler.FindClass(
               JarResolverName, "Google.JarResolver.PlayServicesSupport");
            if (playServicesSupportType == null)
            {
                Debug.LogErrorFormat(
                    "ARCoreExtensions: Cannot find PlayServicesSupport class in this project. " +
                    "Please ensure External Dependency Manager exists and is enabled.");
                return;
            }

            object[] args = new object[]
            {
                "ARCoreExtensions", // clientName, must be a valid filename.
                null,               // sdkPath (unused).
                "ProjectSettings",  // settingsDirectory, this parameter is obsolete.
            };
            _playServicesSupport = Google.VersionHandler.InvokeStaticMethod(
                playServicesSupportType, "CreateInstance", args);
            if (_playServicesSupport == null)
            {
                Debug.LogErrorFormat(
                    "ARCoreExtensions: Failed to instantiate PlayServicesSupport. " +
                    "Please ensure External Dependency Manager exists and is enabled.");
                return;
            }
        }
    }
}
