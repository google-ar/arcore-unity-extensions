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
        private static readonly string _cloudAnchorName = "ARCloudAnchor";
        private static readonly string _geospatialAnchorName = "ARGeospatialAnchor";

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
                (new GameObject(_cloudAnchorName)).AddComponent<ARCloudAnchor>();
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
                new GameObject(_cloudAnchorName).AddComponent<ARCloudAnchor>();
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
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
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
            Debug.LogError(
                "AuthToken only works with iOS Support Enabled " +
                "in ARCore Extensions Project Settings and the target platform has set to iOS.");
#endif //  UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
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
                (new GameObject(_cloudAnchorName)).AddComponent<ARCloudReferencePoint>();
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
                (new GameObject(_cloudAnchorName)).AddComponent<ARCloudAnchor>();
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
                (new GameObject(_cloudAnchorName)).AddComponent<ARCloudReferencePoint>();
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

        /// <summary>
        /// Creates a new anchor at the specified geodetic location and orientation
        /// relative to the Earth.
        ///
        /// Latitude and longitude are defined by the
        /// <a href="https://en.wikipedia.org/wiki/World_Geodetic_System">WGS84
        /// specification</a>, and altitude values are defined by the elevation above
        /// the WGS84 ellipsoid.
        ///
        /// Creating anchors near the north pole or south pole is not supported. If
        /// the latitude is within 0.1 degrees of the north pole or south pole (90
        /// degrees or -90 degrees), this function will return <c>null</c>.
        ///
        /// The rotation provided by <paramref name="eunRotation"/> is a rotation
        /// with respect to an east-up-north coordinate frame. An identity rotation
        /// will have the anchor oriented such that X+ points to the east, Y+ points up
        /// away from the center of the earth, and Z+ points to the north.
        ///
        /// To create a quaternion that represents a clockwise angle theta from
        /// north around the +Y anchor frame axis, use the following formula:
        /// <code>
        /// Quaternion.AngleAxis(180f - theta, Vector3.up);
        /// </code>
        ///
        /// An anchor's tracking state will be TrackingState.None while
        /// <see cref="AREarthManager.EarthTrackingState"/> is TrackingState.None.
        /// The tracking state will permanently become TrackingState.None if the
        /// configuration is set to <see cref="GeospatialMode"/>.<c>Disabled</c>.
        /// </summary>
        /// <param name="anchorManager">The ARAnchorManager instance.</param>
        /// <param name="latitude">
        /// The latitude of the anchor relative to the WGS84 ellipsoid.</param>
        /// <param name="longitude">
        /// The longitude of the anchor relative to the WGS84 ellipsoid.</param>
        /// <param name="altitude">
        /// The altitude of the anchor relative to the WGS84 ellipsoid.</param>
        /// <param name="eunRotation">The rotation of the anchor with respect to
        /// the east-up-north coordinate frame where X+ points east, Y+ points up
        /// away from gravity, and Z+ points north. A rotation about the Y+ axis
        /// creates a rotation counterclockwise from north.</param>
        /// <returns>
        /// If successful, a <see cref="ARGeospatialAnchor"/>, otherwise, <c>null</c>.
        /// </returns>
        public static ARGeospatialAnchor AddAnchor(
            this ARAnchorManager anchorManager, double latitude, double longitude,
            double altitude, Quaternion eunRotation)
        {
            IntPtr earthHandle = SessionApi.AcquireEarth(
                ARCoreExtensions._instance.currentARCoreSessionHandle);
            if (earthHandle == IntPtr.Zero)
            {
                Debug.LogError("Failed to acquire earth.");
                return null;
            }

            IntPtr anchorHandle = EarthApi.AddAnchor(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                earthHandle, latitude, longitude, altitude, eunRotation);
            if (anchorHandle == IntPtr.Zero)
            {
                Debug.LogError("Failed to add geospatial anchor.");
                return null;
            }

            // Create the GameObject that is the Geospatial Anchor.
            ARGeospatialAnchor anchor =
                new GameObject(_geospatialAnchorName).AddComponent<ARGeospatialAnchor>();
            if (anchor)
            {
                anchor.SetAnchorHandle(anchorHandle);
            }

            // Parent the new Geospatial Anchor to the session origin.
            anchor.transform.SetParent(
                ARCoreExtensions._instance.SessionOrigin.trackablesParent, false);
            return anchor;
        }
    }
}
