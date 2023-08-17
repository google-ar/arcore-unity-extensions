//-----------------------------------------------------------------------
// <copyright file="ResolveAnchorOnRooftopPromise.cs" company="Google LLC">
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
    /// An <c><see
    /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
    /// launched by <c><see
    /// cref="ARAnchorManagerExtensions.ResolveAnchorOnRooftopAsync(this ARAnchorManager, double,
    /// double, double, UnityEngine.Quaternion)"/></c> with result type
    /// <c><see cref="ResolveAnchorOnRooftopResult"/></c>.
    /// See <c><see
    /// cref="Google.XR.ARCoreExtensions.Internal.InterruptiblePromise">InterruptiblePromise</see></c>
    /// for more information on how to retrieve results from the Promise, and the <a
    /// href="https://developers.google.com/ar/develop/geospatial/unity-arf/anchors#rooftop-anchors">Rooftop
    /// anchors developer guide</a>.
    /// </summary>
    public class ResolveAnchorOnRooftopPromise : InterruptiblePromise<ResolveAnchorOnRooftopResult>
    {
        private static readonly string _rooftopAnchorName = "ARRooftopAnchor";

        /// <summary>
        /// Constructs a default promise with an immutable result <c><see
        /// cref="ResolveAnchorOnRooftopResult"/></c>.
        /// </summary>
        internal ResolveAnchorOnRooftopPromise()
        {
            _state = PromiseState.Done;
            _result = new ResolveAnchorOnRooftopResult();
        }

        /// <summary>
        /// Constructs a specific promise with the associated handle. The <c><see cref="State"/>
        /// </c> must be polled in a coroutine until it returns <c><see cref="PromiseState.Done"/>
        /// </c> or <c><see cref="PromiseState.Cancelled"/></c>. When done, the promise result is
        /// accessible via <c><see cref="Result"/></c>. The promise can be cancelled by
        /// <c><see cref="Cancel()"/></c>.
        /// </summary>
        /// <param name="futureHandle">The future handle associated with this promise.</param>
        internal ResolveAnchorOnRooftopPromise(IntPtr futureHandle)
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
            _result = new ResolveAnchorOnRooftopResult(RooftopAnchorState.ErrorInternal, null);
            _future = futureHandle;

            if (sessionHandle != IntPtr.Zero && futureHandle != IntPtr.Zero)
            {
                // Set defaults.
                _state = PromiseState.Pending;
                _result = new ResolveAnchorOnRooftopResult();
                _onPromiseDone += AssignResult;
            }

#if UNITY_ANDROID
            ARPrestoApi.SetSessionRequired(false);
#endif
        }

        private void AssignResult()
        {
            IntPtr sessionHandle = GetSessionHandle();
            RooftopAnchorState rooftopAnchorState = FutureApi.GetRooftopAnchorState(
                sessionHandle, _future);

            ARGeospatialAnchor anchor = null;
            if (rooftopAnchorState == RooftopAnchorState.Success)
            {
                IntPtr anchorHandle = FutureApi.GetRooftopAnchorHandle(sessionHandle, _future);

                if (anchorHandle != IntPtr.Zero)
                {
                    // Create the GameObject that is the Geospatial Rooftop anchor.
                    anchor = new GameObject(_rooftopAnchorName).AddComponent<ARGeospatialAnchor>();
                    if (anchor)
                    {
                        anchor.SetAnchorHandle(anchorHandle);

                        // Parent the new Geospatial Rooftop anchor to the session origin.
                        anchor.transform.SetParent(
                            ARCoreExtensions._instance.SessionOrigin.trackablesParent,
                            false);
                        anchor.Update();
                    }
                }
            }

            _result = new ResolveAnchorOnRooftopResult(rooftopAnchorState, anchor);
        }
    }
}
