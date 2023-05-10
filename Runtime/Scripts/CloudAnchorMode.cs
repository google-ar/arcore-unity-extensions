//-----------------------------------------------------------------------
// <copyright file="CloudAnchorMode.cs" company="Google LLC">
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
    /// Describes the desired behavior of the ARCore Cloud Anchor API. The Cloud
    /// Anchors API uses feature maps to persist an anchor throughout sessions and
    /// across devices. See the <a
    /// href="https://developers.google.com/ar/develop/unity-arf/cloud-anchors/developer-guide">Cloud
    /// Anchors developer guide</a> for more information.
    ///
    /// The default value is <c><see cref="CloudAnchorMode.Disabled"/></c>. Use <c><see
    /// cref="ARCoreExtensionsConfig.CloudAnchorMode"/></c> to set the desired mode.
    /// </summary>
    public enum CloudAnchorMode
    {
        /// <summary>
        /// The Cloud Anchor API is disabled. Calling <c><see
        /// cref="ARAnchorManagerExtensions.HostCloudAnchorAsync(this ARAnchorManager, ARAnchor,
        /// int)"/></c> and <c><see
        /// cref="ARAnchorManagerExtensions.ResolveCloudAnchorAsync(this ARAnchorManager,
        /// string)"/></c> will cause the promise to fail immediately.
        ///
        /// This is the default value.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// The Cloud Anchor API is enabled. <c><see
        /// cref="ARAnchorManagerExtensions.HostCloudAnchorAsync(this ARAnchorManager, ARAnchor,
        /// int)"/></c> and <c><see
        /// cref="ARAnchorManagerExtensions.ResolveCloudAnchorAsync(this ARAnchorManager,
        /// string)"/></c> can be used to host and resolve Cloud Anchors.
        ///
        /// Using this mode requires your app to do the following:
        ///
        /// - On Android: Include the <a
        ///   href="https://developer.android.com/training/basics/network-ops/connecting"><c>ACCESS_INTERNET</c></a>
        ///   permission to the app's AndroidManifest,
        /// - Configure <a
        ///   href="https://developers.google.com/ar/develop/unity-arf/cloud-anchors/developer-guide">authorization</a>.
        ///
        /// Use <c><see cref="ARCoreExtensionsConfig.CloudAnchorMode"/></c> to set this mode.
        /// </summary>
        Enabled = 1,
    }
}
