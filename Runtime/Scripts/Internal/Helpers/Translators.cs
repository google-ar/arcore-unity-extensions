//-----------------------------------------------------------------------
// <copyright file="Translators.cs" company="Google LLC">
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
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    internal static class Translators
    {
        private static readonly Matrix4x4 _unityWorldToGLWorld
            = Matrix4x4.Scale(new Vector3(1, 1, -1));

        private static readonly Matrix4x4 _unityWorldToGLWorldInverse
            = _unityWorldToGLWorld.inverse;

        private static readonly Quaternion _unityWorldToGLWorldRotation
            = Quaternion.LookRotation(
                _unityWorldToGLWorld.GetColumn(2), _unityWorldToGLWorld.GetColumn(1));

        private static readonly Quaternion _glWorldToUnityWorldRotation
            = Quaternion.Inverse(_unityWorldToGLWorldRotation);
        public static CloudAnchorState ToCloudAnchorState(this ApiCloudAnchorState state)
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

        public static TrackingState ToTrackingState(this ApiTrackingState state)
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

        public static ApiPose ToApiPose(this Pose unityPose)
        {
            Matrix4x4 glWorld_T_glLocal =
                Matrix4x4.TRS(unityPose.position, unityPose.rotation, Vector3.one);
            Matrix4x4 unityWorld_T_unityLocal =
                _unityWorldToGLWorld * glWorld_T_glLocal * _unityWorldToGLWorldInverse;

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

        public static Pose ToUnityPose(this ApiPose apiPose)
        {
            Matrix4x4 glWorld_T_glLocal =
                Matrix4x4.TRS(
                    new Vector3(apiPose.X, apiPose.Y, apiPose.Z),
                    new Quaternion(apiPose.Qx, apiPose.Qy, apiPose.Qz, apiPose.Qw), Vector3.one);
            Matrix4x4 unityWorld_T_unityLocal =
                _unityWorldToGLWorld * glWorld_T_glLocal * _unityWorldToGLWorldInverse;

            Vector3 position = unityWorld_T_unityLocal.GetColumn(3);
            Quaternion rotation = Quaternion.LookRotation(unityWorld_T_unityLocal.GetColumn(2),
                unityWorld_T_unityLocal.GetColumn(1));

            return new Pose(position, rotation);
        }

        public static RecordingStatus ToRecordingStatus(this ApiRecordingStatus apiStatus)
        {
            switch (apiStatus)
            {
                case ApiRecordingStatus.OK:
                    return RecordingStatus.OK;
                case ApiRecordingStatus.IOError:
                    return RecordingStatus.IOError;
                case ApiRecordingStatus.None:
                    return RecordingStatus.None;
                default:
                    Debug.LogErrorFormat("Unrecognized ApiRecordingStatus value {0}", apiStatus);
                    return RecordingStatus.None;
            }
        }

        public static RecordingResult ToRecordingResult(this ApiArStatus apiArStatus)
        {
            switch (apiArStatus)
            {
                case ApiArStatus.Success:
                    return RecordingResult.OK;
                case ApiArStatus.ErrorIllegalState:
                    return RecordingResult.ErrorIllegalState;
                case ApiArStatus.ErrorInvalidArgument:
                    return RecordingResult.ErrorInvalidArgument;
                case ApiArStatus.ErrorRecordingFailed:
                    return RecordingResult.ErrorRecordingFailed;
                default:
                    Debug.LogErrorFormat(
                        "Recording failed with unexpected status: {0}", apiArStatus);
                    return RecordingResult.ErrorRecordingFailed;
            }
        }

        public static PlaybackStatus ToPlaybackStatus(this ApiPlaybackStatus apiStatus)
        {
            switch (apiStatus)
            {
                case ApiPlaybackStatus.None:
                    return PlaybackStatus.None;
                case ApiPlaybackStatus.OK:
                    return PlaybackStatus.OK;
                case ApiPlaybackStatus.IOError:
                    return PlaybackStatus.IOError;
                case ApiPlaybackStatus.FinishedSuccess:
                    return PlaybackStatus.FinishedSuccess;
                default:
                    Debug.LogErrorFormat("Unrecognized ApiPlaybackStatus value {0}", apiStatus);
                    return PlaybackStatus.None;
            }
        }

        public static PlaybackResult ToPlaybackResult(this ApiArStatus apiArStatus)
        {
            switch (apiArStatus)
            {
                case ApiArStatus.Success:
                    return PlaybackResult.OK;
                case ApiArStatus.ErrorSessionNotPaused:
                    return PlaybackResult.ErrorSessionNotPaused;
                case ApiArStatus.ErrorSessionUnsupported:
                    return PlaybackResult.ErrorSessionUnsupported;
                case ApiArStatus.ErrorPlaybackFailed:
                    return PlaybackResult.ErrorPlaybackFailed;
                default:
                    Debug.LogErrorFormat(
                        "Playback dataset failed with unexpected status: {0}", apiArStatus);
                    return PlaybackResult.ErrorPlaybackFailed;
            }
        }

        public static ApiGeospatialMode ToApiGeospatialMode(this GeospatialMode mode)
        {
            switch (mode)
            {
                case GeospatialMode.Enabled:
                    return ApiGeospatialMode.Enabled;
                case GeospatialMode.Disabled:
                    return ApiGeospatialMode.Disabled;
                default:
                    Debug.LogErrorFormat("Unrecognized GeospatialMode value: {0}", mode);
                    return ApiGeospatialMode.Disabled;
            }
        }

        public static Quaternion ToUnityQuaternion(this ApiQuaternion apiQuaternion)
        {
            var glWorldQuaternion = new Quaternion(
                apiQuaternion.Qx, apiQuaternion.Qy, apiQuaternion.Qz, apiQuaternion.Qw);
            return _unityWorldToGLWorldRotation * glWorldQuaternion;
        }

        public static ApiQuaternion ToApiQuaternion(this Quaternion quaternion)
        {
            Quaternion glWorldQuaternion = _glWorldToUnityWorldRotation * quaternion;
            var apiQuaternion = new ApiQuaternion();
            apiQuaternion.Qx = glWorldQuaternion.x;
            apiQuaternion.Qy = glWorldQuaternion.y;
            apiQuaternion.Qz = glWorldQuaternion.z;
            apiQuaternion.Qw = glWorldQuaternion.w;
            return apiQuaternion;
        }
    }
}
