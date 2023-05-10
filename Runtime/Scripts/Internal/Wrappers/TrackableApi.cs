//-----------------------------------------------------------------------
// <copyright file="TrackableApi.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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
    using Google.XR.ARCoreExtensions;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

#if UNITY_IOS && !GEOSPATIAL_IOS_SUPPORT
    using GeospatialImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#else
    using GeospatialImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class TrackableApi
    {
        public static TrackingState GetTrackingState(IntPtr sessionHandle, IntPtr trackableHandle)
        {
            ApiTrackingState apiTrackingState = ApiTrackingState.Stopped;
            ExternApi.ArTrackable_getTrackingState(sessionHandle, trackableHandle,
                ref apiTrackingState);
            return apiTrackingState.ToTrackingState();
        }

        public static void Release(IntPtr trackableHandle)
        {
            ExternApi.ArTrackable_release(trackableHandle);
        }

        public static bool AcquireNewAnchor(IntPtr sessionHandle, IntPtr trackableHandle, Pose pose,
            out IntPtr anchorHandle)
        {
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            IntPtr poseHandle = PoseApi.Create(sessionHandle, pose);

            anchorHandle = IntPtr.Zero;
            int status = ExternApi.ArTrackable_acquireNewAnchor(
                sessionHandle, trackableHandle, poseHandle, ref anchorHandle);

            PoseApi.Destroy(poseHandle);
            return status == 0;
#else
            anchorHandle = IntPtr.Zero;
            return false;
#endif
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackable_getTrackingState(
                IntPtr sessionHandle, IntPtr trackableHandle, ref ApiTrackingState trackingState);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackable_release(IntPtr trackableHandle);

            [GeospatialImport(ApiConstants.ARCoreNativeApi)]
            public static extern int ArTrackable_acquireNewAnchor(
                IntPtr sessionHandle, IntPtr trackableHandle, IntPtr poseHandle,
                ref IntPtr anchorHandle);
        }
    }
}
