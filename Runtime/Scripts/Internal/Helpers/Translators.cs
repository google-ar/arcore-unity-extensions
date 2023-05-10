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
    using System;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    internal static class Translators
    {
        private static readonly Matrix4x4 _unityWorldToGLWorld
            = Matrix4x4.Scale(new Vector3(1, 1, -1));

        private static readonly Matrix4x4 _unityWorldToGLWorldInverse
            = _unityWorldToGLWorld.inverse;

        public static ApiSemanticMode ToApiSemanticMode(this SemanticMode mode)
        {
            switch (mode)
            {
                case SemanticMode.Enabled:
                    return ApiSemanticMode.Enabled;
                case SemanticMode.Disabled:
                default:
                    return ApiSemanticMode.Disabled;
            }
        }

        public static ApiSemanticLabel ToApiSemanticLabel(this SemanticLabel label)
        {
            // StreetView12
            switch (label)
            {
                case SemanticLabel.Sky:
                    return ApiSemanticLabel.Sky;
                case SemanticLabel.Building:
                    return ApiSemanticLabel.Building;
                case SemanticLabel.Tree:
                    return ApiSemanticLabel.Tree;
                case SemanticLabel.Road:
                    return ApiSemanticLabel.Road;
                case SemanticLabel.Sidewalk:
                    return ApiSemanticLabel.Sidewalk;
                case SemanticLabel.Terrain:
                    return ApiSemanticLabel.Terrain;
                case SemanticLabel.Structure:
                    return ApiSemanticLabel.Structure;
                case SemanticLabel.Object:
                    return ApiSemanticLabel.Object;
                case SemanticLabel.Vehicle:
                    return ApiSemanticLabel.Vehicle;
                case SemanticLabel.Person:
                    return ApiSemanticLabel.Person;
                case SemanticLabel.Water:
                    return ApiSemanticLabel.Water;
                case SemanticLabel.Unlabeled:
                default:
                    return ApiSemanticLabel.Unlabeled;
            }
        }

        public static CloudAnchorState ToCloudAnchorState(this ApiCloudAnchorState state)
        {
            switch (state)
            {
                case ApiCloudAnchorState.None:
                    return CloudAnchorState.None;
#pragma warning disable CS0618 // Handle obsoleted value CloudAnchorState.TaskInProgress
                case ApiCloudAnchorState.TaskInProgress:
                    return CloudAnchorState.TaskInProgress;
#pragma warning restore CS0618 // Handle obsoleted value CloudAnchorState.TaskInProgress
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

        public static Vector3 ToUnityVector(this Vector3 apiVector)
        {
            return _unityWorldToGLWorld.MultiplyVector(apiVector);
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

        public static Quaternion ToUnityQuaternion(this ApiQuaternion apiEusQuaternion)
        {
            // change from EUS (OpenGl) to EUN (Unity)
            // This mirror z axis which switches from right to left handed
            // reverse the rotations direction of x and y
            // z rotation are not effected
            var unityEunQuaternion = new Quaternion(-apiEusQuaternion.Qx, -apiEusQuaternion.Qy,
                                                    apiEusQuaternion.Qz, apiEusQuaternion.Qw);
            return unityEunQuaternion;
        }

        public static ApiQuaternion ToApiQuaternion(this Quaternion eunQuaternion)
        {
            // change from EUN (Unity) to EUS (OpenGl)
            // This mirror z axis which switches from left to right handed
            // reverse the rotations direction of x and y
            // z rotation are not effected
            var apiEusQuaternion = new ApiQuaternion();
            apiEusQuaternion.Qx = -eunQuaternion.x;
            apiEusQuaternion.Qy = -eunQuaternion.y;
            apiEusQuaternion.Qz = eunQuaternion.z;
            apiEusQuaternion.Qw = eunQuaternion.w;
            return apiEusQuaternion;
        }

        public static TerrainAnchorState ToTerrainAnchorState(this ApiTerrainAnchorState state)
        {
            switch (state)
            {
                case ApiTerrainAnchorState.None:
                    return TerrainAnchorState.None;
#pragma warning disable CS0618 // Handle obsoleted value TerrainAnchorState.TaskInProgress
                case ApiTerrainAnchorState.TaskInProgress:
                    return TerrainAnchorState.TaskInProgress;
#pragma warning restore CS0618 // Handle obsoleted value TerrainAnchorState.TaskInProgress
                case ApiTerrainAnchorState.Success:
                    return TerrainAnchorState.Success;
                case ApiTerrainAnchorState.ErrorInternal:
                    return TerrainAnchorState.ErrorInternal;
                case ApiTerrainAnchorState.ErrorNotAuthorized:
                    return TerrainAnchorState.ErrorNotAuthorized;
                case ApiTerrainAnchorState.ErrorUnsupportedLocation:
                    return TerrainAnchorState.ErrorUnsupportedLocation;
                default:
                    return TerrainAnchorState.None;
            }
        }

        public static TrackableId ToTrackableId(this IntPtr trackableHandle)
        {
            return new TrackableId(0, (ulong)trackableHandle);
        }

        public static IntPtr ToNativePtr(this TrackableId trackableId)
        {
            return (System.IntPtr)trackableId.subId2;
        }

        public static StreetscapeGeometryType ToStreetscapeGeometryType(
            this ApiStreetscapeGeometryType type)
        {
            switch (type)
            {
                case ApiStreetscapeGeometryType.Terrain:
                    return StreetscapeGeometryType.Terrain;
                case ApiStreetscapeGeometryType.Building:
                    return StreetscapeGeometryType.Building;
                default:
                    return StreetscapeGeometryType.Terrain;
            }
        }

        public static ApiStreetscapeGeometryMode ToApiStreetscapeGeometryMode(
            this StreetscapeGeometryMode mode)
        {
            switch (mode)
            {
                case StreetscapeGeometryMode.Enabled:
                    return ApiStreetscapeGeometryMode.Enabled;
                default:
                    return ApiStreetscapeGeometryMode.Disabled;
            }
        }

        public static StreetscapeGeometryQuality ToStreetscapeGeometryQuality(
            this ApiStreetscapeGeometryQuality quality)
        {
            switch (quality)
            {
                case ApiStreetscapeGeometryQuality.BuildingLOD1:
                    return StreetscapeGeometryQuality.BuildingLOD1;
                case ApiStreetscapeGeometryQuality.BuildingLOD2:
                    return StreetscapeGeometryQuality.BuildingLOD2;
                default:
                    return StreetscapeGeometryQuality.None;
            }
        }

        public static RooftopAnchorState ToRooftopAnchorState(this ApiRooftopAnchorState state)
        {
            switch (state)
            {
                case ApiRooftopAnchorState.Success:
                    return RooftopAnchorState.Success;
                case ApiRooftopAnchorState.ErrorInternal:
                    return RooftopAnchorState.ErrorInternal;
                case ApiRooftopAnchorState.ErrorNotAuthorized:
                    return RooftopAnchorState.ErrorNotAuthorized;
                case ApiRooftopAnchorState.ErrorUnsupportedLocation:
                    return RooftopAnchorState.ErrorUnsupportedLocation;
                default:
                    return RooftopAnchorState.None;
            }
        }
    }
}
