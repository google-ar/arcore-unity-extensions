//-----------------------------------------------------------------------
// <copyright file="MeshApi.cs" company="Google LLC">
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

#if UNITY_IOS && !GEOSPATIAL_IOS_SUPPORT
    using MeshImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#else
    using MeshImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    // TODO(b/262257477): This design copies all verts and indices and can be optimized.
    internal class MeshApi
    {
        private const int _numIndicesPerTriangle = 3;

        public static Mesh AcquireMesh(IntPtr sessionHandle, IntPtr meshHandle)
        {
            Mesh mesh = new Mesh
            {
                name = "ARMesh"
            };
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            mesh.vertices = GetVertices(sessionHandle, meshHandle);

            // This converts from c api counterclockwise winding to Unity's clockwise winding.
            int[] indices = GetIndices(sessionHandle, meshHandle);
            int lastTriangleStartIndex = indices.Length - _numIndicesPerTriangle;
            //// This rounds down to the nearest multiple of the number of indices in a triangle to
            //// avoid any array-out-of-bounds errors.
            int nearestMultiple =
                (int)(lastTriangleStartIndex / _numIndicesPerTriangle) * _numIndicesPerTriangle;
            for (int i = 0; i <= nearestMultiple; i += _numIndicesPerTriangle)
            {
                // Reverse each triangle in place.
                int temp = indices[i + 1];
                indices[i + 1] = indices[i + 2];
                indices[i + 2] = temp;
            }

            mesh.triangles = indices;
#endif
            return mesh;
        }

        public static int[] GetIndices(IntPtr sessionHandle, IntPtr meshHandle)
        {
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            IntPtr outIndicesPtr = IntPtr.Zero;
            ExternApi.ArMesh_getIndexList(sessionHandle, meshHandle, ref outIndicesPtr);

            int numIndices = 0;
            ExternApi.ArMesh_getIndexListSize(sessionHandle, meshHandle, ref numIndices);

            int[] indices = new int[numIndices];
            Marshal.Copy(outIndicesPtr, indices, 0, indices.Length);
            return indices;
#else
            return new int[0];
#endif
        }

        public static Vector3[] GetVertices(IntPtr sessionHandle, IntPtr meshHandle)
        {
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            IntPtr outVerticesPtr = IntPtr.Zero;
            ExternApi.ArMesh_getVertexList(sessionHandle, meshHandle, ref outVerticesPtr);

            int numVertices = 0;
            ExternApi.ArMesh_getVertexListSize(sessionHandle, meshHandle, ref numVertices);

            float[] vertices = new float[numVertices * 3];
            Marshal.Copy(outVerticesPtr, vertices, 0, vertices.Length);

            return ConvertFloatArrayToVector3Array(vertices);
#else
            return new Vector3[0];
#endif
        }

        public static void Release(IntPtr meshHandle)
        {
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArMesh_release(meshHandle);
#endif
        }

        private static Vector3[] ConvertFloatArrayToVector3Array(float[] array)
        {
            int numberComponentsPerVertex = 3;
            int numberOfVertices = array.Length / 3;
            Vector3[] vertices = new Vector3[numberOfVertices];
            for (int index = 0; index < numberOfVertices; ++index)
            {
                int offset = index * numberComponentsPerVertex;
                Vector3 vector = new Vector3(
                    array[offset],
                    array[offset + 1],
                    array[offset + 2]);
                vertices[index] = vector.ToUnityVector();
            }

            return vertices;
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [MeshImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArMesh_getVertexListSize(IntPtr sessionHandle,
                IntPtr meshHandle, ref int outNumberVertices);

            [MeshImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArMesh_getIndexListSize(IntPtr sessionHandle,
                IntPtr meshHandle, ref int outNumIndices);

            [MeshImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArMesh_getIndexList(IntPtr sessionHandle,
                IntPtr meshHandle, ref IntPtr outIndicesPtr);

            [MeshImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArMesh_getVertexList(IntPtr sessionHandle,
                IntPtr meshHandle, ref IntPtr outVerticesPtr);

            [MeshImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArMesh_release(IntPtr meshHandle);
#pragma warning restore 626
        }
    }
}
