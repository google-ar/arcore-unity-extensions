//-----------------------------------------------------------------------
// <copyright file="TerrainAnchorState.cs" company="Google LLC">
//
// Copyright 2022 Google LLC
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
    /// Describes the result of a Terrain anchor resolving operation.
    /// </summary>
    public enum TerrainAnchorState
    {
        /// <summary>
        /// Not a valid value for a Terrain anchor operation.
        /// </summary>
        None = 0,

        /// <summary>
        /// A resolving task is in progress for this Terrain anchor.
        /// Once the task completes in the background, the Terrain anchor will get
        /// a new state after the next update.
        /// </summary>
        /// @deprecated Not returned by async methods.
        /// Replaced by <c><see cref="PromiseState.Pending"/></c>.
        [Obsolete("This enum value has been deprecated. " +
            "Not returned by async methods - replaced by PromiseState.Pending.")]
        TaskInProgress = 1,

        /// <summary>
        /// A resolving task for this Terrain anchor has completed successfully.
        /// </summary>
        Success = 2,

        /// <summary>
        /// A resolving task for this Terrain anchor has completed with an
        /// internal error. The app should not attempt to recover from this error.
        /// </summary>
        ErrorInternal = -1,

        /// <summary>
        /// The app cannot communicate with the ARCore Cloud because of an invalid authentication.
        /// Check Project Settings > XR Plug-in Management > ARCore Extensions for a valid
        /// authentication strategy.
        /// </summary>
        ErrorNotAuthorized = -2,

        /// <summary>
        /// There is no terrain info at this location, such as the center of the ocean.
        /// </summary>
        ErrorUnsupportedLocation = -3,
    }
}
