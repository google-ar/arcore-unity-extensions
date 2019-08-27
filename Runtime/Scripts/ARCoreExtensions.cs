//-----------------------------------------------------------------------
// <copyright file="ARCoreExtensions.cs" company="Google">
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

namespace Google.XR.ARCoreExtensions
{
    using System;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// ARCore Extensions, this script allows an app to specify and provide access to
    /// AR Foundation object instances that should be used by ARCore Extensions.
    /// </summary>
    public class ARCoreExtensions : MonoBehaviour
    {
        /// <summary>
        /// AR Foundation <c>ARSession</c> used by the scene.
        /// </summary>
        public ARSession Session;

        /// <summary>
        /// AR Foundation <c>ARSessionOrigin</c> used by the scene.
        /// </summary>
        public ARSessionOrigin SessionOrigin;

        /// <summary>
        /// Supplementary configuration to define features and options for the
        /// ARCore Extensions.
        /// </summary>
        public ARCoreExtensionsConfig ARCoreExtensionsConfig;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        public void Awake()
        {
            if (Instance)
            {
                Debug.LogError("ARCore Extensions is already initialized. You many only " +
                    "have one instance in your scene at a time.");
            }
            Instance = this;
        }

        /// <summary>
        /// Unity's OnDestroy method.
        /// </summary>
        public void OnDestroy()
        {
            if (Instance)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Unity's Update method.
        /// </summary>
        public void Update()
        {
            // Ensure Cloud Anchors are enabled or disabled as requested.
            IntPtr sessionHandle = Session.SessionHandle();
            if (sessionHandle != IntPtr.Zero)
            {
                SessionApi.EnableCloudAnchors(
                    sessionHandle,
                    ARCoreExtensionsConfig.EnableCloudAnchors);
            }
        }

        internal static ARCoreExtensions Instance { get; private set; }

        internal IntPtr CurrentARCoreSessionHandle
        {
            get
            {
                if (Instance == null || Instance.Session == null)
                {
                    Debug.LogError("ARCore Extensions not found or not configured. Please " +
                        "include an ARCore Extensions game object in your scene. " +
                        "GameObject -> XR -> ARCore Extensions");
                    return IntPtr.Zero;
                }

                return Instance.Session.SessionHandle();
            }
        }
    }
}
