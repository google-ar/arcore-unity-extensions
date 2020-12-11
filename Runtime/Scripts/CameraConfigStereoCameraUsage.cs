//-----------------------------------------------------------------------
// <copyright file="CameraConfigStereoCameraUsage.cs" company="Google LLC">
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
    /// Stereo Camera usage options.
    /// </summary>
    [Flags]
    [SuppressMessage("UnityRules.UnityStyleRules", "US1200:FlagsEnumsMustBePlural",
                     Justification = "Usage is plural.")]
    public enum CameraConfigStereoCameraUsage
    {
        /// <summary>
        /// A stereo camera is present on the device and will be used by ARCore.
        /// Not available on all ARCore supported devices.
        /// </summary>
        [Tooltip("ARCore requires a stereo camera to be present on the device. " +
                 "Not available on all ARCore supported devices.")]
        RequireAndUse = 0x0001,

        /// <summary>
        /// ARCore will not attempt to use a stereo camera, even if one is
        /// present.
        /// Valid on all <a href="https://developers.google.com/ar/discover/supported-devices">
        /// ARCore supported devices</a>.
        /// </summary>
        [Tooltip("ARCore will not use the stereo camera, even if it is present. " +
                 "Available on all supported devices.")]
        DoNotUse = 0x0002,
    }
}
