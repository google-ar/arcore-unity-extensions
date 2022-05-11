//-----------------------------------------------------------------------
// <copyright file="DependentModuleBase.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.Internal
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The interface needed for a feature module.
    /// </summary>
    public class DependentModuleBase : IDependentModule
    {
        /// <summary>
        /// Get the permissions required by this module during the runtime.
        /// </summary>
        /// <param name="sessionConfig">ARCore Extensions Config.</param>
        /// <returns>
        /// The array containing the Runtime Permissions’ names required by this module.
        /// </returns>
        public virtual string[] GetRuntimePermissions(ARCoreExtensionsConfig sessionConfig)
        {
            return Array.Empty<string>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Checking whether it needs to be included into the application. If it returns true,
        /// all related content, including AndroidManifest, proguard rules and Android external
        /// dependencies would be injected during build time. The default values for new fields in
        /// ARCoreExtensionsProjectSettings should guarantee the associated module to return false.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <param name="buildTarget">Target build platform.</param>
        /// <returns>The boolean shows whether the module is enabled.</returns>
        public virtual bool IsEnabled(ARCoreExtensionsProjectSettings settings,
            UnityEditor.BuildTarget buildTarget)
        {
            return false;
        }

        /// <summary>
        /// Checking whether this module is compatible with given ARCoreExtensionsConfig and
        /// ARCoreExtensionsProjectSettings. If it returns false, the
        /// CompatibilityCheckPreprocessBuild will throw a general Build Failure Error.
        /// A feature developer should use this function to log detailed error messages that
        /// also include a recommendation of how to resolve the issue. Note: This method is
        /// called once per session config. Do not log any warning or info messages here to
        /// avoid duplicated and/or conflicted information.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <param name="sessionConfig">ARCore Extensions Config.</param>
        /// <param name="buildTarget">Target build platform.</param>
        /// <returns>The boolean shows whether the ARCoreExtensionsProjectSettings is compatible
        /// with the ARCoreExtensionsConfig.</returns>
        public virtual bool IsCompatible(
            ARCoreExtensionsProjectSettings settings, ARCoreExtensionsConfig sessionConfig,
            UnityEditor.BuildTarget buildTarget)
        {
            if (GetModuleNecessity(sessionConfig) == ModuleNecessity.Required &&
                !IsEnabled(settings, buildTarget))
            {
                Debug.LogErrorFormat("{0} is required but not enabled.", this.GetType().Name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checking whether this module is required with given ARCoreExtensionsConfig.
        /// In CompatibilityCheckPreprocessBuild, it would loop all explicit sessionConfigs
        /// via this funcion. If no session configs require this module but this module is
        /// still enabled, then a warning returned by <see ref="GetEnabledNotRequiredWarning()"/>
        /// will be logged.
        /// </summary>
        /// <param name="sessionConfig">ARCore Extensions Config.</param>
        /// <returns>
        /// Whether the module is Required/NotRequired/Optional by the sessionConfig.
        /// </returns>
        public virtual ModuleNecessity GetModuleNecessity(ARCoreExtensionsConfig sessionConfig)
        {
            return ModuleNecessity.NotRequired;
        }

        /// <summary>
        /// In CompatibilityCheckPreprocessBuild, if no session configs require this
        /// module but this module is still enabled, it would call this function to get
        /// a warning message.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <param name="buildTarget">Target build platform.</param>
        /// <returns>
        /// The warning message if this module is enabled but not required by any sessionConfigs.
        /// </returns>
        public virtual string GetEnabledNotRequiredWarning(
            ARCoreExtensionsProjectSettings settings, UnityEditor.BuildTarget buildTarget)
        {
            return string.Format("{0} is enabled but not required. Should it be disabled?",
                this.GetType().Name);
        }

        /// <summary>
        /// Return the XML snippet to include if this module is enabled. The string output will be
        /// added as a child node of the ‘manifest’ node of the customized AndroidManifest.xml.
        /// The android namespace will be available.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>The XML string snippet to add as a child of node 'manifest'.</returns>
        public virtual string GetAndroidManifestSnippet(ARCoreExtensionsProjectSettings settings)
        {
            return string.Empty;
        }

        /// <summary>
        /// Return the Proguard snippet to include if this module is enabled. The string output
        /// will be added into "proguard-user.txt" directly.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>The proguard rule string snippet.</returns>
        public virtual string GetProguardSnippet(ARCoreExtensionsProjectSettings settings)
        {
            return string.Empty;
        }

        /// <summary>
        /// Return all JarArtifacts to be used in External Dependencies Resolvor while building
        /// Android app.
        /// It will generate an AndroidResolverDependencies.xml file under ProjectSettings folder
        /// and then be used in Gradle build process.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>An array defining all Android dependencies.</returns>
        public virtual JarArtifact[] GetAndroidDependencies(
            ARCoreExtensionsProjectSettings settings)
        {
            return null;
        }
#endif // UNITY_EDITOR
    }
}
