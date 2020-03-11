//-----------------------------------------------------------------------
// <copyright file="CameraConfigDepthSensorUsages.cs" company="Google">
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
    using System;

    /// <summary>
    /// Type of depth sensor usage for a camera config.
    /// </summary>
    [Flags]
    public enum CameraConfigDepthSensorUsages
    {
        /// <summary>
        /// Indicates that a depth sensor must be present on the device,
        /// and the depth sensor will be used by ARCore.
        /// Not supported on all devices.
        /// </summary>
        RequireAndUse = 0x0001,

        /// <summary>
        /// Indicates that ARCore will not attempt to use a depth sensor, even if it is present.
        /// Most commonly used to filter camera configurations when the app requires
        /// exclusive access to the depth sensor outside of ARCore, for example to
        /// support 3D mesh reconstruction. Available on all ARCore supported devices.
        /// </summary>
        DoNotUse = 0x0002,
    }
}
