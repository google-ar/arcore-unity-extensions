//-----------------------------------------------------------------------
// <copyright file="StreetscapeGeometryApi.cs" company="Google LLC">
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
    using UnityEngine.XR.ARSubsystems;

#if UNITY_IOS && !GEOSPATIAL_IOS_SUPPORT
    using FacadeImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#else
    using FacadeImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class StreetscapeGeometryApi
    {
        public static ApiPose GetPose(IntPtr sessionHandle, IntPtr streetscapeGeometryHandle)
        {
            ApiPose apiPose = Pose.identity.ToApiPose();
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            IntPtr poseHandle = PoseApi.Create(sessionHandle);
            ExternApi.ArStreetscapeGeometry_getMeshPose(sessionHandle, streetscapeGeometryHandle,
                poseHandle);
            apiPose = PoseApi.ExtractPoseValue(sessionHandle, poseHandle);
            PoseApi.Destroy(poseHandle);
#endif
            return apiPose;
        }

        public static Mesh AcquireMesh(IntPtr sessionHandle, IntPtr meshHandle)
        {
            return MeshApi.AcquireMesh(sessionHandle, meshHandle);
        }

        public static StreetscapeGeometryType GetStreetscapeGeometryType(IntPtr sessionHandle,
            IntPtr streetscapeGeometryHandle)
        {
            ApiStreetscapeGeometryType outstreetscapeGeometryType =
                ApiStreetscapeGeometryType.Terrain;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArStreetscapeGeometry_getType(sessionHandle, streetscapeGeometryHandle,
                ref outstreetscapeGeometryType);
#endif
            return outstreetscapeGeometryType.ToStreetscapeGeometryType();
        }

        public static TrackingState GetTrackingState(IntPtr sessionHandle,
            IntPtr streetscapeGeometryHandle)
        {
            if (sessionHandle == IntPtr.Zero || streetscapeGeometryHandle == IntPtr.Zero)
            {
                return TrackingState.None;
            }

            ApiTrackingState trackingState = ApiTrackingState.Stopped;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArTrackable_getTrackingState(sessionHandle, streetscapeGeometryHandle,
                ref trackingState);
#endif
            return trackingState.ToTrackingState();
        }

        public static IntPtr AcquireMeshHandle(IntPtr sessionHandle,
            IntPtr streetscapeGeometryHandle)
        {
            var meshHandle = IntPtr.Zero;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArStreetscapeGeometry_acquireMesh(sessionHandle, streetscapeGeometryHandle,
                ref meshHandle);
#endif
            return meshHandle;
        }

        public static StreetscapeGeometryQuality GetStreetscapeGeometryQuality(
            IntPtr sessionHandle, IntPtr streetscapeGeometryHandle)
        {
            ApiStreetscapeGeometryQuality apiQuality = ApiStreetscapeGeometryQuality.None;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArStreetscapeGeometry_getQuality(sessionHandle,
                streetscapeGeometryHandle, ref apiQuality);
#endif
            return apiQuality.ToStreetscapeGeometryQuality();
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [FacadeImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArStreetscapeGeometry_getMeshPose(IntPtr sessionHandle,
                IntPtr streetscapeGeometryHandle, IntPtr outPoseHandle);

            [FacadeImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArStreetscapeGeometry_getType(IntPtr sessionHandle,
                IntPtr streetscapeGeometryHandle,
                ref ApiStreetscapeGeometryType outstreetscapeGeometryType);

            [FacadeImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArStreetscapeGeometry_acquireMesh(IntPtr sessionHandle,
                IntPtr streetscapeGeometryHandle, ref IntPtr outArMeshHandle);

            [FacadeImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackable_getTrackingState(
                IntPtr sessionHandle, IntPtr trackableHandle, ref ApiTrackingState trackingState);

            [FacadeImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArStreetscapeGeometry_getQuality(
                IntPtr sessionHandle, IntPtr facadeHandle,
                ref ApiStreetscapeGeometryQuality outQuality);
#pragma warning restore 626
        }
    }
}
