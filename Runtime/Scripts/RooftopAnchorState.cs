//-----------------------------------------------------------------------
// <copyright file="RooftopAnchorState.cs" company="Google LLC">
//
// Copyright 2023 Google LLC
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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Describes the Rooftop anchor state of an synchronous operation launched by <c><see
    /// cref="ARAnchorManagerExtensions.ResolveAnchorOnRooftopAsync(this ARAnchorManager, double,
    /// double, double, UnityEngine.Quaternion)"/></c>.
    ///
    /// Obttained by <c><see cref="ResolveAnchorOnRooftopResult.RooftopAnchorState"/></c>.
    /// </summary>
    public enum RooftopAnchorState
    {
        /// <summary>
        /// Not a valid value for a Rooftop anchor operation.
        /// </summary>
        None = 0,

        /// <summary>
        /// A resolving task for this Rooftop anchor has completed successfully.
        /// </summary>
        Success = 1,

        /// <summary>
        /// A resolving task for this Rooftop anchor has completed with an
        /// internal error. The app should not attempt to recover from this error.
        /// </summary>
        ErrorInternal = -1,

        /// <summary>
        /// The app cannot communicate with the ARCore Cloud because of an invalid
        /// authentication. Check Project Settings > XR Plug-in Management >
        /// ARCore Extensions for a valid authentication strategy.
        /// </summary>
        ErrorNotAuthorized = -2,

        /// <summary>
        /// There is no rooftop or terrain information at this location, such as the center of the
        /// ocean.
        /// </summary>
        ErrorUnsupportedLocation = -3,
    }
}
