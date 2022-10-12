//-----------------------------------------------------------------------
// <copyright file="InterruptiblePromise.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.Internal
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using UnityEngine;

    /// <summary>
    /// A yield instruction that blocks a coroutine until an async task has completed.
    /// </summary>
    /// <typeparam name="T">The type of the async task result.</typeparam>
    public abstract class InterruptiblePromise<T> : CustomYieldInstruction
    {
        /// <summary>
        /// The async task status.
        /// </summary>
        protected PromiseState _state;

        /// <summary>
        /// The async task result.
        /// </summary>
        protected T _result;

        /// <summary>
        /// The async task the yield instruction waits on.
        /// </summary>
        protected IntPtr _future;

        /// <summary>
        /// Gets a value indicating whether the coroutine instruction should keep waiting.
        /// </summary>
        /// <value><c>true</c> if the state is not <c><see cref="PromiseState.Pending"/></c>,
        /// otherwise <c>false</c>.</value>
        [SuppressMessage("UnityRules.UnityStyleRules",
         "US1109:PublicPropertiesMustBeUpperCamelCase", Justification = "Overridden method.")]
        public override bool keepWaiting
        {
            get
            {
                return State == PromiseState.Pending;
            }
        }

        /// <summary>
        /// Gets the operation status.
        /// </summary>
        public abstract PromiseState State { get; }

        /// <summary>
        /// Gets the result, if the operation is done.
        /// </summary>
        public abstract T Result { get; }

        /// <summary>
        /// Cancels execution of this promise if it's still pending.
        /// </summary>
        public abstract void Cancel();

        /// <summary>
        /// Gets the underlying native handle.
        /// </summary>
        /// <returns>If the session is initialized successfully, returns a pointer to the native
        /// session.</returns>
        protected IntPtr GetSessionHandle()
        {
#if UNITY_IOS
            return IOSSupportManager.Instance.ARCoreSessionHandle;
#else
            return ARPrestoApi.GetSessionHandle();
#endif
        }
    }
}
