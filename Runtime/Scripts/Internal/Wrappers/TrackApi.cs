//-----------------------------------------------------------------------
// <copyright file="TrackApi.cs" company="Google LLC">
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
#endif // UNITY_ANDROID

    internal class TrackApi
    {
        public static IntPtr Create(IntPtr sessionHandle, Track track)
        {
            IntPtr trackHandle = IntPtr.Zero;
#if UNITY_ANDROID
            ExternApi.ArTrack_create(sessionHandle, ref trackHandle);

            // Track ID
            GCHandle trackIdHandle = GCHandle.Alloc(track.Id.ToByteArray(), GCHandleType.Pinned);
            ExternApi.ArTrack_setId(sessionHandle, trackHandle, trackIdHandle.AddrOfPinnedObject());

            if (trackIdHandle.IsAllocated)
            {
                trackIdHandle.Free();
            }

            // Metadata
            GCHandle metadataHandle = GCHandle.Alloc(track.Metadata, GCHandleType.Pinned);
            ExternApi.ArTrack_setMetadata(sessionHandle, trackHandle,
                                          metadataHandle.AddrOfPinnedObject(),
                                          track.Metadata.Length);

            if (metadataHandle.IsAllocated)
            {
                metadataHandle.Free();
            }

            // Mime Type
            ExternApi.ArTrack_setMimeType(sessionHandle, trackHandle, track.MimeType);
#endif // UNITY_ANDROID
            return trackHandle;
        }

        public static void Destroy(IntPtr trackHandle)
        {
#if UNITY_ANDROID
            ExternApi.ArTrack_destroy(trackHandle);
#endif
        }

        private struct ExternApi
        {
#if UNITY_ANDROID
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrack_create(IntPtr session, ref IntPtr trackHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrack_destroy(IntPtr trackHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrack_setId(IntPtr session, IntPtr trackHandle,
                                                    IntPtr trackIdBytes);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrack_setMetadata(IntPtr session, IntPtr trackHandle,
                                                          IntPtr metadataBytes,
                                                          int metadataBufferSize);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrack_setMimeType(IntPtr session, IntPtr trackHandle,
                                                          string mimeType);
#endif
        }
    }
}
