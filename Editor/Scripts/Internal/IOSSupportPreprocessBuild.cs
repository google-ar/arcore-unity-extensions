//-----------------------------------------------------------------------
// <copyright file="IOSSupportPreprocessBuild.cs" company="Google">
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
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    internal class IOSSupportPreprocessBuild : IPreprocessBuildWithReport
    {
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
            if (report.summary.platform == BuildTarget.iOS)
            {
                bool arcoreiOSEnabled =
                    ARCoreExtensionsProjectSettings.Instance.IsIOSSupportEnabled;
                Debug.LogFormat("Building application with ARCore Extensions for AR Foundation " +
                    "iOS Support {0}", arcoreiOSEnabled ? "ENABLED" : "DISABLED");

                IOSSupportHelper.SetARCoreIOSSupportEnabled(arcoreiOSEnabled);
            }
        }
    }
}
