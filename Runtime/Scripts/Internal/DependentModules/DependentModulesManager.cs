//-----------------------------------------------------------------------
// <copyright file="DependentModulesManager.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using UnityEditor;

    /// <summary>
    /// Manager for available modules.
    /// </summary>
    public class DependentModulesManager
    {
        private static List<IDependentModule> _modules;

        /// <summary>
        /// Get Feature Dependent Modules.
        /// </summary>
        /// <returns>The list of all modules.</returns>
        public static List<IDependentModule> GetModules()
        {
            if (_modules == null)
            {
                List<IDependentModule> modules = new List<IDependentModule>()
                {
                    new LocationModule(),
                    new AuthenticationModule(),
                };

#if UNITY_EDITOR
                _modules = modules;
#else
                List<string> modulesEnabled = RuntimeConfig.Instance.ModulesEnabled;
                _modules = modules.FindAll(
                    module => modulesEnabled.Contains(module.GetType().Name));
#endif
            }

            return _modules;
        }

    }
}
