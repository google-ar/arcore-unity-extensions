//-----------------------------------------------------------------------
// <copyright file="RaycastApi.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

#if UNITY_IOS && !GEOSPATIAL_IOS_SUPPORT
    using RaycastImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#else
    using RaycastImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class RaycastApi
    {
        public static bool Raycast(IntPtr sessionHandle, IntPtr frameHandle, Vector2 screenPoint,
            ref List<XRRaycastHit> outHitList)
        {
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            // Convert to 3D origin and direction ray in Unity space
            Ray ray = Camera.main.ScreenPointToRay(screenPoint);
            return Raycast(sessionHandle, frameHandle, ray, ref outHitList);
#else
            return false;
#endif
        }

        public static bool Raycast(IntPtr sessionHandle, IntPtr frameHandle, Ray ray,
            ref List<XRRaycastHit> outHitList)
        {
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            outHitList.Clear();
            IntPtr hitListHandle = IntPtr.Zero;
            ExternApi.ArHitResultList_create(
                sessionHandle, ref hitListHandle);

            // Invert z to match the ARCore coordinate system.
            Vector3 origin = ray.origin;
            origin.z = -ray.origin.z;
            Vector3 direction = ray.direction;
            direction.z = -ray.direction.z;
            ExternApi.ArFrame_hitTestRay(
                sessionHandle, frameHandle, ref origin, ref direction, hitListHandle);
            FilterAndConstructFacadeHits(sessionHandle, hitListHandle, ref outHitList);
            ExternApi.ArHitResultList_destroy(hitListHandle);
            return outHitList.Count != 0;
#else
            return false;
#endif
        }

        private static void FilterAndConstructFacadeHits(IntPtr sessionHandle,
            IntPtr hitListHandle, ref List<XRRaycastHit> outHitList)
        {
            int hitListSize = 0;
            ExternApi.ArHitResultList_getSize(sessionHandle, hitListHandle, ref hitListSize);

            // Loop over everything the ray hit.
            for (int i = 0; i < hitListSize; i++)
            {
                XRRaycastHit XRRaycastHit;
                if (IsValidHit(sessionHandle, hitListHandle, i, out XRRaycastHit))
                {
                    outHitList.Add(XRRaycastHit);
                }
            }
        }

        private static bool IsValidHit(IntPtr sessionHandle, IntPtr hitListHandle, int index,
            out XRRaycastHit outXRRaycastHit)
        {
            outXRRaycastHit = new XRRaycastHit();

            if (hitListHandle == IntPtr.Zero)
            {
                return false;
            }

            IntPtr hitHandle = IntPtr.Zero;
            ExternApi.ArHitResult_create(sessionHandle, ref hitHandle);
            if (hitHandle == IntPtr.Zero)
            {
                return false;
            }

            ExternApi.ArHitResultList_getItem(sessionHandle, hitListHandle, index, hitHandle);

            // Get the trackable from the hit result.
            IntPtr trackableHandle = IntPtr.Zero;
            ExternApi.ArHitResult_acquireTrackable(sessionHandle, hitHandle, ref trackableHandle);
            if (trackableHandle == IntPtr.Zero)
            {
                return false;
            }

            float distance = 0.0f;
            ExternApi.ArHitResult_getDistance(sessionHandle, hitHandle, ref distance);

            // Check that the hit trackable is a StreetscapeGeometry type.
            ApiTrackableType apiTrackableType = ApiTrackableType.Invalid;
            ExternApi.ArTrackable_getType(sessionHandle, trackableHandle, ref apiTrackableType);
            if (apiTrackableType != ApiTrackableType.StreetscapeGeometry)
            {
                TrackableApi.Release(trackableHandle);
                return false;
            }

            // Get the pose from the hit result.
            IntPtr poseHandle = PoseApi.Create(sessionHandle);
            ExternApi.ArHitResult_getHitPose(sessionHandle, hitHandle, poseHandle);
            Pose pose = PoseApi.ExtractPoseValue(sessionHandle, poseHandle).ToUnityPose();
            PoseApi.Destroy(poseHandle);

            // NOTE: using 'TrackableType.None' to represent a StreetscapeGeometry type
            outXRRaycastHit = new XRRaycastHit(
                trackableHandle.ToTrackableId(), pose, distance, TrackableType.None);

            return true;
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [RaycastImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_hitTestRay(
                IntPtr session, IntPtr frame, ref Vector3 origin, ref Vector3 direction,
                IntPtr hitResultList);

            [RaycastImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResult_create(IntPtr session, ref IntPtr outHitResult);

            [RaycastImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResult_getHitPose(IntPtr session, IntPtr hitResult,
                IntPtr outPose);

            [RaycastImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResult_getDistance(IntPtr session, IntPtr hitResult,
                ref float outDistance);

            [RaycastImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResult_acquireTrackable(IntPtr session,
                IntPtr hitResult, ref IntPtr outTrackable);

            [RaycastImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResult_destroy(IntPtr hitResult);

            [RaycastImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResultList_create(
                IntPtr session, ref IntPtr outHitResultList);

            [RaycastImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResultList_getSize(
                IntPtr session, IntPtr hitResultList, ref int outSize);

            [RaycastImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResultList_getItem(
                IntPtr session, IntPtr hitResultList, int index, IntPtr outHitResult);

            [RaycastImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResultList_destroy(IntPtr hitResultList);

            [RaycastImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackable_getType(IntPtr session, IntPtr trackableHandle,
                ref ApiTrackableType outTrackableType);
#pragma warning restore 626
        }
    }
}
