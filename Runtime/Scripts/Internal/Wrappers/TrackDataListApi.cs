//-----------------------------------------------------------------------
// <copyright file="TrackDataListApi.cs" company="Google LLC">
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

    internal class TrackDataListApi
    {
        public static IntPtr Create(IntPtr sessionHandle)
        {
            IntPtr handle = IntPtr.Zero;
#if UNITY_ANDROID
            ExternApi.ArTrackDataList_create(sessionHandle, ref handle);
#endif
            return handle;
        }

        public static void Destroy(IntPtr listHandle)
        {
#if UNITY_ANDROID
            ExternApi.ArTrackDataList_destroy(listHandle);
#endif
        }

        public static int GetCount(IntPtr sessionHandle, IntPtr listHandle)
        {
            int count = 0;
#if UNITY_ANDROID
            ExternApi.ArTrackDataList_getSize(sessionHandle, listHandle, ref count);
#endif
            return count;
        }

        public static IntPtr AcquireItem(IntPtr sessionHandle, IntPtr listHandle, int index)
        {
            IntPtr trackDataHandle = IntPtr.Zero;
#if UNITY_ANDROID
            ExternApi.ArTrackDataList_acquireItem(sessionHandle, listHandle, index,
                ref trackDataHandle);
#endif
            return trackDataHandle;
        }

        private struct ExternApi
        {
#if UNITY_ANDROID
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackDataList_create(
                IntPtr sessionHandle, ref IntPtr trackDataListHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackDataList_destroy(IntPtr trackDataListHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackDataList_getSize(
                IntPtr sessionHandle, IntPtr trackDataListHandle, ref int outSize);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackDataList_acquireItem(
                IntPtr sessionHandle, IntPtr trackDataListHandle, int index,
                ref IntPtr outTrackData);
#endif // UNITY_ANDROID
        }
    }
}
