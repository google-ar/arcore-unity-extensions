//-----------------------------------------------------------------------
// <copyright file="GeospatialMode.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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
    /// Describes the desired behavior of ARCore Geospatial API features and capabilities. Not all
    /// devices support all modes. Use <see
    /// cref="AREarthManager.IsGeospatialModeSupported(GeospatialMode)"/> to find whether the
    /// current device supports a particular <c><see cref="GeospatialMode"/></c>.
    /// The default value is <c>Disabled</c>.
    ///
    /// Use <see cref="ARCoreExtensionsConfig.GeospatialMode"/> to set the desired mode.
    /// </summary>
    public enum GeospatialMode
    {
        /// <summary>
        /// The Geospatial API is disabled. When a configuration with <c>Disabled</c> becomes active
        /// on the AR session, current anchors created from
        /// <see cref="ARAnchorManagerExtensions.AddAnchor(
        /// UnityEngine.XR.ARFoundation.ARAnchorManager, double, double, double,
        /// UnityEngine.Quaternion)"/> will stop updating and have their
        /// <see cref="UnityEngine.XR.ARSubsystems.TrackingState"/> set to <c>None</c>.
        ///
        /// ARCore Extensions will not request location permissions.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// The Geospatial API is enabled, and <see cref="AREarthManager"/> can be used.
        ///
        /// Using this mode requires your app do the following:
        /// <list type="bullet">
        /// <item>
        /// On Android, be granted the <a
        /// href="https://developer.android.com/training/location/permissions">Android
        /// <c>ACCESS_FINE_LOCATION</c> permission</a>. ARCore Extensions will request this
        /// permission when Geospatial is enabled in ARCore Extensions Project Settings > Optional
        /// Features.
        /// </item>
        /// <item>
        /// On Android, have enabled Unity's Internet Access permission in <a
        /// href="https://docs.unity3d.com/Manual/class-PlayerSettingsAndroid.html">Android Player
        /// Settings</a>.
        /// </item>
        /// <item>
        /// On iOS, have at least
        /// <a href="https://developer.apple.com/documentation/corelocation/clauthorizationstatus">
        /// <c>CLAuthorizationStatus authorizedWhenInUse</c></a> and <a
        /// href="https://developer.apple.com/documentation/corelocation/claccuracyauthorization">
        /// <c>CLAccuracyAuthorization fullAccuracy</c></a> in iOS's Location Services permissions.
        /// </item>
        /// </list>
        ///
        /// Location is tracked only while <see cref="ARSession"/> is enabled.
        /// While it is disabled, <see cref="AREarthManager.EarthTrackingState"/>
        /// will be <see cref="UnityEngine.XR.ARSubsystems.TrackingState"/>.<c>None</c>.
        ///
        /// For more information, see documentation on <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/developer-guide">the
        /// Geospatial API on Google Developers</a>.
        ///
        /// This mode is not compatible with the
        /// <see cref="UnityEngine.XR.ARFoundation.CameraFacingDirection"/>.<c>User</c>
        /// (selfie) camera; use the <c>World</c> camera instead.
        ///
        /// Not all devices support this mode, use
        /// <see cref="AREarthManager.IsGeospatialModeSupported(GeospatialMode)"/> to check if the
        /// current device and selected camera support enabling this mode.
        /// </summary>
        Enabled = 2,
    }
}
