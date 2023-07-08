//-----------------------------------------------------------------------
// <copyright file="VpsAvailabilityPromise.cs" company="Google LLC">
//
// Copyright 2022 Google LLC
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
    /// A specific interruptible promise to check the VPS availability using the given location by
    /// initializing a query with a remote service, used in coroutines to poll <c><see
    /// cref="VpsAvailability"/></c> results across multiple frames.
    /// </summary>
    public class VpsAvailabilityPromise : InterruptiblePromise<VpsAvailability>
    {
        /// <summary>
        /// Constructs a default promise with an immutable result <c><see
        /// cref="VpsAvailability.Unknown"/></c>.
        /// </summary>
        internal VpsAvailabilityPromise()
        {
            _state = PromiseState.Done;
            _result = VpsAvailability.Unknown;
        }

        /// <summary>
        /// Constructs a specific promise from the given VpsAvailabilityFuture. It polls the
        /// result in the Update event every frame until the result gets resolved. The promise
        /// result is accessible via <c><see cref="Result"/></c>, and can be cancelled by
        /// <c><see cref="Cancel()"/></c>.
        /// </summary>
        /// <param name="future">The native future. If null initializes a Done promise with <c><see
        /// cref="VpsAvailability.ErrorInternal"/></c> state.</param>
        internal VpsAvailabilityPromise(IntPtr future)
        {
            _future = future;
            if (_future == IntPtr.Zero)
            {
                _state = PromiseState.Done;
                _result = VpsAvailability.ErrorInternal;
            }
            else
            {
                _onPromiseDone = AssignResult;
            }
        }

        private void AssignResult()
        {
            _result = FutureApi.GetVpsAvailabilityResult(GetSessionHandle(), _future);
        }
    }
}
