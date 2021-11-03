//-----------------------------------------------------------------------
// <copyright file="ARPlaybackManager.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Provides access to session playback functionality.
    /// </summary>
    public class ARPlaybackManager : MonoBehaviour
    {
        /// <summary>
        /// Gets the current state of the playback.
        /// </summary>
        /// <returns>The current <see cref="PlaybackStatus"/>.</returns>
        public PlaybackStatus PlaybackStatus
        {
            get
            {
                return SessionApi.GetPlaybackStatus(
                    ARCoreExtensions._instance.currentARCoreSessionHandle);
            }
        }

        /// <summary>
        /// Sets an MP4 dataset file to playback instead of using the live camera feed and IMU
        /// sensor data.
        ///
        /// Restrictions:
        /// <list type="bullet">
        /// <item>
        /// Can only be called while the session is paused. Playback of the MP4 dataset file will
        /// start once the session is resumed.
        /// </item>
        /// <item>
        /// The MP4 dataset file must use the same camera facing direction as is configured in the
        /// session.
        /// </item>
        /// <item>
        /// Due to the way session data is processed, ARCore APIs may sometimes produce different
        /// results during playback than during recording and produce different results during
        /// subsequent playback sessions. For example, the number of detected planes and other
        /// trackables, the precise timing of their detection and their pose over time may be
        /// different in subsequent playback sessions.
        /// </item>
        /// <item>
        /// Once playback has started pausing the session (by disabling the ARSession) will
        /// suspend processing of all camera image frames and any other recorded sensor data in
        /// the dataset. Camera image frames and sensor frame data that is discarded in this way
        /// will not be reprocessed when the session is again resumed (by re-enabling the
        /// ARSession). AR tracking for the session will generally suffer due to the gap in
        /// processed data.
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="datasetFilepath"> The filepath of the MP4 dataset. Null if
        /// stopping the playback and resuming a live feed.</param>
        /// <returns><see cref="PlaybackResult"/>.<c>Success</c> if playback filepath was
        /// set without issue. Otherwise, the <see cref="PlaybackResult"/> will indicate the
        /// error.</returns>
        /// @deprecated Please use SetPlaybackDatasetUri(Uri) instead.
        [Obsolete("This method has been deprecated. "
            + "Please use SetPlaybackDatasetUri(Uri) instead.")]
        public PlaybackResult SetPlaybackDataset(string datasetFilepath)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero &&
                ARCoreExtensions._instance.Session.subsystem != null &&
                ARCoreExtensions._instance.Session.subsystem.nativePtr != null)
            {
                return PlaybackResult.SessionNotReady;
            }

            return SessionApi.SetPlaybackDataset(
                ARCoreExtensions._instance.currentARCoreSessionHandle, datasetFilepath);
        }

        /// <summary>
        /// Sets the uri for a dataset to be played back. The ARCore Session must be paused when
        /// using this method. Resume the Session for the change to take effect. The AbsoluteUri
        /// property of the Uri will be passed to ARCore to create an android.net.Uri.
        ///
        /// The uri must point to a seekable resource.
        ///
        /// See <c><see cref="SetPlaybackDataset(string)"/></c> for more restrictions.
        /// </summary>
        /// <param name="datasetUri"> The uri of the MP4 dataset. Null if stopping the playback and
        /// resuming a live feed.</param>
        /// <returns><see cref="PlaybackResult"/>.<c>Success</c> if playback uri was set without
        /// issue. Otherwise, the <see cref="PlaybackResult"/> will indicate the error.</returns>
        public PlaybackResult SetPlaybackDatasetUri(Uri datasetUri)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero &&
                ARCoreExtensions._instance.Session.subsystem != null &&
                ARCoreExtensions._instance.Session.subsystem.nativePtr != null)
            {
                return PlaybackResult.SessionNotReady;
            }

            return SessionApi.SetPlaybackDatasetUri(
                ARCoreExtensions._instance.currentARCoreSessionHandle, datasetUri.AbsoluteUri);
        }

        /// <summary>
        /// Gets the set of data recorded to the given track available during playback on this
        /// frame.
        /// Note, currently playback continues internally while the session is paused. Therefore, on
        /// pause/resume, track data discovered internally will be discarded to prevent stale track
        /// data from flowing through when the session resumed.
        /// Note, if the app's frame rate is higher than ARCore's frame rate, subsequent
        /// <c><cref="XRCameraFrame"/></c> objects may reference the same underlying ARCore Frame,
        /// which would mean the list of <c><see cref="TrackData"/></c> returned could be the same.
        /// One can differentiate by examining <c><see cref="TrackData.FrameTimestamp"/></c>.
        /// </summary>
        /// <param name="trackId">The ID of the track being queried.</param>
        /// <returns>Returns a list of <see cref="TrackData"/>. Will be empty if none are available.
        /// </returns>
        public List<TrackData> GetUpdatedTrackData(Guid trackId)
        {
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero &&
                ARCoreExtensions._instance.Session.subsystem != null &&
                ARCoreExtensions._instance.Session.subsystem.nativePtr != null)
            {
                Debug.LogWarning("Failed to fetch track data. The Session is not yet available. " +
                                 "Try again later.");
                return new List<TrackData>();
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
                Debug.LogWarning("Failed to fetch track data. The current XRCameraFrame is not " +
                                 "available. Try again later.");
                return new List<TrackData>();
            }

            if (frame.timestampNs == 0 || frame.nativePtr == IntPtr.Zero)
            {
                Debug.LogWarning("Failed to fetch track data. The current XRCameraFrame is not " +
                                 "ready. Try again later.");
                return new List<TrackData>();
            }

            return FrameApi.GetUpdatedTrackData(
                ARCoreExtensions._instance.currentARCoreSessionHandle, frame.FrameHandle(),
                trackId);
        }
    }
}
