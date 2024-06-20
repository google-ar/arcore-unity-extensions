//-----------------------------------------------------------------------
// <copyright file="CustomizedAndroidProjectFilesModifier.cs" company="Google LLC">
//
// Copyright 2024 Google LLC
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

#if UNITY_2023_1
namespace Google.XR.ARCoreExtensions.Editor.Internal
{
#if UNITY_ANDROID
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEditor.Android;
    using UnityEngine;
    using Unity.Android.Gradle.Manifest;

    /// <summary>
    /// Inject customized AndroidManifest configurations via Unity's AndroidProjectFilesModifier
    /// Interface.
    /// That interface is added since Unity 2023.1, so this class should only be enabled
    /// for Unity 2023 or newer.
    /// For more details, please refer to
    /// https://docs.unity3d.com/2023.1/Documentation/ScriptReference/Android.AndroidProjectFilesModifier.html
    /// </summary>
    public class CustomizedAndroidProjectFilesModifier : AndroidProjectFilesModifier
    {

        /// <summary>
        /// A callback for modifying files in the Android Gradle project after Unity editor creates
        /// it.
        /// </summary>
        /// <param name="projectFiles">Object which contains C# representations of Android Gradle
        /// project files.</param>
        public override void OnModifyAndroidProjectFiles(AndroidProjectFiles projectFiles)
        {
            ARCoreExtensionsProjectSettings settings = ARCoreExtensionsProjectSettings.Instance;

            List<IDependentModule> featureModules = DependentModulesManager.GetModules();
            foreach (IDependentModule module in featureModules)
            {
                if (module.IsEnabled(settings, UnityEditor.BuildTarget.Android))
                {
                    module.ModifyAndroidManifest(settings, projectFiles.UnityLibraryManifest.Manifest);
                }
            }
        }
    }
#endif // UNITY_ANDROID
}
#endif // UNITY_2023_1
