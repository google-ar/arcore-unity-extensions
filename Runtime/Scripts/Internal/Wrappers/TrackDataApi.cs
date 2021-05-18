//-----------------------------------------------------------------------
// <copyright file="TrackDataApi.cs" company="Google LLC">
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

#if UNITY_ANDROID
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class TrackDataApi
    {
        public static long GetFrameTimestamp(IntPtr sessionHandle, IntPtr trackDataHandle)
        {
            long timestamp = 0L;
#if UNITY_ANDROID
            ExternApi.ArTrackData_getFrameTimestamp(sessionHandle, trackDataHandle, ref timestamp);
#endif
            return timestamp;
        }

        public static byte[] GetData(IntPtr sessionHandle, IntPtr trackDataHandle)
        {
            IntPtr dataPtr = IntPtr.Zero;
            int size = 0;
#if UNITY_ANDROID
            ExternApi.ArTrackData_getData(sessionHandle, trackDataHandle, ref dataPtr, ref size);
#endif
            byte[] data = new byte[size];
            if (size > 0)
            {
                Marshal.Copy(dataPtr, data, 0, size);
            }

            return data;
        }

        public static void Release(IntPtr trackDataHandle)
        {
#if UNITY_ANDROID
            ExternApi.ArTrackData_release(trackDataHandle);
#endif
        }

        private struct ExternApi
        {
#if UNITY_ANDROID
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackData_getFrameTimestamp(
                IntPtr sessionHandle, IntPtr trackDataHandle, ref long timestamp);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackData_getData(
                IntPtr sessionHandle, IntPtr trackDataHandle, ref IntPtr dataBytesHandle,
                ref int size);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackData_release(IntPtr trackDataHandle);
#endif // UNITY_ANDROID
        }
    }
}
