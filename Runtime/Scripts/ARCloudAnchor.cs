//-----------------------------------------------------------------------
// <copyright file="ARCloudAnchor.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    /// The <c>ARCloudAnchor</c> is an ARCore Extensions object that provides a
    /// similar service to AR Foundation's <c><see cref="ARAnchor"/></c> as an anchor for game
    /// objects in your scene.
    /// It is backed by an ARCore Cloud Anchor to synchronize pose data across multiple devices.
    /// </summary>
    [SuppressMessage("UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
     Justification = "Match Unity's naming style.")]
    public class ARCloudAnchor : MonoBehaviour, ITrackable
    {
        internal IntPtr _anchorHandle;
        private Pose _pose;

        /// <summary>
        /// Gets the Cloud Anchor Id associated with this Cloud Anchor. For newly
        /// created points the Id will be an empty string until the Cloud Anchor is
        /// in the <c><see cref="CloudAnchorState.Success"/></c> state. This Id is
        /// provided on the device hosting the Cloud Anchor, and is used to resolve
        /// a corresponding Cloud Anchor on other devices.
        /// See <c><see cref="ARAnchorManagerExtensions.ResolveCloudAnchorId(
        /// UnityEngine.XR.ARFoundation.ARAnchorManager, string)"/></c>
        /// for more information.
        /// </summary>
        /// @deprecated When hosting an anchor using an async method, get the Cloud Anchor ID from
        /// the <c><see cref="HostCloudAnchorResult"/></c> object. This will always return the
        /// empty string or null, except for anchors created by deprecated methods.
        [Obsolete(
            "This property has been deprecated. When hosting an anchor using an async " +
            "method, get the Cloud Anchor ID from the HostCloudAnchorResult object. This will " +
            "always return the empty string or null, except for anchors created by deprecated " +
            "methods.")]
        public string cloudAnchorId
        {
            get
            {
                if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                    _anchorHandle == IntPtr.Zero)
                {
                    return null;
                }

                return AnchorApi.GetCloudAnchorId(
                    ARCoreExtensions._instance.currentARCoreSessionHandle,
                    _anchorHandle);
            }
        }

        /// <summary>
        /// Gets the <c><see cref="CloudAnchorState"/></c> associated with this Cloud Anchor.
        /// </summary>
        /// @deprecated When hosting or resolving an anchor using an async method, get the result
        /// status from the <c><see cref="HostCloudAnchorResult"/></c> or <c><see
        /// cref="ResolveCloudAnchorResult"/></c> object. This will always return
        /// <c>CloudAnchorState.None</c>, except for anchors created by deprecated methods.
        [Obsolete(
            "This property has been deprecated. When hosting or resolving an anchor using " +
            "an async method, get the result status from the HostCloudAnchorResult or " +
            "ResolveCloudAnchorResult object. This will always return CloudAnchorState.None, " +
            "except for anchors created by deprecated methods.")]
        public CloudAnchorState cloudAnchorState
        {
            get
            {
                if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                    _anchorHandle == IntPtr.Zero)
                {
                    return CloudAnchorState.None;
                }

                return AnchorApi.GetCloudAnchorState(
                    ARCoreExtensions._instance.currentARCoreSessionHandle,
                    _anchorHandle).ToCloudAnchorState();
            }
        }

        /// <summary>
        /// Gets the <c>TrackableId</c> associated with this Cloud Anchor.
        /// </summary>
        public TrackableId trackableId
        {
            get
            {
                return new TrackableId(0, (ulong)_anchorHandle);
            }
        }

        /// <summary>
        /// Gets the <c>Pose</c> associated with this Cloud Anchor.
        /// </summary>
        public Pose pose
        {
            get
            {
                return _pose;
            }
        }

        /// <summary>
        /// Gets the <c>TrackingState</c> associated with this Cloud Anchor.
        /// </summary>
        public TrackingState trackingState
        {
            get
            {
                if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                    _anchorHandle == IntPtr.Zero)
                {
                    return TrackingState.None;
                }

                return AnchorApi.GetTrackingState(
                    ARCoreExtensions._instance.currentARCoreSessionHandle,
                    _anchorHandle).ToTrackingState();
            }
        }

        /// <summary>
        /// Gets the native pointer that represents this Cloud Anchor.
        /// </summary>
        public IntPtr nativePtr
        {
            get
            {
                return _anchorHandle;
            }
        }

        /// <summary>
        /// Unity Update method.
        /// </summary>
        public void Update()
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                _anchorHandle == IntPtr.Zero)
            {
                return;
            }

            // Get the current Pose.
            ApiPose apiPose = AnchorApi.GetAnchorPose(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                _anchorHandle);
            _pose = apiPose.ToUnityPose();

            // Update the Cloud Anchor transform to match.
            transform.localPosition = _pose.position;
            transform.localRotation = _pose.rotation;
        }

        /// <summary>
        /// When the game object containing the <c><see cref="ARCloudAnchor"/></c> component is
        /// destroyed, the underlying native Cloud Anchor object will be detached and the resource
        /// will be released.
        /// </summary>
        public void OnDestroy()
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle != IntPtr.Zero &&
                _anchorHandle != IntPtr.Zero)
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
