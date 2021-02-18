//-----------------------------------------------------------------------
// <copyright file="KeylessModule.cs" company="Google LLC">
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
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using UnityEngine;

    /// <summary>
    /// The implemented class of keyless module.
    /// </summary>
    public class KeylessModule : DependentModuleBase
    {
        /// <summary>
        /// Checking whether it needs to be included in the customized AndroidManifest.
        /// The default values for new fields in ARCoreProjectSettings should cause the
        /// associated module to return false.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>The boolean shows whether the module is enabled.</returns>
        public override bool IsEnabled(ARCoreExtensionsProjectSettings settings)
        {
            return settings.AndroidAuthenticationStrategySetting ==
                AndroidAuthenticationStrategy.Keyless;
        }

        /// <summary>
        /// Return the Proguard to include if this module is enabled. The string output will be
        /// added into "proguard-user.txt" directly.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>The proguard rule string snippet.</returns>
        public override string GetProguardSnippet(ARCoreExtensionsProjectSettings settings)
        {
            return @"-keep class com.google.android.gms.common.** { *; }
                    -keep class com.google.android.gms.auth.** { *; }
                    -keep class com.google.android.gms.tasks.** { *; }";
        }
    }
}
