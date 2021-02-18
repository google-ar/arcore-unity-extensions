//-----------------------------------------------------------------------
// <copyright file="ARCoreExtensionsCameraConfigFilter.cs" company="Google LLC">
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
    using System;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// The <c><see cref="ARCoreExtensionsCameraConfigFilter"/></c> class is used by the camera
    /// manager to derive a list of camera configurations available on the device at runtime.
    /// </summary>
    /// <remarks>
    /// It is possible to select options in such a way that some devices will have no available
    /// configurations at runtime. In this case, your app will not run.
    ///
    /// Beginning with ARCore SDK 1.15.0, some devices support additional camera configs with lower
    /// GPU texture resolutions than the device's default GPU texture resolution. See the
    /// <a href="https://developers.google.com/ar/discover/supported-devices">ARCore supported
    /// devices</a> page for an up to date list of devices with this capability.
    ///
    /// An app may adjust its capabilities at runtime by selecting a wider range of config filters
    /// and using <c><see cref="ARCoreExtensions.OnChooseXRCameraConfiguration"/></c> to specify a
    /// selection function. In that function the app may then adjust its runtime settings and
    /// select an appropriate camera configuration.
    ///
    /// If no callback is registered, ARCore Extensions will use the first
    /// <c><see cref="XRCameraConfiguration"/></c> in the list of available configurations.
    /// </remarks>
    [CreateAssetMenu(
        fileName = "ARCoreExtensionsCameraConfigFilter",
        menuName = "ARCore Extensions/Camera Config Filter",
        order = 2)]
    public class ARCoreExtensionsCameraConfigFilter : ScriptableObject
    {
        /// <summary>
        /// The camera frame rate filter for the currently selected camera.
        /// </summary>
        [DynamicHelp("GetTargetCameraFramerateInfo")]
        [EnumFlags(typeof(CameraConfigTargetFps))]
        public CameraConfigTargetFps TargetCameraFramerate =
            CameraConfigTargetFps.Target30FPS | CameraConfigTargetFps.Target60FPS;

        /// <summary>
        /// Allows an app to use or disable a hardware depth sensor, such as a
        /// time-of-flight sensor (or ToF sensor), if present on the device.
        /// </summary>
        [DynamicHelp("GetDepthSensorUsageInfo")]
        [EnumFlags(typeof(CameraConfigDepthSensorUsage))]
        public CameraConfigDepthSensorUsage DepthSensorUsage =
            CameraConfigDepthSensorUsage.RequireAndUse | CameraConfigDepthSensorUsage.DoNotUse;

        /// <summary>
        /// Allows an app to use or disable additional cameras to improve tracking.
        /// </summary>
        [DynamicHelp("GetStereoCameraUsageInfo")]
        [EnumFlags(typeof(CameraConfigStereoCameraUsage))]
        public CameraConfigStereoCameraUsage StereoCameraUsage =
            CameraConfigStereoCameraUsage.RequireAndUse | CameraConfigStereoCameraUsage.DoNotUse;

        /// <summary>
        /// Reflection function used by 'DynamicHelp' for
        /// <c><see cref="TargetCameraFramerate"/></c>.
        /// </summary>
        /// <returns>The help attribute of target camera framerate filter.</returns>
        public HelpAttribute GetTargetCameraFramerateInfo()
        {
            if ((TargetCameraFramerate & CameraConfigTargetFps.Target30FPS) == 0)
            {
                if (TargetCameraFramerate == 0)
                {
                    return new HelpAttribute(
                        "No options are selected, " +
                        "there will be no camera configs and this app will fail to run.",
                        HelpAttribute.HelpMessageType.Error);
                }

                return new HelpAttribute(
                    "Target30FPS is not selected, this may cause no camera config be available " +
                    "for this filter and the app may not run on all devices.",
                    HelpAttribute.HelpMessageType.Warning);
            }

            return null;
        }

        /// <summary>
        /// Reflection function used by 'DynamicHelp' for <c><see cref="DepthSensorUsage"/></c>.
        /// </summary>
        /// <returns>The help attribute of the hardware depth sensor usage filter, such as a
        /// time-of-flight (ToF) sensor.</returns>
        public HelpAttribute GetDepthSensorUsageInfo()
        {
            if ((DepthSensorUsage & CameraConfigDepthSensorUsage.DoNotUse) == 0)
            {
                if (DepthSensorUsage == 0)
                {
                    return new HelpAttribute(
                        "No options are selected, " +
                        "there will be no camera configs and this app will fail to run.",
                        HelpAttribute.HelpMessageType.Error);
                }

                return new HelpAttribute(
                    "DoNotUse is not selected, this may cause no camera config be available " +
                    "for this filter and the app may not run on all devices.",
                    HelpAttribute.HelpMessageType.Warning);
            }

            return null;
        }

        /// <summary>
        /// Reflection function used by 'DynamicHelp' for <c><see cref="StereoCameraUsage"/></c>.
        /// </summary>
        /// <returns>The help attribute of stereo sensor usage filter.</returns>
        public HelpAttribute GetStereoCameraUsageInfo()
        {
            if ((StereoCameraUsage & CameraConfigStereoCameraUsage.DoNotUse) == 0)
            {
                if (StereoCameraUsage == 0)
                {
                    return new HelpAttribute(
                        "No options are selected, " +
                        "there will be no camera configs and this app will fail to run.",
                        HelpAttribute.HelpMessageType.Error);
                }

                return new HelpAttribute(
                    "DoNotUse is not selected, this may cause no camera config be available " +
                    "for this filter and the app may not run on all devices.",
                    HelpAttribute.HelpMessageType.Warning);
            }

            return null;
        }

        /// <summary>
        /// Unity's OnValidate.
        /// </summary>
        public void OnValidate()
        {
            if ((TargetCameraFramerate & CameraConfigTargetFps.Target30FPS) == 0)
            {
                if (TargetCameraFramerate == 0)
                {
                    Debug.LogError(
                        "No options in Target Camera Framerate are selected, " +
                        "there will be no camera configs and this app will fail to run.");
                }

                Debug.LogWarning("Target30FPS is not selected, this may cause " +
                   "no camera config be available for this filter and " +
                   "the app may not run on all devices.");
            }

            if ((DepthSensorUsage & CameraConfigDepthSensorUsage.DoNotUse) == 0)
            {
                if (DepthSensorUsage == 0)
                {
                    Debug.LogError(
                        "No options in Depth Sensor Usage are selected, " +
                        "there will be no camera configs and this app will fail to run.");
                }

                Debug.LogWarning(
                    "DoNotUseDepthSensor is not selected, this may cause no camera config be " +
                    "available for this filter and the app may not run on all devices.");
            }

            if ((StereoCameraUsage & CameraConfigStereoCameraUsage.DoNotUse) == 0)
            {
                if (StereoCameraUsage == 0)
                {
                    Debug.LogError(
                        "No options in Stereo Camera Usage are selected, " +
                        "there will be no camera configs and this app will fail to run.");
                }

                Debug.LogWarning(
                    "DoNotUseStereoCamera is not selected, this may cause no camera config be " +
                    "available for this filter and the app may not run on all devices.");
            }
        }

        /// <summary>
        /// ValueType check if two ARCoreExtensionsCameraConfigFilter objects are equal.
        /// </summary>
        /// <param name="other">The other ARCoreExtensionsCameraConfigFilter.</param>
        /// <returns>True if the two ARCoreExtensionsCameraConfigFilter objects are
        /// value-type equal, otherwise false.</returns>
        public override bool Equals(object other)
        {
            ARCoreExtensionsCameraConfigFilter otherFilter =
                other as ARCoreExtensionsCameraConfigFilter;
            if (otherFilter == null)
            {
                return false;
            }

            if (TargetCameraFramerate != otherFilter.TargetCameraFramerate ||
                StereoCameraUsage != otherFilter.StereoCameraUsage ||
                DepthSensorUsage != otherFilter.DepthSensorUsage)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return a hash code for this object.
        /// </summary>
        /// <returns>A hash code value.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// ValueType copy from another ARCoreExtensionsCameraConfigFilter object into this one.
        /// </summary>
        /// <param name="otherFilter">The ARCoreExtensionsCameraConfigFilter to copy from.</param>
        public void CopyFrom(ARCoreExtensionsCameraConfigFilter otherFilter)
        {
            TargetCameraFramerate = otherFilter.TargetCameraFramerate;
            DepthSensorUsage = otherFilter.DepthSensorUsage;
            StereoCameraUsage = otherFilter.StereoCameraUsage;
        }
    }
}
