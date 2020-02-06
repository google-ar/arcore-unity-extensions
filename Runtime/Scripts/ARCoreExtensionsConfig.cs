//-----------------------------------------------------------------------
// <copyright file="ARCoreExtensionsConfig.cs" company="Google">
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

namespace Google.XR.ARCoreExtensions
{
    using UnityEngine;

    /// <summary>
    /// Holds settings that are used to configure the ARCore Extensions.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ARCoreExtensionsConfig",
        menuName = "ARCore Extensions/ARCore Extensions Config",
        order = 1)]
    public class ARCoreExtensionsConfig : ScriptableObject
    {
        [Header("Cloud Anchors")]

        /// <summary>
        /// Toggles whether the Cloud Anchors are enabled.
        /// </summary>
        [Tooltip("Toggles whether Cloud Anchors are enabled.")]
        public bool EnableCloudAnchors = false;
    }
}
