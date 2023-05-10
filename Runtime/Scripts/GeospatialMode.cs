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
    /// Describes the desired behavior of the ARCore Geospatial API. The Geospatial API uses a
    /// combination of Google's Visual Positioning System (VPS) and GPS to determine the geospatial
    /// pose.
    ///
    /// The Geospatial API is able to provide the best user experience when it is able to generate
    /// high accuracy poses. However, the Geospatial API can be used anywhere, as long as the device
    /// is able to determine its location, even if the available location information has low
    /// accuracy.
    ///
    /// <list type="bullet">
    /// <item>
    /// In areas with VPS coverage, the Geospatial API is able to generate high accuracy poses.
    /// This can work even where GPS accuracy is low, such as dense urban environments. Under
    /// typical conditions, VPS can be expected to provide positional accuracy typically better
    /// than 5 meters and often around 1 meter, and a rotational accuracy of better than 5 degrees.
    /// Use <c><see cref="AREarthManager.CheckVpsAvailabilityAsync(double, double)"/></c> to
    /// determine if a given location has VPS coverage.
    /// </item>
    /// <item>
    /// In outdoor environments with few or no overhead obstructions, GPS may be sufficient to
    /// generate high accuracy poses. GPS accuracy may be low in dense urban environments and
    /// indoors.
    /// </item>
    /// </list>
    ///
    /// A small number of ARCore supported devices do not support the Geospatial API. Use
    /// <c><see cref="AREarthManager.IsGeospatialModeSupported(GeospatialMode)"/></c> to determine
    /// if the current device is supported. Affected devices are also indicated on the <a
    /// href="https://developers.google.com/ar/devices">ARCore supported devices page</a>.
    ///
    /// When the Geospatial API and the Depth API are enabled, output images
    /// from the Depth API will include terrain and building geometry when in a
    /// location with VPS coverage. See the <a
    /// href="https://developers.google.com/ar/develop/unity-arf/depth/geospatial-depth">Geospatial
    /// Depth Developer Guide</a> for more information.
    ///
    /// The default value is <c><see cref="GeospatialMode.Disabled"/></c>. Use
    /// <c><see cref="ARCoreExtensionsConfig.GeospatialMode"/></c> to set the desired mode.
    /// </summary>
    public enum GeospatialMode
    {
        /// <summary>
        /// The Geospatial API is disabled. When a configuration with <c>Disabled</c> becomes active
        /// on the AR session, current anchors created from
        /// <c><see cref="ARAnchorManagerExtensions.AddAnchor(
        /// UnityEngine.XR.ARFoundation.ARAnchorManager, double, double, double,
        /// UnityEngine.Quaternion)"/></c> will stop updating and have their
        /// <c><see cref="UnityEngine.XR.ARSubsystems.TrackingState"/></c> set to
        /// <c><see
        /// cref="UnityEngine.XR.ARSubsystems.TrackingState.None"/></c>.
        ///
        /// ARCore Extensions will not request location permissions.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// The Geospatial API is enabled, and <c><see cref="AREarthManager"/></c> can be used.
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
        /// Location is tracked only while <c><see cref="ARSession"/></c> is enabled.
        /// While it is disabled, <c><see cref="AREarthManager.EarthTrackingState"/></c>
        /// will be <c><see
        /// cref="UnityEngine.XR.ARSubsystems.TrackingState.None"/></c>.
        ///
        /// For more information, see documentation on <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/developer-guide-android">the
        /// Geospatial API on Google Developers</a>.
        ///
        /// This mode is not compatible with the
        /// <c><see cref="CameraFacingDirection.User"/></c>
        /// (selfie) camera; use the <c><see cref="CameraFacingDirection.World"/></c> camera
        /// instead.
        ///
        /// Not all devices support this mode, use
        /// <c><see cref="AREarthManager.IsGeospatialModeSupported(GeospatialMode)"/></c> to check
        /// if the current device and selected camera support enabling this mode.
        ///
        /// When the Geospatial API and the Depth API are enabled, output images
        /// from the Depth API will include terrain and building geometry when in a
        /// location with VPS coverage. See the <a
        /// href="https://developers.google.com/ar/develop/unity-arf/depth/geospatial-depth">Geospatial
        /// Depth Developer Guide</a> for more information.
        /// </summary>
        Enabled = 2,
    }
}
