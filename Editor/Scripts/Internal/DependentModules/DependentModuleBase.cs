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

namespace Google.XR.ARCoreExtensions.Editor.Internal
{
    using System;
    using System.Xml;
    using UnityEngine;

    /// <summary>
    /// The interface needed for a feature module.
    /// </summary>
    public class DependentModuleBase : IDependentModule
    {
        /// <summary>
        /// Checking whether it needs to be included in the customized AndroidManifest.
        /// The default values for new fields in ARCoreExtensionsProjectSettings should cause the
        /// associated module to return false.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>The boolean shows whether the module is enabled.</returns>
        public virtual bool IsEnabled(ARCoreExtensionsProjectSettings settings)
        {
            return false;
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
        /// Checking whether this module is compatible with given ARCoreExtensionsConfig.
        /// If it returns false, the preprocessbuild will throw a general Build Failure Error.
        /// A feature developer should use this function to log detailed error messages that
        /// also include a recommendation of how to resolve the issue.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <param name="sessionConfig">ARCore Extensions Config.</param>
        /// <returns>The boolean shows whether the ARCoreExtensionsProjectSettings is compatible
        /// with the ARCoreExtensionsConfig.</returns>
        public virtual bool IsCompatibleWithSessionConfig(
            ARCoreExtensionsProjectSettings settings, ARCoreExtensionsConfig sessionConfig)
        {
            return true;
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
    }
}
