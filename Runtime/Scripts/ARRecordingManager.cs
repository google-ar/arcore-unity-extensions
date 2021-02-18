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

    /// <summary>
    /// Provides access to session recording functionality.
    /// </summary>
    public static class ARRecordingManager
    {
        /// <summary>
        /// Gets the current state of the recorder.
        /// </summary>
        /// <returns>The current <see cref="RecordingStatus"/>.</returns>
        public static RecordingStatus RecordingStatus
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
        public static RecordingResult StartRecording(ARCoreRecordingConfig config)
        {
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
        public static RecordingResult StopRecording()
        {
            return SessionApi.StopRecording(ARCoreExtensions._instance.currentARCoreSessionHandle);
        }
    }
}
