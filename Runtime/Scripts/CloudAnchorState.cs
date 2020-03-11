//-----------------------------------------------------------------------
// <copyright file="CloudAnchorState.cs" company="Google">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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
    /// Describes the current state of a <see cref="ARCloudAnchor"/>.
    /// </summary>
    public enum CloudAnchorState
    {
        /// <summary>
        /// The Cloud Anchor is not ready to use.
        /// </summary>
        None,

        /// <summary>
        /// A hosting or resolving task is in progress for this Cloud Anchor.
        /// Once the task completes in the background, the Cloud Anchor will get
        /// a new state after the next update.
        /// </summary>
        TaskInProgress,

        /// <summary>
        /// A hosting or resolving task for this Cloud Anchor has completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// A hosting or resolving task for this Cloud Anchor has completed with an
        /// internal error. The app should not attempt to recover from this error.
        /// </summary>
        ErrorInternal,

        /// <summary>
        /// The app cannot communicate with the ARCore Cloud because of an invalid or unauthorized
        /// API key in the manifest, or because there was no API key present in the manifest.
        /// </summary>
        ErrorNotAuthorized,

        /// <summary>
        /// The application has exhausted the request quota alloted to the given API key. The
        /// developer should request additional quota for the ARCore Cloud for their API key
        /// from the Google Developers Console.
        /// </summary>
        ErrorResourceExhausted,

        /// <summary>
        /// Hosting failed because the server could not successfully process the dataset for
        /// the given Cloud Anchor. The developer should try again after the devices has
        /// gathered more data from the environment.
        /// </summary>
        ErrorHostingDatasetProcessingFailed,

        /// <summary>
        /// Resolving failed because the ARCore Cloud Anchor service could not find the provided
        /// Cloud Anchor Id.
        /// </summary>
        ErrorResolvingCloudIdNotFound,

        /// <summary>
        /// The Cloud Anchor could not be resolved because the ARCore Extensions package
        /// used to host the Cloud Anchor was newer than and incompatible with the version
        /// being used to acquire it.
        /// </summary>
        ErrorResolvingPackageTooOld,

        /// <summary>
        /// The Cloud Anchor could not be acquired because the ARCore Extensions package
        /// used to host the Cloud Anchor was older than and incompatible with the version
        /// being used to acquire it.
        /// </summary>
        ErrorResolvingPackageTooNew,

        /// <summary>
        /// The ARCore Cloud Anchor service was unreachable. This can happen because of a
        /// number of reasons. The device may be in airplane mode or does not have a working
        /// internet connection. The request sent to the server could have timed out with
        /// no response, there could be a bad network connection, DNS unavailability, firewall
        /// issues, or anything that could affect the device's ability to connect to the
        /// ARCore Cloud Anchor service.
        /// </summary>
        ErrorHostingServiceUnavailable,
    }
}
