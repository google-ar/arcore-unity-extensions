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

#if UNITY_IOS
    using ImageImport = System.Runtime.InteropServices.DllImportAttribute;
#else // UNITY_IOS
    using ImageImport = System.Runtime.InteropServices.DllImportAttribute;
#endif // UNITY_IOS
    internal class ImageApi
    {
        public static void GetPlaneData(IntPtr sessionHandle, IntPtr imageHandle, int planeIndex,
            ref IntPtr surfaceData, ref int dataLength)
        {
            ExternApi.ArImage_getPlaneData(sessionHandle, imageHandle, planeIndex,
                ref surfaceData, ref dataLength);
        }

        public static int GetWidth(IntPtr sessionHandle, IntPtr imageHandle)
        {
            int width = 0;
            ExternApi.ArImage_getWidth(sessionHandle, imageHandle, out width);
            return width;
        }

        public static int GetHeight(IntPtr sessionHandle, IntPtr imageHandle)
        {
            int height = 0;
            ExternApi.ArImage_getHeight(sessionHandle, imageHandle, out height);
            return height;
        }

        public static long GetTimestamp(IntPtr sessionHandle, IntPtr imageHandle)
        {
            long timestamp = 0;
            ExternApi.ArImage_getTimestamp(sessionHandle, imageHandle, out timestamp);
            return timestamp;
        }

        public static void Release(IntPtr imageHandle)
        {
            ExternApi.ArImage_release(imageHandle);
        }

        public static void UpdateTexture(
            IntPtr sessionHandle, IntPtr imageHandle, TextureFormat format, ref Texture2D texture)
        {
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
        }

        public static Vector2Int UpdateRawData(
            IntPtr sessionHandle, IntPtr imageHandle, ref byte[] imageBuffer)
        {
            int width = GetWidth(sessionHandle, imageHandle);
            int height = GetHeight(sessionHandle, imageHandle);
            IntPtr planeDataHandle = IntPtr.Zero;
            int planeSize = 0;
            GetPlaneData(sessionHandle, imageHandle, 0, ref planeDataHandle, ref planeSize);
            if (imageBuffer.Length != planeSize)
            {
                imageBuffer = new byte[planeSize];
            }

            Marshal.Copy(planeDataHandle, imageBuffer, 0, planeSize);
            return new Vector2Int(width, height);
        }

        private struct ExternApi
        {
            [ImageImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_release(IntPtr imageHandle);

            [ImageImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getWidth(
                IntPtr sessionHandle, IntPtr imageHandle, out int width);

            [ImageImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getHeight(
                IntPtr sessionHandle, IntPtr imageHandle, out int height);

            [ImageImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getTimestamp(
                IntPtr sessionHandle, IntPtr imageHandle, out long timestamp);

            [ImageImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getPlaneData(
                IntPtr sessionHandle, IntPtr imageHandle, int planeIndex, ref IntPtr surfaceData,
                ref int dataLength);
        }
    }
}
