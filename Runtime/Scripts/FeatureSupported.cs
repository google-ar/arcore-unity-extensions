//-----------------------------------------------------------------------
// <copyright file="FeatureSupported.cs" company="Google LLC">
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
    /// Indicates whether a feature or capability is supported on the device.
    /// </summary>
    public enum FeatureSupported
    {
        /// <summary>
        /// The feature or capability is supported.
        /// </summary>
        Supported,

        /// <summary>
        /// Support is unknown. This could be because support is still being determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// The feature or capability is not supported.
        /// </summary>
        Unsupported,
    }
}
