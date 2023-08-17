//-----------------------------------------------------------------------
// <copyright file="ARStreetscapeGeometry.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Defines geometry such as terrain, buildings, or other structures obtained
    /// from the Streetscape Geometry API. See the <a
    /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/streetscape-geometry">Streetscape
    /// Geometry Developer Guide</a> for additional information.
    /// </summary>
    [SuppressMessage("UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
        Justification = "Match Unity's naming style.")]
    public class ARStreetscapeGeometry : ITrackable
    {
        internal IntPtr _streetscapeGeometryHandle = IntPtr.Zero;
        private StreetscapeGeometryType _streetscapeGeometryType = StreetscapeGeometryType.Terrain;
        private Mesh _mesh = null;

        /// <summary>
        /// Sets the ARStreetscapeGeometry Handle.
        /// </summary>
        /// <param name="streetscapeGeometryHandle">The native facade object handle.</param>
        internal ARStreetscapeGeometry(IntPtr streetscapeGeometryHandle)
        {
            _streetscapeGeometryHandle = streetscapeGeometryHandle;
        }

        /// <summary>
        /// Gets the <c>TrackableId</c> associated with this geometry.
        /// </summary>
        public TrackableId trackableId
        {
            get
            {
                return _streetscapeGeometryHandle.ToTrackableId();
            }
        }

        /// <summary>
        /// Gets the <c>Pose</c> associated with this geometry.
        /// </summary>
        public Pose pose
        {
            get
            {
                ApiPose apiPose = StreetscapeGeometryApi.GetPose(
                    ARCoreExtensions._instance.currentARCoreSessionHandle,
                    _streetscapeGeometryHandle);
                return apiPose.ToUnityPose();
            }
        }

        /// <summary>
        /// Gets the native pointer that represents this geometry.
        /// </summary>
        public IntPtr nativePtr
        {
            get
            {
                return _streetscapeGeometryHandle;
            }
        }

        /// <summary>
        /// Gets the <c>StreetscapeGeometryType</c> corrosponding to this geometry.
        /// </summary>
        public StreetscapeGeometryType streetscapeGeometryType
        {
            get
            {
                _streetscapeGeometryType = StreetscapeGeometryApi.GetStreetscapeGeometryType(
                    ARCoreExtensions._instance.currentARCoreSessionHandle,
                    _streetscapeGeometryHandle);
                return _streetscapeGeometryType;
            }
        }

        /// <summary>
        /// Gets the Unity <c>Mesh</c> associated with this geometry.
        /// Normals are not calculated: if normals are required, use
        /// <c><a
        /// href="https://docs.unity3d.com/ScriptReference/Mesh.RecalculateNormals.html">Mesh.RecalculateNormals()</a></c>.
        /// </summary>
        public Mesh mesh
        {
            get
            {
                if (_mesh == null)
                {
                    IntPtr meshHandle = StreetscapeGeometryApi.AcquireMeshHandle(
                        ARCoreExtensions._instance.currentARCoreSessionHandle,
                        _streetscapeGeometryHandle);
                    if (meshHandle == IntPtr.Zero)
                    {
                        Debug.LogError("ARStreetscapeGeometry failed to acquire mesh Handle.");
                        _mesh = new Mesh();
                        return _mesh;
                    }

                    _mesh = StreetscapeGeometryApi.AcquireMesh(
                        ARCoreExtensions._instance.currentARCoreSessionHandle, meshHandle);
                    MeshApi.Release(meshHandle);
                }

                return _mesh;
            }
        }

        /// <summary>
        /// Gets the <c>TrackingState</c> associated with this geometry.
        /// </summary>
        public TrackingState trackingState
        {
            get
            {
                return StreetscapeGeometryApi.GetTrackingState(
                    ARCoreExtensions._instance.currentARCoreSessionHandle,
                    _streetscapeGeometryHandle);
            }
        }

        /// <summary>
        /// Gets the <c>StreetscapeGeometryQuality</c> associated with this geometry.
        /// </summary>
        public StreetscapeGeometryQuality quality
        {
            get
            {
                return StreetscapeGeometryApi.GetStreetscapeGeometryQuality(
                    ARCoreExtensions._instance.currentARCoreSessionHandle,
                    _streetscapeGeometryHandle);
            }
        }
    }
}
