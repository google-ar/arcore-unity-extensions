//-----------------------------------------------------------------------
// <copyright file="ARStreetscapeGeometryManager.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;

    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Provides ARCore Geospatial Streetscape Geometry APIs. See the <a
    /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/streetscape-geometry">Streetscape
    /// Geometry Developer Guide</a> for additional information.
    /// </summary>
    public class ARStreetscapeGeometryManager : MonoBehaviour
    {
        /// <summary>
        /// The 3D object that represents an <c><see cref="ARAnchor"/></c>s.
        /// </summary>
        public GameObject AnchorPrefab = null;

        private Dictionary<IntPtr, ARStreetscapeGeometry> _newHandleToGeometries =
            new Dictionary<IntPtr, ARStreetscapeGeometry>();

        private Dictionary<IntPtr, ARStreetscapeGeometry> _oldHandleToGeometries =
            new Dictionary<IntPtr, ARStreetscapeGeometry>();

        private List<ARStreetscapeGeometry> _added = new List<ARStreetscapeGeometry>();

        private List<ARStreetscapeGeometry> _updated = new List<ARStreetscapeGeometry>();

        private List<ARStreetscapeGeometry> _removed = new List<ARStreetscapeGeometry>();

        /// <summary>
        /// Invoked when Streetscape Geometries have changed (added, updated, or removed).
        /// </summary>
        public event Action<ARStreetscapeGeometriesChangedEventArgs> StreetscapeGeometriesChanged;

        /// <summary>
        /// Unity's Update() method.
        /// </summary>
        public void Update()
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero)
            {
                return;
            }

            IntPtr listHandle =
                SessionApi.GetAllStreetscapeGeometryHandles(
                    ARCoreExtensions._instance.currentARCoreSessionHandle);

            // Get Added, Updated, Removed.
            _newHandleToGeometries.Clear();
            _added.Clear();
            _updated.Clear();
            _removed.Clear();
            int count = TrackableListApi.GetCount(
                ARCoreExtensions._instance.currentARCoreSessionHandle, listHandle);
            for (int i = 0; i < count; i++)
            {
                IntPtr trackableHandle =
                    TrackableListApi.AcquireItem(
                        ARCoreExtensions._instance.currentARCoreSessionHandle, listHandle, i);
                if (_oldHandleToGeometries.ContainsKey(trackableHandle))
                {
                    ARStreetscapeGeometry geometry = _oldHandleToGeometries[trackableHandle];
                    _updated.Add(geometry);
                    _newHandleToGeometries[trackableHandle] = geometry;
                    _oldHandleToGeometries.Remove(trackableHandle);
                }
                else
                {
                    ARStreetscapeGeometry geometry = new ARStreetscapeGeometry(
                        trackableHandle);
                    _added.Add(geometry);
                    _newHandleToGeometries[trackableHandle] = geometry;
                }
            }

            foreach (KeyValuePair<IntPtr, ARStreetscapeGeometry> kvp in _oldHandleToGeometries)
            {
                _removed.Add(kvp.Value);
            }

            _oldHandleToGeometries.Clear();
            foreach (KeyValuePair<IntPtr, ARStreetscapeGeometry> kvp in _newHandleToGeometries)
            {
                _oldHandleToGeometries[kvp.Key] = kvp.Value;
            }

            if (StreetscapeGeometriesChanged != null && (_added.Count > 0 || _updated.Count > 0
                || _removed.Count > 0))
            {
                StreetscapeGeometriesChanged(new ARStreetscapeGeometriesChangedEventArgs(_added,
                    _updated, _removed));
            }
        }

        /// <summary>
        /// Attempts to create a new anchor attached to <c>geometry</c>.
        ///
        /// If <c><see cref="AnchorPrefab"/></c> is not <c>null</c>, a new instance of that prefab
        /// will be instantiated. Otherwise, a new <c>GameObject</c> will be created. In either
        /// case, the resulting <c>GameObject</c> will have an <c><see cref="ARAnchor"/></c>
        /// component added to it.
        /// </summary>
        /// <param name="geometry">The <c><see cref="ARStreetscapeGeometry"/></c> that this anchor
        /// will be associated with.</param>
        /// <param name="pose">The pose in Unity world space for the newly created anchor.
        /// </param>
        /// <returns> The <c><see cref="ARAnchor"/></c> if successful, otherwise <c>null</c>.
        /// </returns>
        public ARAnchor AttachAnchor(ARStreetscapeGeometry geometry, Pose pose)
        {
            IntPtr anchorHandle;
            TrackableApi.AcquireNewAnchor(
                ARCoreExtensions._instance.currentARCoreSessionHandle, geometry.nativePtr, pose,
                out anchorHandle);

            if (AnchorPrefab == null)
            {
                AnchorPrefab = new GameObject();
            }

            ARAnchor anchor =
                (Instantiate(AnchorPrefab, pose.position, pose.rotation)).AddComponent<ARAnchor>();

            if (anchor)
            {
                anchor.gameObject.name = "ARAnchor";
            }

            return anchor;
        }

        /// <summary>
        /// Gets the <c><see cref="ARStreetscapeGeometry"/></c> based on <c>trackableId</c>.
        /// </summary>
        /// <param name="trackableId"> The id for the <c><see
        /// cref="ARStreetscapeGeometry"/></c>.</param> <returns>The <see
        /// cref="ARStreetscapeGeometry"/> associated with this <c>TrackableId</c> if successful,
        /// <c>null</c> otherwise.</returns>
        public ARStreetscapeGeometry GetStreetscapeGeometry(TrackableId trackableId)
        {
            IntPtr streetscapeGeometryHandle = trackableId.ToNativePtr();
            if (_newHandleToGeometries.ContainsKey(streetscapeGeometryHandle))
            {
                return _newHandleToGeometries[streetscapeGeometryHandle];
            }

            Debug.LogWarning(
                "ARStreetscapeGeometryManager does not contain a StreetscapeGeometry for " +
                "trackableId: " + trackableId);
            return null;
        }
    }
}
