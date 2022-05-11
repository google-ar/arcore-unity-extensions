//-----------------------------------------------------------------------
// <copyright file="EarthApi.cs" company="Google LLC">
//
// Copyright 2021 Google LLC
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
    using EarthImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#else
    using EarthImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class EarthApi
    {
        public static EarthState GetEarthState(IntPtr sessionHandle)
        {
            var earthState = EarthState.ErrorInternal;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            IntPtr earthHandle = SessionApi.AcquireEarth(sessionHandle);
            if (earthHandle == IntPtr.Zero)
            {
                return earthState;
            }

            ExternApi.ArEarth_getEarthState(sessionHandle, earthHandle, ref earthState);
            ExternApi.ArTrackable_release(earthHandle);
#endif
            return earthState;
        }

        public static TrackingState GetEarthTrackingState(IntPtr sessionHandle)
        {
            var trackingState = ApiTrackingState.Stopped;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            IntPtr earthHandle = SessionApi.AcquireEarth(sessionHandle);
            if (earthHandle == IntPtr.Zero)
            {
                return trackingState.ToTrackingState();
            }

            ExternApi.ArTrackable_getTrackingState(sessionHandle, earthHandle, ref trackingState);
            ExternApi.ArTrackable_release(earthHandle);
#endif
            return trackingState.ToTrackingState();
        }

        public static void TryGetCameraGeospatialPose(
            IntPtr sessionHandle, ref GeospatialPose geospatialPose)
        {
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            IntPtr earthHandle = SessionApi.AcquireEarth(sessionHandle);
            if (earthHandle == IntPtr.Zero)
            {
                return;
            }

            IntPtr geospatialPoseHandle = GeospatialPoseApi.Create(sessionHandle);
            ExternApi.ArEarth_getCameraGeospatialPose(sessionHandle, earthHandle,
                                                      geospatialPoseHandle);
            GeospatialPoseApi.GetGeospatialPose(
                sessionHandle, geospatialPoseHandle, ref geospatialPose);
            GeospatialPoseApi.Destroy(geospatialPoseHandle);
            ExternApi.ArTrackable_release(earthHandle);
#endif
        }

        public static IntPtr AddAnchor(IntPtr sessionHandle, IntPtr earthHandle,
            double latitude, double longitude, double altitude, Quaternion eunRotation)
        {
            IntPtr anchorHandle = IntPtr.Zero;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ApiQuaternion apiQuaternion = eunRotation.ToApiQuaternion();
            ApiArStatus status = ExternApi.ArEarth_acquireNewAnchor(
                sessionHandle, earthHandle, latitude, longitude, altitude,
                ref apiQuaternion, ref anchorHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to add Geospatial Anchor, status '{0}'", status);
            }
#endif
            return anchorHandle;
        }

        private struct ExternApi
        {
            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArEarth_getEarthState(
                IntPtr session, IntPtr earth, ref EarthState out_state);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArEarth_getCameraGeospatialPose(
                IntPtr sessionHandle, IntPtr earthHandle, IntPtr outGeospatialPoseHandle);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackable_getTrackingState(IntPtr sessionHandle,
                IntPtr trackableHandle, ref ApiTrackingState trackingState);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackable_release(IntPtr earthHandle);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArEarth_acquireNewAnchor(
                IntPtr session, IntPtr earth, double latitude, double longitude,
                double altitude, ref ApiQuaternion eus_quaternion_4, ref IntPtr out_anchor);
        }
  }
}
