//-----------------------------------------------------------------------
// <copyright file="ApiQuaternion.cs" company="Google LLC">
//
// Copyright 2021 Google LLC
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

namespace Google.XR.ARCoreExtensions.Internal
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// Quaternion data container.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
    Justification = "Internal")]
    public struct ApiQuaternion
    {
        [MarshalAs(UnmanagedType.R4)]
        public float Qx;

        [MarshalAs(UnmanagedType.R4)]
        public float Qy;

        [MarshalAs(UnmanagedType.R4)]
        public float Qz;

        [MarshalAs(UnmanagedType.R4)]
        public float Qw;

        public override string ToString()
        {
            return string.Format("qx: {0}, qy: {1}, qz: {2}, qw: {3}", Qx, Qy, Qz, Qw);
        }
    }
}
