//-----------------------------------------------------------------------
// <copyright file="ARAnchorManagerExtensions.cs" company="Google LLC">
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
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;

    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// Extensions to AR Foundation's ARAnchorManager class.
    /// </summary>
    public static class ARAnchorManagerExtensions
    {
        private static readonly string _gameObjectName = "ARCloudAnchor";

        /// <summary>
        /// Creates a new Cloud Anchor using an existing local ARAnchor.
        /// <example>
        /// The sample code below illustrates how to host a Cloud Anchor.
        /// <pre>
        /// <code>
        /// private ARCloudAnchor _cloudAnchor;
        /// &nbsp;
        /// void HostCloudAnchor(Pose pose)
        /// {
        ///     // Create a local anchor, you may also use another ARAnchor you already have.
        ///     ARAnchor localAnchor = AnchorManager.AddAnchor(pose);
        /// &nbsp;
        ///     // Request the Cloud Anchor.
        ///     _cloudAnchor = AnchorManager.HostCloudAnchor(localAnchor);
        /// }
        /// &nbsp;
        /// void Update()
        /// {
        ///     if (_cloudAnchor)
        ///     {
        ///         // Check the Cloud Anchor state.
        ///         CloudAnchorState cloudAnchorState = _cloudAnchor.cloudAnchorState;
        ///         if (cloudAnchorState == CloudAnchorState.Success)
        ///         {
        ///             myOtherGameObject.transform.SetParent(_cloudAnchor.transform, false);
        ///             _cloudAnchor = null;
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
        /// <param name="anchorManager">The ARAnchorManager instance.</param>
        /// <param name="anchor">The local <c>ARAnchor</c> to be used as the
        /// basis to host a new Cloud Anchor.</param>
        /// <returns>If successful, a <c><see cref="ARCloudAnchor"/></c>,
        /// otherwise <c>null</c>.</returns>
        public static ARCloudAnchor HostCloudAnchor(
            this ARAnchorManager anchorManager, ARAnchor anchor)
        {
            // Create the underlying ARCore Cloud Anchor.
            IntPtr cloudAnchorHandle = SessionApi.HostCloudAnchor(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                anchor.AnchorHandle());
            if (cloudAnchorHandle == IntPtr.Zero)
            {
                return null;
            }

            // Create the GameObject that is the Cloud Anchor.
            ARCloudAnchor cloudAnchor =
                (new GameObject(_gameObjectName)).AddComponent<ARCloudAnchor>();
            if (cloudAnchor)
            {
                cloudAnchor.SetAnchorHandle(cloudAnchorHandle);
            }

            // Parent the new Cloud Anchor to the session origin.
            cloudAnchor.transform.SetParent(
                ARCoreExtensions._instance.SessionOrigin.trackablesParent, false);

            return cloudAnchor;
        }

        /// <summary>
        /// Creates a new Cloud Anchor with a given lifetime using an existing local ARAnchor.
        /// </summary>
        /// <param name="anchorManager">The ARAnchorManager instance.</param>
        /// <param name="anchor">The local <c>ARAnchor</c> to be used as the
        /// basis to host a new Cloud Anchor.</param>
        /// <param name="ttlDays">The lifetime of the anchor in days. Must be positive. The
        /// maximum allowed value is 1 if using an API Key to authenticate with the
        /// ARCore Cloud Anchor service, otherwise the maximum allowed value is 365.</param>
        /// <returns>If successful, an <c><see cref="ARCloudAnchor"/></c>,
        /// otherwise <c>null</c>.</returns>
        public static ARCloudAnchor HostCloudAnchor(
            this ARAnchorManager anchorManager, ARAnchor anchor, int ttlDays)
        {
            if (ttlDays <= 0 || ttlDays > 365)
            {
                Debug.LogErrorFormat("Failed to host a Cloud Anchor with invalid TTL {0}. " +
                    "The lifetime of the anchor in days must be positive, " +
                    "the maximum allowed value is 1 when using an API Key to authenticate with " +
                    "the ARCore Cloud Anchor service, otherwise the maximum allowed value is 365.",
                    ttlDays);
                return null;
            }

            // Create the underlying ARCore Cloud Anchor with given ttlDays.
            IntPtr cloudAnchorHandle = SessionApi.HostCloudAnchor(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                anchor.AnchorHandle(), ttlDays);
            if (cloudAnchorHandle == IntPtr.Zero)
            {
                return null;
            }

            // Create the GameObject that is the Cloud Anchor.
            ARCloudAnchor cloudAnchor =
                new GameObject(_gameObjectName).AddComponent<ARCloudAnchor>();
            if (cloudAnchor)
            {
                cloudAnchor.SetAnchorHandle(cloudAnchorHandle);
            }

            // Parent the new Cloud Anchor to the session origin.
            cloudAnchor.transform.SetParent(
                ARCoreExtensions._instance.SessionOrigin.trackablesParent, false);

            return cloudAnchor;
        }

        /// <summary>
        /// Set the token to use when authenticating with the ARCore Cloud Anchor service
        /// on the iOS platform.  This should be called each time the application's
        /// token is refreshed.
        /// </summary>
        /// <param name="anchorManager">The ARAnchorManager instance.</param>
        /// <param name="authToken">The authentication token to set.</param>
        public static void SetAuthToken(this ARAnchorManager anchorManager, string authToken)
        {
            // Only iOS needs AuthToken for Cloud Anchor persistence.
#if ARCORE_EXTENSIONS_IOS_SUPPORT && UNITY_IOS
            if (!string.IsNullOrEmpty(RuntimeConfig.Instance.IOSCloudServicesApiKey))
            {
                Debug.LogError(
                    "Cannot set token in applications built using the 'API Key' " +
                    "authentication strategy. To use it, check Edit > Project Settings " +
                    "> XR > ARCore Extensions > iOS Support Enabled and " +
                    "set iOS Authentication Strategy to Authentication Token.");
                return;
            }

            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Cannot set empty token in applications.");
                return;
            }

            SessionApi.SetAuthToken(
                ARCoreExtensions._instance.currentARCoreSessionHandle, authToken);
#else
            Debug.LogError("AuthToken only works with iOS Supported enabled in " +
                "ARCore Extensions Project Settings and the target platform has set to iOS.");
#endif // ARCORE_IOS_SUPPORT && UNITY_IOS
        }

        /// <summary>
        /// Creates a new cloud reference point using an existing local Reference Point.
        /// </summary>
        /// <param name="referencePointManager">The ARAnchorManager instance.</param>
        /// <param name="referencePoint">The local <c>ARAnchor</c> to be used as the
        /// basis to host a new cloud reference point.</param>
        /// <returns>If successful, a <c><see cref="ARCloudReferencePoint"/></c>,
        /// otherwise <c>null</c>.</returns>
        /// @deprecated Please use HostCloudAnchor(ARAnchor) instead.
        [Obsolete("This method has been deprecated. Please use HostCloudAnchor(ARAnchor) instead.")]
        public static ARCloudReferencePoint AddCloudReferencePoint(
            this ARAnchorManager referencePointManager, ARAnchor referencePoint)
        {
            // Create the underlying ARCore Cloud Anchor.
            IntPtr cloudAnchorHandle = SessionApi.HostCloudAnchor(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                referencePoint.AnchorHandle());
            if (cloudAnchorHandle == IntPtr.Zero)
            {
                return null;
            }

            // Create the GameObject that is the cloud reference point.
            ARCloudReferencePoint cloudReferencePoint =
                (new GameObject(_gameObjectName)).AddComponent<ARCloudReferencePoint>();
            if (cloudReferencePoint)
            {
                cloudReferencePoint.SetAnchorHandle(cloudAnchorHandle);
            }

            // Parent the new cloud reference point to the session origin.
            cloudReferencePoint.transform.SetParent(
                ARCoreExtensions._instance.SessionOrigin.trackablesParent, false);

            return cloudReferencePoint;
        }

        /// <summary>
        /// Creates a new local Cloud Anchor from the provided Id.
        /// <example>
        /// The sample code below illustrates how to resolve a Cloud Anchor.
        /// <pre>
        /// <code>
        /// private ARCloudAnchor _cloudAnchor;
        /// &nbsp;
        /// void ResolveCloudAnchor(string cloudAnchorId)
        /// {
        ///     // Request the Cloud Anchor.
        ///     _cloudAnchor = AnchorManager.ResolveCloudAnchorId(cloudAnchorId);
        /// }
        /// &nbsp;
        /// void Update()
        /// {
        ///     if (_cloudAnchor)
        ///     {
        ///         // Check the Cloud Anchor state.
        ///         CloudAnchorState cloudAnchorState = _cloudAnchor.cloudAnchorState;
        ///         if (cloudAnchorState == CloudAnchorState.Success)
        ///         {
        ///             myOtherGameObject.transform.SetParent(_cloudAnchor.transform, false);
        ///             _cloudAnchor = null;
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
        /// <param name="anchorManager">The ARAnchorManager instance.</param>
        /// <param name="cloudAnchorId">String representing the Cloud Anchor.</param>
        /// <returns>If successful, a <c><see cref="ARCloudAnchor"/></c>,
        /// otherwise <c>null</c>.</returns>
        public static ARCloudAnchor ResolveCloudAnchorId(
            this ARAnchorManager anchorManager, string cloudAnchorId)
        {
            // Create the underlying ARCore Cloud Anchor.
            IntPtr cloudAnchorHandle = SessionApi.ResolveCloudAnchor(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                cloudAnchorId);
            if (cloudAnchorHandle == IntPtr.Zero)
            {
                return null;
            }

            // Create the GameObject that is the Cloud Anchor.
            ARCloudAnchor cloudAnchor =
                (new GameObject(_gameObjectName)).AddComponent<ARCloudAnchor>();
            if (cloudAnchor)
            {
                cloudAnchor.SetAnchorHandle(cloudAnchorHandle);
            }

            // Parent the new Cloud Anchor to the session origin.
            cloudAnchor.transform.SetParent(
                ARCoreExtensions._instance.SessionOrigin.trackablesParent, false);

            return cloudAnchor;
        }

        /// <summary>
        /// Creates a new local cloud reference point from the provided Id.
        /// </summary>
        /// <param name="referencePointManager">The ARAnchorManager instance.</param>
        /// <param name="cloudReferenceId">String representing the cloud reference.</param>
        /// <returns>If successful, a <c><see cref="ARCloudReferencePoint"/></c>,
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
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                cloudReferenceId);
            if (cloudAnchorHandle == IntPtr.Zero)
            {
                return null;
            }

            // Create the GameObject that is the cloud reference point.
            ARCloudReferencePoint cloudReferencePoint =
                (new GameObject(_gameObjectName)).AddComponent<ARCloudReferencePoint>();
            if (cloudReferencePoint)
            {
                cloudReferencePoint.SetAnchorHandle(cloudAnchorHandle);
            }

            // Parent the new cloud reference point to the session origin.
            cloudReferencePoint.transform.SetParent(
                ARCoreExtensions._instance.SessionOrigin.trackablesParent, false);

            return cloudReferencePoint;
        }

        /// <summary>
        /// Estimates the quality of the visual feature points seen by ARCore in the
        /// preceding few seconds and visible from the provided camera <paramref name="pose"/>.
        /// Cloud Anchors hosted using higher feature map quality will generally result
        /// in easier and more accurately resolved <c><see cref="ARCloudAnchor"/></c> poses.
        /// If feature map quality cannot be estimated for the given <paramref name="pose"/>,
        /// a warning message "Failed to estimate feature map quality" with the error status
        /// is logged and <c><see cref="FeatureMapQuality"/></c>.<c>Insufficient</c> is returned.
        /// </summary>
        /// <param name="anchorManager">The ARAnchorManager instance.</param>
        /// <param name="pose">The camera pose to use in estimating the quality.</param>
        /// <returns>The estimated feature map quality.</returns>
        public static FeatureMapQuality EstimateFeatureMapQualityForHosting(
            this ARAnchorManager anchorManager, Pose pose)
        {
            return SessionApi.EstimateFeatureMapQualityForHosting(
                ARCoreExtensions._instance.currentARCoreSessionHandle, pose);
        }
    }
}
