//-----------------------------------------------------------------------
// <copyright file="CameraConfigTargetFps.cs" company="Google LLC">
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
    using UnityEngine;

    /// <summary>
    /// The camera frame rate filter for the currently selected camera.
    /// </summary>
    [Flags]
    public enum CameraConfigTargetFps
    {
        /// <summary>
        /// Target 30fps camera capture frame rate.
        ///
        /// Available on all
        /// <a href="https://developers.google.com/ar/discover/supported-devices">
        /// ARCore supported devices</a>.
        /// </summary>
        [Tooltip("Target 30fps camera capture frame rate. " +
                 "Available on all ARCore supported devices.")]
        Target30FPS = 0x0001,

        /// <summary>
        /// Target 60fps camera capture frame rate.
        ///
        /// Increases power consumption and may increase app memory usage.
        ///
        /// See the <a href="https://developers.google.com/ar/discover/supported-devices">
        /// ARCore supported devices</a> page for a list of
        /// devices that currently support 60fps.
        /// </summary>
        [Tooltip("Target 60fps camera capture frame rate on supported devices.")]
        Target60FPS = 0x0002,
    }
}
