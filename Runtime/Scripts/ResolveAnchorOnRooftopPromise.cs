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
    /// Handle to an async operation launched by <c><see
    /// cref="ARAnchorManagerExtensions.ResolveAnchorOnRooftopAsync(this ARAnchorManager, double, double, double, UnityEngine.Quaternion)"/></c>.
    /// See the <a
    /// href="https://developers.google.com/ar/develop/geospatial/unity-arf/anchors#rooftop-anchors">Rooftop
    /// anchors developer guide</a> for more information.
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
        /// Constructs a specific promise with the associated handle. It polls the
        /// result in the Update event every frame until the result gets resolved. The promise
        /// result is accessible via <c><see cref="Result"/></c>, and can be cancelled by
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
            }

#if UNITY_ANDROID
            ARPrestoApi.SetSessionRequired(false);
#endif
        }

        /// <summary>
        /// Gets the <c><see cref="ResolveAnchorOnRooftopResult"/></c> associated with this
        /// promise or the default values of <c><see cref="RooftopAnchorState.None"/></c> and
        /// <c>null</c> if the promise was cancelled.
        /// </summary>
        public override ResolveAnchorOnRooftopResult Result
        {
            get
            {
                IntPtr sessionHandle = GetSessionHandle();

                if (_future != IntPtr.Zero && sessionHandle != IntPtr.Zero &&
                    _result.RooftopAnchorState == RooftopAnchorState.None &&
                    this.State == PromiseState.Done)
                {
                    RooftopAnchorState rooftopAnchorState = FutureApi.GetRooftopAnchorState(
                        sessionHandle, _future);

                    ARGeospatialAnchor anchor = null;
                    if (rooftopAnchorState == RooftopAnchorState.Success)
                    {
                        IntPtr anchorHandle = FutureApi.GetRooftopAnchorHandle(sessionHandle,
                            _future);

                        if (anchorHandle != IntPtr.Zero)
                        {
                            // Create the GameObject that is the Geospatial Rooftop anchor.
                            anchor = new GameObject(_rooftopAnchorName)
                                .AddComponent<ARGeospatialAnchor>();
                            if (anchor)
                            {
                                anchor.SetAnchorHandle(anchorHandle);

                                // Parent the new Geospatial Rooftop anchor to the session origin.
                                anchor.transform.SetParent(
                                    ARCoreExtensions._instance.SessionOrigin.TrackablesParent,
                                    false);
                                anchor.Update();
                            }
                        }
                    }

                    _result = new ResolveAnchorOnRooftopResult(rooftopAnchorState, anchor);
                }

                return _result;
            }
        }
    }
}
