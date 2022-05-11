//-----------------------------------------------------------------------
// <copyright file="GeospatialPoseApi.cs" company="Google LLC">
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

#if UNITY_IOS && !GEOSPATIAL_IOS_SUPPORT
    using EarthImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#else
    using EarthImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class GeospatialPoseApi
    {
        public static IntPtr Create(IntPtr sessionHandle)
        {
            IntPtr geospatialPoseHandle = IntPtr.Zero;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArGeospatialPose_create(sessionHandle, ref geospatialPoseHandle);
#endif
            return geospatialPoseHandle;
        }

        public static void Destroy(IntPtr geospatialPoseHandle)
        {
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArGeospatialPose_destroy(geospatialPoseHandle);
#endif
        }

        public static void GetGeospatialPose(
            IntPtr sessionHandle, IntPtr geospatialPoseHandle, ref GeospatialPose pose)
        {
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArGeospatialPose_getLatitudeLongitude(
                sessionHandle, geospatialPoseHandle, ref pose.Latitude, ref pose.Longitude);
            ExternApi.ArGeospatialPose_getAltitude(
                sessionHandle, geospatialPoseHandle, ref pose.Altitude);
            ExternApi.ArGeospatialPose_getHeading(sessionHandle, geospatialPoseHandle,
                                                  ref pose.Heading);
            ExternApi.ArGeospatialPose_getHeadingAccuracy(
                sessionHandle, geospatialPoseHandle, ref pose.HeadingAccuracy);
            ExternApi.ArGeospatialPose_getHorizontalAccuracy(
                sessionHandle, geospatialPoseHandle, ref pose.HorizontalAccuracy);
            ExternApi.ArGeospatialPose_getVerticalAccuracy(
                sessionHandle, geospatialPoseHandle, ref pose.VerticalAccuracy);

#endif
        }

        private struct ExternApi
        {
            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArGeospatialPose_create(IntPtr sessionHandle,
                ref IntPtr outGeospatialPoseHandle);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArGeospatialPose_destroy(IntPtr geospatialPoseHandle);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArGeospatialPose_getLatitudeLongitude(
                IntPtr sessionHandle, IntPtr geospatialPoseHandle, ref double outLatitudeDegrees,
                ref double outLongitudeDegrees);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArGeospatialPose_getHorizontalAccuracy(
                IntPtr sessionHandle, IntPtr geospatialPoseHandle,
                ref double outHorizontalAccuracyMeters);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArGeospatialPose_getVerticalAccuracy(
                IntPtr sessionHandle, IntPtr geospatialPoseHandle,
                ref double outVerticalAccuracyMeters);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArGeospatialPose_getAltitude(IntPtr sessionHandle,
                IntPtr geospatialPoseHandle, ref double outAltitudeMeters);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArGeospatialPose_getHeading(
                IntPtr sessionHandle, IntPtr geospatialPoseHandle, ref double outHeadingDegrees);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArGeospatialPose_getHeadingAccuracy(
                IntPtr sessionHandle, IntPtr geospatialPoseHandle,
                 ref double outHeadingAccuracyDegrees);
        }
    }
}
