//-----------------------------------------------------------------------
// <copyright file="CameraConfigFacingDirection.cs" company="Google LLC">
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

    /// <summary>
    /// Facing direction options for camera config.
    /// </summary>
    public enum CameraConfigFacingDirection
    {
        /// <summary>
        /// Back-facing (world) camera is enabled.
        ///
        /// Available on all <a href="https://developers.google.com/ar/discover/supported-devices">
        /// ARCore supported devices</a>.
        /// </summary>
        Back = 0,

        /// <summary>
        /// Front-facing (selfie) camera is enabled.
        ///
        /// See <a
        /// href="https://developers.google.com/ar/discover/supported-devices">
        /// ARCore supported devices</a> for available camera configs by device.
        ///
        /// To limit distribution of your app to only devices that have a
        /// front-facing camera, use <a
        /// href="https://developer.android.com/guide/topics/manifest/uses-feature-element
        /// #camera-hw-features."uses-feature</a> with <c>android.hardware.camera</c>.
        /// </summary>
        Front = 1,
    }
}
