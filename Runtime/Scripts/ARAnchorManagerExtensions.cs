//-----------------------------------------------------------------------
// <copyright file="ARAnchorManagerExtensions.cs" company="Google">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;

    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// Extensions to AR Foundation's ARAnchorManager class.
    /// </summary>
    public static class ARAnchorManagerExtensions
    {
        private static readonly string k_GameObjectName = "ARCloudAnchor";

        /// <summary>
        /// Creates a new Cloud Anchor using an existing local ARAnchor.
        /// <example>
        /// The sample code below illustrates how to host a Cloud Anchor.
        /// <pre>
        /// <code>
        /// private ARCloudAnchor m_CloudAnchor;
        /// &nbsp;
        /// void HostCloudAnchor(Pose pose)
        /// {
        ///     // Create a local anchor, you may also use another ARAnchor you already have.
        ///     ARAnchor localAnchor = AnchorManager.AddAnchor(pose);
        /// &nbsp;
        ///     // Request the Cloud Anchor.
        ///     m_CloudAnchor = AnchorManager.HostCloudAnchor(localAnchor);
        /// }
        /// &nbsp;
        /// void Update()
        /// {
        ///     if (m_CloudAnchor)
        ///     {
        ///         // Check the Cloud Anchor state.
        ///         CloudAnchorState cloudAnchorState = m_CloudAnchor.cloudAnchorState;
        ///         if (cloudAnchorState == CloudAnchorState.Success)
        ///         {
        ///             myOtherGameObject.transform.SetParent(m_CloudAnchor.transform, false);
        ///             m_CloudAnchor = null;
        ///         }
        ///         else if (cloudAnchorState == CloudAnchorState.TaskInProgress)
        ///         {
        ///             // Wait, not ready yet.
        ///         }
        ///         else
        ///         {
        ///             // An error has occurred.
        ///         }
        ///     }
        /// }
        /// </code>
        /// </pre>
        /// </example>
        /// </summary>
        /// <param name="anchorManager">The ARAnchorManager instance for extending.
        /// </param>
        /// <param name="anchor">The local <c>ARAnchor</c> to be used as the
        /// basis to host a new Cloud Anchor.</param>
        /// <returns>If successful, a <see cref="ARCloudAnchor"/>,
        /// otherwise <c>null</c>.</returns>
        public static ARCloudAnchor HostCloudAnchor(
            this ARAnchorManager anchorManager, ARAnchor anchor)
        {
            // Create the underlying ARCore Cloud Anchor.
            IntPtr cloudAnchorHandle = SessionApi.HostCloudAnchor(
                ARCoreExtensions.Instance.CurrentARCoreSessionHandle,
                anchor.AnchorHandle());
            if (cloudAnchorHandle == IntPtr.Zero)
            {
                return null;
            }

            // Create the GameObject that is the Cloud Anchor.
            ARCloudAnchor cloudAnchor =
                (new GameObject(k_GameObjectName)).AddComponent<ARCloudAnchor>();
            if (cloudAnchor)
            {
                cloudAnchor.SetAnchorHandle(cloudAnchorHandle);
            }

            // Parent the new Cloud Anchor to the session origin.
            cloudAnchor.transform.SetParent(
                ARCoreExtensions.Instance.SessionOrigin.trackablesParent, false);

            return cloudAnchor;
        }

        /// <summary>
        /// Creates a new cloud reference point using an existing local Reference Point.
        /// </summary>
        /// <param name="referencePointManager">The ARAnchorManager instance for extending.
        /// </param>
        /// <param name="referencePoint">The local <c>ARAnchor</c> to be used as the
        /// basis to host a new cloud reference point.</param>
        /// <returns>If successful, a <see cref="ARCloudReferencePoint"/>,
        /// otherwise <c>null</c>.</returns>
        /// @deprecated Please use HostCloudAnchor(ARAnchor) instead.
        [Obsolete("This method has been deprecated. Please use HostCloudAnchor(ARAnchor) instead.")]
        public static ARCloudReferencePoint AddCloudReferencePoint(
            this ARAnchorManager referencePointManager, ARAnchor referencePoint)
        {
            // Create the underlying ARCore Cloud Anchor.
            IntPtr cloudAnchorHandle = SessionApi.HostCloudAnchor(
                ARCoreExtensions.Instance.CurrentARCoreSessionHandle,
                referencePoint.AnchorHandle());
            if (cloudAnchorHandle == IntPtr.Zero)
            {
                return null;
            }

            // Create the GameObject that is the cloud reference point.
            ARCloudReferencePoint cloudReferencePoint =
                (new GameObject(k_GameObjectName)).AddComponent<ARCloudReferencePoint>();
            if (cloudReferencePoint)
            {
                cloudReferencePoint.SetAnchorHandle(cloudAnchorHandle);
            }

            // Parent the new cloud reference point to the session origin.
            cloudReferencePoint.transform.SetParent(
                ARCoreExtensions.Instance.SessionOrigin.trackablesParent, false);

            return cloudReferencePoint;
        }

        /// <summary>
        /// Creates a new local Cloud Anchor from the provided Id.
        /// <example>
        /// The sample code below illustrates how to resolve a Cloud Anchor.
        /// <pre>
        /// <code>
        /// private ARCloudAnchor m_CloudAnchor;
        /// &nbsp;
        /// void ResolveCloudAnchor(string cloudAnchorId)
        /// {
        ///     // Request the Cloud Anchor.
        ///     m_CloudAnchor = AnchorManager.ResolveCloudAnchorId(cloudAnchorId);
        /// }
        /// &nbsp;
        /// void Update()
        /// {
        ///     if (m_CloudAnchor)
        ///     {
        ///         // Check the Cloud Anchor state.
        ///         CloudAnchorState cloudAnchorState = m_CloudAnchor.cloudAnchorState;
        ///         if (cloudAnchorState == CloudAnchorState.Success)
        ///         {
        ///             myOtherGameObject.transform.SetParent(m_CloudAnchor.transform, false);
        ///             m_CloudAnchor = null;
        ///         }
        ///         else if (cloudAnchorState == CloudAnchorState.TaskInProgress)
        ///         {
        ///             // Wait, not ready yet.
        ///         }
        ///         else
        ///         {
        ///             // An error has occurred.
        ///         }
        ///     }
        /// }
        /// </code>
        /// </pre>
        /// </example>
        /// </summary>
        /// <param name="anchorManager">The ARAnchorManager instance for extending.
        /// </param>
        /// <param name="cloudAnchorId">String representing the Cloud Anchor.</param>
        /// <returns>If successful, a <see cref="ARCloudAnchor"/>,
        /// otherwise <c>null</c>.</returns>
        public static ARCloudAnchor ResolveCloudAnchorId(
            this ARAnchorManager anchorManager, string cloudAnchorId)
        {
            // Create the underlying ARCore Cloud Anchor.
            IntPtr cloudAnchorHandle = SessionApi.ResolveCloudAnchor(
                ARCoreExtensions.Instance.CurrentARCoreSessionHandle,
                cloudAnchorId);
            if (cloudAnchorHandle == IntPtr.Zero)
            {
                return null;
            }

            // Create the GameObject that is the Cloud Anchor.
            ARCloudAnchor cloudAnchor =
                (new GameObject(k_GameObjectName)).AddComponent<ARCloudAnchor>();
            if (cloudAnchor)
            {
                cloudAnchor.SetAnchorHandle(cloudAnchorHandle);
            }

            // Parent the new Cloud Anchor to the session origin.
            cloudAnchor.transform.SetParent(
                ARCoreExtensions.Instance.SessionOrigin.trackablesParent, false);

            return cloudAnchor;
        }

        /// <summary>
        /// Creates a new local cloud reference point from the provided Id.
        /// </summary>
        /// <param name="referencePointManager">The ARAnchorManager instance for extending.
        /// </param>
        /// <param name="cloudReferenceId">String representing the cloud reference.</param>
        /// <returns>If successful, a <see cref="ARCloudReferencePoint"/>,
        /// otherwise <c>null</c>.</returns>
        /// @deprecated Please use ResolveCloudAnchorId(string) instead.
        [Obsolete("This method has been deprecated. " +
            "Please use ResolveCloudAnchorId(string) instead.")]
        public static ARCloudReferencePoint ResolveCloudReferenceId(
            this ARAnchorManager referencePointManager,
            string cloudReferenceId)
        {
            // Create the underlying ARCore Cloud Anchor.
            IntPtr cloudAnchorHandle = SessionApi.ResolveCloudAnchor(
                ARCoreExtensions.Instance.CurrentARCoreSessionHandle,
                cloudReferenceId);
            if (cloudAnchorHandle == IntPtr.Zero)
            {
                return null;
            }

            // Create the GameObject that is the cloud reference point.
            ARCloudReferencePoint cloudReferencePoint =
                (new GameObject(k_GameObjectName)).AddComponent<ARCloudReferencePoint>();
            if (cloudReferencePoint)
            {
                cloudReferencePoint.SetAnchorHandle(cloudAnchorHandle);
            }

            // Parent the new cloud reference point to the session origin.
            cloudReferencePoint.transform.SetParent(
                ARCoreExtensions.Instance.SessionOrigin.trackablesParent, false);

            return cloudReferencePoint;
        }
    }
}
