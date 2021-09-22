//-----------------------------------------------------------------------
// <copyright file="ARRecordingManager.cs" company="Google LLC">
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
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Provides access to session recording functionality.
    /// </summary>
    public class ARRecordingManager : MonoBehaviour
    {
        /// <summary>
        /// Gets the current state of the recorder.
        /// </summary>
        /// <returns>The current <see cref="RecordingStatus"/>.</returns>
        public RecordingStatus RecordingStatus
        {
            get
            {
                return SessionApi.GetRecordingStatus(
                    ARCoreExtensions._instance.currentARCoreSessionHandle);
            }
        }

        /// <summary>
        /// Starts a new recording, using the provided <see cref="ARCoreRecordingConfig"/> to define
        /// the location to save the dataset and other options. If a recording is already in
        /// progress this call will fail. Check <see cref="RecordingStatus"/> before making this
        /// call. When an ARCore session is paused (unless <see
        /// cref="ARCoreRecordingConfig"/>.<c>AutoStopOnPause</c> is enabled), recording may
        /// continue. During this time the camera feed will be recorded as a black screen, but
        /// sensor data will continue to be captured.
        /// </summary>
        /// <param name="config"><see cref="ARCoreRecordingConfig"/> containing the path to save the
        /// dataset along with other recording options.</param>
        /// <returns><see cref="RecordingResult"/>.<c>OK</c> if the recording is started (or will
        /// start on the next Session resume.) Or a <see cref="RecordingResult"/> if there was an
        /// error.
        /// </returns>
        public RecordingResult StartRecording(ARCoreRecordingConfig config)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero &&
                ARCoreExtensions._instance.Session.subsystem != null &&
                ARCoreExtensions._instance.Session.subsystem.nativePtr != null)
            {
                return RecordingResult.SessionNotReady;
            }

            return SessionApi.StartRecording(
                ARCoreExtensions._instance.currentARCoreSessionHandle, config);
        }

        /// <summary>
        /// Stops the current recording. If there is no recording in progress, this method will
        /// return <see cref="RecordingResult"/>.<c>OK</c>.
        /// </summary>
        /// <returns><see cref="RecordingResult"/>.<c>OK</c> if the recording was stopped
        /// successfully, or <see cref="RecordingResult"/>.<c>ErrorRecordingFailed</c> if there was
        /// an error.</returns>
        public RecordingResult StopRecording()
        {
            return SessionApi.StopRecording(ARCoreExtensions._instance.currentARCoreSessionHandle);
        }

        /// <summary>
        /// Writes a data sample in the specified external data track. The external samples recorded
        /// using this API will be muxed into the recorded MP4 dataset in a corresponding additional
        /// MP4 stream.
        ///
        /// For smooth playback of the MP4 on video players and for future compatibility
        /// of the MP4 datasets with ARCore's playback of external data tracks it is
        /// recommended that the external samples are recorded at a frequency no higher
        /// than 90kHz.
        ///
        /// Additionally, if the external samples are recorded at a frequency lower than
        /// 1Hz, empty padding samples will be automatically recorded at approximately
        /// one second intervals to fill in the gaps.
        ///
        /// Recording external samples introduces additional CPU and/or I/O overhead and
        /// may affect app performance.
        /// </summary>
        /// <param name="trackId">The unique ID of the track being recorded to. This will be
        /// the <see cref="TrackData.Id"/> used to configure the track.</param>
        /// <param name="data">The data being recorded at current time.</param>
        /// <returns><see cref="RecordingResult"/>.<c>OK</c> if the data was recorded successfully,
        /// or a different <see cref="RecordingResult"/> if there was an error.
        /// </returns>
        public RecordingResult RecordTrackData(Guid trackId, byte[] data)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero &&
                ARCoreExtensions._instance.Session.subsystem != null &&
                ARCoreExtensions._instance.Session.subsystem.nativePtr != null)
            {
                Debug.LogWarning("Failed to record track data. The Session is not yet available. " +
                                 "Try again later.");
                return RecordingResult.ErrorIllegalState;
            }

            ARCameraManager cameraManager = ARCoreExtensions._instance.CameraManager;
            var cameraParams = new XRCameraParams
            {
                zNear = cameraManager.GetComponent<Camera>().nearClipPlane,
                zFar = cameraManager.GetComponent<Camera>().farClipPlane,
                screenWidth = Screen.width,
                screenHeight = Screen.height,
                screenOrientation = Screen.orientation
            };

            if (!cameraManager.subsystem.TryGetLatestFrame(cameraParams, out XRCameraFrame frame))
            {
                Debug.LogWarning("Failed to record track data. The current XRCameraFrame is not " +
                                 "available. Try again later.");
                return RecordingResult.ErrorIllegalState;
            }

            if (frame.timestampNs == 0 || frame.nativePtr == IntPtr.Zero)
            {
                Debug.LogWarning("Failed to record track data. The current XRCameraFrame is not " +
                                 "ready. Try again later.");
                return RecordingResult.ErrorRecordingFailed;
            }

            return FrameApi.RecordTrackData(
                ARCoreExtensions._instance.currentARCoreSessionHandle, frame.FrameHandle(), trackId,
                data);
        }
    }
}
