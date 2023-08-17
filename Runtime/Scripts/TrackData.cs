//-----------------------------------------------------------------------
// <copyright file="TrackData.cs" company="Google LLC">
//
// Copyright 2021 Google LLC
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
    using UnityEngine;

    /// <summary>
    /// Data that was recorded to an external <c><see cref="Track"/></c>. Obtained by
    /// <c><see cref="ARPlaybackManager.GetUpdatedTrackData(System.Guid)"/></c>.
    /// </summary>
    public struct TrackData
    {
        /// <summary>
        /// The timestamp in nanoseconds of the frame the given <c><see cref="TrackData"/></c> was
        /// recorded on. If frames are skipped during playback, the played back external track data
        /// may be attached to a later frame. This timestamp is equal to the result of
        /// <c><see cref="UnityEngine.XR.ARSubsystems.XRCameraFrame.timestampNs"/></c>
        /// on the frame during which track data was written.
        /// </summary>
        public long FrameTimestamp;

        /// <summary>
        /// The byte data array that was recorded via
        /// <c><see cref="ARRecordingManager.RecordTrackData(Guid, byte[])"/></c>.
        /// </summary>
        public byte[] Data;
    }
}
