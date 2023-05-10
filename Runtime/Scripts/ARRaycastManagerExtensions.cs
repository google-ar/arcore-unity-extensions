//-----------------------------------------------------------------------
// <copyright file="ARRaycastManagerExtensions.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions
{
    using System;
    using System.Collections.Generic;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;

    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Extensions to AR Foundation's <c><see cref="ARRaycastManager"/></c> class.
    /// </summary>
    public static class ARRaycastManagerExtensions
    {
        /// <summary>
        /// Performs a raycast against <c><see cref="ARStreetscapeGeometry"/></c>s being tracked by
        /// ARCore. Outputs all intersections along the ray sorted in ascending order of distance.
        /// Accepts Unity's native 2D screen coordinate (0, 0) starting from the bottom left.
        /// </summary>
        /// <param name="raycastManager">The <c><see cref="ARRaycastManager"/></c> this method
        /// extends.</param>
        /// <param name="screenPoint">2D screen touch position in Unity screen coordinates.</param>
        /// <param name="raycastResults">A list of <c><see cref="XRRaycastHit"/></c>
        /// if the raycast is successful.</param>
        /// <returns><c>true</c> if the raycast had a hit, otherwise <c>false</c>.</returns>
        public static bool RaycastStreetscapeGeometry(this ARRaycastManager raycastManager,
            Vector2 screenPoint, ref List<XRRaycastHit> raycastResults)
        {
            IntPtr frameHandle;
            if (!GetFrameHandle(out frameHandle))
            {
                return false;
            }

            bool hasHit = RaycastApi.Raycast(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                frameHandle,
                screenPoint,
                ref raycastResults);

            return hasHit && raycastResults.Count != 0;
        }

        /// <summary>
        /// Performs a raycast against <c><see cref="ARStreetscapeGeometry"/></c>s being tracked by
        /// ARCore. Outputs all intersections along the ray sorted in ascending order of distance.
        /// Accepts Unity's native 3D coordinates.
        /// </summary>
        /// <param name="raycastManager"> The <c><see cref="ARRaycastManager"/></c> this methods extends.</param>
        /// <param name="ray"> Ray for the origin and direction of the raycast.</param>
        /// <param name="raycastResults">A list of <c><see cref="XRRaycastHit"/></c>
        /// if the raycast is successful.</param>
        /// <returns><c>true</c> if the raycast had a hit, otherwise <c>false</c>.</returns>
        public static bool RaycastStreetscapeGeometry(this ARRaycastManager raycastManager,
            Ray ray, ref List<XRRaycastHit> raycastResults)
        {
            IntPtr frameHandle;
            if (!GetFrameHandle(out frameHandle))
            {
                return false;
            }

            bool hasHit = RaycastApi.Raycast(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                frameHandle,
                ray,
                ref raycastResults);

            return hasHit && raycastResults.Count != 0;
        }

        /// <summary>
        /// Gets current frame handle.
        /// </summary>
        /// <param name="frameHandle"> The handle to the <c><see cref="XRCameraFrame"/></c>
        /// .</param>
        /// <returns><c>true</c> if successful, otherwise <c>false</c>.</returns>
        private static bool GetFrameHandle(out IntPtr frameHandle)
        {
            // TODO(b/277252165): refactor this method to return frameHandle and avoid out.
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            frameHandle = IOSSupportManager.Instance.ARCoreFrameHandle;
            if (frameHandle == null)
            {
                return false;
            }

            return true;
#else
            XRCameraFrame frame;
            if (!ARCoreExtensions.TryGetLatestFrame(out frame))
            {
                frameHandle = IntPtr.Zero;
                return false;
            }

            frameHandle = frame.FrameHandle();
            return true;
#endif
        }
    }
}
