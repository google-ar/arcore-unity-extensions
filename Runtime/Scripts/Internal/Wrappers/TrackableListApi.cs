//-----------------------------------------------------------------------
// <copyright file="TrackableListApi.cs" company="Google LLC">
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
    using UnityEngine;

#if UNITY_IOS && !GEOSPATIAL_IOS_SUPPORT
    using TrackableListImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#else
    using TrackableListImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class TrackableListApi
    {
        public static IntPtr Create(IntPtr sessionHandle)
        {
            IntPtr handle = IntPtr.Zero;
            ExternApi.ArTrackableList_create(sessionHandle, ref handle);
            return handle;
        }

        public static void Destroy(IntPtr listHandle)
        {
            ExternApi.ArTrackableList_destroy(listHandle);
        }

        public static int GetCount(IntPtr sessionHandle, IntPtr listHandle)
        {
            int count = 0;
            ExternApi.ArTrackableList_getSize(sessionHandle, listHandle, ref count);
            return count;
        }

        public static IntPtr AcquireItem(IntPtr sessionHandle, IntPtr listHandle, int index)
        {
            IntPtr trackableHandle = IntPtr.Zero;
            ExternApi.ArTrackableList_acquireItem(
                sessionHandle, listHandle, index, ref trackableHandle);
            return trackableHandle;
        }

        private struct ExternApi
        {
            [TrackableListImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackableList_create(
                IntPtr sessionHandle, ref IntPtr trackableListHandle);

            [TrackableListImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackableList_destroy(IntPtr trackableListHandle);

            [TrackableListImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackableList_getSize(
                IntPtr sessionHandle, IntPtr trackableListHandle, ref int outSize);

            [TrackableListImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackableList_acquireItem(
                IntPtr sessionHandle, IntPtr trackableListHandle, int index,
                ref IntPtr outTrackable);
        }
    }
}
