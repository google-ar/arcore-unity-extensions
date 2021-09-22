//-----------------------------------------------------------------------
// <copyright file="CloudReferenceState.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    /// Deprecated version of <c><see cref="CloudAnchorState"/></c>.
    /// </summary>
    /// @deprecated Please use CloudAnchorState instead.
    [System.Obsolete("This enum has been renamed to CloudAnchorState. " +
        "See details in release notes v1.16.0.")]
    public enum CloudReferenceState
    {
        /// <summary>
        /// The cloud reference point is not ready to use.
        /// </summary>
        None = CloudAnchorState.None,

        /// <summary>
        /// A hosting or resolving task is in progress for this Reference Point.
        /// Once the task completes in the background, the Reference Point will get
        /// a new state after the next update.
        /// </summary>
        TaskInProgress = CloudAnchorState.TaskInProgress,

        /// <summary>
        /// A hosting or resolving task for this Reference Point has completed successfully.
        /// </summary>
        Success = CloudAnchorState.Success,

        /// <summary>
        /// A hosting or resolving task for this Reference Point has completed with an
        /// internal error. The app should not attempt to recover from this error.
        /// </summary>
        ErrorInternal = CloudAnchorState.ErrorInternal,

        /// <summary>
        /// The app cannot communicate with the ARCore Cloud because of an invalid or unauthorized
        /// API key in the manifest, or because there was no API key present in the manifest.
        /// </summary>
        ErrorNotAuthorized = CloudAnchorState.ErrorNotAuthorized,

        /// <summary>
        /// The application has exhausted the request quota alloted to the given API key. The
        /// developer should request additional quota for the ARCore Cloud for their API key
        /// from the Google Developers Console.
        /// </summary>
        ErrorResourceExhausted = CloudAnchorState.ErrorResourceExhausted,

        /// <summary>
        /// Hosting failed because the server could not successfully process the dataset for
        /// the given Reference Point. The developer should try again after the devices has
        /// gathered more data from the environment.
        /// </summary>
        ErrorHostingDatasetProcessingFailed = CloudAnchorState.ErrorHostingDatasetProcessingFailed,

        /// <summary>
        /// Resolving failed because the ARCore Cloud Anchor service could not find the provided
        /// Cloud Anchor Id.
        /// </summary>
        ErrorResolvingCloudIdNotFound = CloudAnchorState.ErrorResolvingCloudIdNotFound,

        /// <summary>
        /// The Reference Point could not be resolved because the ARCore Extensions package
        /// used to host the Cloud Anchor was newer than and incompatible with the version
        /// being used to acquire it.
        /// </summary>
        ErrorResolvingPackageTooOld = CloudAnchorState.ErrorResolvingPackageTooOld,

        /// <summary>
        /// The Reference Point could not be acquired because the ARCore Extensions package
        /// used to host the Cloud Anchor was older than and incompatible with the version
        /// being used to acquire it.
        /// </summary>
        ErrorResolvingPackageTooNew = CloudAnchorState.ErrorResolvingPackageTooNew,

        /// <summary>
        /// The ARCore Cloud Anchor service was unreachable. This can happen because of a
        /// number of reasons. The device may be in airplane mode or does not have a working
        /// internet connection. The request sent to the server could have timed out with
        /// no response, there could be a bad network connection, DNS unavailability, firewall
        /// issues, or anything that could affect the device's ability to connect to the
        /// ARCore Cloud Anchor service.
        /// </summary>
        ErrorHostingServiceUnavailable = CloudAnchorState.ErrorHostingServiceUnavailable,
    }
}
