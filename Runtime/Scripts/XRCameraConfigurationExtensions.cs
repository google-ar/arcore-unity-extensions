//-----------------------------------------------------------------------
// <copyright file="XRCameraConfigurationExtensions.cs" company="Google">
//
// Copyright 2020 Google LLC. All Rights Reserved.
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
        /// Gets the dimensions of the GPU-accessible external texture for this camera config.
        /// </summary>
        /// <param name="cameraConfig">An XRCameraConfiguration instance.
        /// </param>
        /// <returns>Returns the GPU texture dimensions.</returns>
        public static Vector2Int GetTextureDimensions(this XRCameraConfiguration cameraConfig)
        {
            return CameraConfigApi.GetTextureDimensions(
                ARCoreExtensions.Instance.CurrentARCoreSessionHandle,
                cameraConfig.nativeConfigurationHandle);
        }

        /// <summary>
        /// Gets minimum target camera capture frame rate range for this camera config.
        /// </summary>
        /// <param name="cameraConfig">An XRCameraConfiguration instance.
        /// </param>
        /// <returns>Returns the range from minimal target FPS to maximal target FPS.</returns>
        public static Vector2Int GetFPSRange(this XRCameraConfiguration cameraConfig)
        {
            return CameraConfigApi.GetFPSRange(
                ARCoreExtensions.Instance.CurrentARCoreSessionHandle,
                cameraConfig.nativeConfigurationHandle);
        }

        /// <summary>
        /// Gets the depth sensor usage for this camera config.
        /// </summary>
        /// <param name="cameraConfig">An XRCameraConfiguration instance.
        /// </param>
        /// <returns>Returns the depth sensor usage type.</returns>
        public static CameraConfigDepthSensorUsages GetDepthSensorUsages(
            this XRCameraConfiguration cameraConfig)
        {
            return CameraConfigApi.GetDepthSensorUsages(
                ARCoreExtensions.Instance.CurrentARCoreSessionHandle,
                cameraConfig.nativeConfigurationHandle);
        }
    }
}
