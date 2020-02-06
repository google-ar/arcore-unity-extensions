//-----------------------------------------------------------------------
// <copyright file="AnchorApi.cs" company="Google">
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

    internal class AnchorApi
    {
        public static string GetCloudAnchorId(
            IntPtr sessionHandle,
            IntPtr anchorHandle)
        {
            IntPtr stringHandle = IntPtr.Zero;
            ExternApi.ArAnchor_acquireCloudAnchorId(
                sessionHandle, anchorHandle, ref stringHandle);

            string cloudAnchorId = Marshal.PtrToStringAnsi(stringHandle);

            ExternApi.ArString_release(stringHandle);

            return cloudAnchorId;
        }

        public static ApiCloudAnchorState GetCloudAnchorState(
            IntPtr sessionHandle,
            IntPtr anchorHandle)
        {
            ApiCloudAnchorState cloudAnchorState = ApiCloudAnchorState.None;
            ExternApi.ArAnchor_getCloudAnchorState(
                sessionHandle,
                anchorHandle,
                ref cloudAnchorState);
            return cloudAnchorState;
        }

        public static ApiPose GetAnchorPose(
            IntPtr sessionHandle,
            IntPtr anchorHandle)
        {
            IntPtr poseHandle = PoseApi.Create(sessionHandle);
            ExternApi.ArAnchor_getPose(sessionHandle, anchorHandle, poseHandle);
            ApiPose apiPose = PoseApi.ExtractPoseValue(sessionHandle, poseHandle);
            PoseApi.Destroy(poseHandle);
            return apiPose;
        }

        public static ApiTrackingState GetTrackingState(
            IntPtr sessionHandle,
            IntPtr anchorHandle)
        {
            ApiTrackingState apiTrackingState = ApiTrackingState.Stopped;
            ExternApi.ArAnchor_getTrackingState(sessionHandle, anchorHandle, ref apiTrackingState);
            return apiTrackingState;
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_acquireCloudAnchorId(
                IntPtr sessionHandle,
                IntPtr anchorHandle,
                ref IntPtr hostingHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_getCloudAnchorState(
                IntPtr sessionHandle,
                IntPtr anchorHandle,
                ref ApiCloudAnchorState cloudAnchorState);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArString_release(IntPtr stringHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_getPose(
                IntPtr sessionHandle,
                IntPtr anchorHandle,
                IntPtr pose);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_getTrackingState(
                IntPtr sessionHandle,
                IntPtr anchorHandle,
                ref ApiTrackingState trackingState);
        }
    }
}
