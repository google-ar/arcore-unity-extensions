//-----------------------------------------------------------------------
// <copyright file="GeospatialCreatorHelper.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.Editor.Internal
{
    using System;
    using System.Collections.Generic;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    /// <summary> Helper functions for managing the Geospatial Creator state in the Unity Editor.
    /// </summary>
    /// <remarks> The functions defined here can be invoked directly from other parts of ARCore
    /// Extensions, which is why this class is in the Google.XR.ARCoreExtensions.Editor namespace
    /// and ASMDEF. If this code was in the GeospatialCreator assembly, we'd need to modify the
    /// ARCoreExtensions ASMDEF to include additional assemblies. </remarks>
    public static class GeospatialCreatorHelper
    {
        /// <summary> URL for the Geospatial Creator quickstart page. </summary>
        public static readonly string QuickstartUrl =
            "https://developers.google.com/ar/geospatial-creator-unity";

        /// <summary> The scripting symbol that is present if the feature is enabled while
        /// the Editor is running.
        /// </summary>
        /// <remarks> This is separate from the feature flag used by the ARCore Extensions
        /// build system to control the availability of the feature for specific builds
        /// of the Extensions. </remarks>
        public static readonly string CreatorEnabledSymbol =
            "ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED";

        private static readonly BuildTargetGroup[] _buildTargets = new BuildTargetGroup[]
        {
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS
        };

        /// <summary> Enables / disables the Geospatial Creator-specific scripting symbols,
        /// if the state of "enabled" has changed.
        /// </summary>
        /// <remarks> This is independent from setting the field on ARCoreExtensionsProjectSettings
        /// directly; callers may need to manage that setting as well.
        /// </remarks>
        /// <param name="enabled"> The current state of the feature.</param>
        public static void ConfigureScriptingSymbols(bool enabled)
        {
#if !UNITY_2021_3_OR_NEWER
            if (enabled)
            {
                Debug.LogWarning("Geospatial Creator requires Unity 2021.3 or later.");
            }
#endif
            bool currentlyEnabled = CheckForSymbolsInProject();
            if (currentlyEnabled == enabled)
            {
                // no change in state
                return;
            }

            if (enabled)
            {
                AddScriptingSymbols();
            }
            else
            {
                RemoveScriptingSymbols();
            }
        }

        /// <summary> Handles a potential change in the feature checkbox state.
        /// </summary>
        /// <param name="toggleChecked"> The state of the toggle: true if the checkbox is checked,
        /// false otherwise. There is no assumption that the state has actually changed. </param>
        public static void OnToggle(bool toggleChecked)
        {
#if UNITY_2021_3_OR_NEWER
            // _enabled should always have been initialized by now
            bool currentlyEnabled = CheckForSymbolsInProject();
            if (toggleChecked && !currentlyEnabled)
            {
                // The toggle has been checked on to enable the feature. We force it back to
                // disabled immediately, because it isn't fully enabled until the wizard is
                // complete. The wizard will flip it back to enabled if/when all prerequisites
                // are met.
                ARCoreExtensionsProjectSettings.Instance.GeospatialEditorEnabled = false;
                GeospatialCreatorEnabledWizard.ShowWizard();
            } else if (!toggleChecked && currentlyEnabled)
            {
                ConfigureScriptingSymbols(false);
                // :TODO (b/276777888): What to do if GeospatialCreator objects are already in the
                // scene? Could log a warning, or force an error and not allow the feature to be
                // disabled if they are present.
            }
#else // Unsupported version of Unity is being used
            if (toggleChecked)
            {
                // Don't allow the feature to be enabled.
                ARCoreExtensionsProjectSettings.Instance.GeospatialEditorEnabled = false;
                throw new Exception("Geospatial Creator requires Unity 2021.3 or later.");
            }
            else
            {
                // The previous toggled-on state was invalid, but could have occurred if the
                // project was downgraded to an older version of Unity. It is safe to remove the
                // symbols.
                ConfigureScriptingSymbols(false);
            }
#endif
        }

        /// <summary> Adds Geospatial Creator-specific script symbols that enable the
        /// feature in the Unity Editor. </summary>
        public static void AddScriptingSymbols()
        {
            foreach (BuildTargetGroup target in _buildTargets)
            {
                AddSymbol(CreatorEnabledSymbol, target);
            }
        }

        /// <summary> Removes the Geospatial Creator-specific script symbols that enable the
        /// feature in the Unity Editor. </summary>
        public static void RemoveScriptingSymbols()
        {
            foreach (BuildTargetGroup target in _buildTargets)
            {
                RemoveSymbol(CreatorEnabledSymbol, target);
            }
        }

        private static void AddSymbol(string symbol, BuildTargetGroup target)
        {
            HashSet<string> symbolSet = GetCurrentSymbolSet(target);
            bool symbolDefined = symbolSet.Contains(symbol);

            if (!symbolDefined)
            {
                Debug.LogFormat("Adding {0} symbol for {1}.", symbol, target.ToString());
                symbolSet.Add(symbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    target,
                    string.Join(";", symbolSet));
            }
            else
            {
                Debug.LogFormat("Symbol {0} already definied for {1}.", symbol, target.ToString());
            }
        }

        private static void RemoveSymbol(string symbol, BuildTargetGroup target)
        {
            HashSet<string> symbolSet = GetCurrentSymbolSet(target);
            bool symbolDefined = symbolSet.Contains(symbol);

            if (symbolDefined)
            {
                Debug.LogFormat("Removing {0} symbol for {1}.", symbol, target.ToString());
                symbolSet.Remove(symbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    target,
                    string.Join(";", symbolSet));
            }
            else
            {
                Debug.LogFormat(
                    "Symbol {0} not present for {1}; nothing to remove.",
                    symbol,
                    target.ToString());
            }
        }

        private static bool CheckForSymbolsInProject()
        {
            foreach (BuildTargetGroup target in _buildTargets)
            {
                if (!GetCurrentSymbolSet(target).Contains(CreatorEnabledSymbol))
                {
                    return false;
                }
            }

            return true;
        }

        private static HashSet<string> GetCurrentSymbolSet(BuildTargetGroup target)
        {
            return new HashSet<string>(
                PlayerSettings
                    .GetScriptingDefineSymbolsForGroup(target)
                    .Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
