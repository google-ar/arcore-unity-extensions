//-----------------------------------------------------------------------
// <copyright file="CameraConfigApi.cs" company="Google">
//
// Copyright 2020 Google LLC. All Rights Reserved.
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
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class CameraConfigApi
    {
        public static Vector2Int GetTextureDimensions(
            IntPtr sessionHandle, IntPtr cameraConfigHandle)
        {
            int width = 0;
            int height = 0;
            ExternApi.ArCameraConfig_getTextureDimensions(
                sessionHandle, cameraConfigHandle, ref width, ref height);
            return new Vector2Int(width, height);
        }

        public static Vector2Int GetFPSRange(
            IntPtr sessionHandle, IntPtr cameraConfigHandle)
        {
            int minFps = 0;
            int maxFps = 0;
            ExternApi.ArCameraConfig_getFpsRange(
                sessionHandle, cameraConfigHandle, ref minFps, ref maxFps);
            return new Vector2Int(minFps, maxFps);
        }

        public static CameraConfigDepthSensorUsages GetDepthSensorUsages(
            IntPtr sessionHandle, IntPtr cameraConfigHandle)
        {
            int depth = (int)CameraConfigDepthSensorUsages.DoNotUse;
            ExternApi.ArCameraConfig_getDepthSensorUsage(
                sessionHandle, cameraConfigHandle, ref depth);
            return (CameraConfigDepthSensorUsages)depth;
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfig_getTextureDimensions(IntPtr sessionHandle,
                IntPtr cameraConfigHandle, ref int width, ref int height);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfig_getFpsRange(
                IntPtr sessionHandle, IntPtr cameraConfigHandle, ref int minFps, ref int maxFps);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfig_getDepthSensorUsage(
                IntPtr sessionHandle, IntPtr cameraConfigHandle, ref int depthSensorUsage);
#pragma warning restore 626
        }
    }
}
