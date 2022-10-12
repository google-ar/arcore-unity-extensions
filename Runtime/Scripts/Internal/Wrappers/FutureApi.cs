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
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

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
            ApiArStatus status = ExternApi.ArSession_checkVpsAvailabilityAsync(
                sessionHandle, latitude, longitude, IntPtr.Zero, IntPtr.Zero, ref futureHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to get the availability of VPS, status '{0}'", status);
            }

            return futureHandle;
        }

        public static PromiseState GetState(IntPtr sessionHandle, IntPtr futureHandle)
        {
            var state = PromiseState.Pending;
            ExternApi.ArVpsAvailabilityFuture_getState(sessionHandle, futureHandle, ref state);
            return state;
        }

        public static VpsAvailability GetResult(IntPtr sessionHandle, IntPtr futureHandle)
        {
            var result = VpsAvailability.Unknown;
            ExternApi.ArVpsAvailabilityFuture_getResult(sessionHandle, futureHandle, ref result);
            return result;
        }

        public static void Cancel(IntPtr sessionHandle, IntPtr futureHandle)
        {
            int defaultInt = 0;
            ExternApi.ArVpsAvailabilityFuture_cancel(sessionHandle, futureHandle, ref defaultInt);
        }

        public static void Release(IntPtr futureHandle)
        {
            ExternApi.ArVpsAvailabilityFuture_release(futureHandle);
            futureHandle = IntPtr.Zero;
        }

        private struct ExternApi
        {
            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_checkVpsAvailabilityAsync(
                IntPtr sessionHandle, double latitude, double longitude, IntPtr context,
                IntPtr callback, ref IntPtr out_future);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArVpsAvailabilityFuture_getState(IntPtr sessionHandle,
                IntPtr future, ref PromiseState out_state);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArVpsAvailabilityFuture_getResult(IntPtr sessionHandle,
                IntPtr future, ref VpsAvailability out_result);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArVpsAvailabilityFuture_cancel(IntPtr sessionHandle,
                IntPtr future, ref int out_cancel);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArVpsAvailabilityFuture_release(IntPtr future);
        }
    }
}
