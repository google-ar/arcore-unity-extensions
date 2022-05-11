//-----------------------------------------------------------------------
// <copyright file="ARGeospatialAnchor.cs" company="Google LLC">
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
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// The <c>ARGeospatialAnchor</c> is an ARCore Extensions object that provides a
    /// similar service to AR Foundation's <c>ARAnchor</c> as an anchor for game
    /// objects in your scene.
    /// It is created at the specified geodetic location and orientation relative
    /// to the Earth.
    /// </summary>
    [SuppressMessage("UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
     Justification = "Match Unity's naming style.")]
    public class ARGeospatialAnchor : MonoBehaviour, ITrackable
    {
        internal IntPtr _anchorHandle;
        private Pose _pose;

        /// <summary>
        /// Gets the <c>TrackableId</c> associated with this Geospatial Anchor.
        /// </summary>
        public TrackableId trackableId
        {
            get
            {
                return new TrackableId(0, (ulong)_anchorHandle);
            }
        }

        /// <summary>
        /// Gets the <c>Pose</c> associated with this Geospatial Anchor.
        /// </summary>
        public Pose pose
        {
            get
            {
                return _pose;
            }
        }

        /// <summary>
        /// Gets the <c>TrackingState</c> associated with this Geospatial Anchor.
        /// </summary>
        public TrackingState trackingState
        {
            get
            {
                return AnchorApi.GetTrackingState(
                    ARCoreExtensions._instance.currentARCoreSessionHandle,
                    _anchorHandle).ToTrackingState();
            }
        }

        /// <summary>
        /// Gets the native pointer that represents this Geospatial Anchor.
        /// </summary>
        public IntPtr nativePtr
        {
            get
            {
                return _anchorHandle;
            }
        }

        /// <summary>
        /// Unity's Update method.
        /// </summary>
        public void Update()
        {
            // Get the current Pose.
            ApiPose apiPose = AnchorApi.GetAnchorPose(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                _anchorHandle);
            _pose = apiPose.ToUnityPose();

            // Update the Geospatial Anchor transform to match.
            transform.localPosition = _pose.position;
            transform.localRotation = _pose.rotation;
        }

        /// <summary>
        /// When the game object containing the <c><see cref="ARGeospatialAnchor"/></c> component is
        /// destroyed, the underlying native object will be detached and the resource will be
        /// released.
        /// </summary>
        public void OnDestroy()
        {
            if (_anchorHandle != IntPtr.Zero)
            {
                AnchorApi.Detach(
                    ARCoreExtensions._instance.currentARCoreSessionHandle,
                    _anchorHandle);
                AnchorApi.Release(_anchorHandle);
                _anchorHandle = IntPtr.Zero;
            }
        }

        internal void SetAnchorHandle(IntPtr anchorHandle)
        {
            _anchorHandle = anchorHandle;
        }
    }
}
