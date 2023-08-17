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
    /// Promises represent the eventual completion of an asynchronous operation. A promise has one
    /// of three states, <c><see cref="PromiseState"/></c>, which can be obtained with
    /// <c><see cref="InterruptiblePromise<T>.State"/></c>:
    ///
    /// <list>
    ///   <item><c><see cref="PromiseState.Pending"/></c> - The operation is still pending. The
    ///         result of the operation isn't available yet.</item>
    ///   <item><c><see cref="PromiseState.Done"/></c> - The operation is complete, and a result is
    ///         available.</item>
    ///   <item><c><see cref="PromiseState.Cancelled"/></c> - The operation has been
    ///         cancelled.</item>
    /// </list>
    ///
    /// An <c><see cref="InterruptiblePromise"/></c> starts in the
    /// <c><see cref="PromiseState.Pending"/></c> state and transitions to
    /// <c><see cref="PromiseState.Done"/></c> upon completion. If the Promise is cancelled using
    /// <c><see cref="InterruptiblePromise<T>.Cancel()"/></c>,
    /// then its state may become <c><see cref="PromiseState.Cancelled"/></c>
    /// (see <a href="#cancelling-a-promise">cancelling a Promise</a> for caveats).
    ///
    /// <h3>Obtaining results from a Promise</h3>
    ///
    /// There are two ways of obtaining results from an <c><see cref="InterruptiblePromise"/></c>:
    ///
    /// <h4>Polling a Promise</h4>
    /// When the <c><see cref="InterruptiblePromise<T>"/></c> is created, its
    /// <c><see cref="PromiseState"/></c> is set to <c><see cref="PromiseState.Pending"/></c>. You
    /// may poll the future using <c><see cref="InterruptiblePromise<T>.State"/></c> to query the
    /// state of the asynchronous operation. When its state is
    /// <c><see cref="PromiseState.Done"/></c>, you may obtain the operation's result using
    /// <c><see cref="InterruptiblePromise<T>.Result"/></c>.
    ///
    /// <h4>Use a Unity Coroutine</h4>
    /// Promises use a <a
    ///   href="https://docs.unity3d.com/ScriptReference/CustomYieldInstruction.html"
    ///   class="external"><c>CustomYieldInstruction</c></a> to facilitate <a
    ///   href="https://docs.unity3d.com/Manual/Coroutines.html">Unity coroutines</a>.
    /// Use <c>yield return <strong>promiseInstance</strong></c> to pause execution of your
    /// coroutine. Unity will resume execution of your coroutine when
    /// <c><see cref="InterruptiblePromise<T>.State"/></c> is no longer
    /// <c><see cref="PromiseState.Pending"/></c>.
    ///
    /// <pre><code>
    /// public void CreatePromise()
    /// {
    ///    ResolveAnchorOnRooftopPromise rooftopPromise =
    ///        AnchorManager.ResolveAnchorOnRooftopAsync(...);
    ///    StartCoroutine(CheckRooftopPromise(rooftopPromise));
    /// }
    /// &nbsp;
    /// private IEnumerator CheckRooftopPromise(ResolveAnchorOnTerrainPromise promise)
    /// {
    ///    yield return promise;
    ///    if (promise.State == PromiseState.Cancelled) yield break;
    ///    var result = promise.Result;
    ///    /// Use the result of your promise here.
    /// }
    /// </code></pre>
    ///
    /// <h3 id="promise_cancellation">Cancelling a Promise</h3>
    /// You can try to cancel an <c><see cref="InterruptiblePromise"/></c> by calling
    /// <c><see cref="InterruptiblePromise<T>.Cancel()"/></c>. Due to multi-threading, it is
    /// possible that the cancel operation is not successful, and any
    /// <a href="#use-a-unity-coroutine">Unity coroutine</a> may successfully resume execution
    /// regardless.
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
        /// Invoked when the promise successfully completes, so the subclass can assign the result.
        /// </summary>
        protected Action _onPromiseDone;

        /// <summary>
        /// Releases the underlying native handle.
        /// </summary>
        ~InterruptiblePromise()
        {
            FutureApi.Release(_future);
        }

        // @cond

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

        // @endcond

        /// <summary>
        /// Gets the <c><see cref="PromiseState"/></c> associated with this promise. Used to
        /// determine if the promise is still waiting for the result.
        /// </summary>
        public PromiseState State
        {
            get
            {
                CheckState();
                return _state;
            }
        }

        /// <summary>
        /// Gets the result, if the operation is done.
        /// </summary>
        public virtual T Result
        {
            get
            {
                CheckState();
                return _result;
            }
        }

        /// <summary>
        /// Cancels execution of this promise if it's still pending.
        /// </summary>
        public void Cancel()
        {
            FutureApi.Cancel(GetSessionHandle(), _future);
        }

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

        /// <summary>
        /// Update the state field based on the current status of future. If it has transitioned
        /// from Pending to Done, invoke the PromiseDone action to set the result.
        /// </summary>
        private void CheckState()
        {
            if (_state != PromiseState.Pending)
            {
                return;
            }

            var sessionHandle = GetSessionHandle();
            if (_future != IntPtr.Zero && sessionHandle != IntPtr.Zero)
            {
                _state = FutureApi.GetState(sessionHandle, _future);
                if (_state == PromiseState.Done && _onPromiseDone != null)
                {
                    // Guaranteed to be called at most once, since future calls to "get" will
                    // return immediately when _state == Done
                    _onPromiseDone();
                }
            }
        }
    }
}
