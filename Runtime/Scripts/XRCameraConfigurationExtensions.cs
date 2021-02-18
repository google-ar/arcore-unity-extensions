//-----------------------------------------------------------------------
// <copyright file="XRCameraConfigurationExtensions.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Extensions to AR Subsystem's XRCameraConfiguration struct.
    /// </summary>
    public static class XRCameraConfigurationExtensions
    {
        /// <summary>
        /// Gets the camera facing direction for this camera config.
        /// </summary>
        /// <param name="cameraConfig">An XRCameraConfiguration instance.
        /// </param>
        /// <returns>Returns the camera facing direction.</returns>
        public static CameraConfigFacingDirection GetFacingDirection(
            this XRCameraConfiguration cameraConfig)
        {
            return CameraConfigApi.GetFacingDirection(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                cameraConfig.nativeConfigurationHandle);
        }

        /// <summary>
        /// Gets the dimensions of the GPU-accessible external texture for this camera config.
        /// </summary>
        /// <param name="cameraConfig">An XRCameraConfiguration instance.
        /// </param>
        /// <returns>Returns the GPU texture dimensions.</returns>
        public static Vector2Int GetTextureDimensions(this XRCameraConfiguration cameraConfig)
        {
            return CameraConfigApi.GetTextureDimensions(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                cameraConfig.nativeConfigurationHandle);
        }

        /// <summary>
        /// Gets the target camera capture frame rate range for this camera config.
        /// </summary>
        /// <param name="cameraConfig">An XRCameraConfiguration instance.
        /// </param>
        /// <returns>Returns the range from minimal target FPS to maximal target FPS.</returns>
        public static Vector2Int GetFPSRange(this XRCameraConfiguration cameraConfig)
        {
            return CameraConfigApi.GetFPSRange(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                cameraConfig.nativeConfigurationHandle);
        }

        /// <summary>
        /// Gets the hardware depth sensor, such as a time-of-flight sensor (or ToF sensor), usage
        /// for this camera config.
        /// </summary>
        /// <param name="cameraConfig">An XRCameraConfiguration instance.
        /// </param>
        /// <returns>
        /// Returns the hardware depth sensor, such as a time-of-flight sensor (or ToF sensor),
        /// usage type.
        /// </returns>
        public static CameraConfigDepthSensorUsage GetDepthSensorUsage(
            this XRCameraConfiguration cameraConfig)
        {
            return CameraConfigApi.GetDepthSensorUsage(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                cameraConfig.nativeConfigurationHandle);
        }

        /// <summary>
        /// Gets the stereo camera usage for this camera config.
        /// </summary>
        /// <param name="cameraConfig">An XRCameraConfiguration instance.</param>
        /// <returns>Returns the stereo camera usage type.</returns>
        public static CameraConfigStereoCameraUsage GetStereoCameraUsage(
            this XRCameraConfiguration cameraConfig)
        {
            return CameraConfigApi.GetStereoCameraUsage(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                cameraConfig.nativeConfigurationHandle);
        }
    }
}
