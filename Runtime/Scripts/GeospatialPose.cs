//-----------------------------------------------------------------------
// <copyright file="GeospatialPose.cs" company="Google LLC">
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
    using UnityEngine;

    /// <summary>
    /// Describes a specific location, elevation, and compass heading relative to Earth.
    /// It is comprised of:
    /// <list type="bullet">
    /// <item>
    /// Latitude and longitude are specified in degrees, with positive values being north of the
    /// equator and east of the prime meridian as defined by the
    /// <a href="https://en.wikipedia.org/wiki/World_Geodetic_System">WGS84 specification</a>.
    /// </item>
    /// <item>
    /// Altitude is specified in meters above the WGS84 ellipsoid, which is roughly equivalent to
    /// meters above sea level.
    /// </item>
    /// <item>
    /// Heading is specified in degrees clockwise from north. See
    /// <c><see cref="GeospatialPose.Heading"/></c> for more information.
    /// </item>
    /// <item>
    /// Accuracy of the latitude, longitude, altitude, and heading are available as numeric
    /// confidence intervals where a large value (large interval) means low confidence and small
    /// value (small interval) means high confidence.
    /// </item>
    /// </list>
    ///
    /// A GeospatialPose can be retrieved from
    /// <c><see cref="AREarthManager.CameraGeospatialPose"/></c>.
    /// </summary>
    public struct GeospatialPose
    {
        /// <summary>
        /// Latitude of the pose in degrees. Positive values are north of the
        /// equator as defined by the <a
        /// href="https://en.wikipedia.org/wiki/World_Geodetic_System">WGS84 specification</a>.
        /// </summary>
        public double Latitude;

        /// <summary>
        /// Longitude of the pose in degrees. Positive values are east of the
        /// prime meridian as defined by the <a
        /// href="https://en.wikipedia.org/wiki/World_Geodetic_System">WGS84 specification</a>.
        /// </summary>
        public double Longitude;

        /// <summary>
        /// Altitude of the pose in meters above the WGS 84 ellipsoid.
        /// </summary>
        public double Altitude;

        /// <summary>
        /// Heading of the pose in degrees.
        ///
        /// Heading is specified in degrees clockwise from true north and approximates the
        /// direction the device is facing. The value returned when facing north is 0°, when
        /// facing east is 90°, when facing south is +/-180°, and when facing west is -90°.
        ///
        /// The heading approximation is based on the rotation of the device in its current
        /// orientation mode (i.e. portrait or landscape) and pitch. For example, when the device is
        /// held vertically or upright, the heading is based on the camera optical axis. If the
        /// device is held horizontally, looking downwards, the heading is based on the top of the
        /// device, with respect to the orientation mode.
        ///
        /// Note: Heading is currently only supported in the device's default orientation mode,
        /// which is portrait mode for most supported devices.
        /// </summary>
        public double Heading;

        /// <summary>
        /// Estimated heading accuracy in degrees.
        ///
        /// We define heading accuracy as the radius of the 68th percentile confidence level around
        /// <c>Heading</c>. In other words, there is a 68% probability that the true heading is
        /// within <c>HeadingAccuracy</c> of <c>Heading</c>. Larger values indicate lower accuracy.
        ///
        /// For example, if the estimated heading is 60°, and the heading accuracy is 10°, then
        /// there is a 68% probability of the true heading being between 50° and 70°.
        /// </summary>
        public double HeadingAccuracy;

        /// <summary>
        /// Estimated horizontal accuracy in meters with respect to latitude and longitude.
        ///
        /// We define horizontal accuracy as the radius of the 68th percentile confidence level
        /// around the estimated horizontal location. In other words, if you draw a circle centered
        /// at <c>Latitude</c> and <c>Longitude</c>, and with a radius equal to the horizontal
        /// accuracy, then there is a 68% probability that the true location is inside the circle.
        /// Larger numbers indicate lower accuracy.
        ///
        /// For example, if the latitude is 10°, longitude is 10°, and the returned value is 15,
        /// then there is a 68% probability that the true location is within 15 meters of the
        /// (10°, 10°) latitude/longitude coordinate.
        /// </summary>
        public double HorizontalAccuracy;

        /// <summary>
        /// Estimated horizontal accuracy in meters.
        ///
        /// We define vertical accuracy as the radius of the 68th percentile confidence level around
        /// the estimated altitude. In other words, there is a 68% probability that the true
        /// altitude is within the output value (in meters) of this <c>Altitude</c> (above or
        /// below). Larger numbers indicate lower accuracy.
        ///
        /// For example, if <c>Altitude</c> is 100 meters, and <c>VerticalAccuracy</c> is 20 meters,
        /// there is a 68% chance that the true altitude is within 20 meters of 100 meters.
        /// </summary>
        public double VerticalAccuracy;
    }
}
