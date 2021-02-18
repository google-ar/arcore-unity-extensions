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
    using Google.XR.ARCoreExtensions.Internal;

    /// <summary>
    /// Provides access to session playback functionality.
    /// </summary>
    public static class ARPlaybackManager
    {
        /// <summary>
        /// Gets the current state of the playback.
        /// </summary>
        /// <returns>The current <see cref="PlaybackStatus"/>.</returns>
        public static PlaybackStatus PlaybackStatus
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
        /// - Due to the way session data is processed, ARCore APIs may sometimes produce different
        ///   results during playback than during recording and produce different results during
        ///   subsequent playback sessions. For exmaple, the number of detected planes and other
        ///   trackables, the precise timing of their detection and their pose over time may be
        ///   different in subsequent playback sessions.
        /// - Can only be called while the session is paused. Playback of the MP4 dataset file will
        ///   start once the session is resumed.
        /// - The MP4 dataset file must use the same camera facing direction as is configured in the
        ///   session.
        ///
        /// <param name="datasetFilepath"> The filepath of the MP4 dataset. Null if
        /// stopping the playback and resuming a live feed.</param>
        /// <returns><see cref="PlaybackResult"/>.<c>Success</c> if playback filepath was
        /// set without issue. Otherwise, the <see cref="PlaybackResult"/> will indicate the
        /// error.</returns>
        public static PlaybackResult SetPlaybackDataset(string datasetFilepath)
        {
            return SessionApi.SetPlaybackDataset(
                ARCoreExtensions._instance.currentARCoreSessionHandle, datasetFilepath);
        }
    }
}
