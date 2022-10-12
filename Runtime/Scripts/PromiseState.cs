//-----------------------------------------------------------------------
// <copyright file="PromiseState.cs" company="Google LLC">
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
    /// <summary>
    /// Describes state of an async operation.
    /// </summary>
    public enum PromiseState
    {
        /// <summary>
        /// The operation is still pending. It may still be possible to cancel the operation. The
        /// result of the operation isn't available yet.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The operation has been cancelled.
        /// </summary>
        Cancelled = 1,

        /// <summary>
        /// The operation is completed and the result is available.
        /// </summary>
        Done = 2,
    }
}
