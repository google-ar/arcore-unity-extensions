//-----------------------------------------------------------------------
// <copyright file="ARCoreRecordingConfig.cs" company="Google LLC">
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
    using UnityEngine;

    /// <summary>
    /// Configuration to record camera and sensor data from an ARCore session.
    /// </summary>
    public class ARCoreRecordingConfig : ScriptableObject
    {
        /// <summary>
        /// A full path and filename on the device where the MP4 recording (including video data
        /// from the camera and other device sensors) will be saved. If the file already exists it
        /// will be overwritten.
        /// </summary>
        public string Mp4DatasetFilepath;

        /// <summary>
        /// Set to <c>true</c> to cause the recording to stop automatically when the session is
        /// paused, or set to <c>false</c> to allow the recording to continue until the session is
        /// destroyed or the recording is stopped manually. When set to <c>false</c> and the session
        /// is paused, recording of sensor data continues, but the camera feed will be recorded as a
        /// black screen until the session is resumed.
        /// </summary>
        public bool AutoStopOnPause = true;
   }
}
