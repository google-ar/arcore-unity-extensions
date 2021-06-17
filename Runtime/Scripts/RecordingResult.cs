//-----------------------------------------------------------------------
// <copyright file="RecordingResult.cs" company="Google LLC">
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
    /// <summary>
    /// Results from recording methods.
    /// </summary>
    public enum RecordingResult
    {
        /// <summary>
        /// The request completed successfully.
        /// </summary>
        OK,

        /// <summary>
        /// The call to <see cref="ARRecordingManager.StartRecording(ARCoreRecordingConfig)"/>
        /// failed because ARCore is currently attempting to resume or pause the session.
        ///
        /// Try calling it again in the next frame. Note:
        /// <list type="bullet">
        /// <item>Resuming session may require several frames to complete.</item>
        /// <item>Pausing session may take up to 10 seconds to pause.</item>
        /// </list>
        /// </summary>
        SessionNotReady,

        /// <summary>
        /// When using <see cref="ARRecordingManager.StartRecording(ARCoreRecordingConfig)"/>, this
        /// means the <see cref="ARCoreRecordingConfig"/> was null or invalid.
        /// When using <see cref="ARRecordingManager.RecordTrackData(Guid, byte[])"/>,
        /// this means the track id or payload given are null or invalid.
        /// </summary>
        ErrorInvalidArgument,

        /// <summary>
        /// IO or other general failure.
        /// </summary>
        ErrorRecordingFailed,

        /// <summary>
        /// When using <see cref="ARRecordingManager.StartRecording(ARCoreRecordingConfig)"/>, this
        /// means a recording is already in progress.
        /// When using <see cref="ARRecordingManager.RecordTrackData(Guid, byte[])"/>, this means
        /// either <see cref="RecordingStatus"/> is not currently <c>RecordingStatus.OK</c> or the
        /// system is currently under excess load for images to be produced. The system should not
        /// be under such excess load for more than a few frames and an app should try to record the
        /// data again during the next frame.
        /// </summary>
        ErrorIllegalState,
    }
}
