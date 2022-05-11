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
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Provides localization in Earth-relative coordinates.
    ///
    /// <c><see cref="ARCoreExtensionsConfig.GeospatialMode"/></c> must be
    /// <see cref="GeospatialMode"/>.<c>Enabled</c> in order to make use of the Geospatial API.
    /// Not all devices support <see cref="GeospatialMode"/>.<c>Enabled</c>, use
    /// <see cref="AREarthManager.IsGeospatialModeSupported"/> to find whether the current device
    /// supports enabling this mode.
    ///
    /// <c><see cref="AREarthManager.CameraGeospatialPose/></c> should only be used when
    /// <c><see cref="AREarthManager.EarthTrackingState"/></c> is <c>Tracking</c>, and otherwise
    /// should not be used. If the <c>EarthTrackingState</c> does not become <c>Tracking</c>,
    /// then <c><see cref="AREarthManager.EarthState"/></c> may contain more information on this
    /// failure.
    /// </summary>
    public class AREarthManager : MonoBehaviour
    {
        /// <summary>
        /// Gets the <see cref="Google.XR.ARCoreExtensions.EarthState"/> for the latest frame.
        /// </summary>
        public EarthState EarthState
        {
            get
            {
                if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero)
                {
                    return EarthState.ErrorInternal;
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
        /// <c><see cref="EarthTrackingState"/></c> is <c>TrackingState.Tracking</c>;
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
        /// Checks whether the provided <c><see cref="GeospatialMode"/></c> is supported on this
        /// device. The current list of supported devices is documented on the <a
        /// href="https://developers.google.com/ar/devices">ARCore supported devices</a>
        /// page. A device may be incompatible with a given mode due to insufficient sensor
        /// capabilities.
        /// </summary>
        /// <param name="mode">The desired geospatial mode.</param>
        /// <returns>
        /// Indicates whether the given mode is supported on this device.
        /// It will return <c>FeatureSupported.Unknown</c> if the session is still under
        /// initialization.
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
    }
}
