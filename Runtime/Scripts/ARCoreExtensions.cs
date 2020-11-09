//-----------------------------------------------------------------------
// <copyright file="ARCoreExtensions.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    using System.Collections.Generic;
    using System.Linq;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;

#if UNITY_ANDROID
    using UnityEngine.XR.ARCore;
#endif // UNITY_ANDROID
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
        /// AR Foundation <c>ARCameraManager</c> used in the ARSessionOrigin.
        /// </summary>
        public ARCameraManager CameraManager;

        /// <summary>
        /// Supplementary configuration to define features and options for the
        /// ARCore Extensions.
        /// </summary>
        public ARCoreExtensionsConfig ARCoreExtensionsConfig;

#if UNITY_ANDROID
        private string _currentPermissionRequest = null;

        private HashSet<string> _requiredPermissionNames = new HashSet<string>();

#endif
        internal static ARCoreExtensions _instance { get; private set; }

        internal IntPtr currentARCoreSessionHandle
        {
            get
            {
                if (_instance == null || _instance.Session == null)
                {
                    Debug.LogError("ARCore Extensions not found or not configured. Please " +
                        "include an ARCore Extensions game object in your scene. " +
                        "GameObject -> XR -> ARCore Extensions");
                    return IntPtr.Zero;
                }

#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
                return IOSSupportManager.Instance.ARCoreSessionHandle;
#else
                return _instance.Session.SessionHandle();
#endif
            }
        }

#if UNITY_ANDROID
        private ARCoreSessionSubsystem _arCoreSubsystem;

        private ARCoreExtensionsConfig _cachedConfig = null;
#endif

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        public void Awake()
        {
            if (_instance)
            {
                Debug.LogError("ARCore Extensions is already initialized. You may only " +
                    "have one instance in your scene at a time.");
            }

            _instance = this;
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        public void Start()
        {
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            IOSSupportManager.Instance.UpdateARSession(Session);
            IOSSupportManager.Instance.UpdateCameraManager(CameraManager);
#endif
        }

        /// <summary>
        /// Unity's OnEnable method.
        /// </summary>
        public void OnEnable()
        {
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            IOSSupportManager.Instance.SetEnabled(true);
#endif // UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
#if UNITY_ANDROID
            if (_instance.Session == null)
            {
                Debug.LogError("ARSession is required by ARCoreExtensions!");
                return;
            }

            _arCoreSubsystem = (ARCoreSessionSubsystem)Session.subsystem;
            if (_arCoreSubsystem == null)
            {
                Debug.LogError(
                    "No active ARCoreSessionSubsystem is available in this session, Please " +
                    "ensure that a valid loader configuration exists in the XR project settings.");
            }
            else
            {
                _arCoreSubsystem.beforeSetConfiguration += BeforeConfigurationChanged;
            }
#endif // UNITY_ANDROID

            CachedData.Reset();
        }

        /// <summary>
        /// Unity's OnDisable method.
        /// </summary>
        public void OnDisable()
        {
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            IOSSupportManager.Instance.SetEnabled(false);
#endif // UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
#if UNITY_ANDROID
            if (_arCoreSubsystem != null)
            {
                _arCoreSubsystem.beforeSetConfiguration -= BeforeConfigurationChanged;
            }
#endif // UNITY_ANDROID

            CachedData.Reset();
        }

        /// <summary>
        /// Unity's OnDestroy method.
        /// </summary>
        public void OnDestroy()
        {
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            IOSSupportManager.Instance.ResetInstanceAndSession();
#endif

            if (_instance)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Unity's Update method.
        /// </summary>
        public void Update()
        {
#if UNITY_ANDROID
            if (_requiredPermissionNames.Count > 0)
            {
                RequestPermission();
            }

            // Update ARCore session configuration.
            if (Session.SessionHandle() != IntPtr.Zero && ARCoreExtensionsConfig != null)
            {
                if (_cachedConfig != null && _cachedConfig.Equals(ARCoreExtensionsConfig))
                {
                    return;
                }

                _cachedConfig = ScriptableObject.CreateInstance<ARCoreExtensionsConfig>();
                _cachedConfig.CopyFrom(ARCoreExtensionsConfig);

                if (_requiredPermissionNames.Count > 0)
                {
                    RequestPermission();
                    return;
                }

                _arCoreSubsystem.SetConfigurationDirty();
            }
#endif
        }
#if UNITY_ANDROID

        /// <summary>
        /// Unity OnValidate.
        /// </summary>
        public void OnValidate()
        {
        }

        private void RequestPermission()
        {
            // All required permissions are granted.
            if (_requiredPermissionNames.Count == 0)
            {
                return;
            }

            // Waiting for current request.
            if (!AndroidPermissionsManager.IsPermissionGranted(
                AndroidPermissionsManager._cameraPermission) ||
                !string.IsNullOrEmpty(_currentPermissionRequest))
            {
                return;
            }

            _currentPermissionRequest = _requiredPermissionNames.First();
            AndroidPermissionsManager.RequestPermission(
                _currentPermissionRequest, OnPermissionRequestFinish);
        }

        private void OnPermissionRequestFinish(bool isGranted)
        {
            if (_currentPermissionRequest == null)
            {
                Debug.LogWarning("Received unexpected permission request result.");
                return;
            }

            Debug.LogFormat("{0} {1}.",
                isGranted ? "Granted" : "Denied", _currentPermissionRequest);
            _requiredPermissionNames.Remove(_currentPermissionRequest);
            _currentPermissionRequest = null;
            _arCoreSubsystem.SetConfigurationDirty();
        }

        private void BeforeConfigurationChanged(ARCoreBeforeSetConfigurationEventArgs eventArgs)
        {
            if (_cachedConfig == null)
            {
                return;
            }

            if (eventArgs.session != IntPtr.Zero && eventArgs.config != IntPtr.Zero)
            {
                SessionApi.UpdateSessionConfig(eventArgs.session, eventArgs.config, _cachedConfig);
            }
        }
#endif
    }
}
