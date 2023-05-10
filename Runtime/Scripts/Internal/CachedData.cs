//-----------------------------------------------------------------------
// <copyright file="CachedData.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
    using System.Collections.Generic;

    /// <summary>
    /// Due to the Unity & c# features, our functions in extensions class have to be static,
    /// which means they couldn’t be set up as member fields in their instance.
    /// So for those data, they would be put into '_cachedData' of this class instead and they
    /// would be cleared when ARCoreExtensions OnEnable() or OnDisable() get called.
    /// To avoid memory leak, it should not be used to cache native handles or data structures
    /// containing native resources.
    /// </summary>
    internal static class CachedData
    {
        public const string SemanticsTimestamp = "semanticsTimestamp";
        public const string SemanticsTexture = "semanticsTexture";
        public const string SemanticsConfidenceTimestamp = "semanticsConfidenceTimestamp";
        public const string SemanticsConfidenceTexture = "semanticsConfidenceTexture";

        private static Dictionary<string, object> _cachedData = new Dictionary<string, object>();

        /// <summary>
        /// The helper function to get the cached data.
        /// </summary>
        /// <typeparam name="T">The type of data.</typeparam>
        /// <param name="dataName">The name of cached data.</param>
        /// <param name="dataValue">
        /// When this method returns, contains the data value with that name, if it is found;
        /// otherwise, this would be default value of T.
        /// </param>
        /// <returns>
        /// Returns true if '_cachedData' contains the data with provided name and type;
        /// otherwise, false.
        /// </returns>
        internal static bool TryGetCachedData<T>(string dataName, out T dataValue)
        {
            if (_cachedData.ContainsKey(dataName))
            {
                object data = _cachedData[dataName];
                if (data is T)
                {
                    dataValue = (T)data;
                    return true;
                }
            }

            dataValue = default(T);
            return false;
        }

        /// <summary>
        /// The helper function to set the cached data. It would overwrite the previous value
        /// by default. To avoid memory leak, this function would ignore native handles as
        /// dataValue. User should also avoid data structures containing native resources.
        /// </summary>
        /// <param name="dataName">The name of cached data.</param>
        /// <param name="dataValue">
        /// The value of cached data. If the type of data is IntPtr, it would be ignored.
        /// </param>
        internal static void SetCachedData(string dataName, object dataValue)
        {
            if (dataValue is IntPtr)
            {
                return;
            }

            _cachedData[dataName] = dataValue;
        }

        /// <summary>
        /// Reset the cached data.
        /// </summary>
        internal static void Reset()
        {
            _cachedData.Clear();
        }
    }
}
