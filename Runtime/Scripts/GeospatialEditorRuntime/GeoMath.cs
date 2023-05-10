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

#if UNITY_2021_3_OR_NEWER

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Internal
{
    using System;
    using System.Collections.Generic;
#if ARCORE_INTERNAL_USE_UNITY_MATH
    using Unity.Mathematics;
#endif
    using UnityEngine;

    public class GeoCoor
    {
        public double Latitude;
        public double Longitude;
        public double Height;

        public GeoCoor(double _Latitude, double _Longitude, double _Height)
        {
            Latitude = _Latitude;
            Longitude = _Longitude;
            Height = _Height;
        }

        public static GeoCoor ECEFToGeoCoor(double3 ecef)
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
                var lat = (Math.PI / 2.0) * sgn(ecef.z);
                var alt = Math.Sqrt(ecef.z * ecef.z) - B;
                return new GeoCoor(lat * 180.0 / Math.PI, lng * 180.0 / Math.PI, alt);
            }

            var longitude = Math.Atan2(ecef.y, ecef.x);
            var latitude = Math.Atan2(
                (ecef.z + ((A * A - B * B) / B) * Math.Pow(Math.Sin(q), 3.0)),
                (p - ((A * A - B * B) / A) * Math.Pow(Math.Cos(q), 3.0))
            );

            var N =
                A
                / Math.Sqrt(
                    1.0 - (1.0 - (B * B) / (A * A)) * Math.Sin(latitude) * Math.Sin(latitude)
                );
            var altitude = Math.Sqrt(ecef.x * ecef.x + ecef.y * ecef.y) / Math.Cos(latitude) - N;

            return new GeoCoor(latitude * 180.0 / Math.PI, longitude * 180.0 / Math.PI, altitude);
        }

        // conversion between geodetic and earth-centered, earth-fixed (ECEF) coordinates
        // https://en.wikipedia.org/wiki/Geographic_coordinate_conversion
        public static double3 GeoCoorToECEF(GeoCoor Coor)
        {
            double3 ret = new double3();
            // a and b are from from WGS84
            // https://en.wikipedia.org/wiki/World_Geodetic_System
            var a = 6378137.0; // equatorial radius in meters
            var b = 6356752.314245179; // Polar radius in meters
            var rlong = Math.PI * Coor.Longitude / 180.0;
            var rlat = Math.PI * Coor.Latitude / 180.0;
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
            // Debug.Log($"Hello: {n} {n_2}");
            var x = (n + Coor.Height) * coslat * coslong;
            var y = (n + Coor.Height) * coslat * sinlong;
            var neg1f2 = (1 - f) * (1 - f);
            var neg1e2 = 1 - e2;
            var z = (neg1f2 * n + Coor.Height) * sinlat;
            var z_2 = (neg1e2 * n + Coor.Height) * sinlat;
            // Debug.Log($"Hello: {z} {z_2}");

            // tests
            var iszero = (x / coslong) - (y / sinlong);
            // Debug.Log($"Hello: {iszero}");

            // x y z are in meters
            // save things in the structure
            ret.x = x;
            ret.y = y;
            ret.z = z;
            return ret;
        }

        private static int sgn(double val)
        {
            return val < 0 ? -1 : 1;
        }
    }

    public class LoadingPoint
    {
        public double3 PositionInECEF;
        public double3 PositionInUnityWorldSpace;
        public double3 PositionInGLTFSpace;
        public double Radius;
        public GeoCoor LatLongGeoCoor;
        public double4x4 bbToEUNPos;
        public double4x4 bbToEUNRot;
        public double4x4 meshEUNPos;

        public LoadingPoint(GeoCoor coor, double radius)
        {
            LatLongGeoCoor = coor;
            PositionInECEF = GeoCoor.GeoCoorToECEF(coor);
            PositionInUnityWorldSpace = new double3(
                -PositionInECEF.y,
                PositionInECEF.z,
                PositionInECEF.x
            );
            PositionInGLTFSpace = new double3(
                -PositionInECEF.x,
                PositionInECEF.z,
                -PositionInECEF.y
            );
            Radius = radius;

            // Rotate from y up to z up and flip X. References:
            //   https://github.com/CesiumGS/3d-tiles/tree/main/specification#transforms
            //   https://stackoverflow.com/questions/1263072/changing-a-matrix-from-right-handed-to-left-handed-coordinate-system
            //   https://en.wikipedia.org/wiki/Geographic_coordinate_conversion#From_ECEF_to_ENU
            MatrixStack _matrixStack = new MatrixStack();
            _matrixStack.PushMatrix();

            double latSin,
                latCos;
            math.sincos(LatLongGeoCoor.Latitude / 180 * Math.PI, out latSin, out latCos);
            double lngSin,
                lngCos;
            math.sincos(LatLongGeoCoor.Longitude / 180 * Math.PI, out lngSin, out lngCos);
            double4x4 ECEFToENU = new double4x4(
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
                1.0
            );
            double4x4 ENUToECEF = math.inverse(ECEFToENU);

            double4x4 ECEFToESU = new double4x4(
                -lngSin,
                -lngCos,
                0.0,
                0.0,
                -latSin * lngCos,
                latSin * lngSin,
                latCos,
                0.0,
                latCos * lngCos,
                -latCos * lngSin,
                latSin,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0
            );
            double4x4 ESUToECEF = math.inverse(ECEFToENU);

            // https://stackoverflow.com/questions/1263072/changing-a-matrix-from-right-handed-to-left-handed-coordinate-system
            double4x4 GLTFToENU = new double4x4(
                -1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                -1.0,
                0.0,
                0.0,
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0
            );
            double4x4 ENUToGLTF = math.inverse(GLTFToENU);
            double4x4 GLTFToECEF = GLTFToENU;
            double4x4 ECEFToGLTF = ENUToGLTF;

            double4x4 WUSToEUN = new double4x4(
                -1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                -1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0
            );
            double4x4 EUNToWUS = math.inverse(WUSToEUN);

            double4x4 ENUToEUN = new double4x4(
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0,
                0.0,
                0.0,
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0
            );
            double4x4 EUNToENU = math.inverse(ENUToEUN);

            // WUS Mesh space after ENUToGLTF
            // W = +x
            // E = -x
            // Up = +Y
            // Dn = -Y
            // N = -z
            // S = +z

            // ENU Current BB space
            // W = -x
            // E = +x
            // N = +y
            // S = -y
            // Up = +z
            // Dn = -z

            // Unity EUN
            // E = +x
            // W = -x
            // Up = +y
            // Dn = -y
            // N = +z
            // S = -z

            _matrixStack.MultMatrix(
                MatrixStack.Translate(
                    new double3(-PositionInECEF.x, -PositionInECEF.y, -PositionInECEF.z)
                )
            );
            _matrixStack.MultMatrix(ECEFToENU);
            _matrixStack.MultMatrix(ENUToEUN);
            bbToEUNPos = _matrixStack.GetMatrix();
            _matrixStack.PopMatrix();

            _matrixStack.PushMatrix();
            _matrixStack.MultMatrix(ECEFToENU);
            _matrixStack.MultMatrix(ENUToEUN);
            bbToEUNRot = _matrixStack.GetMatrix();
            _matrixStack.PopMatrix();

            _matrixStack.PushMatrix();
            _matrixStack.MultMatrix(GLTFToECEF);
            _matrixStack.MultMatrix(
                MatrixStack.Translate(
                    new double3(-PositionInECEF.x, -PositionInECEF.y, -PositionInECEF.z)
                )
            );
            _matrixStack.MultMatrix(ECEFToENU);
            _matrixStack.MultMatrix(ENUToGLTF);
            _matrixStack.MultMatrix(WUSToEUN);
            meshEUNPos = _matrixStack.GetMatrix();
            _matrixStack.PopMatrix();
        }
    }

    public class MatrixStack
    {
        private List<double4x4> Stack = new List<double4x4>();

        public MatrixStack()
        {
            Stack.Add(double4x4.identity);
        }

        public void PushMatrix()
        {
            Stack.Add(Stack[Stack.Count - 1]);
        }

        public void PushIdentityMatrix()
        {
            Stack.Add(double4x4.identity);
        }

        public void PopMatrix()
        {
            Debug.Assert(Stack.Count >= 2);
            Stack.RemoveAt(Stack.Count - 1);
        }

        public double4x4 GetMatrix()
        {
            Debug.Assert(Stack.Count >= 1);
            return Stack[Stack.Count - 1];
        }

        public static Quaternion GetRotation(double4x4 m)
        {
            Vector3 forward;
            forward.x = (float)m.c2.x;
            forward.y = (float)m.c2.y;
            forward.z = (float)m.c2.z;

            Vector3 upwards;
            upwards.x = (float)m.c1.x;
            upwards.y = (float)m.c1.y;
            upwards.z = (float)m.c1.z;

            return Quaternion.LookRotation(forward, upwards);
        }

        public Quaternion GetRotation()
        {
            double4x4 m = Stack[Stack.Count - 1];

            Vector3 forward;
            forward.x = (float)m.c2.x;
            forward.y = (float)m.c2.y;
            forward.z = (float)m.c2.z;

            Vector3 upwards;
            upwards.x = (float)m.c1.x;
            upwards.y = (float)m.c1.y;
            upwards.z = (float)m.c1.z;

            return Quaternion.LookRotation(forward, upwards);
        }

        public void Transpose()
        {
            Debug.Assert(Stack.Count >= 1);
            Stack[Stack.Count - 1] = math.transpose(Stack[Stack.Count - 1]);
        }

        // Pre multiply
        public void PreMultMatrix(double4x4 m)
        {
            Debug.Assert(Stack.Count >= 1);
            Stack[Stack.Count - 1] = math.mul(Stack[Stack.Count - 1], m);
        }

        // Post multiply
        public void MultMatrix(double4x4 m)
        {
            Debug.Assert(Stack.Count >= 1);
            Stack[Stack.Count - 1] = math.mul(m, Stack[Stack.Count - 1]);
        }

        public void MultMatrix(List<double> a)
        {
            Debug.Assert(Stack.Count >= 1);
            Debug.Assert(a.Count >= 16);
            // load column-major order
            double4x4 m = new double4x4(
                a[0],
                a[4],
                a[8],
                a[12],
                a[1],
                a[5],
                a[9],
                a[13],
                a[2],
                a[6],
                a[10],
                a[14],
                a[3],
                a[7],
                a[11],
                a[15]
            );
            Stack[Stack.Count - 1] = math.mul(Stack[Stack.Count - 1], m);
        }

        public double4 MultPoints(List<double> a)
        {
            Debug.Assert(Stack.Count >= 1);
            Debug.Assert(a.Count >= 4);
            double4 v = new double4(a[0], a[1], a[2], 1.0);
            return math.mul(Stack[Stack.Count - 1], v);
        }

        public static double3 MultPoint(double4x4 mat, double3 a)
        {
            double4 v = new double4(a[0], a[1], a[2], 1.0);
            double4 ret = math.mul(mat, v);
            return new double3(ret[0], ret[1], ret[2]);
        }

        public double3 MultPoint(double3 a)
        {
            Debug.Assert(Stack.Count >= 1);
            double4 v = new double4(a[0], a[1], a[2], 1.0);
            double4 ret = math.mul(Stack[Stack.Count - 1], v);
            return new double3(ret[0], ret[1], ret[2]);
        }

        public double4 MultPoint(double4 v)
        {
            Debug.Assert(Stack.Count >= 1);
            return math.mul(Stack[Stack.Count - 1], v);
        }

        public static double4x4 YupToZupTest()
        {
            return new double4x4(
                -1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0,
                0.0,
                0.0,
                -1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0
            );
        }

        public static double4x4 YupToZup()
        {
            return new double4x4(
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                -1.0,
                0.0,
                0.0,
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0
            );
        }

        public static double4x4 RotateX(double angle)
        {
            // {{1, 0, 0}, {0, c_0, -s_0}, {0, s_0, c_0}}
            double s,
                c;
            math.sincos(angle, out s, out c);
            return new double4x4(
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                c,
                -s,
                0.0,
                0.0,
                s,
                c,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0
            );
        }

        /// <summary>Returns a double4x4 matrix that rotates around the y-axis by a given number of radians.</summary>
        /// <param name="angle">The clockwise rotation angle when looking along the y-axis towards the origin in radians.</param>
        /// <returns>The double4x4 rotation matrix that rotates around the y-axis.</returns>
        public static double4x4 RotateY(double angle)
        {
            // {{c_1, 0, s_1}, {0, 1, 0}, {-s_1, 0, c_1}}
            double s,
                c;
            math.sincos(angle, out s, out c);
            return new double4x4(
                c,
                0.0,
                s,
                0.0,
                0.0,
                1.0,
                0.0,
                0.0,
                -s,
                0.0,
                c,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0
            );
        }

        /// <summary>Returns a double4x4 matrix that rotates around the z-axis by a given number of radians.</summary>
        /// <param name="angle">The clockwise rotation angle when looking along the z-axis towards the origin in radians.</param>
        /// <returns>The double4x4 rotation matrix that rotates around the z-axis.</returns>
        public static double4x4 RotateZ(double angle)
        {
            // {{c_2, -s_2, 0}, {s_2, c_2, 0}, {0, 0, 1}}
            double s,
                c;
            math.sincos(angle, out s, out c);
            return new double4x4(
                c,
                -s,
                0.0,
                0.0,
                s,
                c,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0
            );
        }

        /// <summary>Returns a double4x4 scale matrix given 3 axis scales.</summary>
        /// <param name="s">The uniform scaling factor.</param>
        /// <returns>The double4x4 matrix that represents a uniform scale.</returns>
        public static double4x4 Scale(double3 s)
        {
            return new double4x4(
                s.x,
                0.0,
                0.0,
                0.0,
                0.0,
                s.y,
                0.0,
                0.0,
                0.0,
                0.0,
                s.z,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0
            );
        }

        /// <summary>Returns a double4x4 translation matrix given a double3 translation vector.</summary>
        /// <param name="vector">The translation vector.</param>
        /// <returns>The double4x4 translation matrix.</returns>
        public static double4x4 Translate(double3 vector)
        {
            return new double4x4(
                new double4(1.0, 0.0, 0.0, 0.0),
                new double4(0.0, 1.0, 0.0, 0.0),
                new double4(0.0, 0.0, 1.0, 0.0),
                new double4(vector.x, vector.y, vector.z, 1.0)
            );
        }
    }
}

#endif // UNITY_X_OR_NEWER
