//-----------------------------------------------------------------------
// <copyright file="FutureApi.cs" company="Google LLC">
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

#if UNITY_IOS && !CLOUDANCHOR_IOS_SUPPORT
    using CloudAnchorImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#else
    using CloudAnchorImport = System.Runtime.InteropServices.DllImportAttribute;
#endif
#if UNITY_IOS && !GEOSPATIAL_IOS_SUPPORT
    using EarthImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#else
    using EarthImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class FutureApi
    {
        public static IntPtr CheckVpsAvailabilityAsync(IntPtr sessionHandle, double latitude,
            double longitude)
        {
            IntPtr futureHandle = IntPtr.Zero;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ApiArStatus status = ExternApi.ArSession_checkVpsAvailabilityAsync(
                sessionHandle, latitude, longitude, IntPtr.Zero, IntPtr.Zero, ref futureHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to get the availability of VPS, status '{0}'", status);
            }
#endif
            return futureHandle;
        }

        public static PromiseState GetState(IntPtr sessionHandle, IntPtr futureHandle)
        {
            var state = PromiseState.Pending;
            ExternApi.ArFuture_getState(sessionHandle, futureHandle, ref state);
            return state;
        }

        public static VpsAvailability GetVpsAvailabilityResult(IntPtr sessionHandle,
            IntPtr futureHandle)
        {
            var result = VpsAvailability.Unknown;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArVpsAvailabilityFuture_getResult(sessionHandle, futureHandle, ref result);
#endif
            return result;
        }

        public static RooftopAnchorState GetRooftopAnchorState(IntPtr sessionHandle,
            IntPtr futureHandle)
        {
            ApiRooftopAnchorState outApiRooftopAnchorState = ApiRooftopAnchorState.None;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArResolveAnchorOnRooftopFuture_getResultRooftopAnchorState(sessionHandle,
                futureHandle, ref outApiRooftopAnchorState);
#endif
            return outApiRooftopAnchorState.ToRooftopAnchorState();
        }

        public static IntPtr GetRooftopAnchorHandle(IntPtr sessionHandle, IntPtr futureHandle)
        {
            IntPtr outAnchorHandle = IntPtr.Zero;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArResolveAnchorOnRooftopFuture_acquireResultAnchor(sessionHandle,
                futureHandle, ref outAnchorHandle);
#endif
            return outAnchorHandle;
        }

        public static TerrainAnchorState GetTerrainAnchorState(IntPtr sessionHandle,
            IntPtr futureHandle)
        {
            ApiTerrainAnchorState outApiTerrainAnchorState = ApiTerrainAnchorState.None;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArResolveAnchorOnTerrainFuture_getResultTerrainAnchorState(sessionHandle,
                futureHandle, ref outApiTerrainAnchorState);
#endif
            return outApiTerrainAnchorState.ToTerrainAnchorState();
        }

        public static IntPtr GetTerrainAnchorHandle(IntPtr sessionHandle, IntPtr futureHandle)
        {
            IntPtr outAnchorHandle = IntPtr.Zero;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArResolveAnchorOnTerrainFuture_acquireResultAnchor(sessionHandle,
                futureHandle, ref outAnchorHandle);
#endif
            return outAnchorHandle;
        }

        public static CloudAnchorState GetResolveCloudAnchorState(IntPtr sessionHandle,
            IntPtr futureHandle)
        {
            ApiCloudAnchorState outApiCloudAnchorState = ApiCloudAnchorState.None;
#if !UNITY_IOS || CLOUDANCHOR_IOS_SUPPORT
            ExternApi.ArResolveCloudAnchorFuture_getResultCloudAnchorState(sessionHandle,
                futureHandle, ref outApiCloudAnchorState);
#endif
            return outApiCloudAnchorState.ToCloudAnchorState();
        }

        public static IntPtr GetCloudAnchorHandle(IntPtr sessionHandle, IntPtr futureHandle)
        {
            IntPtr outAnchorHandle = IntPtr.Zero;
#if !UNITY_IOS || CLOUDANCHOR_IOS_SUPPORT
            ExternApi.ArResolveCloudAnchorFuture_acquireResultAnchor(sessionHandle,
                futureHandle, ref outAnchorHandle);
#endif
            return outAnchorHandle;
        }

        public static CloudAnchorState GetHostCloudAnchorState(IntPtr sessionHandle,
            IntPtr futureHandle)
        {
            ApiCloudAnchorState outApiCloudAnchorState = ApiCloudAnchorState.None;
#if !UNITY_IOS || CLOUDANCHOR_IOS_SUPPORT
            ExternApi.ArHostCloudAnchorFuture_getResultCloudAnchorState(sessionHandle,
                futureHandle, ref outApiCloudAnchorState);
#endif
            return outApiCloudAnchorState.ToCloudAnchorState();
        }

        public static string GetCloudAnchorId(IntPtr sessionHandle, IntPtr futureHandle)
        {
#if !UNITY_IOS || CLOUDANCHOR_IOS_SUPPORT
            IntPtr stringHandle = IntPtr.Zero;
            ExternApi.ArHostCloudAnchorFuture_acquireResultCloudAnchorId(
                sessionHandle, futureHandle, ref stringHandle);
            string cloudAnchorId = Marshal.PtrToStringAnsi(stringHandle);

            ExternApi.ArString_release(stringHandle);

            return cloudAnchorId;
#else
            return null;
#endif
        }

        public static void Cancel(IntPtr sessionHandle, IntPtr futureHandle)
        {
            int defaultInt = 0;
            ExternApi.ArFuture_cancel(sessionHandle, futureHandle, ref defaultInt);
        }

        public static void Release(IntPtr futureHandle)
        {
            ExternApi.ArFuture_release(futureHandle);
            futureHandle = IntPtr.Zero;
        }

        private struct ExternApi
        {
            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_checkVpsAvailabilityAsync(
                IntPtr sessionHandle, double latitude, double longitude, IntPtr context,
                IntPtr callback, ref IntPtr out_future);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFuture_getState(IntPtr sessionHandle,
                IntPtr future, ref PromiseState out_state);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArVpsAvailabilityFuture_getResult(IntPtr sessionHandle,
                IntPtr future, ref VpsAvailability out_result);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFuture_cancel(IntPtr sessionHandle,
                IntPtr future, ref int out_cancel);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFuture_release(IntPtr future);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArResolveAnchorOnRooftopFuture_getResultRooftopAnchorState(
                IntPtr sessionHandle, IntPtr future,
                ref ApiRooftopAnchorState outRooftopAnchorState);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArResolveAnchorOnRooftopFuture_acquireResultAnchor(
                IntPtr sessionHandle, IntPtr future, ref IntPtr outAnchorHandle);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArResolveAnchorOnTerrainFuture_getResultTerrainAnchorState(
                IntPtr sessionHandle, IntPtr future,
                ref ApiTerrainAnchorState outTerrainAnchorState);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArResolveAnchorOnTerrainFuture_acquireResultAnchor(
                IntPtr sessionHandle, IntPtr future, ref IntPtr outAnchorHandle);

            [CloudAnchorImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArResolveCloudAnchorFuture_getResultCloudAnchorState(
                IntPtr sessionHandle, IntPtr future, ref ApiCloudAnchorState outCloudAnchorState);

            [CloudAnchorImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArResolveCloudAnchorFuture_acquireResultAnchor(
                IntPtr sessionHandle, IntPtr future, ref IntPtr outAnchorHandle);

            [CloudAnchorImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHostCloudAnchorFuture_getResultCloudAnchorState(
                IntPtr sessionHandle, IntPtr future, ref ApiCloudAnchorState outCloudAnchorState);

            [CloudAnchorImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHostCloudAnchorFuture_acquireResultCloudAnchorId(
                IntPtr sessionHandle, IntPtr futureHandle, ref IntPtr cloudAnchorIdHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArString_release(IntPtr stringHandle);
        }
    }
}
