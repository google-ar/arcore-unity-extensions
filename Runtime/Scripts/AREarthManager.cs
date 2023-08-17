//-----------------------------------------------------------------------
// <copyright file="AREarthManager.cs" company="Google LLC">
//
// Copyright 2021 Google LLC
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
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Provides localization in Earth-relative coordinates.
    ///
    /// <c><see cref="ARCoreExtensionsConfig.GeospatialMode"/></c> must be
    /// <c><see cref="GeospatialMode.Enabled"/></c> in order to make use of the Geospatial API.
    /// Not all devices support <c><see cref="GeospatialMode.Enabled"/></c>, use
    /// <c><see cref="AREarthManager.IsGeospatialModeSupported"/></c> to find whether the current
    /// device supports enabling this mode.
    ///
    /// <c><see cref="AREarthManager.CameraGeospatialPose"/></c> should only be used when
    /// <c><see cref="AREarthManager.EarthTrackingState"/></c> is
    /// <c><see cref="TrackingState.Tracking"/></c>, and otherwise
    /// should not be used. If the <c><see cref="EarthTrackingState"/></c> does not become
    /// <c>Tracking</c>, then <c><see cref="AREarthManager.EarthState"/></c> may contain more
    /// information on this failure.
    /// </summary>
    public class AREarthManager : MonoBehaviour
    {
        /// <summary>
        /// Gets the <c><see cref="EarthState"/></c> for the latest frame.
        /// </summary>
        public EarthState EarthState
        {
            get
            {
                if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero)
                {
                    return EarthState.ErrorSessionNotReady;
                }

                if (ARCoreExtensions._instance.ARCoreExtensionsConfig == null ||
                    ARCoreExtensions._instance.ARCoreExtensionsConfig.GeospatialMode ==
                    GeospatialMode.Disabled)
                {
                    return EarthState.ErrorGeospatialModeDisabled;
                }

                return EarthApi.GetEarthState(
                    ARCoreExtensions._instance.currentARCoreSessionHandle);
            }
        }

        /// <summary>
        /// Gets the tracking state of Earth for the latest frame.
        /// </summary>
        public TrackingState EarthTrackingState
        {
            get
            {
                if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero)
                {
                    return TrackingState.None;
                }

                return EarthApi.GetEarthTrackingState(
                    ARCoreExtensions._instance.currentARCoreSessionHandle);
            }
        }

        /// <summary>
        /// Gets the <c><see cref="GeospatialPose"/></c> for the camera in the latest frame,
        /// describing the geodedic position of the device.
        ///
        /// The position of the pose is located at the device's camera, while the orientation
        /// closely approximates the orientation of the display.
        ///
        /// Note: This pose is only valid when
        /// <c><see cref="EarthTrackingState"/></c> is <c><see cref="TrackingState.Tracking"/></c>;
        /// otherwise, it should not be used.
        /// </summary>
        public GeospatialPose CameraGeospatialPose
        {
            get
            {
                var geospatialPose = new GeospatialPose();
                if (ARCoreExtensions._instance.currentARCoreSessionHandle != IntPtr.Zero)
                {
                    EarthApi.TryGetCameraGeospatialPose(
                        ARCoreExtensions._instance.currentARCoreSessionHandle, ref geospatialPose);
                }

                return geospatialPose;
            }
        }

        /// <summary>
        /// Gets the availability of the Visual Positioning System (VPS) at a specified horizontal
        /// position. The availability of VPS in a given location helps to improve the quality of
        /// Geospatial localization and tracking accuracy. See the <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/check-vps-availability">developer's
        /// guide on VPS availability</a>.
        ///
        /// This launches an asynchronous operation used to query the Google Cloud ARCore API. See
        /// <c><see
        /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
        /// for information on obtaining results and cancelling the operation.
        ///
        /// Your app must be properly set up to communicate with the Google Cloud ARCore API in
        /// order to obtain a result from this call. See <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/enable">Enable the
        /// Geospatial API</a> for more details on required permissions and setup steps.
        /// </summary>
        /// <param name="latitude">The latitude in degrees.</param>
        /// <param name="longitude">The longitude in degrees.</param>
        /// <returns>Returns a <c><see cref="VpsAvailabilityPromise"/></c> used in a <a
        /// href="https://docs.unity3d.com/Manual/Coroutines.html">Unity Coroutine</a>. It updates
        /// its results in frame update events. See <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/check-vps-availability">Check
        /// VPS Availability</a> for a usage example.</returns>
        public static VpsAvailabilityPromise CheckVpsAvailabilityAsync(double latitude,
            double longitude)
        {
            if (ARSession.state != ARSessionState.Ready &&
                ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                // ARCore is not available, return default value.
                return new VpsAvailabilityPromise();
            }

#if UNITY_ANDROID
            ARPrestoApi.SetSessionRequired(true);
#elif UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            SessionApi.SetAuthToken(ARCoreExtensions._instance.currentARCoreSessionHandle);
#endif
            IntPtr sessionHandle = ARCoreExtensions._instance.currentARCoreSessionHandle;
            IntPtr future =
                (sessionHandle == IntPtr.Zero)
                    ? IntPtr.Zero
                    : FutureApi.CheckVpsAvailabilityAsync(sessionHandle, latitude, longitude);
#if UNITY_ANDROID
            ARPrestoApi.SetSessionRequired(false);
#endif
            return new VpsAvailabilityPromise(future);
        }

        /// <summary>
        /// Gets the availability of the Visual Positioning System (VPS) at a specified horizontal
        /// position. The availability of VPS in a given location helps to improve the quality of
        /// Geospatial localization and tracking accuracy. See the <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/check-vps-availability">developer's
        /// guide on VPS availability</a>.
        ///
        /// This launches an asynchronous operation used to query the Google Cloud ARCore API. See
        /// <c><see
        /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
        /// for information on obtaining results and cancelling the operation.
        ///
        /// Your app must be properly set up to communicate with the Google Cloud ARCore API in
        /// order to obtain a result from this call. See <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/enable">Enable the
        /// Geospatial API</a> for more details on required permissions and setup steps.
        /// </summary>
        /// <param name="latitude">The latitude in degrees.</param>
        /// <param name="longitude">The longitude in degrees.</param>
        /// <returns>Returns a <c><see cref="VpsAvailabilityPromise"/></c>. See <c><see
        /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
        /// for more information on how to retrieve results from the Promise.</returns>
        ///
        /// @deprecated Please use <c><see cref="CheckVpsAvailabilityAsync(double, double)"/></c>
        /// instead.
        [Obsolete("This method has been deprecated. Please use " +
            "CheckVpsAvailabilityAsync(double, double) instead.")]
        public static VpsAvailabilityPromise CheckVpsAvailability(double latitude,
            double longitude)
        {
            return CheckVpsAvailabilityAsync(latitude, longitude);
        }

        /// <summary>
        /// Checks whether the provided <c><see cref="GeospatialMode"/></c> is supported on this
        /// device. The current list of supported devices is documented on the <a
        /// href="https://developers.google.com/ar/devices">ARCore supported devices</a>
        /// page. A device may be incompatible with a given mode due to insufficient sensor
        /// capabilities.
        /// </summary>
        /// <param name="mode">The desired geospatial mode.</param>
        /// <returns>
        /// Indicates whether the given mode is supported on this device.
        /// It will return <c><see cref="FeatureSupported.Unknown"/></c> if the session is still
        /// under initialization.
        /// </returns>
        public FeatureSupported IsGeospatialModeSupported(GeospatialMode mode)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero)
            {
                return FeatureSupported.Unknown;
            }

            return SessionApi.IsGeospatialModeSupported(
                ARCoreExtensions._instance.currentARCoreSessionHandle, mode);
        }

        /// <summary>
        /// Gets the <c><see cref="GeospatialPose"/></c> from a local pose relative to camera,
        /// relative to the last frame. The rotation quaternion of <c><see
        /// cref="GeospatialPose"/></c> is a rotation with respect to an East-Up-North coordinate
        /// frame. An identity quaternion will have the anchor oriented such that X+ points to the
        /// east, Y+ points up away from the center of the earth, and Z+ points to the north.
        ///
        /// <c><see cref="EarthTrackingState"/></c> must be in the <c><see
        /// cref="TrackingState.Tracking"/></c> state for this to return a valid pose.
        /// The <c>Heading</c> property will be zero for a <c><see cref="GeospatialPose"/></c>
        /// returned by this method.
        /// </summary>
        /// <param name="pose">The pose to be converted.</param>
        /// <returns>A <c><see cref="GeospatialPose"/></c>.</returns>
        public GeospatialPose Convert(Pose pose)
        {
            var geospatialPose = new GeospatialPose();
            if (ARCoreExtensions._instance.currentARCoreSessionHandle != IntPtr.Zero)
            {
                EarthApi.Convert(ARCoreExtensions._instance.currentARCoreSessionHandle, pose,
                                 ref geospatialPose);
            }

            return geospatialPose;
        }

        /// <summary>
        /// Gets the local pose relative to camera from the <c><see cref="GeospatialPose"/></c>.
        /// <c><see cref="AREarthManager.EarthTrackingState"/></c> must be in the <c><see
        /// cref="TrackingState.Tracking"/></c> state for this to return a valid pose.
        /// </summary>
        /// <param name="geospatialPose">
        /// A <c><see cref="GeospatialPose"/></c> with valid <c>Latitude</c>, <c>Longitude</c>,
        /// <c>Altitude</c> and <c>EunRotation</c>.
        /// </param>
        /// <returns>A <c><see cref="Pose"/></c>.</returns>
        public Pose Convert(GeospatialPose geospatialPose)
        {
            ApiPose apiPose = new ApiPose();
            if (ARCoreExtensions._instance.currentARCoreSessionHandle != IntPtr.Zero)
            {
                EarthApi.Convert(ARCoreExtensions._instance.currentARCoreSessionHandle,
                    geospatialPose, ref apiPose);
            }

            return apiPose.ToUnityPose();
        }
    }
}
