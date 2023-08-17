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
    /// Extensions to AR Foundation's <c><see cref="ARAnchorManager"/></c> class.
    /// </summary>
    public static class ARAnchorManagerExtensions
    {
        private static readonly string _cloudAnchorName = "ARCloudAnchor";
        private static readonly string _geospatialAnchorName = "ARGeospatialAnchor";
        private static readonly string _terrainAnchorName = "ARTerrainAnchor";

        /// <summary>
        /// Creates an anchor at a specified horizontal position and altitude relative to the
        /// horizontal position’s rooftop. See the <a
        /// href="https://developers.google.com/ar/develop/geospatial/unity-arf/anchors#rooftop-anchors">Rooftop
        /// anchors developer guide</a> for more information.
        ///
        /// The specified <c><paramref name="altitudeAboveRooftop"/></c> is interpreted to be
        /// relative to the top of a building at the given horizontal location, rather than relative
        /// to the WGS84 ellipsoid. If there is no building at the given location, then the altitude
        /// is interpreted to be relative to the terrain instead. Specifying an altitude of 0 will
        /// position the anchor directly on the rooftop whereas specifying a positive altitude will
        /// position the anchor above the rooftop, against the direction of gravity.
        ///
        /// This launches an asynchronous operation used to query the Google Cloud ARCore API. See
        /// <c><see
        /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
        /// for information on obtaining results and cancelling the operation.
        ///
        /// You may resolve multiple anchors at a time, but a session cannot be tracking more than
        /// 100 Terrain and Rooftop Anchors at time.
        ///
        /// Creating a Rooftop anchor requires
        /// <c><see cref="AREarthManager.EarthState"/></c> to be
        /// <c><see cref="EarthState.Enabled"/></c>
        /// and <c><see cref="AREarthManager.EarthTrackingState"/></c> to be
        /// <c><see cref="TrackingState.Tracking"/></c> or
        /// <c><see cref="TrackingState.Paused"/></c>. If it is not, then
        /// <c><see cref="ResolveAnchorOnRooftopResult.Anchor"/></c> will be <c>null</c>. This call
        /// also requires a working internet connection to communicate with the ARCore API on Google
        /// Cloud. See <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/enable">Enable the
        /// Geospatial API</a> for more details on required permissions and setup steps. ARCore will
        /// continue to retry if it is unable to establish a connection to the ARCore service.
        ///
        /// Latitude and longitude are defined by the
        /// <a href="https://en.wikipedia.org/wiki/World_Geodetic_System">WGS84
        /// specification</a>.
        ///
        /// The rotation provided by <c><paramref name="eunRotation"/></c> is a rotation with
        /// respect to an east-up-north coordinate frame. An identity rotation will have the anchor
        /// oriented such that X+ points to the east, Y+ points up away from the center of the
        /// earth, and Z+ points to the north.
        ///
        /// To create a quaternion that represents a clockwise angle theta from north around the +Y
        /// anchor frame axis, use the following formula:
        /// <code>
        /// Quaternion.AngleAxis(180f - theta, Vector3.up);
        /// </code>
        ///
        /// </summary>
        /// <param name="anchorManager">The <c>ARAnchorManager</c> instance.</param>
        /// <param name="latitude">
        /// The latitude of the anchor relative to the WGS84 ellipsoid.</param>
        /// <param name="longitude">
        /// The longitude of the anchor relative to the WGS84 ellipsoid.</param>
        /// <param name="altitudeAboveRooftop">
        /// The altitude of the anchor above the Earth's Rooftop.</param>
        /// <param name="eunRotation">The rotation of the anchor with respect to the east-up-north
        /// coordinate frame where X+ points east, Y+ points up away from gravity, and Z+ points
        /// north. A rotation about the Y+ axis creates a rotation counterclockwise from north.
        /// </param>
        /// <returns>Returns a <c><see cref="ResolveAnchorOnRooftopPromise"/></c>. See <c><see
        /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
        /// for more information on how to retrieve results from the Promise.</returns>
        public static ResolveAnchorOnRooftopPromise ResolveAnchorOnRooftopAsync(
            this ARAnchorManager anchorManager, double latitude, double longitude,
            double altitudeAboveRooftop, Quaternion eunRotation)
        {
            IntPtr earthHandle = SessionApi.AcquireEarth(
                ARCoreExtensions._instance.currentARCoreSessionHandle);
            if (earthHandle == IntPtr.Zero)
            {
                Debug.LogError("Failed to acquire earth.");
                return new ResolveAnchorOnRooftopPromise(IntPtr.Zero);
            }

            IntPtr future = EarthApi.ResolveAnchorOnRooftopFuture(
                ARCoreExtensions._instance.currentARCoreSessionHandle, earthHandle, latitude,
                longitude, altitudeAboveRooftop, eunRotation, IntPtr.Zero);

            TrackableApi.Release(earthHandle);

            return new ResolveAnchorOnRooftopPromise(future);
        }

        /// <summary>
        /// Creates a new Cloud Anchor using an existing local ARAnchor.
        /// </summary>
        /// <param name="anchorManager">The <c><see cref="ARAnchorManager"/></c> instance.</param>
        /// <param name="anchor">The local <c><see cref="ARAnchor"/></c> to be used as the
        /// basis to host a new Cloud Anchor.</param>
        /// <returns>If successful, a <c><see cref="ARCloudAnchor"/></c>,
        /// otherwise <c>null</c>.</returns>
        /// @deprecated Please use <c><see
        /// cref="ARAnchorManagerExtensions.HostCloudAnchorAsync(this ARAnchorManager, ARAnchor,
        /// int)"/></c> instead with <c>ttlDays=1</c>.
        [Obsolete("This method has been deprecated. " +
            "Please use HostCloudAnchorAsync(ARAnchor, int) instead with ttlDays=1.")]
        public static ARCloudAnchor HostCloudAnchor(
            this ARAnchorManager anchorManager, ARAnchor anchor)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                anchor == null || anchor.nativePtr == IntPtr.Zero ||
                anchor.AnchorHandle() == IntPtr.Zero)
            {
                return null;
            }

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
        /// Creates a new Cloud Anchor with a given lifetime using an existing local
        /// <c>ARAnchor</c>.
        /// </summary>
        /// <param name="anchorManager">The <c><see cref="ARAnchorManager"/></c> instance.</param>
        /// <param name="anchor">The local <c><see cref="ARAnchor"/></c> to be used as the
        /// basis to host a new Cloud Anchor.</param>
        /// <param name="ttlDays">The lifetime of the anchor in days. Must be positive. The
        /// maximum allowed value is 1 if using an API Key to authenticate with the
        /// ARCore API, otherwise the maximum allowed value is 365.</param>
        /// <returns>If successful, an <c><see cref="ARCloudAnchor"/></c>,
        /// otherwise <c>null</c>.</returns>
        /// @deprecated Use <c><see cref="HostCloudAnchorAsync(this ARAnchorManager, ARAnchor,
        /// int)"/></c> instead with <c>ttlDays=1</c>.
        [Obsolete("This method has been deprecated. " +
            "Please use HostCloudAnchorAsync(ARAnchor, int) instead with ttlDays=1.")]
        public static ARCloudAnchor HostCloudAnchor(
            this ARAnchorManager anchorManager, ARAnchor anchor, int ttlDays)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                anchor == null || anchor.nativePtr == IntPtr.Zero ||
                anchor.AnchorHandle() == IntPtr.Zero)
            {
                return null;
            }

            if (ttlDays <= 0 || ttlDays > 365)
            {
                Debug.LogErrorFormat("Failed to host a Cloud Anchor with invalid TTL {0}. " +
                    "The lifetime of the anchor in days must be positive, " +
                    "the maximum allowed value is 1 when using an API Key to authenticate with " +
                    "the ARCore API, otherwise the maximum allowed value is 365.",
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
        /// Uses the pose and other data from <c>anchor</c> to host a new Cloud Anchor.
        /// A Cloud Anchor is assigned an identifier that can be used to create an
        /// <c><see cref="ARAnchor"/></c> in the same position in subsequent sessions across devices
        /// using <c><see
        /// cref="ARAnchorManagerExtensions.ResolveCloudAnchorAsync(this ARAnchorManager,
        /// string)"/></c>. See the <a
        /// href="https://developers.google.com/ar/develop/unity-arf/cloud-anchors/developer-guide">Cloud
        /// Anchors developer guide</a> for more information.
        ///
        /// The duration that a Cloud Anchor can be resolved for is specified by
        /// <c>ttlDays</c>. When using <a
        /// href="https://developers.google.com/ar/develop/unity-arf/cloud-anchors/developer-guide-android#keyless-authorization">Keyless
        /// authorization</a> or <a
        /// href="https://developers.google.com/ar/develop/unity-arf/cloud-anchors/developer-guide-ios#token-signed-jwt-authorization">Token
        /// authorization</a>, the maximum allowed value is 365 days. When using an <a
        /// href="https://developers.google.com/ar/develop/unity-arf/cloud-anchors/developer-guide-android#api-key-authorization">API
        /// Key</a> to authenticate with the ARCore API, the maximum allowed value is 1 day.
        ///
        /// Cloud Anchors requires a <c><see cref="ARCoreExtensionsConfig.CloudAnchorMode"/></c>
        /// with <c><see cref="CloudAnchorMode.Enabled"/></c> set on this session. Use
        /// <c><see cref="ARCoreExtensionsConfig.CloudAnchorMode"/></c> to enable the Cloud Anchors
        /// API.
        ///
        /// Hosting a Cloud Anchor works best when ARCore is able to create a good
        /// feature map around the <c><see cref="ARAnchor"/></c>. Use <c><see
        /// cref="EstimateFeatureMapQualityForHosting(this ARAnchorManager anchorManager,
        /// Pose)"/></c> to determine the quality of visual features seen by ARCore in the preceding
        /// few seconds. Cloud Anchors hosted using higher quality features will generally result in
        /// quicker and more accurately resolved Cloud Anchor poses.
        ///
        /// This launches an asynchronous operation used to query the Google Cloud ARCore API. See
        /// <c><see
        /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
        /// for information on obtaining results and cancelling the operation.
        ///
        /// ARCore can have up to 40 simultaneous Cloud Anchor operations, including
        /// resolved anchors and active hosting operations.
        ///
        /// </summary>
        /// <param name="anchorManager">The <c><see cref="ARAnchorManager"/></c> instance.</param>
        /// <param name="anchor">The local <c><see cref="ARAnchor"/></c> to be used as the
        /// basis to host a new Cloud Anchor.</param>
        /// <param name="ttlDays">The lifetime of the anchor in days. Must be positive. The
        /// maximum allowed value is 1 if using an API Key to authenticate with the
        /// ARCore API, otherwise the maximum allowed value is 365.</param>
        /// <returns>Returns a <c><see cref="HostCloudAnchorPromise"/></c>. See <c><see
        /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
        /// for more information on how to retrieve results from the Promise.</returns>
        /// </returns>
        public static HostCloudAnchorPromise HostCloudAnchorAsync(
            this ARAnchorManager anchorManager, ARAnchor anchor, int ttlDays)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                anchor == null || anchor.nativePtr == IntPtr.Zero ||
                anchor.AnchorHandle() == IntPtr.Zero)
            {
                return new HostCloudAnchorPromise(IntPtr.Zero);
            }

            IntPtr future = SessionApi.HostCloudAnchorAsync(
                ARCoreExtensions._instance.currentARCoreSessionHandle, anchor.AnchorHandle(),
                ttlDays);

            return new HostCloudAnchorPromise(future);
        }

        /// <summary>
        /// Set the token to use when authenticating with the ARCore API
        /// on the iOS platform. This should be called each time the application's
        /// token is refreshed.
        /// </summary>
        /// <param name="anchorManager">The <c>ARAnchorManager</c> instance.</param>
        /// <param name="authToken">The authentication token to set.</param>
        public static void SetAuthToken(this ARAnchorManager anchorManager, string authToken)
        {
            // Only iOS needs AuthToken for Cloud Anchor persistence.
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero)
            {
                return;
            }

            if (!string.IsNullOrEmpty(RuntimeConfig.Instance.IOSCloudServicesApiKey))
            {
                Debug.LogError(
                    "Cannot set token in applications built using the 'API Key' " +
                    "authentication strategy. To use it, check Edit > Project Settings " +
                    "> XR Plug-in Management > ARCore Extensions > iOS Support Enabled and " +
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
        /// <param name="referencePointManager">The <c><see cref="ARAnchorManager"/></c>
        /// instance.</param>
        /// <param name="referencePoint">The local <c><see cref="ARAnchor"/></c> to be used as the
        /// basis to host a new cloud reference point.</param>
        /// <returns>If successful, a <c><see cref="ARCloudReferencePoint"/></c>,
        /// otherwise <c>null</c>.</returns>
        ///
        /// @deprecated Use <c><see
        /// cref="ARAnchorManagerExtensions.HostCloudAnchorAsync(this ARAnchorManager, ARAnchor,
        /// int)"/></c> instead.
        [Obsolete("This method is deprecated. Use HostCloudAnchorAsync(ARAnchor, int) instead.")]
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
        /// A session can be resolving up to 40 Cloud Anchors at a given time.
        /// If resolving fails, the anchor will be automatically removed from the session.
        /// </summary>
        /// <param name="anchorManager">The <c>ARAnchorManager</c> instance.</param>
        /// <param name="cloudAnchorId">String representing the Cloud Anchor.</param>
        /// <returns>If successful, a <c><see cref="ARCloudAnchor"/></c>,
        /// otherwise <c>null</c>.</returns>
        /// @deprecated Use <c><see
        /// cref="ARAnchorManagerExtensions.ResolveCloudAnchorAsync(this ARAnchorManager,
        /// string)"/></c> instead.
        [Obsolete("This method has been deprecated. " +
            "Please use ResolveCloudAnchorAsync(ARAnchor, string) instead.")]
        public static ARCloudAnchor ResolveCloudAnchorId(
            this ARAnchorManager anchorManager, string cloudAnchorId)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                string.IsNullOrEmpty(cloudAnchorId))
            {
                return null;
            }

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
        /// Attempts to resolve a Cloud Anchor using the provided <c>cloudAnchorId</c>.
        /// The Cloud Anchor must previously have been hosted by <c><see
        /// cref="ARAnchorManagerExtensions.HostCloudAnchorAsync(this ARAnchorManager, ARAnchor,
        /// int)"/></c> or another Cloud Anchor hosting method within the allotted <c>ttlDays</c>.
        /// See the <a
        /// href="https://developers.google.com/ar/develop/unity-arf/cloud-anchors/developer-guide">Cloud
        /// Anchors developer guide</a> for more information.
        ///
        /// When resolving a Cloud Anchor, the ARCore API periodically compares visual
        /// features from the scene against the anchor's 3D feature map to pinpoint the
        /// user's position and orientation relative to the anchor. When it finds a
        /// match, the task completes.
        ///
        /// This launches an asynchronous operation used to query the Google Cloud ARCore API. See
        /// <c><see
        /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
        /// for information on obtaining results and cancelling the operation.
        ///
        /// Cloud Anchors requires a <c><see cref="ARCoreExtensionsConfig.CloudAnchorMode"/></c>
        /// with <c><see cref="CloudAnchorMode.Enabled"/></c> set on this session. Use
        /// <c><see cref="ARCoreExtensionsConfig.CloudAnchorMode"/></c> to enable the Cloud Anchors
        /// API.
        ///
        /// ARCore can have up to 40 simultaneous Cloud Anchor operations, including
        /// resolved anchors and active hosting operations.
        ///
        /// </summary>
        /// <param name="anchorManager">The <c>ARAnchorManager</c> instance.</param>
        /// <param name="cloudAnchorId">The Cloud Anchor ID to resolve.</param>
        /// <returns>Returns a <c><see cref="ResolveCloudAnchorPromise"/></c>. See <c><see
        /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
        /// for more information on how to retrieve results from the Promise.</returns>
        public static ResolveCloudAnchorPromise ResolveCloudAnchorAsync(
            this ARAnchorManager anchorManager, string cloudAnchorId)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                string.IsNullOrEmpty(cloudAnchorId))
            {
                return new ResolveCloudAnchorPromise(IntPtr.Zero);
            }

            IntPtr future = SessionApi.ResolveCloudAnchorAsync(
                ARCoreExtensions._instance.currentARCoreSessionHandle, cloudAnchorId);

            return new ResolveCloudAnchorPromise(future);
        }

        /// <summary>
        /// Creates a new local cloud reference point from the provided Id.
        /// </summary>
        /// <param name="referencePointManager">The <c>ARAnchorManager</c> instance.</param>
        /// <param name="cloudReferenceId">String representing the cloud reference.</param>
        /// <returns>If successful, a <c><see cref="ARCloudReferencePoint"/></c>,
        /// otherwise <c>null</c>.</returns>
        /// @deprecated Use <c><see
        /// cref="ARAnchorManagerExtensions.ResolveCloudAnchorAsync(this ARAnchorManager,
        /// string)"/></c> instead.
        [Obsolete("This method has been deprecated. " +
            "Please use ResolveCloudAnchorAsync(ARAnchor, string) instead.")]
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
        ///
        /// Cloud Anchors hosted using higher feature map quality will generally result
        /// in easier and more accurately resolved <c><see cref="ARCloudAnchor"/></c> poses.
        /// </summary>
        /// <param name="anchorManager">The <c>ARAnchorManager</c> instance.</param>
        /// <param name="pose">The camera pose to use in estimating the quality.</param>
        /// <returns>The estimated feature map quality.</returns>
        public static FeatureMapQuality EstimateFeatureMapQualityForHosting(
            this ARAnchorManager anchorManager, Pose pose)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero)
            {
                return FeatureMapQuality.Insufficient;
            }

            return SessionApi.EstimateFeatureMapQualityForHosting(
                ARCoreExtensions._instance.currentARCoreSessionHandle, pose);
        }

        /// <summary>
        /// Creates a new anchor at the specified geospatial location and orientation
        /// relative to the Earth.
        ///
        /// Latitude and longitude are defined by the
        /// <a href="https://en.wikipedia.org/wiki/World_Geodetic_System">WGS84
        /// specification</a>, and altitude values are defined by the elevation above
        /// the WGS84 ellipsoid.
        /// To create an anchor using an altitude
        /// relative to the Earth's terrain instead of altitude above the WGS84
        /// ellipsoid, use <c><see
        /// cref="ARAnchorManagerExtensions.ResolveAnchorOnTerrainAsync(this ARAnchorManager,
        /// double, double, double, Quaternion)"/></c>.
        ///
        /// Creating anchors near the north pole or south pole is not supported. If
        /// the latitude is within 0.1 degrees of the north pole or south pole (90
        /// degrees or -90 degrees), this function will return <c>null</c>.
        ///
        /// The rotation provided by <c><paramref name="eunRotation"/></c> is a rotation
        /// with respect to an east-up-north coordinate frame. An identity rotation
        /// will have the anchor oriented such that X+ points to the east, Y+ points up
        /// away from the center of the earth, and Z+ points to the north.
        ///
        /// An anchor's tracking state will be <c><see cref="TrackingState.None"/></c> while
        /// <c><see cref="AREarthManager.EarthTrackingState"/></c> is
        /// <c><see cref="TrackingState.None"/></c>.
        /// The tracking state will permanently become <c><see cref="TrackingState.None"/></c> if
        /// the configuration is set to <c><see cref="GeospatialMode.Disabled"/></c>.
        /// </summary>
        /// <param name="anchorManager">The <c><see cref="ARAnchorManager"/></c> instance.</param>
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
        /// If successful, a <c><see cref="ARGeospatialAnchor"/></c>, otherwise, <c>null</c>.
        /// </returns>
        public static ARGeospatialAnchor AddAnchor(
            this ARAnchorManager anchorManager, double latitude, double longitude,
            double altitude, Quaternion eunRotation)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero)
            {
                return null;
            }

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

        /// <summary>
        /// Creates a <c><see cref="ARGeospatialAnchor"/></c> at a specified horizontal position and
        /// altitude relative to the horizontal position's terrain. See the <a
        /// href="https://developers.google.com/ar/develop/geospatial/unity-arf/anchors#terrain-anchors">Terrain
        /// anchors developer guide</a> for more information. If the altitude relative to the WGS84
        /// ellipsoid is known, use
        /// <c><see cref="ARAnchorManagerExtensions.AddAnchor(this ARAnchorManager, double, double,
        /// double, Quaternion)"/></c> instead.
        ///
        /// The specified <c><paramref name="altitudeAboveTerrain"/></c> is interpreted to be
        /// relative to the Earth's terrain (or floor) at the specified latitude/longitude
        /// geospatial coordinates, rather than relative to the WGS84 ellipsoid.
        /// Specifying an altitude of 0 will position the anchor directly on the
        /// terrain (or floor) whereas specifying a positive altitude will position
        /// the anchor above the terrain (or floor), against the direction of gravity.
        ///
        /// This creates a new <c><see cref="ARGeospatialAnchor"/></c> and schedules a task to
        /// resolve the anchor's pose using the given parameters. You may resolve multiple anchors
        /// at a time, but a session cannot be tracking more than 100 Terrain anchors at
        /// time.
        ///
        /// The returned Terrain anchor will have its <c><see
        /// cref="ARGeospatialAnchor.terrainAnchorState"/></c> set to
        /// <c><see cref="TerrainAnchorState.TaskInProgress"/></c>, and its
        /// <c><see cref="ARGeospatialAnchor.trackingState"/></c> set to
        /// <c><see cref="TrackingState.None"/></c>.
        /// The anchor will remain in this state until its pose has been successfully resolved. If
        /// the resolving task results in an error, the anchor's
        /// <c><see cref="ARGeospatialAnchor.terrainAnchorState"/></c> will detail error
        /// information.
        ///
        /// Creating a Terrain anchor requires <c><see cref="AREarthManager.EarthState"/></c> to be
        /// <c><see cref="EarthState.Enabled"/></c>,
        /// and <c><see cref="AREarthManager.EarthTrackingState"/></c> to be
        /// <c><see cref="TrackingState.Tracking"/></c> or
        /// <c><see cref="TrackingState.Paused"/></c>. If it is not, then
        /// If it is not, then this function returns <c>null</c>. This call also requires a working
        /// internet. connection to communicate with the ARCore API on Google Cloud. ARCore will
        /// continue to retry if it is unable to establish a connection to the ARCore
        /// service.
        ///
        /// Latitude and longitude are defined by the
        /// <a href="https://en.wikipedia.org/wiki/World_Geodetic_System">WGS84
        /// specification</a>. Creating anchors near the north pole or south pole is not supported.
        /// If the latitude is within 0.1 degrees of the north pole or south pole (90
        /// degrees or -90 degrees), this function will return <c>null</c>.
        ///
        /// The rotation provided by <c><paramref name="eunRotation"/></c> is a rotation
        /// with respect to an east-up-north coordinate frame. An identity rotation
        /// will have the anchor oriented such that X+ points to the east, Y+ points up
        /// away from the center of the earth, and Z+ points to the north.
        ///
        /// An anchor's tracking state will be <c><see cref="TrackingState.None"/></c> while
        /// <c><see cref="AREarthManager.EarthTrackingState"/></c> is
        /// <c><see cref="TrackingState.None"/></c>. The tracking state will permanently become
        /// <c><see cref="TrackingState.None"/></c> if the configuration is set to
        /// <c><see cref="GeospatialMode.Disabled"/></c>.
        /// </summary>
        /// <param name="anchorManager">The <c><see cref="ARAnchorManager"/></c> instance.</param>
        /// <param name="latitude">
        /// The latitude of the anchor relative to the WGS84 ellipsoid.</param>
        /// <param name="longitude">
        /// The longitude of the anchor relative to the WGS84 ellipsoid.</param>
        /// <param name="altitudeAboveTerrain">
        /// The altitude of the anchor above the Earth's terrain (or floor).</param>
        /// <param name="eunRotation">The rotation of the anchor with respect to
        /// the east-up-north coordinate frame where X+ points east, Y+ points up
        /// away from gravity, and Z+ points north. A rotation about the Y+ axis
        /// creates a rotation counterclockwise from north.</param>
        /// <returns>
        /// If successful, a <c><see cref="ARGeospatialAnchor"/></c>, otherwise, <c>null</c>.
        /// </returns>
        /// @deprecated Use <c><see
        /// cref="ARAnchorManagerExtensions.ResolveAnchorOnTerrainAsync(this ARAnchorManager,
        /// double, double, double, Quaternion)"/></c> instead.
        [Obsolete("This method has been deprecated. Please use " +
            "ResolveAnchorOnTerrainAsync(double, double, double, Quaternion) instead.")]
        public static ARGeospatialAnchor ResolveAnchorOnTerrain(this ARAnchorManager anchorManager,
            double latitude, double longitude, double altitudeAboveTerrain, Quaternion eunRotation)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero)
            {
                return null;
            }

            IntPtr earthHandle = SessionApi.AcquireEarth(
                ARCoreExtensions._instance.currentARCoreSessionHandle);
            if (earthHandle == IntPtr.Zero)
            {
                Debug.LogError("Failed to acquire earth.");
                return null;
            }

            IntPtr anchorHandle = EarthApi.ResolveAnchorOnTerrain(
                ARCoreExtensions._instance.currentARCoreSessionHandle, earthHandle, latitude,
                longitude, altitudeAboveTerrain, eunRotation);
            if (anchorHandle == IntPtr.Zero)
            {
                Debug.LogError("Failed to add geospatial terrain anchor.");
                return null;
            }

            // Create the GameObject that is the Geospatial Terrain anchor.
            ARGeospatialAnchor anchor =
                new GameObject(_terrainAnchorName).AddComponent<ARGeospatialAnchor>();
            if (anchor)
            {
                anchor.SetAnchorHandle(anchorHandle);
            }

            // Parent the new Geospatial Terrain anchor to the session origin.
            anchor.transform.SetParent(
                ARCoreExtensions._instance.SessionOrigin.trackablesParent, false);
            return anchor;
        }

        /// <summary>
        /// Asynchronously creates a <c><see cref="ARGeospatialAnchor"/></c> at a specified
        /// horizontal position and altitude relative to the horizontal position's terrain.
        /// See the <a
        /// href="https://developers.google.com/ar/develop/geospatial/java/anchors#terrain-anchors">Terrain
        /// anchors developer guide</a> for more information.
        ///
        /// The specified <c><paramref name="altitudeAboveTerrain"/></c> is interpreted to be
        /// relative to the terrain at the specified latitude/longitude geodetic coordinates, rather
        /// than relative to the WGS-84 ellipsoid. Specifying an altitude of 0 will position the
        /// anchor directly on the terrain whereas specifying a positive altitude will position the
        /// anchor above the terrain, against the direction of gravity.
        ///
        /// This launches an asynchronous operation used to query the Google Cloud ARCore API. See
        /// <c><see
        /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
        /// for information on obtaining results and cancelling the operation.
        ///
        /// Creating anchors near the north pole or south pole is not supported. If the latitude is
        /// within 0.1 degrees of the north pole or south pole (90 degrees or -90 degrees),
        /// <c><see cref="ResolveAnchorOnTerrainResult.Anchor"/></c> will be <c>null</c>.
        ///
        /// This schedules a task to resolve the anchor's pose using the given parameters. You may
        /// resolve multiple anchors at a time, but a session cannot be tracking more than 100
        /// Terrain and Rooftop anchors at time.
        ///
        /// Creating a Terrain anchor requires an <c><see cref="AREarthManager.EarthState"/></c> to
        /// be <c><see cref="EarthState.Enabled"/></c> and
        /// <c><see cref="AREarthManager.EarthTrackingState"/></c> to be
        /// <c><see cref="TrackingState.Tracking"/></c> or
        /// <c><see cref="TrackingState.Paused"/></c>. If it is not, then
        /// <c><see cref="ResolveAnchorOnTerrainResult.Anchor"/></c> will be <c>null</c>. This call
        /// also requires a working internet connection to communicate with the ARCore API on Google
        /// Cloud. See <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/enable">Enable the
        /// Geospatial API</a> for more details on required permissions and setup steps. ARCore will
        /// continue to retry if it is unable to establish a connection to the ARCore service.
        ///
        /// Latitude and longitude are defined by the
        /// <a href="https://en.wikipedia.org/wiki/World_Geodetic_System">WGS84
        /// specification</a>.
        ///
        /// The rotation provided by <c><paramref name="eunRotation"/></c> is a rotation with
        /// respect to an east-up-north coordinate frame. An identity rotation will have the anchor
        /// oriented such that X+ points to the east, Y+ points up away from the center of the
        /// earth, and Z+ points to the north.
        ///
        /// To create a quaternion that represents a clockwise angle theta from north around the +Y
        /// anchor frame axis, use the following formula:
        /// <code>
        /// Quaternion.AngleAxis(180f - theta, Vector3.up);
        /// </code>
        ///
        /// </summary>
        /// <param name="anchorManager">The <c>ARAnchorManager</c> instance.</param>
        /// <param name="latitude">
        /// The latitude of the anchor relative to the WGS84 ellipsoid.</param>
        /// <param name="longitude">
        /// The longitude of the anchor relative to the WGS84 ellipsoid.</param>
        /// <param name="altitudeAboveTerrain">
        /// The altitude of the anchor above the Earth's terrain.</param>
        /// <param name="eunRotation">The rotation of the anchor with respect to the east-up-north
        /// coordinate frame where X+ points east, Y+ points up away from gravity, and Z+ points
        /// north. A rotation about the Y+ axis creates a rotation counterclockwise from north.
        /// </param>
        /// <returns>Returns a <c><see cref="ResolveAnchorOnTerrainPromise"/></c>. See <c><see
        /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
        /// for more information on how to retrieve results from the Promise.</returns>
        public static ResolveAnchorOnTerrainPromise ResolveAnchorOnTerrainAsync(
            this ARAnchorManager anchorManager, double latitude, double longitude,
            double altitudeAboveTerrain, Quaternion eunRotation)
        {
            IntPtr earthHandle = SessionApi.AcquireEarth(
                ARCoreExtensions._instance.currentARCoreSessionHandle);
            if (earthHandle == IntPtr.Zero)
            {
                Debug.LogError("Failed to acquire earth.");
                return new ResolveAnchorOnTerrainPromise(IntPtr.Zero);
            }

            IntPtr future = EarthApi.ResolveAnchorOnTerrainFuture(
                ARCoreExtensions._instance.currentARCoreSessionHandle, earthHandle, latitude,
                longitude, altitudeAboveTerrain, eunRotation, IntPtr.Zero);
            TrackableApi.Release(earthHandle);

            return new ResolveAnchorOnTerrainPromise(future);
        }
    }
}
