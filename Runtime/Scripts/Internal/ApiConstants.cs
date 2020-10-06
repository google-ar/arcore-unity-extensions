//-----------------------------------------------------------------------
// <copyright file="ApiConstants.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    internal static class ApiConstants
    {
#if UNITY_ANDROID
        public const string ARCoreNativeApi = "arcore_sdk_c";
#elif UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
        public const string ARCoreNativeApi = "__Internal";
#else
        public const string ARCoreNativeApi = "NOT_AVAILABLE";
#endif
    }
}
