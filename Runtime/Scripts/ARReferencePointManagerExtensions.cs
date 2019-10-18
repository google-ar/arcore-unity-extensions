//-----------------------------------------------------------------------
// <copyright file="ARReferencePointManagerExtensions.cs" company="Google">
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
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// Extensions to AR Foundation's ARReferencePointManager class.
    /// </summary>
    public static class ARReferencePointManagerExtensions
    {
        private static readonly string k_GameObjectName = "ARCloudReferencePoint";

        /// <summary>
        /// Creates a new cloud reference point using an existing local Reference Point.
        /// <example>
        /// The sample code below illustrates how to host a cloud reference point.
        /// <code>
        /// private ARCloudReferencePoint m_CloudReferencePoint;
        ///
        /// void HostCloudReference(Pose pose)
        /// {
        ///     // Create a local Reference Point, you may also use another
        ///     // Reference Point you may already have.
        ///     ARReferencePoint localReferencePoint =
        ///         ReferencePointManager.AddReferencePoint(pose);
        ///
        ///     // Request the cloud reference point.
        ///     m_CloudReferencePoint =
        ///         ReferencePointManager.AddCloudReferencePoint(localReferencePoint);
        /// }
        ///
        /// void Update()
        /// {
        ///     if (m_CloudReferencePoint)
        ///     {
        ///         // Check the cloud reference point state.
        ///         CloudReferenceState cloudReferenceState =
        ///             m_CloudReferencePoint.cloudReferenceState;
        ///         if (cloudReferenceState == CloudReferenceState.Success)
        ///         {
        ///             myOtherGameObject.transform.SetParent
        ///                 m_CloudReferencePoint.transform, false);
        ///             m_CloudReferencePoint = null;
        ///         }
        ///         else if (cloudReferenceState == CloudReferenceState.TaskInProgress)
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
        /// </example>
        /// </summary>
        /// <param name="referencePointManager">The ReferencePointManager instance for extending.
        /// </param>
        /// <param name="referencePoint">The local <c>ARReferencePoint</c> to be used as the
        /// basis to host a new cloud reference point.</param>
        /// <returns>If successful, a <see cref="ARCloudReferencePoint"/>,
        /// otherwise <c>null</c>.</returns>
        public static ARCloudReferencePoint AddCloudReferencePoint(
            this ARReferencePointManager referencePointManager,
            ARReferencePoint referencePoint)
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
        /// Creates a new local cloud reference point from the provided Id.
        /// <example>
        /// The sample code below illustrates how to resolve a cloud reference point.
        /// <code>
        /// private ARCloudReferencePoint m_CloudReferencePoint;
        ///
        /// void ResolveCloudReference(string cloudReferenceId)
        /// {
        ///     // Request the cloud reference point.
        ///     m_CloudReferencePoint =
        ///         ReferencePointManager.ResolveCloudReferenceId(cloudReferenceId);
        /// }
        ///
        /// void Update()
        /// {
        ///     if (m_CloudReferencePoint)
        ///     {
        ///         // Check the cloud reference point state.
        ///         CloudReferenceState cloudReferenceState =
        ///             m_CloudReferencePoint.cloudReferenceState;
        ///         if (cloudReferenceState == CloudReferenceState.Success)
        ///         {
        ///             myOtherGameObject.transform.SetParent
        ///                 m_CloudReferencePoint.transform, false);
        ///             m_CloudReferencePoint = null;
        ///         }
        ///         else if (cloudReferenceState == CloudReferenceState.TaskInProgress)
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
        /// </example>
        /// </summary>
        /// <param name="referencePointManager">The ReferencePointManager instance for extending.
        /// </param>
        /// <param name="cloudReferenceId">String representing the cloud reference.</param>
        /// <returns>If successful, a <see cref="ARCloudReferencePoint"/>,
        /// otherwise <c>null</c>.</returns>
        public static ARCloudReferencePoint ResolveCloudReferenceId(
            this ARReferencePointManager referencePointManager,
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
