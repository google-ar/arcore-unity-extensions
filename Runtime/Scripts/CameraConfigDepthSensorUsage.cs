//-----------------------------------------------------------------------
// <copyright file="CameraConfigDepthSensorUsage.cs" company="Google LLC">
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
    using System.Diagnostics.CodeAnalysis;
    using UnityEngine;

    /// <summary>
    /// Type of hardware depth sensor usage for a camera config, such as a time-of-flight (ToF)
    /// sensor.
    /// </summary>
    [Flags]
    [SuppressMessage("UnityRules.UnityStyleRules", "US1200:FlagsEnumsMustBePlural",
                     Justification = "Usage is plural.")]
    public enum CameraConfigDepthSensorUsage
    {
        /// <summary>
        /// Indicates that a hardware depth sensor, such as a time-of-flight sensor (or ToF sensor),
        /// must be present on the device, and the hardware depth sensor will be used by ARCore.
        /// Not supported on all devices.
        /// </summary>
        [Tooltip("ARCore requires a hardware depth sensor, such as a time-of-flight sensor" +
                 " (or ToF sensor), to be present and will use it." +
                 " Not supported on all devices.")]
        RequireAndUse = 0x0001,

        /// <summary>
        /// Indicates that ARCore will not attempt to use a hardware depth sensor, such as a
        /// time-of-flight sensor (or ToF sensor), even if it is present.
        /// Most commonly used to filter camera configurations when the app requires
        /// exclusive access to the hardware depth sensor outside of ARCore, for example to
        /// support 3D mesh reconstruction. Available on all
        /// <a href="https://developers.google.com/ar/discover/supported-devices">
        /// ARCore supported devices</a>.
        /// </summary>
        [Tooltip("ARCore will not use the hardware depth sensor, such as a time-of-flight sensor" +
                 " (or ToF sensor), even if it is present." +
                 " Available on all supported devices.")]
        DoNotUse = 0x0002,
    }
}
