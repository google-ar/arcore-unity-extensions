//-----------------------------------------------------------------------
// <copyright file="EarthState.cs" company="Google LLC">
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
    /// Describes the current state of Earth localization. When
    /// <c><see cref="AREarthManager.EarthTrackingState"/></c> does not become
    /// <c>TrackingState.Tracking</c>, <c>EarthState</c> may contain the cause of this failure.
    /// </summary>
    public enum EarthState
    {
        /// <summary>
        /// Earth localization is enabled, and has not encountered any problems.
        /// Check <c><see cref="AREarthManager.EarthTrackingState"/></c> to determine if the
        /// Geospatial API can be used.
        /// </summary>
        Enabled = 0,

        /// <summary>
        /// Earth localization has encountered an internal error. The app should not
        /// attempt to recover from this error. Please see application logs for
        /// additional information.
        /// </summary>
        ErrorInternal = -1,

        /// <summary>
        /// Earth localization has been disabled on this session.
        /// All <c><see cref="ARGeospatialAnchor"/></c> created during this session will have
        /// <c>TrackingState</c> set to <c>None</c> and should be destroyed.
        /// </summary>
        ErrorGeospatialModeDisabled = -2,

        /// <summary>
        /// The authorization provided by the application is not valid.
        /// <list type="bullet">
        /// <item>
        /// The Google Cloud project may not have enabled the ARCore API.
        /// </item>
        /// <item>
        /// When using API key authentication, this will happen if the API key in
        /// the manifest is invalid, unauthorized. It may also fail if the API key
        /// is restricted to a set of apps not including the current one.
        /// </item>
        /// <item>
        /// When using keyless authentication, this may happen when no OAuth
        /// client has been created, or when the signing key and package name
        /// combination does not match the values used in the Google Cloud project.
        /// On Android, it may also fail if Google Play Services isn't installed,
        /// is too old, or is malfunctioning for some reason (e.g. killed due to memory pressure).
        /// </item>
        /// </list>
        /// </summary>
        ErrorNotAuthorized = -3,

        /// <summary>
        /// The application has exhausted the quota allotted to the given
        /// Google Cloud project. The developer should <a
        /// href="https://cloud.google.com/docs/quota#requesting_higher_quota">request additional quota</a>
        /// for the ARCore API for their project from the Google Cloud Console.
        /// </summary>
        ErrorResourcesExhausted = -4,

        /// <summary>
        /// The package is older than the supported version.
        /// </summary>
        ErrorPackageTooOld = -5,
    }
}
