//-----------------------------------------------------------------------
// <copyright file="IOSSupportManager.cs" company="Google">
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

namespace Google.XR.ARCoreExtensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    internal class IOSSupportManager
    {
        private const string k_IOSCloudServicesApiKeyPath =
            "RuntimeSettings/iOSCloudServiceApiKey";

        private static IOSSupportManager s_Instance;

        private bool m_IsEnabled = false;

        private string m_IOSCloudServicesApiKey = string.Empty;

        private IntPtr m_SessionHandle = IntPtr.Zero;

        private IntPtr m_FrameHandle = IntPtr.Zero;

        private ARSession m_ARKitSession;

        private ARCameraManager m_CameraManager;

        public static IOSSupportManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new IOSSupportManager();
#if UNITY_IOS && (!UNITY_EDITOR || UNITY_INCLUDE_TESTS)
#if ARCORE_EXTENSIONS_IOS_SUPPORT
                    s_Instance._CreateARCoreSession();
#else
                    Debug.LogError("ARCore Extensions iOS Support is not enabled. " +
                        "To enable it, go to 'Project Settings > XR > ARCore Extensionts' " +
                        "to change the settings.");
#endif
#else
                    Debug.LogError("IOSSupportManager should only work on iOS platform.");
#endif
                }

                return s_Instance;
            }
        }

        public IntPtr ARCoreSessionHandle
        {
            get
            {
                return m_SessionHandle;
            }
        }

        public void SetEnabled(bool enabled)
        {
            m_IsEnabled = enabled;
        }

        public void UpdateARSession(ARSession session)
        {
            if (session == null)
            {
                ResetARCoreSession();
            }

            m_ARKitSession = session;
        }

        public void UpdateCameraManager(ARCameraManager cameraManager)
        {
            if (m_CameraManager == cameraManager)
            {
                return;
            }

            if (m_CameraManager != null)
            {
                cameraManager.frameReceived -= _OnFrameUpdate;
            }

            m_CameraManager = cameraManager;
            m_CameraManager.frameReceived += _OnFrameUpdate;
        }

        public void ResetARCoreSession()
        {
            if (m_SessionHandle != IntPtr.Zero)
            {
                Debug.Log("Reset cross platform ARCoreSession.");
                if (m_FrameHandle != IntPtr.Zero)
                {
                    SessionApi.ReleaseFrame(m_FrameHandle);
                    m_FrameHandle = IntPtr.Zero;
                }

                ExternApi.ArSession_destroy(m_SessionHandle);
                m_SessionHandle = IntPtr.Zero;
            }
        }

        public void ResetInstanceAndSession()
        {
            ResetARCoreSession();
            if (s_Instance != null)
            {
                s_Instance = null;
            }
        }

        private void _CreateARCoreSession()
        {
            ResetARCoreSession();

            m_IOSCloudServicesApiKey = RuntimeConfig.Instance == null ?
                string.Empty : RuntimeConfig.Instance.IOSCloudServicesApiKey;
            Debug.Log("Creating a cross platform ARCore session with IOS Cloud Services API Key:" +
                m_IOSCloudServicesApiKey);
            var status = ExternApi.ArSession_create(
                m_IOSCloudServicesApiKey, null, ref m_SessionHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to create a cross platform ARCore session with " +
                    "error: {0}.", status);
                return;
            }
        }

        private void _OnFrameUpdate(ARCameraFrameEventArgs frameEventArgs)
        {
            if (!_ShouldUpdateARCoreSession())
            {
                return;
            }

            if (m_SessionHandle == IntPtr.Zero)
            {
                return;
            }

            if (m_FrameHandle != IntPtr.Zero)
            {
                SessionApi.ReleaseFrame(m_FrameHandle);
                m_FrameHandle = IntPtr.Zero;
            }

            if (m_ARKitSession != null && m_CameraManager != null && m_ARKitSession.enabled)
            {
                var cameraParams = new XRCameraParams
                {
                zNear = m_CameraManager.GetComponent<Camera>().nearClipPlane,
                zFar = m_CameraManager.GetComponent<Camera>().farClipPlane,
                screenWidth = Screen.width,
                screenHeight = Screen.height,
                screenOrientation = Screen.orientation
                };

                if (!m_CameraManager.subsystem.TryGetLatestFrame(
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
                    m_SessionHandle, frame.FrameHandle(), ref m_FrameHandle);
                if (status != ApiArStatus.Success)
                {
                    Debug.LogErrorFormat("Failed to update and acquire ARFrame with error: " +
                        "{0}", status);
                }
            }
        }

        private bool _ShouldUpdateARCoreSession()
        {
            return m_IsEnabled &&
                ARCoreExtensions.Instance.ARCoreExtensionsConfig.EnableCloudAnchors;
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_create(
                string apiKey, string bundleIdentifier, ref IntPtr sessionHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_destroy(IntPtr session);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_updateAndAcquireArFrame(
                IntPtr sessionHandle, IntPtr arkitFrameHandle, ref IntPtr arFrame);
        }
    }
}
