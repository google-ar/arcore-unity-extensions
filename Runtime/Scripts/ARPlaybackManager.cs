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
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;

    /// <summary>
    /// Provides access to session playback functionality.
    /// </summary>
    public static class ARPlaybackManager
    {
        /// <summary>
        /// The current state of the playback.
        /// </summary>
        /// <returns>The current <cref="PlaybackStatus"/>.</returns>
        public static PlaybackStatus PlaybackStatus
        {
            get
            {
                return SessionApi.GetPlaybackStatus(
                    ARCoreExtensions._instance.currentARCoreSessionHandle);
            }
        }

        /// <summary>
        /// Sets the filepath for a dataset to be played back. The ARCore session
        /// must be paused when using this method. Resume the session for the
        /// change to take effect.
        /// <param name="datasetFilepath"> The filepath of the dataset. Null if
        /// stopping the playback and resuming a live feed.</param>
        /// <returns><cref="PlaybackResult"/>.<c>Success</c> if playback filepath was
        /// set without issue. Otherwise, the <cref="PlaybackResult"/> will indicate the
        /// error.</returns>
        public static PlaybackResult SetPlaybackDataset(string datasetFilepath)
        {
            return SessionApi.SetPlaybackDataset(
                ARCoreExtensions._instance.currentARCoreSessionHandle, datasetFilepath);
        }
    }
}
