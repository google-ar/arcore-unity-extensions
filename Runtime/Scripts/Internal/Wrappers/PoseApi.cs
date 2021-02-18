//-----------------------------------------------------------------------
// <copyright file="PoseApi.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using UnityEngine;

    internal class PoseApi
    {
        public static IntPtr Create(IntPtr sessionHandle)
        {
            return Create(sessionHandle, Pose.identity);
        }

        public static IntPtr Create(IntPtr sessionHandle, Pose pose)
        {
            ApiPose apiPose = pose.ToApiPose();
            IntPtr poseHandle = IntPtr.Zero;
            ExternApi.ArPose_create(
                sessionHandle,
                ref apiPose,
                ref poseHandle);
            return poseHandle;
        }

        public static void Destroy(IntPtr poseHandle)
        {
            ExternApi.ArPose_destroy(poseHandle);
        }

        public static ApiPose ExtractPoseValue(
            IntPtr sessionHandle,
            IntPtr poseHandle)
        {
            ApiPose apiPose = Pose.identity.ToApiPose();
            ExternApi.ArPose_getPoseRaw(
                sessionHandle,
                poseHandle,
                ref apiPose);
            return apiPose;
        }

        [SuppressMessage("UnityRules.UnityStyleRules", "US1113:MethodsMustBeUpperCamelCase",
         Justification = "External call.")]
        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPose_create(
                IntPtr sessionHandle,
                ref ApiPose apiPose,
                ref IntPtr poseHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPose_destroy(IntPtr poseHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPose_getPoseRaw(
                IntPtr sessionHandle,
                IntPtr poseHandle,
                ref ApiPose apiPose);
        }
    }
}
