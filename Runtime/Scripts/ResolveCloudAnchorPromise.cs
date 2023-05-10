//-----------------------------------------------------------------------
// <copyright file="ResolveCloudAnchorPromise.cs" company="Google LLC">
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
    /// cref="ARAnchorManagerExtensions.ResolveCloudAnchorAsync(this ARAnchorManager,
    /// string)"/></c>. Used with <a href="https://docs.unity3d.com/Manual/Coroutines.html">Unity
    /// Coroutines</a> to poll results across multiple frames. See the <a
    /// href="https://developers.google.com/ar/develop/unity-arf/cloud-anchors/developer-guide">Cloud
    /// Anchors developer guide</a> for more information.
    /// </summary>
    public class ResolveCloudAnchorPromise : InterruptiblePromise<ResolveCloudAnchorResult>
    {
        private static readonly string _cloudAnchorName = "ARCloudAnchor";

        /// <summary>
        /// Constructs a default promise with an immutable result <c><see
        /// cref="ResolveCloudAnchorResult"/></c>.
        /// </summary>
        internal ResolveCloudAnchorPromise()
        {
            _state = PromiseState.Done;
            _result = new ResolveCloudAnchorResult();
        }

        /// <summary>
        /// Constructs a specific promise with the associated handle. It polls the
        /// result in the Update event every frame until the result gets resolved. The promise
        /// result is accessible via <c><see cref="Result"/></c>, and can be cancelled by
        /// <c><see cref="Cancel()"/></c>.
        /// </summary>
        /// <param name="futureHandle">The future handle associated with this promise.</param>
        internal ResolveCloudAnchorPromise(IntPtr futureHandle)
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
            _result = new ResolveCloudAnchorResult(CloudAnchorState.ErrorInternal, null);
            _future = futureHandle;

            if (sessionHandle != IntPtr.Zero && futureHandle != IntPtr.Zero)
            {
                // Set defaults.
                _state = PromiseState.Pending;
                _result = new ResolveCloudAnchorResult();
            }

#if UNITY_ANDROID
            ARPrestoApi.SetSessionRequired(false);
#endif
        }

        /// <summary>
        /// Gets the <c><see cref="ResolveCloudAnchorResult"/></c> associated with this
        /// promise or the default values of <c><see cref="CloudAnchorState.None"/></c> and
        /// <c>null</c> if the promise was cancelled.
        /// </summary>
        public override ResolveCloudAnchorResult Result
        {
            get
            {
                IntPtr sessionHandle = GetSessionHandle();

                if (_future != IntPtr.Zero && sessionHandle != IntPtr.Zero &&
                    _result.CloudAnchorState == CloudAnchorState.None &&
                    this.State == PromiseState.Done)
                {
                    CloudAnchorState cloudAnchorState = FutureApi.GetResolveCloudAnchorState(
                        sessionHandle, _future);

                    ARCloudAnchor anchor = null;
                    if (cloudAnchorState == CloudAnchorState.Success)
                    {
                        IntPtr anchorHandle = FutureApi.GetCloudAnchorHandle(sessionHandle,
                            _future);

                        if (anchorHandle != IntPtr.Zero)
                        {
                            // Create the GameObject that is the cloud anchor.
                            anchor = new GameObject(_cloudAnchorName).AddComponent<ARCloudAnchor>();
                            if (anchor)
                            {
                                anchor.SetAnchorHandle(anchorHandle);

                                // Parent the new cloud anchor to the session origin.
                                anchor.transform.SetParent(
                                    ARCoreExtensions._instance.SessionOrigin.trackablesParent,
                                    false);
                                anchor.Update();
                            }
                        }
                    }

                    _result = new ResolveCloudAnchorResult(cloudAnchorState, anchor);
                }

                return _result;
            }
        }
    }
}
