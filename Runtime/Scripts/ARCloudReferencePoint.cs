//-----------------------------------------------------------------------
// <copyright file="ARCloudReferencePoint.cs" company="Google">
//
// Copyright 2019 Google LLC All Rights Reserved.
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
    /// The <c>ARCloudReferencePoint</c> is an ARCore Extensions object that provides a
    /// similar service to AR Foundations <c>ARReferencePoint</c> as an anchor for
    /// game objects in your scene. It is backed by an ARCore Cloud Anchor to synchronize
    /// pose data across multiple devices.
    /// </summary>
    [SuppressMessage("UnityRules.UnityStyleRules", "US1000:FieldsMustBeUpperCamelCase",
     Justification = "Match Unity's naming style.")]
    public class ARCloudReferencePoint : MonoBehaviour, ITrackable
    {
        internal IntPtr m_AnchorHandle;
        private Pose m_Pose;

        /// <summary>
        /// Gets the Cloud Reference Id associated with this cloud reference point. For newly
        /// created points the Id will be an empty string until the cloud reference point is
        /// in the <see cref="CloudReferenceState"/>.<c>Success</c> state. This Id is
        /// provided on the device hosting the cloud reference point, and is used to resolve
        /// a corresponding cloud reference point on other devices.
        /// See <see cref="ARReferencePointManagerExtensions.ResolveCloudReferencePoint()"/>
        /// for more information.
        /// </summary>
        public string cloudReferenceId
        {
            get
            {
                return AnchorApi.GetCloudAnchorId(
                    ARCoreExtensions.Instance.CurrentARCoreSessionHandle,
                    m_AnchorHandle);
            }
        }

        /// <summary>
        /// Gets the <see cref="CloudReferenceState"/> associated with cloud reference point.
        /// </summary>
        public CloudReferenceState cloudReferenceState
        {
            get
            {
                return Translators.ToCloudReferenceState(
                    AnchorApi.GetCloudAnchorState(
                        ARCoreExtensions.Instance.CurrentARCoreSessionHandle,
                        m_AnchorHandle));
            }
        }

        /// <summary>
        /// Gets the <c>TrackableId</c> associated with this cloud reference point.
        /// </summary>
        public TrackableId trackableId
        {
            get
            {
                return new TrackableId(0, (ulong)m_AnchorHandle);
            }
        }

        /// <summary>
        /// Gets the <c>Pose</c> associated with this cloud reference point.
        /// </summary>
        public Pose pose
        {
            get
            {
                return m_Pose;
            }
        }

        /// <summary>
        /// Gets the <c>TrackingState</c> associated with this cloud reference point.
        /// </summary>
        public TrackingState trackingState
        {
            get
            {
                return Translators.ToTrackingState(
                    AnchorApi.GetTrackingState(
                        ARCoreExtensions.Instance.CurrentARCoreSessionHandle,
                        m_AnchorHandle));
            }
        }

        /// <summary>
        /// Unity Update method.
        /// </summary>
        public void Update()
        {
            // Get the current Pose.
            ApiPose apiPose = AnchorApi.GetAnchorPose(
                ARCoreExtensions.Instance.CurrentARCoreSessionHandle,
                m_AnchorHandle);
            m_Pose = Translators.ToUnityPose(apiPose);

            // Update the cloud reference point transform to match.
            transform.localPosition = m_Pose.position;
            transform.localRotation = m_Pose.rotation;
        }

        internal void SetAnchorHandle(IntPtr anchorHandle)
        {
            m_AnchorHandle = anchorHandle;
        }
    }
}
