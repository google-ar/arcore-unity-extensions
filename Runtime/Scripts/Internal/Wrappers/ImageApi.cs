//-----------------------------------------------------------------------
// <copyright file="ImageApi.cs" company="Google LLC">
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
    using UnityEngine;

#if UNITY_ANDROID
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class ImageApi
    {
        public static void GetPlaneData(IntPtr sessionHandle, IntPtr imageHandle, int planeIndex,
            ref IntPtr surfaceData, ref int dataLength)
        {
#if UNITY_ANDROID
            ExternApi.ArImage_getPlaneData(sessionHandle, imageHandle, planeIndex,
                ref surfaceData, ref dataLength);
#endif
        }

        public static int GetWidth(IntPtr sessionHandle, IntPtr imageHandle)
        {
#if UNITY_ANDROID
            ExternApi.ArImage_getWidth(sessionHandle, imageHandle, out int width);

            return width;
#else
            return 0;
#endif
        }

        public static int GetHeight(IntPtr sessionHandle, IntPtr imageHandle)
        {
#if UNITY_ANDROID
            ExternApi.ArImage_getHeight(sessionHandle, imageHandle, out int height);
            return height;
#else
            return 0;
#endif
        }

        public static long GetTimestamp(IntPtr sessionHandle, IntPtr imageHandle)
        {
#if UNITY_ANDROID
            ExternApi.ArImage_getTimestamp(sessionHandle, imageHandle, out long timestamp);
            return timestamp;
#else
            return 0;
#endif
        }

        public static void Release(IntPtr imageHandle)
        {
#if UNITY_ANDROID
            ExternApi.ArImage_release(imageHandle);
#endif
        }

        public static void UpdateTexture(
            IntPtr sessionHandle, IntPtr imageHandle, TextureFormat format, ref Texture2D texture)
        {
#if UNITY_ANDROID
            int width = GetWidth(sessionHandle, imageHandle);
            int height = GetHeight(sessionHandle, imageHandle);
            IntPtr planeDataHandle = IntPtr.Zero;
            int planeSize = 0;
            GetPlaneData(sessionHandle, imageHandle, 0, ref planeDataHandle, ref planeSize);
            if (texture == null || width != texture.width || height != texture.height ||
                format != texture.format)
            {
                texture = new Texture2D(width, height, format, false)
                {
                    filterMode = FilterMode.Bilinear
                };
            }

            texture.LoadRawTextureData(planeDataHandle, planeSize);
            texture.Apply();
#endif
        }

        public static Vector2Int UpdateRawData(
            IntPtr sessionHandle, IntPtr imageHandle, ref byte[] imageBuffer)
        {
            int width = GetWidth(sessionHandle, imageHandle);
            int height = GetHeight(sessionHandle, imageHandle);
#if UNITY_ANDROID
            IntPtr planeDataHandle = IntPtr.Zero;
            int planeSize = 0;
            GetPlaneData(sessionHandle, imageHandle, 0, ref planeDataHandle, ref planeSize);
            if (imageBuffer.Length != planeSize)
            {
                imageBuffer = new byte[planeSize];
            }

            Marshal.Copy(planeDataHandle, imageBuffer, 0, planeSize);
#endif
            return new Vector2Int(width, height);
        }

        private struct ExternApi
        {
#if UNITY_ANDROID
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_release(IntPtr imageHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getWidth(
                IntPtr sessionHandle, IntPtr imageHandle, out int width);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getHeight(
                IntPtr sessionHandle, IntPtr imageHandle, out int height);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getTimestamp(
                IntPtr sessionHandle, IntPtr imageHandle, out long timestamp);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getPlaneData(
                IntPtr sessionHandle, IntPtr imageHandle, int planeIndex, ref IntPtr surfaceData,
                ref int dataLength);
#endif
        }
    }
}
