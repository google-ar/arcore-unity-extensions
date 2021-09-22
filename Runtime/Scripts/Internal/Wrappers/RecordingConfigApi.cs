//-----------------------------------------------------------------------
// <copyright file="RecordingConfigApi.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.Internal
{
    using System;
    using System.Runtime.InteropServices;

#if UNITY_ANDROID
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class RecordingConfigApi
    {
        public static IntPtr Create(IntPtr sessionHandle, ARCoreRecordingConfig config)
        {
            IntPtr configHandle = IntPtr.Zero;
#if UNITY_ANDROID
            ExternApi.ArRecordingConfig_create(sessionHandle, ref configHandle);

            if (config != null)
            {
                ExternApi.ArRecordingConfig_setMp4DatasetUri(
                    sessionHandle,
                    configHandle,
                    config.Mp4DatasetUri?.AbsoluteUri);
                ExternApi.ArRecordingConfig_setAutoStopOnPause(
                    sessionHandle,
                    configHandle,
                    config.AutoStopOnPause ? 1 : 0);
                foreach (Track track in config.Tracks)
                {
                    IntPtr trackHandle = TrackApi.Create(sessionHandle, track);

                    ExternApi.ArRecordingConfig_addTrack(sessionHandle, configHandle, trackHandle);

                    // Internally the recording config uses the TrackData to generate its
                    // own local structures, so it is appropriate to destroy it after sending it to
                    // the recording config.
                    TrackApi.Destroy(trackHandle);
                }
            }
#endif

            return configHandle;
        }

        public static void Destroy(IntPtr recordingConfigHandle)
        {
#if UNITY_ANDROID
            ExternApi.ArRecordingConfig_destroy(recordingConfigHandle);
#endif
        }

        private struct ExternApi
        {
#if UNITY_ANDROID
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArRecordingConfig_create(
                IntPtr session, ref IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArRecordingConfig_destroy(
                IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArRecordingConfig_setMp4DatasetUri(
                IntPtr session, IntPtr configHandle, String datasetUri);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArRecordingConfig_setAutoStopOnPause(
                IntPtr session, IntPtr configHandle, int configEnabled);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArRecordingConfig_addTrack(
                IntPtr session, IntPtr configHandle, IntPtr trackHandle);
#endif
        }
    }
}
