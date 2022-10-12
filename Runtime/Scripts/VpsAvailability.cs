//-----------------------------------------------------------------------
// <copyright file="VpsAvailability.cs" company="Google LLC">
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
    /// Describes the result of checking VPS availability at specific location.
    /// </summary>
    public enum VpsAvailability
    {
        /// <summary>
        /// The request to the remote service is not yet completed, so the availability is not yet
        /// known, or the AR Subsystem is not ready yet.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// VPS is available at the requested location.
        /// </summary>
        Available = 1,

        /// <summary>
        /// VPS is not available at the requested location.
        /// </summary>
        Unavailable = 2,

        /// <summary>
        /// An internal error occurred while determining availability.
        /// </summary>
        ErrorInternal = -1,

        /// <summary>
        /// The external service could not be reached due to a network connection error.
        /// </summary>
        ErrorNetworkConnection = -2,

        /// <summary>
        /// An authorization error occurred when communicating with the Google Cloud
        /// ARCore API. See <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/enable">Enable
        /// the Geospatial API</a> for troubleshooting steps.
        /// </summary>
        ErrorNotAuthorized = -3,

        /// <summary>
        /// Too many requests were sent.
        /// </summary>
        ErrorResourceExhausted = -4,
    }
}
