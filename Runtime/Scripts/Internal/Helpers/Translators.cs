//-----------------------------------------------------------------------
// <copyright file="Translators.cs" company="Google">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    internal static class Translators
    {
        private static readonly Matrix4x4 k_UnityWorldToGLWorld
            = Matrix4x4.Scale(new Vector3(1, 1, -1));

        private static readonly Matrix4x4 k_UnityWorldToGLWorldInverse
            = k_UnityWorldToGLWorld.inverse;

        public static CloudAnchorState ToCloudAnchorState(ApiCloudAnchorState state)
        {
            switch (state)
            {
                case ApiCloudAnchorState.None:
                    return CloudAnchorState.None;
                case ApiCloudAnchorState.TaskInProgress:
                    return CloudAnchorState.TaskInProgress;
                case ApiCloudAnchorState.Success:
                    return CloudAnchorState.Success;
                case ApiCloudAnchorState.ErrorInternal:
                    return CloudAnchorState.ErrorInternal;
                case ApiCloudAnchorState.ErrorNotAuthorized:
                    return CloudAnchorState.ErrorNotAuthorized;
                case ApiCloudAnchorState.ErrorResourceExhausted:
                    return CloudAnchorState.ErrorResourceExhausted;
                case ApiCloudAnchorState.ErrorHostingDatasetProcessingFailed:
                    return CloudAnchorState.ErrorHostingDatasetProcessingFailed;
                case ApiCloudAnchorState.ErrorResolvingCloudIdNotFound:
                    return CloudAnchorState.ErrorResolvingCloudIdNotFound;
                case ApiCloudAnchorState.ErrorResolvingSDKTooOld:
                    return CloudAnchorState.ErrorResolvingPackageTooOld;
                case ApiCloudAnchorState.ErrorResolvingSDKTooNew:
                    return CloudAnchorState.ErrorResolvingPackageTooNew;
                case ApiCloudAnchorState.ErrorHostingServiceUnavailable:
                    return CloudAnchorState.ErrorHostingServiceUnavailable;
            }

            return CloudAnchorState.None;
        }

        public static TrackingState ToTrackingState(ApiTrackingState state)
        {
            switch (state)
            {
                case ApiTrackingState.Tracking:
                    return TrackingState.Tracking;
                case ApiTrackingState.Paused:
                case ApiTrackingState.Stopped:
                    return TrackingState.None;
            }

            return TrackingState.None;
        }

        public static ApiPose ToApiPose(Pose unityPose)
        {
            Matrix4x4 glWorld_T_glLocal =
                Matrix4x4.TRS(unityPose.position, unityPose.rotation, Vector3.one);
            Matrix4x4 unityWorld_T_unityLocal =
                k_UnityWorldToGLWorld * glWorld_T_glLocal * k_UnityWorldToGLWorldInverse;

            Vector3 position = unityWorld_T_unityLocal.GetColumn(3);
            Quaternion rotation = Quaternion.LookRotation(unityWorld_T_unityLocal.GetColumn(2),
                unityWorld_T_unityLocal.GetColumn(1));

            ApiPose apiPose;
            apiPose.X = position.x;
            apiPose.Y = position.y;
            apiPose.Z = position.z;
            apiPose.Qx = rotation.x;
            apiPose.Qy = rotation.y;
            apiPose.Qz = rotation.z;
            apiPose.Qw = rotation.w;

            return apiPose;
        }

        public static Pose ToUnityPose(ApiPose apiPose)
        {
            Matrix4x4 glWorld_T_glLocal =
                Matrix4x4.TRS(
                    new Vector3(apiPose.X, apiPose.Y, apiPose.Z),
                    new Quaternion(apiPose.Qx, apiPose.Qy, apiPose.Qz, apiPose.Qw), Vector3.one);
            Matrix4x4 unityWorld_T_unityLocal =
                k_UnityWorldToGLWorld * glWorld_T_glLocal * k_UnityWorldToGLWorldInverse;

            Vector3 position = unityWorld_T_unityLocal.GetColumn(3);
            Quaternion rotation = Quaternion.LookRotation(unityWorld_T_unityLocal.GetColumn(2),
                unityWorld_T_unityLocal.GetColumn(1));

            return new Pose(position, rotation);
        }
    }
}
