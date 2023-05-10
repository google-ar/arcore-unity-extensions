//-----------------------------------------------------------------------
// <copyright file="ResolveCloudAnchorResult.cs" company="Google LLC">
//
// Copyright 2023 Google LLC
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
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Result object from <c><see cref="ResolveCloudAnchorPromise"/></c> containing the resolved
    /// Cloud Anchor and Anchor state. See the <a
    /// href="https://developers.google.com/ar/develop/unity-arf/cloud-anchors/developer-guide">Cloud
    /// Anchors developer guide</a> for more information.
    /// </summary>
    public class ResolveCloudAnchorResult
    {
        private ARCloudAnchor _anchor;
        private CloudAnchorState _state;

        internal ResolveCloudAnchorResult()
        {
            _state = CloudAnchorState.None;
            _anchor = null;
        }

        internal ResolveCloudAnchorResult(CloudAnchorState state, ARCloudAnchor anchor)
        {
            _state = state;
            _anchor = anchor;
        }

        /// <summary>
        /// Gets the <c>CloudAnchorState</c> associated with this result.
        /// </summary>
        public CloudAnchorState CloudAnchorState
        {
            get
            {
                return _state;
            }
        }

        /// <summary>
        /// Gets the <c>ARCloudAnchor</c> associated with this result. May be <c>null</c>.
        /// </summary>
        public ARCloudAnchor Anchor
        {
            get
            {
                return _anchor;
            }
        }
    }
}
