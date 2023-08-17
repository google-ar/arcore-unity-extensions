//-----------------------------------------------------------------------
// <copyright file="MatrixStack.cs" company="Google LLC">
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
    using System.Collections.Generic;
#if ARCORE_INTERNAL_USE_UNITY_MATH
    using Unity.Mathematics;
#endif
    using UnityEngine;

    internal class MatrixStack
    {
        private List<double4x4> _stack = new List<double4x4>();

        public MatrixStack()
        {
            _stack.Add(double4x4.identity);
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

        public static double3 MultPoint(double4x4 mat, double3 a)
        {
            double4 v = new double4(a[0], a[1], a[2], 1.0);
            double4 ret = math.mul(mat, v);
            return new double3(ret[0], ret[1], ret[2]);
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
                1.0);
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
                1.0);
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
                1.0);
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
                1.0);
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
                1.0);
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
                1.0);
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
                new double4(vector.x, vector.y, vector.z, 1.0));
        }

        public void PushMatrix()
        {
            _stack.Add(_stack[_stack.Count - 1]);
        }

        public void PushIdentityMatrix()
        {
            _stack.Add(double4x4.identity);
        }

        public void PopMatrix()
        {
            Debug.Assert(_stack.Count >= 2);
            _stack.RemoveAt(_stack.Count - 1);
        }

        public double4x4 GetMatrix()
        {
            Debug.Assert(_stack.Count >= 1);
            return _stack[_stack.Count - 1];
        }

        public Quaternion GetRotation()
        {
            double4x4 m = _stack[_stack.Count - 1];

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
            Debug.Assert(_stack.Count >= 1);
            _stack[_stack.Count - 1] = math.transpose(_stack[_stack.Count - 1]);
        }

        // Pre multiply
        public void PreMultMatrix(double4x4 m)
        {
            Debug.Assert(_stack.Count >= 1);
            _stack[_stack.Count - 1] = math.mul(_stack[_stack.Count - 1], m);
        }

        // Post multiply
        public void MultMatrix(double4x4 m)
        {
            Debug.Assert(_stack.Count >= 1);
            _stack[_stack.Count - 1] = math.mul(m, _stack[_stack.Count - 1]);
        }

        public void MultMatrix(List<double> a)
        {
            Debug.Assert(_stack.Count >= 1);
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
                a[15]);
            _stack[_stack.Count - 1] = math.mul(_stack[_stack.Count - 1], m);
        }

        public double4 MultPoints(List<double> a)
        {
            Debug.Assert(_stack.Count >= 1);
            Debug.Assert(a.Count >= 4);
            double4 v = new double4(a[0], a[1], a[2], 1.0);
            return math.mul(_stack[_stack.Count - 1], v);
        }

        public double3 MultPoint(double3 a)
        {
            Debug.Assert(_stack.Count >= 1);
            double4 v = new double4(a[0], a[1], a[2], 1.0);
            double4 ret = math.mul(_stack[_stack.Count - 1], v);
            return new double3(ret[0], ret[1], ret[2]);
        }

        public double4 MultPoint(double4 v)
        {
            Debug.Assert(_stack.Count >= 1);
            return math.mul(_stack[_stack.Count - 1], v);
        }
    }
}
