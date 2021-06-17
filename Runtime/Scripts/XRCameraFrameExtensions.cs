//-----------------------------------------------------------------------
// <copyright file="XRCameraFrameExtensions.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Extensions to AR Subsystem's XRCameraFrame struct.
    /// </summary>
    public static class XRCameraFrameExtensions
    {
        /// <summary>
        /// Get the 4x4 image display matrix for the camera frame. This is used by the texture
        /// returned <c><see cref="AROcclusionManagerExtensions.GetPersonMaskTexture(
        /// UnityEngine.XR.ARFoundation.AROcclusionManager)"/></c> to calculate the display
        /// coordinates.
        /// </summary>
        /// <param name="frame">The XRCameraFrame instance.</param>
        /// <returns>The 4x4 image display matrix.</returns>
        public static Matrix4x4 GetImageDisplayMatrix(this XRCameraFrame frame)
        {
            // Unity Screen Coordinate:      Android Screen Coordinate (flipped Y-Axis):
            // (0, 1)      (1, 1)            (0, 0)      (1, 0)
            // |----------------|            |----------------|
            // |                |            |                |
            // |                |            |                |
            // |                |            |                |
            // |                |            |                |
            // |                |            |                |
            // |                |            |                |
            // |                |            |                |
            // |----------------|            |----------------|
            // (0, 0)      (1, 0)            (0, 1)      (1, 1)
            IntPtr sessionHandle = ARCoreExtensions._instance.currentARCoreSessionHandle;

            // X-Axis (1, 0) in Unity view maps to (1, 1) on Android screen.
            Vector2 affineBasisX = new Vector2(1.0f, 1.0f);

            // Y-Axis (0, 1) in Unity view maps to (0, 0) on Android screen.
            Vector2 affineBasisY = new Vector2(0.0f, 0.0f);

            // Origin (0, 0) in Unity view maps to (0, 1) on Android screen.
            Vector2 affineOrigin = new Vector2(0.0f, 1.0f);

            Vector2 transformedX = FrameApi.TransformCoordinates2d(
                sessionHandle, frame.FrameHandle(), ApiCoordinates2dType.ViewNormalized,
                ApiCoordinates2dType.ImageNormalized, ref affineBasisX);
            Vector2 transformedY = FrameApi.TransformCoordinates2d(
                sessionHandle, frame.FrameHandle(), ApiCoordinates2dType.ViewNormalized,
                ApiCoordinates2dType.ImageNormalized, ref affineBasisY);
            Vector2 transformedOrigin = FrameApi.TransformCoordinates2d(
                sessionHandle, frame.FrameHandle(), ApiCoordinates2dType.ViewNormalized,
                ApiCoordinates2dType.ImageNormalized, ref affineOrigin);

            Matrix4x4 imageMatrix = Matrix4x4.identity;
            imageMatrix[0, 0] = transformedX.x - transformedOrigin.x;
            imageMatrix[0, 1] = transformedX.y - transformedOrigin.y;
            imageMatrix[1, 0] = transformedY.x - transformedOrigin.x;
            imageMatrix[1, 1] = transformedY.y - transformedOrigin.y;
            imageMatrix[2, 0] = transformedOrigin.x;
            imageMatrix[2, 1] = transformedOrigin.y;

            return imageMatrix;
        }
    }
}
