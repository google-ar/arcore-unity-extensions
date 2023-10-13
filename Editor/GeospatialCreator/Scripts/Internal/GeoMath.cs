//-----------------------------------------------------------------------
// <copyright file="GeoMath.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Editor.Internal
{
    using System;

    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
#if ARCORE_INTERNAL_USE_UNITY_MATH
    using Unity.Mathematics;
#endif
    using UnityEngine;

    // :TODO: b/277365140 Automated testing
    internal static class GeoMath
    {
        // sub-centimeter tollerance according to https://en.wikipedia.org/wiki/Decimal_degrees
        private const double _epsilon_degrees = 0.00000001d;
        private const double _epsilon_meters = 0.0001d;

        public static double3 EarthCenteredEarthFixedToLongitudeLatitudeHeight(double3 ecef)
        {
#if ARCORE_INTERNAL_USE_CESIUM && ARCORE_INTERNAL_USE_UNITY_MATH
            return CesiumForUnity.CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecef);
#else
            throw new Exception("Missing dependencies: Cesium 1.0.0+");
#endif
        }

        /// <summary>
        /// Conversion between geodetic and earth-centered, earth-fixed (ECEF) coordinates
        /// https://en.wikipedia.org/wiki/Geographic_coordinate_conversion.
        /// </summary>
        /// <param name="ecef">Vector in earth centered, earth fixed coordinates.</param>
        /// <returns>The GeoCoordinate that corresponds to the ecef location.</returns>
        public static GeoCoordinate ECEFToGeoCoordinate(double3 ecef)
        {
            var A = 6378137.0; // equatorial radius in meters
            var B = 6356752.314245179; // Polar radius in meters
            var p = Math.Sqrt(ecef.x * ecef.x + ecef.y * ecef.y); // Temporary value
            var q = Math.Atan2((ecef.z * A), (p * B)); // Temporary value

            // special case of north/south pole
            var epsilon = 1e-10;
            if (p < epsilon)
            {
                var lng = 0.0;
                var zSign = (ecef.z < 0) ? -1 : 1;
                var lat = (Math.PI / 2.0) * zSign;
                var alt = Math.Sqrt(ecef.z * ecef.z) - B;
                return new GeoCoordinate(lat * 180.0 / Math.PI, lng * 180.0 / Math.PI, alt);
            }

            var longitude = Math.Atan2(ecef.y, ecef.x);
            var latitude = Math.Atan2(
                (ecef.z + ((A * A - B * B) / B) * Math.Pow(Math.Sin(q), 3.0)),
                (p - ((A * A - B * B) / A) * Math.Pow(Math.Cos(q), 3.0)));

            var N =
                A
                / Math.Sqrt(
                    1.0 - (1.0 - (B * B) / (A * A)) * Math.Sin(latitude) * Math.Sin(latitude));
            var altitude = Math.Sqrt(ecef.x * ecef.x + ecef.y * ecef.y) / Math.Cos(latitude) - N;

            return new GeoCoordinate(
                latitude * 180.0 / Math.PI,
                longitude * 180.0 / Math.PI,
                altitude);
        }

        public static double3 GeoCoordinateToECEF(GeoCoordinate coor)
        {
            double3 ret = new double3();

            // a and b are from from WGS84
            // https://en.wikipedia.org/wiki/World_Geodetic_System
            var a = 6378137.0; // equatorial radius in meters
            var b = 6356752.314245179; // Polar radius in meters
            var rlong = Math.PI * coor.Longitude / 180.0;
            var rlat = Math.PI * coor.Latitude / 180.0;
            var coslong = Math.Cos(rlong);
            var sinlong = Math.Sin(rlong);
            var coslat = Math.Cos(rlat);
            var sinlat = Math.Sin(rlat);
            var a2 = a * a;
            var b2 = b * b;
            var f = 1 - (b / a);
            var e2 = 1 - (b2 / a2);
            var n_2 = a2 / Math.Sqrt((a2 * (coslat * coslat)) + ((b2) * (sinlat * sinlat)));
            var n = a / Math.Sqrt(1 - (e2 * sinlat * sinlat));
            var x = (n + coor.Altitude) * coslat * coslong;
            var y = (n + coor.Altitude) * coslat * sinlong;
            var neg1f2 = (1 - f) * (1 - f);
            var neg1e2 = 1 - e2;
            var z = (neg1f2 * n + coor.Altitude) * sinlat;
            var z_2 = (neg1e2 * n + coor.Altitude) * sinlat;

            // x y z are in meters
            ret.x = x;
            ret.y = y;
            ret.z = z;
            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "UnityRules.UnityStyleRules",
            "US1300:LinesMustBe100CharactersOrShorter",
            Justification = "URL length > 100")]
        public static double4x4 CalculateEcefToEnuTransform(GeoCoordinate originPoint)
        {
            // :TODO b/277370107: This could be optimized by only changing the position if the
            // object or origin has moved
            double3 PositionInECEF = GeoCoordinateToECEF(originPoint);

            // Rotate from y up to z up and flip X. References:
            //   https://github.com/CesiumGS/3d-tiles/tree/main/specification#transforms
            //   https://stackoverflow.com/questions/1263072/changing-a-matrix-from-right-handed-to-left-handed-coordinate-system
            //   https://en.wikipedia.org/wiki/Geographic_coordinate_conversion#From_ECEF_to_ENU
            MatrixStack matrixStack = new MatrixStack();
            matrixStack.PushMatrix();

            double latSin, latCos;
            math.sincos(originPoint.Latitude / 180 * Math.PI, out latSin, out latCos);
            double lngSin, lngCos;
            math.sincos(originPoint.Longitude / 180 * Math.PI, out lngSin, out lngCos);
            double4x4 ECEFToENURot = new double4x4(
                -lngSin,
                lngCos,
                0.0,
                0.0,
                -latSin * lngCos,
                -latSin * lngSin,
                latCos,
                0.0,
                latCos * lngCos,
                latCos * lngSin,
                latSin,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0);

            matrixStack.MultMatrix(
                MatrixStack.Translate(
                    new double3(-PositionInECEF.x, -PositionInECEF.y, -PositionInECEF.z)));
            matrixStack.MultMatrix(ECEFToENURot);
            return matrixStack.GetMatrix();
        }

        public static double4x4 CalculateEnuToEcefTransform(GeoCoordinate originPoint)
        {
            return math.inverse(CalculateEcefToEnuTransform(originPoint));
        }

        // Compares two values as decimal degrees, and returns true if they are equal within
        // a sub-centimeter tolerance, which is sufficient accuracy for Geospatial Creator.
        public static bool ApproximatelyEqualsDegrees(double d1, double d2)
        {
            return ApproximatelyEquals(d1, d2, _epsilon_degrees);
        }

        // Compares two values as meters, and returns true if they are equal within a
        // sub-centimeter tollerance, which is sufficient accuracy for Geospatial Creator.
        public static bool ApproximatelyEqualsMeters(double m1, double m2)
        {
            return ApproximatelyEquals(m1, m2, _epsilon_meters);
        }

        private static bool ApproximatelyEquals(double a, double b, double epsilon)
        {
            return (Math.Abs(a - b) < epsilon);
        }
    }
}
