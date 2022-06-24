//-----------------------------------------------------------------------
// <copyright file="IOSSupportManager.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using IOSImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#endif

    internal class IOSSupportManager
    {
        private const string _iosCloudServicesApiKeyPath =
            "RuntimeSettings/iOSCloudServiceApiKey";

        private static IOSSupportManager _instance;

        private bool _isEnabled = false;

        private string _iosCloudServicesApiKey = string.Empty;

        private ARCoreExtensionsConfig _cachedConfig;

        private IntPtr _sessionHandle = IntPtr.Zero;

        private IntPtr _frameHandle = IntPtr.Zero;

        private ARSession _arKitSession;

        private ARCameraManager _cameraManager;

        public static IOSSupportManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IOSSupportManager();
#if UNITY_IOS && (!UNITY_EDITOR || UNITY_INCLUDE_TESTS)
#if ARCORE_EXTENSIONS_IOS_SUPPORT
                    _instance.CreateARCoreSession();
#else
                    Debug.LogError("ARCore Extensions iOS Support is not enabled. " +
                        "To enable it, go to 'Project Settings > XR > ARCore Extensionts' " +
                        "to change the settings.");
#endif
#else
                    Debug.LogError("IOSSupportManager should only work on iOS platform.");
#endif
                }

                return _instance;
            }
        }

        public IntPtr ARCoreSessionHandle
        {
            get
            {
                return _sessionHandle;
            }
        }

        public IntPtr ARCoreFrameHandle
        {
            get
            {
                return _frameHandle;
            }
        }

        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }

        public void UpdateARSession(ARSession session)
        {
            if (session == null)
            {
                ResetARCoreSession();
            }

            _arKitSession = session;
        }

        public void UpdateCameraManager(ARCameraManager cameraManager)
        {
            if (_cameraManager == cameraManager)
            {
                return;
            }

            if (_cameraManager != null)
            {
                cameraManager.frameReceived -= OnFrameUpdate;
            }

            _cameraManager = cameraManager;
            if (_cameraManager != null)
            {
                _cameraManager.frameReceived += OnFrameUpdate;
            }
        }

        public void ResetARCoreSession()
        {
            if (_sessionHandle != IntPtr.Zero)
            {
                Debug.Log("Reset cross platform ARCoreSession.");
                if (_frameHandle != IntPtr.Zero)
                {
                    FrameApi.ReleaseFrame(_frameHandle);
                    _frameHandle = IntPtr.Zero;
                }

                ExternApi.ArSession_destroy(_sessionHandle);
                _sessionHandle = IntPtr.Zero;
            }
        }

        public void ResetInstanceAndSession()
        {
            ResetARCoreSession();
            if (_instance != null)
            {
                _instance = null;
            }
        }

        private void CreateARCoreSession()
        {
            ResetARCoreSession();

            _iosCloudServicesApiKey = RuntimeConfig.Instance == null ?
                string.Empty : RuntimeConfig.Instance.IOSCloudServicesApiKey;
            Debug.Log("Creating a cross platform ARCore session with IOS Cloud Services API Key:" +
                _iosCloudServicesApiKey);
            var status = ExternApi.ArSession_create(
                _iosCloudServicesApiKey, null, ref _sessionHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to create a cross platform ARCore session with " +
                    "error: {0}.", status);
                return;
            }
        }

        private void OnFrameUpdate(ARCameraFrameEventArgs frameEventArgs)
        {
            if (!_isEnabled)
            {
                return;
            }

            if (_sessionHandle == IntPtr.Zero)
            {
                return;
            }

            if (_frameHandle != IntPtr.Zero)
            {
                FrameApi.ReleaseFrame(_frameHandle);
                _frameHandle = IntPtr.Zero;
            }

            if (_arKitSession != null && _cameraManager != null && _arKitSession.enabled)
            {
                var cameraParams = new XRCameraParams
                {
                    zNear = _cameraManager.GetComponent<Camera>().nearClipPlane,
                    zFar = _cameraManager.GetComponent<Camera>().farClipPlane,
                    screenWidth = Screen.width,
                    screenHeight = Screen.height,
                    screenOrientation = Screen.orientation
                };

                if (!_cameraManager.subsystem.TryGetLatestFrame(
                        cameraParams, out XRCameraFrame frame))
                {
                    Debug.LogWarning("XRCamera's latest frame is not available now.");
                    return;
                }

                if (frame.timestampNs == 0 || frame.FrameHandle() == IntPtr.Zero)
                {
                    Debug.LogWarning("ARKit Plugin Frame is not ready.");
                    return;
                }

                var status = ExternApi.ArSession_updateAndAcquireArFrame(
                    _sessionHandle, frame.FrameHandle(), ref _frameHandle);
                if (status != ApiArStatus.Success)
                {
                    Debug.LogErrorFormat("Failed to update and acquire ARFrame with error: " +
                        "{0}", status);
                    return;
                }

                // Update session configuration.
                if (ARCoreExtensions._instance.ARCoreExtensionsConfig != null &&
                    !ARCoreExtensions._instance.ARCoreExtensionsConfig.Equals(_cachedConfig))
                {
                    _cachedConfig = ScriptableObject.CreateInstance<ARCoreExtensionsConfig>();
                    _cachedConfig.CopyFrom(ARCoreExtensions._instance.ARCoreExtensionsConfig);
                    ConfigApi.ConfigureSession(_sessionHandle, _cachedConfig);
                }
            }
        }

        private struct ExternApi
        {
            [IOSImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_create(
                string apiKey, string bundleIdentifier, ref IntPtr sessionHandle);

            [IOSImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_updateAndAcquireArFrame(
                IntPtr sessionHandle, IntPtr arkitFrameHandle, ref IntPtr arFrame);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_destroy(IntPtr session);
        }
    }
}
