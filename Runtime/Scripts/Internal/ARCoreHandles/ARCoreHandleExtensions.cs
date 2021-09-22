//-----------------------------------------------------------------------
// <copyright file="ARCoreHandleExtensions.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    // Internal interface for the extension methods to retrieve native pointers.
    internal static class ARCoreHandleExtensions
    {
        public static IntPtr SessionHandle(this ARSession session)
        {
            if (session.subsystem == null || session.subsystem.nativePtr == null)
            {
                return IntPtr.Zero;
            }

            SessionNativePointerStruct info = (SessionNativePointerStruct)
                Marshal.PtrToStructure(
                    session.subsystem.nativePtr,
                    typeof(SessionNativePointerStruct));

            return info.SessionHandle;
        }

        public static IntPtr AnchorHandle(this ARAnchor anchor)
        {
            AnchorNativePointerStruct info = (AnchorNativePointerStruct)
                Marshal.PtrToStructure(
                    anchor.nativePtr,
                    typeof(AnchorNativePointerStruct));

            return info.AnchorHandle;
        }

        public static IntPtr PlaneHandle(this ARPlane plane)
        {
            PlaneNativePointerStruct info = (PlaneNativePointerStruct)
                Marshal.PtrToStructure(
                    plane.nativePtr,
                    typeof(PlaneNativePointerStruct));

            return info.PlaneHandle;
        }

        public static IntPtr FrameHandle(this XRCameraFrame frame)
        {
            FrameNativePointerStruct info = (FrameNativePointerStruct)
                Marshal.PtrToStructure(
                    frame.nativePtr,
                    typeof(FrameNativePointerStruct));

            return info.FrameHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SessionNativePointerStruct
        {
            public int Version;
            public IntPtr SessionHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AnchorNativePointerStruct
        {
            public int Version;
            public IntPtr AnchorHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PlaneNativePointerStruct
        {
            public int Version;
            public IntPtr PlaneHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrameNativePointerStruct
        {
            public int Version;
            public IntPtr FrameHandle;
        }
    }
}
