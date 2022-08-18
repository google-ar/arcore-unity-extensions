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
    /// <summary>
    /// Describes the current terrain anchor state of a <c><see cref="ARGeospatialAnchor"/></c>.
    /// </summary>
    public enum TerrainAnchorState
    {
        /// <summary>
        /// Not a Terrain Anchor or is not ready to use.
        /// </summary>
        None,

        /// <summary>
        /// A resolving task is in progress for this Terrain Anchor.
        /// Once the task completes in the background, the Terrain Anchor will get
        /// a new state after the next update.
        /// </summary>
        TaskInProgress,

        /// <summary>
        /// A resolving task for this Terrain Anchor has completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// A resolving task for this Terrain Anchor has completed with an
        /// internal error. The app should not attempt to recover from this error.
        /// </summary>
        ErrorInternal,

        /// <summary>
        /// The app cannot communicate with the ARCore Cloud because of an invalid authentication.
        /// Check Project Settings > XR Plug-in Management > ARCore Extensions for a valid
        /// authentication strategy.
        /// </summary>
        ErrorNotAuthorized,

        /// <summary>
        /// There is no terrain info at this location, such as the center of the ocean.
        /// </summary>
        ErrorUnsupportedLocation,
    }
}
