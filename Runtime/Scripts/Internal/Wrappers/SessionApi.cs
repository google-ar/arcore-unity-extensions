//-----------------------------------------------------------------------
// <copyright file="SessionApi.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

#if UNITY_IOS
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#if CLOUDANCHOR_IOS_SUPPORT
    using CloudAnchorImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using CloudAnchorImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#endif
#if GEOSPATIAL_IOS_SUPPORT
    using EarthImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using EarthImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#endif
#else // UNITY_ANDROID
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using CloudAnchorImport = System.Runtime.InteropServices.DllImportAttribute;
    using EarthImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class SessionApi
    {
        public static IntPtr HostCloudAnchor(IntPtr sessionHandle, IntPtr anchorHandle)
        {
            IntPtr cloudAnchorHandle = IntPtr.Zero;
#if !UNITY_IOS || CLOUDANCHOR_IOS_SUPPORT
            ApiArStatus status = ExternApi.ArSession_hostAndAcquireNewCloudAnchor(
                sessionHandle,
                anchorHandle,
                ref cloudAnchorHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to host a new Cloud Anchor, status '{0}'", status);
            }
#endif
            return cloudAnchorHandle;
        }

        public static IntPtr HostCloudAnchor(IntPtr sessionHandle, IntPtr anchorHandle, int ttlDays)
        {
            IntPtr cloudAnchorHandle = IntPtr.Zero;
#if !UNITY_IOS || CLOUDANCHOR_IOS_SUPPORT
            ApiArStatus status = ExternApi.ArSession_hostAndAcquireNewCloudAnchorWithTtl(
                sessionHandle, anchorHandle, ttlDays, ref cloudAnchorHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to host a Cloud Anchor with TTL {0}, status '{1}'",
                    ttlDays, status);
            }
#endif
            return cloudAnchorHandle;
        }

        public static void SetAuthToken(IntPtr sessionHandle, string authToken)
        {
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            ExternApi.ArSession_setAuthToken(sessionHandle, authToken);
#endif
        }

        public static IntPtr ResolveCloudAnchor(IntPtr sessionHandle, string cloudAnchorId)
        {
            IntPtr cloudAnchorHandle = IntPtr.Zero;
#if !UNITY_IOS || CLOUDANCHOR_IOS_SUPPORT
            ApiArStatus status = ExternApi.ArSession_resolveAndAcquireNewCloudAnchor(
                sessionHandle,
                cloudAnchorId,
                ref cloudAnchorHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to resolve a new Cloud Anchor, status '{0}'", status);
            }
#endif
            return cloudAnchorHandle;
        }

        public static FeatureMapQuality EstimateFeatureMapQualityForHosting(
            IntPtr sessionHandle, Pose pose)
        {
            int featureMapQuality = (int)FeatureMapQuality.Insufficient;
#if !UNITY_IOS || CLOUDANCHOR_IOS_SUPPORT
            IntPtr poseHandle = PoseApi.Create(sessionHandle, pose);
            var status = ExternApi.ArSession_estimateFeatureMapQualityForHosting(
                sessionHandle, poseHandle, ref featureMapQuality);
            PoseApi.Destroy(poseHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to estimate feature map quality with status '{0}'.",
                    status);
            }
#endif
            return (FeatureMapQuality)featureMapQuality;
        }

        public static RecordingStatus GetRecordingStatus(IntPtr sessionHandle)
        {
            ApiRecordingStatus apiStatus = ApiRecordingStatus.None;
#if UNITY_ANDROID
            ExternApi.ArSession_getRecordingStatus(sessionHandle, ref apiStatus);
#endif
            return apiStatus.ToRecordingStatus();
        }

        public static RecordingResult StartRecording(
            IntPtr sessionHandle, ARCoreRecordingConfig config)
        {
            ApiArStatus status = ApiArStatus.ErrorFatal;
#if UNITY_ANDROID
            IntPtr recordingConfigHandle = RecordingConfigApi.Create(sessionHandle, config);
            status = ExternApi.ArSession_startRecording(sessionHandle, recordingConfigHandle);
            RecordingConfigApi.Destroy(recordingConfigHandle);
#endif
            return status.ToRecordingResult();
        }

        public static RecordingResult StopRecording(IntPtr sessionHandle)
        {
            ApiArStatus status = ApiArStatus.ErrorFatal;
#if UNITY_ANDROID
            status = ExternApi.ArSession_stopRecording(sessionHandle);
#endif
            return status.ToRecordingResult();
        }

        public static PlaybackStatus GetPlaybackStatus(IntPtr sessionHandle)
        {
            ApiPlaybackStatus apiStatus = ApiPlaybackStatus.None;
#if UNITY_ANDROID
            ExternApi.ArSession_getPlaybackStatus(sessionHandle, ref apiStatus);
#endif
            return apiStatus.ToPlaybackStatus();
        }

        public static PlaybackResult SetPlaybackDataset(
            IntPtr sessionHandle, string datasetFilepath)
        {
            ApiArStatus status = ApiArStatus.ErrorFatal;
#if UNITY_ANDROID
            status = ExternApi.ArSession_setPlaybackDataset(sessionHandle, datasetFilepath);
#endif
            return status.ToPlaybackResult();
        }

        public static PlaybackResult SetPlaybackDatasetUri(
            IntPtr sessionHandle, string datasetUri)
        {
            ApiArStatus status = ApiArStatus.ErrorFatal;
#if UNITY_ANDROID
            status = ExternApi.ArSession_setPlaybackDatasetUri(sessionHandle, datasetUri);
#endif
            return status.ToPlaybackResult();
        }

        public static FeatureSupported IsGeospatialModeSupported(
            IntPtr sessionHandle, GeospatialMode mode)
        {
            FeatureSupported supported = FeatureSupported.Unknown;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            int isSupported = 0;
            ExternApi.ArSession_isGeospatialModeSupported(
                sessionHandle, mode.ToApiGeospatialMode(), ref isSupported);
            supported = isSupported == 0 ?
                FeatureSupported.Unsupported : FeatureSupported.Supported;
#endif
            return supported;
        }

        public static IntPtr AcquireEarth(IntPtr sessionHandle)
        {
            var earthHandle = IntPtr.Zero;
#if !UNITY_IOS || GEOSPATIAL_IOS_SUPPORT
            ExternApi.ArSession_acquireEarth(sessionHandle, ref earthHandle);
#endif
            return earthHandle;
        }

        private struct ExternApi
        {
            [CloudAnchorImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_hostAndAcquireNewCloudAnchor(
                IntPtr sessionHandle,
                IntPtr anchorHandle,
                ref IntPtr cloudAnchorHandle);

            [CloudAnchorImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_resolveAndAcquireNewCloudAnchor(
                IntPtr sessionHandle,
                string cloudAnchorId,
                ref IntPtr cloudAnchorHandle);

            [CloudAnchorImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_hostAndAcquireNewCloudAnchorWithTtl(
                IntPtr sessionHandle,
                IntPtr anchorHandle,
                int ttlDays,
                ref IntPtr cloudAnchorHandle);

            [CloudAnchorImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_estimateFeatureMapQualityForHosting(
                IntPtr sessionHandle,
                IntPtr poseHandle,
                ref int featureMapQuality);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_isGeospatialModeSupported(
                IntPtr sessionHandle, ApiGeospatialMode mode, ref int out_is_supported);

            [EarthImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_acquireEarth(IntPtr sessionHandle,
                                                             ref IntPtr earthHandle);
#if UNITY_IOS
#if ARCORE_EXTENSIONS_IOS_SUPPORT
            [IOSImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_setAuthToken(
                IntPtr sessionHandle, string authToken);
#endif // ARCORE_EXTENSIONS_IOS_SUPPORT
#elif UNITY_ANDROID

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getConfig(
                IntPtr sessionHandle,
                IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getRecordingStatus(
                IntPtr sessionHandle, ref ApiRecordingStatus recordingStatus);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_startRecording(
                IntPtr sessionHandle, IntPtr recordingConfigHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_stopRecording(
                IntPtr sessionHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getPlaybackStatus(
                IntPtr sessionHandle, ref ApiPlaybackStatus playbackStatus);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_setPlaybackDataset(
                IntPtr sessionHandle, string datasetFilepath);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_setPlaybackDatasetUri(
                IntPtr sessionHandle, string encodedDatasetUri);
#endif // UNITY_ANDROID
        }
    }
}
