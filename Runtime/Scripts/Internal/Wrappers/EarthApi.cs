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
                return EarthState.ErrorEarthNotReady;
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

        public static IntPtr ResolveAnchorOnTerrain(IntPtr sessionHandle, IntPtr earthHandle,
            double latitude, double longitude, double altitudeAboveTerrain, Quaternion eunRotation)
        {
            IntPtr anchorHandle = IntPtr.Zero;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ApiQuaternion apiQuaternion = eunRotation.ToApiQuaternion();
            ApiArStatus status = ExternApi.ArEarth_resolveAndAcquireNewAnchorOnTerrain(
                sessionHandle, earthHandle, latitude, longitude, altitudeAboveTerrain,
                ref apiQuaternion, ref anchorHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to add Geospatial Terrain anchor, status '{0}'",
                    status);
            }
#endif
            return anchorHandle;
        }

        public static void Convert(IntPtr sessionHandle, Pose pose,
            ref GeospatialPose geospatialPose)
        {
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            IntPtr earthHandle = SessionApi.AcquireEarth(sessionHandle);
            if (earthHandle == IntPtr.Zero)
            {
                return;
            }

            IntPtr poseHandle = PoseApi.Create(sessionHandle, pose);
            IntPtr geospatialPoseHandle = GeospatialPoseApi.Create(sessionHandle);
            ApiArStatus status = ExternApi.ArEarth_getGeospatialPose(
                sessionHandle, earthHandle, poseHandle, geospatialPoseHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to convert Pose to GeospatialPose, status '{0}'",
                    status);
            }

            GeospatialPoseApi.GetGeospatialPose(
                sessionHandle, geospatialPoseHandle, ref geospatialPose);
            GeospatialPoseApi.Destroy(geospatialPoseHandle);
            PoseApi.Destroy(poseHandle);
            ExternApi.ArTrackable_release(earthHandle);
#endif
        }

        public static void Convert(IntPtr sessionHandle, GeospatialPose geospatialPose,
            ref ApiPose apiPose)
        {
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            IntPtr earthHandle = SessionApi.AcquireEarth(sessionHandle);
            if (earthHandle == IntPtr.Zero)
            {
                return;
            }

            IntPtr poseHandle = PoseApi.Create(sessionHandle);
            ApiQuaternion apiQuaternion = geospatialPose.EunRotation.ToApiQuaternion();
            ApiArStatus status = ExternApi.ArEarth_getPose(
                sessionHandle, earthHandle, geospatialPose.Latitude, geospatialPose.Longitude,
                geospatialPose.Altitude, ref apiQuaternion, poseHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to convert GeospatialPose to Pose, status '{0}'",
                    status);
            }

            apiPose = PoseApi.ExtractPoseValue(sessionHandle, poseHandle);
            PoseApi.Destroy(poseHandle);
            ExternApi.ArTrackable_release(earthHandle);
#endif
        }

        public static IntPtr ResolveAnchorOnRooftopFuture(IntPtr sessionHandle, IntPtr earthHandle,
            double latitude, double longitude, double altitudeAboveRooftop, Quaternion eunRotation,
            IntPtr context)
        {
            ApiQuaternion apiQuaternion = eunRotation.ToApiQuaternion();
            IntPtr outFutureHandle = IntPtr.Zero;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ApiArStatus status = ExternApi.ArEarth_resolveAnchorOnRooftopAsync(sessionHandle,
                earthHandle, latitude, longitude, altitudeAboveRooftop, ref apiQuaternion, context,
                IntPtr.Zero, ref outFutureHandle);

            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to add Rooftop Anchor, status '{0}'", status);
            }
#endif
            return outFutureHandle;
        }

        public static IntPtr ResolveAnchorOnTerrainFuture(IntPtr sessionHandle, IntPtr earthHandle,
            double latitude, double longitude, double altitudeAboveTerrain, Quaternion eunRotation,
            IntPtr context)
        {
            ApiQuaternion apiQuaternion = eunRotation.ToApiQuaternion();
            IntPtr outFutureHandle = IntPtr.Zero;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ApiArStatus status = ExternApi.ArEarth_resolveAnchorOnTerrainAsync(sessionHandle,
                earthHandle, latitude, longitude, altitudeAboveTerrain, ref apiQuaternion, context,
                IntPtr.Zero, ref outFutureHandle);

            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to add Terrain Anchor, status '{0}'", status);
            }
#endif
            return outFutureHandle;
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

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArEarth_resolveAndAcquireNewAnchorOnTerrain(
                IntPtr session, IntPtr earth, double latitude, double longitude,
                double altitudeAboveTerrain, ref ApiQuaternion eus_quaternion_4,
                ref IntPtr out_anchor);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArEarth_getGeospatialPose(
                IntPtr session, IntPtr earth, IntPtr poseHandle,
                IntPtr outGeospatialPoseHandle);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArEarth_getPose(
                IntPtr session, IntPtr earth, double latitude, double longitude, double altitude,
                ref ApiQuaternion eus_quaternion_4, IntPtr outPoseHandle);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArEarth_resolveAnchorOnRooftopAsync(
                IntPtr session, IntPtr earth, double latitude, double longitude,
                double altitudeAboveRooftop, ref ApiQuaternion eus_quaternion_4, IntPtr context,
                IntPtr callback, ref IntPtr outFuture);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArEarth_resolveAnchorOnTerrainAsync(
                IntPtr session, IntPtr earth, double latitude, double longitude,
                double altitudeAboveTerrain, ref ApiQuaternion eus_quaternion_4, IntPtr context,
                IntPtr callback, ref IntPtr outFuture);
        }
  }
}
