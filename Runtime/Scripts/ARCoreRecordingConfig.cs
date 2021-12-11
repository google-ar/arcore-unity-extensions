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
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Configuration to record camera and sensor data from an ARCore session.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ARCoreRecordingConfig",
        menuName = "XR/ARCore Recording Config",
        order = 3)]
    public class ARCoreRecordingConfig : ScriptableObject
    {
        /// <summary>
        /// Set to <c>true</c> to cause the recording to stop automatically when the session is
        /// paused, or set to <c>false</c> to allow the recording to continue until the session is
        /// destroyed or the recording is stopped manually. When set to <c>false</c> and the session
        /// is paused, recording of sensor data continues, but the camera feed will be recorded as a
        /// black screen until the session is resumed.
        /// </summary>
        public bool AutoStopOnPause = true;

        /// <summary>
        /// The list of <c><see cref="Track"/></c> to add the recording config. This field is not
        /// available in the editor and should be set at runtime.
        /// </summary>
        [HideInInspector]
        public List<Track> Tracks = new List<Track>();

        /// <summary>
        /// A URI where the MP4 recording (including video data from the camera
        /// and other device sensors) will be saved. If the resource already
        /// exists it will be overwritten. The <c>AbsoluteUri</c> property of the Uri
        /// will be passed to ARCore to create an <c>android.net.Uri</c>.
        /// The URI must point to a seekable resource.
        /// </summary>
        [HideInInspector]
        public Uri Mp4DatasetUri;

        /// <summary>
        /// Gets or sets the URI on the device where the MP4 recording will be
        /// saved as a file path. The recording consists of video data from the
        /// camera along with data from the device sensors. If the file already
        /// exists it will be overwritten.
        /// </summary>
        ///
        /// @deprecated Please use Mp4DatasetUri instead.
        [Obsolete("This field has been deprecated. Please use Mp4DatasetUri instead.")]
        public string Mp4DatasetFilepath
        {
            get
            {
                if (Mp4DatasetUri == null)
                {
                    return null;
                }

                if (!Mp4DatasetUri.IsFile)
                {
                    return null;
                }

                return Mp4DatasetUri.AbsolutePath;
            }

            set
            {
                Mp4DatasetUri = new Uri(value);
            }
        }
    }
}
