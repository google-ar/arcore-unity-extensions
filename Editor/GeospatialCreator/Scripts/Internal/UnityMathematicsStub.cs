//-----------------------------------------------------------------------
// <copyright file="UnityMathematicsStub.cs" company="Google LLC">
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

#if !ARCORE_INTERNAL_USE_UNITY_MATH

[module: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName",
    Justification = "Must match names in Unity.Mathematics")]

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Editor.Internal
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.UnityStyleRules",
        "US1107:PublicFieldsMustBeUpperCamelCase",
        Justification = "Must match names in Unity.Mathematics")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.UnityStyleRules",
        "US1111:StructsMustBeUpperCamelCase",
        Justification = "Must match names in Unity.Mathematics")]
    internal struct double3
    {
        public double x,
            y,
            z;

        public double3(double v)
        {
            x = 0;
            y = 0;
            z = 0;
            math.throwDependencyException();
        }

        public double3(double x, double y, double z) : this(0.0d)
        {
        }

        public double this[int i]
        {
            get
            {
                math.throwDependencyException();
                return 0.0d;
            }

            set
            {
                math.throwDependencyException();
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.UnityStyleRules",
        "US1107:PublicFieldsMustBeUpperCamelCase",
        Justification = "Must match names in Unity.Mathematics")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.UnityStyleRules",
        "US1111:StructsMustBeUpperCamelCase",
        Justification = "Must match names in Unity.Mathematics")]
    internal struct double4
    {
        public double x,
            y,
            z,
            w;

        public double4(double v)
        {
            x = v;
            y = v;
            z = v;
            w = v;
            math.throwDependencyException();
        }

        public double4(double x, double y, double z, double w) : this(0d)
        {
        }

        public double this[int i]
        {
            get
            {
                math.throwDependencyException();
                return 0.0d;
            }

            set
            {
                math.throwDependencyException();
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.UnityStyleRules",
        "US1107:PublicFieldsMustBeUpperCamelCase",
        Justification = "Must match names in Unity.Mathematics")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.UnityStyleRules",
        "US1111:StructsMustBeUpperCamelCase",
        Justification = "Must match names in Unity.Mathematics")]
    internal struct double4x4
    {
        public static readonly double4x4 identity = new double4x4(0.0d);

        public double4 c0,
            c1,
            c2,
            c3;

        public double4x4(double v)
        {
            this.c0 = new double4(0);
            this.c1 = new double4(0);
            this.c2 = new double4(0);
            this.c3 = new double4(0);
            math.throwDependencyException();
        }

        public double4x4(double4 c0, double4 c1, double4 c2, double4 c3) : this(0.0d)
        {
        }

        public double4x4(
            double m00,
            double m01,
            double m02,
            double m03,
            double m10,
            double m11,
            double m12,
            double m13,
            double m20,
            double m21,
            double m22,
            double m23,
            double m30,
            double m31,
            double m32,
            double m33)
            : this()
        {
        }
    }

    /// <summary> Stub that allows our Math-dependent code to compile when the real Unity math
    /// dependency is not present. This gives us control over the error message without requiring
    /// ifdefs everywhere we use the math library.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.UnityStyleRules",
        "US1110:ClassesMustBeUpperCamelCase",
        Justification = "Must match names in Unity.Mathematics")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.UnityStyleRules",
        "US1113:MethodsMustBeUpperCamelCase",
        Justification = "Must match names in Unity.Mathematics")]
    internal static class math
    {
        public static void throwDependencyException()
        {
            throw new System.Exception("Unity.Mathematics 1.2.0+ not available.");
        }

        public static void sincos(double x, out double s, out double c)
        {
            s = 0;
            c = 0;
            throwDependencyException();
        }

        public static double4 mul(double4x4 m1, double4 v)
        {
            throwDependencyException();
            return new double4(0);
        }

        public static double4x4 mul(double4x4 m1, double4x4 m2)
        {
            throwDependencyException();
            return new double4x4(0);
        }

        public static void sincos(double3 x, out double3 s, out double3 c)
        {
            s = new double3(0);
            c = new double3(0);
            throwDependencyException();
        }

        public static double4x4 transpose(double4x4 m)
        {
            throwDependencyException();
            return new double4x4(0);
        }

        public static double4x4 inverse(double4x4 m)
        {
            throwDependencyException();
            return new double4x4(0);
        }
    }
}

#endif // !ARCORE_INTERNAL_USE_UNITY_MATH
