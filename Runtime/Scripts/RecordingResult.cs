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
        /// The <see cref="ARCoreRecordingConfig"/> was null or invalid.
        /// </summary>
        ErrorInvalidArgument,

        /// <summary>
        /// IO or other general failure.
        /// </summary>
        ErrorRecordingFailed,

        /// <summary>
        /// A recording is already in progress.
        /// </summary>
        ErrorIllegalState,
    }
}
