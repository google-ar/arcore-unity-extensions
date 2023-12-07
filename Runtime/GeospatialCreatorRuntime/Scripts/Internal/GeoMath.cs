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

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Internal
{
    using System;

#if ARCORE_INTERNAL_USE_UNITY_MATH
    using Unity.Mathematics;
#endif
    using UnityEngine;

    // :TODO: b/277365140 Automated testing
    internal static class GeoMath
    {
        // Equatorial radius in meters
        private const double _wgs84EllipsoidSemiMajorAxis = 6378137.0;

        // Polar radius in meters
        private const double _wgs84EllipsoidSemiMinorAxis = 6356752.314245;

        // sub-centimeter tollerance according to https://en.wikipedia.org/wiki/Decimal_degrees
        private const double _epsilon_degrees = 0.00000001d;
        private const double _epsilon_meters = 0.0001d;

        public static double3 ECEFToLongitudeLatitudeHeight(double3 ecef)
        {
            double latitude, longitude, altitude;
            ECEFToGeodetic(ecef, out latitude, out longitude, out altitude);
            return new double3(longitude, latitude, altitude);
        }

        public static GeoCoordinate ECEFToGeoCoordinate(double3 ecef)
        {
            double latitude, longitude, altitude;
            ECEFToGeodetic(ecef, out latitude, out longitude, out altitude);
            return new GeoCoordinate(latitude, longitude, altitude);
        }

        public static double3 GeoCoordinateToECEF(GeoCoordinate coor)
        {
            double3 ret = new double3();

            const double a = _wgs84EllipsoidSemiMajorAxis;
            const double b = _wgs84EllipsoidSemiMinorAxis;
            const double a2 = a * a;
            const double b2 = b * b;
            const double f = 1 - (b / a);
            const double e2 = 1 - (b2 / a2);
            const double neg1f2 = (1 - f) * (1 - f);
            const double neg1e2 = 1 - e2;

            var rlong = Math.PI * coor.Longitude / 180.0;
            var rlat = Math.PI * coor.Latitude / 180.0;
            var coslong = Math.Cos(rlong);
            var sinlong = Math.Sin(rlong);
            var coslat = Math.Cos(rlat);
            var sinlat = Math.Sin(rlat);
            var n_2 = a2 / Math.Sqrt((a2 * (coslat * coslat)) + ((b2) * (sinlat * sinlat)));
            var n = a / Math.Sqrt(1 - (e2 * sinlat * sinlat));
            var x = (n + coor.Altitude) * coslat * coslong;
            var y = (n + coor.Altitude) * coslat * sinlong;
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
            // TODO: b/305998580 - Take into account that angles wrap around every 360 degrees.
            return ApproximatelyEquals(d1, d2, _epsilon_degrees);
        }

        // Compares two values as meters, and returns true if they are equal within a
        // sub-centimeter tollerance, which is sufficient accuracy for Geospatial Creator.
        public static bool ApproximatelyEqualsMeters(double m1, double m2)
        {
            return ApproximatelyEquals(m1, m2, _epsilon_meters);
        }

        public static GeoCoordinate UnityWorldToGeoCoordinate(
            Vector3 unityPosition, GeoCoordinate originGeoCoordinate, Vector3 originUnityPosition)
        {
            double4x4 ENUToECEF = GeoMath.CalculateEnuToEcefTransform(originGeoCoordinate);
            Vector3 EUN = unityPosition - originUnityPosition;
            double3 ENU = new double3(EUN.x, EUN.z, EUN.y);
            double3 ECEF = MatrixStack.MultPoint(ENUToECEF, ENU);
            return ECEFToGeoCoordinate(ECEF);
        }

        public static Vector3 GeoCoordinateToUnityWorld(
            GeoCoordinate geoCoordinate,
            GeoCoordinate originGeoCoordinate,
            Vector3 originUnityPosition)
        {
            double4x4 ECEFToENU = GeoMath.CalculateEcefToEnuTransform(originGeoCoordinate);
            double3 localInECEF = GeoMath.GeoCoordinateToECEF(geoCoordinate);
            double3 ENU = MatrixStack.MultPoint(ECEFToENU, localInECEF);

            // Unity is EUN not ENU so swap z and y
            Vector3 EUN = new Vector3((float)ENU.x, (float)ENU.z, (float)ENU.y);

            // Add the origin's world displacement to convert from EUN to Unity World Position.
            Vector3 unityWorldPosition = EUN + originUnityPosition;
            return unityWorldPosition;
        }

        private static bool ApproximatelyEquals(double a, double b, double epsilon)
        {
            return (Math.Abs(a - b) < epsilon);
        }

        // Conversion between geodetic decimal degrees and earth-centered, earth-fixed (ECEF)
        // coordinates. Ref https://en.wikipedia.org/wiki/Geographic_coordinate_conversion.
        private static void ECEFToGeodetic(
            double3 ecef, out double latitude, out double longitude, out double altitude)
        {
            const double a = _wgs84EllipsoidSemiMajorAxis;
            const double b = _wgs84EllipsoidSemiMinorAxis;
            const double a2 = a * a;
            const double b2 = b * b;

            var p = Math.Sqrt(ecef.x * ecef.x + ecef.y * ecef.y); // Temporary value
            var q = Math.Atan2((ecef.z * a), (p * b)); // Temporary value

            // special case of north/south pole
            const double epsilon = 1e-9;
            double latRad, lonRad;
            if (p < epsilon)
            {
                lonRad = 0.0;
                var zSign = (ecef.z < 0) ? -1 : 1;
                latRad = (Math.PI / 2.0) * zSign;
                altitude = Math.Sqrt(ecef.z * ecef.z) - b;
            }
            else
            {
                lonRad = Math.Atan2(ecef.y, ecef.x);
                latRad = Math.Atan2(
                    (ecef.z + ((a2 - b2) / b) * Math.Pow(Math.Sin(q), 3.0)),
                    (p - ((a2 - b2) / a) * Math.Pow(Math.Cos(q), 3.0)));
                var n = a /
                    Math.Sqrt(1.0 - (1.0 - b2 / a2) * Math.Sin(latRad) * Math.Sin(latRad));

                altitude = Math.Sqrt(ecef.x * ecef.x + ecef.y * ecef.y) / Math.Cos(latRad) - n;
            }

            latitude = latRad * 180.0 / Math.PI;
            longitude = lonRad * 180.0 / Math.PI;
        }
    }
}
