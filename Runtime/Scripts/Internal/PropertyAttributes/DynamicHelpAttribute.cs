//-----------------------------------------------------------------------
// <copyright file="DynamicHelpAttribute.cs" company="Google LLC">
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
    using UnityEngine;

    /// <summary>
    /// This attribute is used to generate a HelpBox based on the HelpAttribute
    /// return by the given reflection function. If both <see cref="DynamicHelpAttribute"/>
    /// and <see cref="HelpAttribute"/> are in use, only HelpAttribute will take effect.
    ///
    /// Note: the function must return the type HelpAttribute, take no parameters,
    /// and be a public member of the target object.
    /// </summary>
    public class DynamicHelpAttribute : PropertyAttribute
    {
        /// <summary>
        /// Reflection function that return the type HelpAttribute, take no parameters,
        /// and must be a public member of the target object.
        /// </summary>
        public readonly string CheckingFunction;

        /// <summary>
        /// Initializes a new instance of the DynamicHelpAttribute class.
        /// </summary>
        /// <param name="checkingFunction">A reflection function.</param>
        public DynamicHelpAttribute(string checkingFunction)
        {
            CheckingFunction = checkingFunction;
        }
    }
}
