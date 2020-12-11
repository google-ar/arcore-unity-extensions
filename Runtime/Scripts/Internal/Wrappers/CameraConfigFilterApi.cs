//-----------------------------------------------------------------------
// <copyright file="CameraConfigFilterApi.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.Internal
{
    using System;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class CameraConfigFilterApi
    {
        public static void UpdateFilter(IntPtr sessionHandle, IntPtr filterHandle,
            ARCoreExtensionsCameraConfigFilter extensionsFilter)
        {
            if (extensionsFilter != null)
            {
                ExternApi.ArCameraConfigFilter_setTargetFps(
                    sessionHandle, filterHandle, (int)extensionsFilter.TargetCameraFramerate);
                ExternApi.ArCameraConfigFilter_setDepthSensorUsage(
                    sessionHandle, filterHandle, (int)extensionsFilter.DepthSensorUsage);
                ExternApi.ArCameraConfigFilter_setStereoCameraUsage(
                    sessionHandle, filterHandle, (int)extensionsFilter.StereoCameraUsage);
            }
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigFilter_setTargetFps(IntPtr sessionHandle,
                IntPtr cameraConfigFilterHandle, int fpsFilter);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigFilter_setDepthSensorUsage(IntPtr sessionHandle,
                IntPtr cameraConfigFilterHandle, int depthFilter);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigFilter_setStereoCameraUsage(
                IntPtr sessionHandle, IntPtr cameraConfigFilterHandle, int stereoFilter);
#pragma warning restore 626
        }
    }
}
