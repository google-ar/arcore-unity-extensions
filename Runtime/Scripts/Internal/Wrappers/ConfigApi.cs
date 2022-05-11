//-----------------------------------------------------------------------
// <copyright file="ConfigApi.cs" company="Google LLC">
//
// Copyright 2022 Google LLC
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

#if UNITY_IOS
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#if CLOUDANCHOR_IOS_SUPPORT
    using CloudAnchorImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using CloudAnchorImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#endif
#if GEOSPATIAL_IOS_SUPPORT
    using EarthImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using EarthImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#endif
#else // UNITY_ANDROID
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using CloudAnchorImport = System.Runtime.InteropServices.DllImportAttribute;
    using EarthImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class ConfigApi
    {
        public static void ConfigureSession(IntPtr sessionHandle, ARCoreExtensionsConfig config)
        {
            IntPtr configHandle = IntPtr.Zero;
            ExternApi.ArConfig_create(sessionHandle, ref configHandle);
            UpdateSessionConfig(sessionHandle, configHandle, config);
            ApiArStatus status = ExternApi.ArSession_configure(sessionHandle, configHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to configure the session with error: {0}.", status);
            }

            ExternApi.ArConfig_destroy(configHandle);
        }

        public static void UpdateSessionConfig(
            IntPtr sessionHandle, IntPtr configHandle, ARCoreExtensionsConfig config)
        {
#if !UNITY_IOS || CLOUDANCHOR_IOS_SUPPORT
            ApiCloudAnchorMode cloudAnchorMode = (ApiCloudAnchorMode)config.CloudAnchorMode;
            ExternApi.ArConfig_setCloudAnchorMode(sessionHandle, configHandle, cloudAnchorMode);
#endif // CLOUDANCHORS

#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ApiGeospatialMode geospatialMode = config.GeospatialMode.ToApiGeospatialMode();
            ExternApi.ArConfig_setGeospatialMode(sessionHandle, configHandle, geospatialMode);
#endif // EARTH
#if UNITY_ANDROID
#endif // UNITY_ANDROID
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_create(
                IntPtr sessionHandle, ref IntPtr configHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_destroy(IntPtr configHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_configure(
                IntPtr sessionHandle, IntPtr configHandle);

            [CloudAnchorImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setCloudAnchorMode(
                IntPtr sessionHandle, IntPtr configHandle, ApiCloudAnchorMode mode);

            [CloudAnchorImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_getCloudAnchorMode(
                IntPtr sessionHandle, IntPtr configHandle, ref ApiCloudAnchorMode mode);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setGeospatialMode(IntPtr sessionHandle,
                                                                 IntPtr config,
                                                                 ApiGeospatialMode mode);
#if UNITY_ANDROID
#endif // UNITY_ANDROID
        }
    }
}
