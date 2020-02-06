//-----------------------------------------------------------------------
// <copyright file="SessionApi.cs" company="Google">
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
    using System.Runtime.InteropServices;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#endif

    internal class SessionApi
    {
        private static IntPtr s_ArConfig = IntPtr.Zero;

        public static void ReleaseFrame(IntPtr frameHandle)
        {
            ExternApi.ArFrame_release(frameHandle);
        }

        public static IntPtr HostCloudAnchor(
            IntPtr sessionHandle,
            IntPtr anchorHandle)
        {
            IntPtr cloudAnchorHandle = IntPtr.Zero;
            ApiArStatus status = ExternApi.ArSession_hostAndAcquireNewCloudAnchor(
                sessionHandle,
                anchorHandle,
                ref cloudAnchorHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to host a new Cloud Anchor, status '{0}'", status);
            }

            return cloudAnchorHandle;
        }

        public static IntPtr ResolveCloudAnchor(
            IntPtr sessionHandle,
            string cloudAnchorId)
        {
            IntPtr cloudAnchorHandle = IntPtr.Zero;
            ApiArStatus status = ExternApi.ArSession_resolveAndAcquireNewCloudAnchor(
                sessionHandle,
                cloudAnchorId,
                ref cloudAnchorHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to resolve a new Cloud Anchor, status '{0}'", status);
            }

            return cloudAnchorHandle;
        }

        public static void EnableCloudAnchors(
            IntPtr sessionHandle,
            bool enabled)
        {
            // Get the config we're using for updates.
            IntPtr configHandle = _GetArConfig(sessionHandle);
            if (configHandle == IntPtr.Zero)
            {
                return;
            }

            // Get the current configuration.
            ExternApi.ArSession_getConfig(sessionHandle, configHandle);

            // Check the current mode to see if an update is needed.
            ApiCloudAnchorMode currentCloudAnchorMode = ApiCloudAnchorMode.Disabled;
            ExternApi.ArConfig_getCloudAnchorMode(
                sessionHandle,
                configHandle,
                ref currentCloudAnchorMode);

            // Update the configuration if needed.
            ApiCloudAnchorMode newCloudAnchorMode =
                enabled ? ApiCloudAnchorMode.Enabled : ApiCloudAnchorMode.Disabled;
            if (currentCloudAnchorMode != newCloudAnchorMode)
            {
                // Set the new Cloud Anchor mode.
                ExternApi.ArConfig_setCloudAnchorMode(
                    sessionHandle,
                    configHandle,
                    enabled ? ApiCloudAnchorMode.Enabled : ApiCloudAnchorMode.Disabled);

                // Configure the session.
                ApiArStatus status = ExternApi.ArSession_configure(
                    sessionHandle, configHandle);
                if (status != ApiArStatus.Success)
                {
                    Debug.LogErrorFormat("Unable to set ARCore configuration for Cloud Anchors, " +
                        "status '{0}'", status);
                }
            }
        }

        private static IntPtr _GetArConfig(IntPtr sessionHandle)
        {
            if (s_ArConfig == IntPtr.Zero)
            {
                ExternApi.ArConfig_create(sessionHandle, ref s_ArConfig);
                if (s_ArConfig == IntPtr.Zero)
                {
                    Debug.LogError("Unable to create ARCore configuration");
                }
            }

            return s_ArConfig;
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_release(IntPtr frameHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_hostAndAcquireNewCloudAnchor(
                IntPtr sessionHandle,
                IntPtr anchorHandle,
                ref IntPtr cloudAnchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_resolveAndAcquireNewCloudAnchor(
                IntPtr sessionHandle,
                string cloudAnchorId,
                ref IntPtr cloudAnchorHandle);

#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getConfig(
                IntPtr sessionHandle,
                IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_configure(
                IntPtr sessionHandle,
                IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_create(
                IntPtr sessionHandle,
                ref IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_destroy(
                IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setCloudAnchorMode(
                IntPtr sessionHandle,
                IntPtr configHandle,
                ApiCloudAnchorMode mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_getCloudAnchorMode(
                IntPtr sessionHandle,
                IntPtr configHandle,
                ref ApiCloudAnchorMode mode);
#pragma warning restore 626
        }
    }
}
