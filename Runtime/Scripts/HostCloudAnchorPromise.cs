//-----------------------------------------------------------------------
// <copyright file="HostCloudAnchorPromise.cs" company="Google LLC">
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
    /// An interruptible promise launched by <c><see
    /// cref="ARAnchorManagerExtensions.HostCloudAnchorAsync(this ARAnchorManager, ARAnchor,
    /// int)"/></c>. Used with <a href="https://docs.unity3d.com/Manual/Coroutines.html">Unity
    /// Coroutines</a> to poll results across multiple frames. See the <a
    /// href="https://developers.google.com/ar/develop/unity-arf/cloud-anchors/developer-guide">Cloud
    /// Anchors developer guide</a> for more information.
    /// </summary>
    public class HostCloudAnchorPromise : InterruptiblePromise<HostCloudAnchorResult>
    {
        /// <summary>
        /// Constructs a default promise with an immutable result <c><see
        /// cref="HostCloudAnchorResult"/></c>.
        /// </summary>
        internal HostCloudAnchorPromise()
        {
            _state = PromiseState.Done;
            _result = new HostCloudAnchorResult();
        }

        /// <summary>
        /// Constructs a specific promise with the associated handle. It polls the
        /// result in the Update event every frame until the result gets resolved. The promise
        /// result is accessible via <c><see cref="Result"/></c>, and can be cancelled by
        /// <c><see cref="Cancel()"/></c>.
        /// </summary>
        /// <param name="futureHandle">The future handle associated with this promise.</param>
        internal HostCloudAnchorPromise(IntPtr futureHandle)
        {
            // TODO(b/269522532) move logic to FutureApi
#if UNITY_ANDROID
            ARPrestoApi.SetSessionRequired(true);
#elif UNITY_IOS
#if ARCORE_EXTENSIONS_IOS_SUPPORT
            SessionApi.SetAuthToken(GetSessionHandle());
#endif // ARCORE_EXTENSIONS_IOS_SUPPORT
#endif // UNIT_ANDROID

            IntPtr sessionHandle = GetSessionHandle();

            // Set defaults if sessionHandle or futureHandle are null
            _state = PromiseState.Done;
            _result = new HostCloudAnchorResult(CloudAnchorState.ErrorInternal, null);
            _future = futureHandle;

            if (sessionHandle != IntPtr.Zero && futureHandle != IntPtr.Zero)
            {
                // Set defaults.
                _state = PromiseState.Pending;
                _result = new HostCloudAnchorResult();
            }

#if UNITY_ANDROID
            ARPrestoApi.SetSessionRequired(false);
#endif
        }

        /// <summary>
        /// Gets the <c><see cref="HostCloudAnchorResult"/></c> associated with this
        /// promise or the default values of <c><see cref="CloudAnchorState.None"/></c> and
        /// <c>null</c> if the promise was cancelled.
        /// </summary>
        public override HostCloudAnchorResult Result
        {
            get
            {
                IntPtr sessionHandle = GetSessionHandle();

                if (_future != IntPtr.Zero && sessionHandle != IntPtr.Zero &&
                    _result.CloudAnchorState == CloudAnchorState.None &&
                    this.State == PromiseState.Done)
                {
                    CloudAnchorState cloudAnchorState = FutureApi.GetHostCloudAnchorState(
                        sessionHandle, _future);
                    string cloudAnchorId = null;

                    if (cloudAnchorState == CloudAnchorState.Success)
                    {
                        cloudAnchorId = FutureApi.GetCloudAnchorId(sessionHandle, _future);
                    }

                    _result = new HostCloudAnchorResult(cloudAnchorState, cloudAnchorId);
                }

                return _result;
            }
        }
    }
}
