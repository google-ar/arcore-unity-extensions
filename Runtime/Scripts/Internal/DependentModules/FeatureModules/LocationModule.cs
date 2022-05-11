//-----------------------------------------------------------------------
// <copyright file="LocationModule.cs" company="Google LLC">
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
    using UnityEngine;

    /// <summary>
    /// The implemented class of location module.
    /// </summary>
    public class LocationModule : DependentModuleBase
    {
        /// <summary>
        /// Get the permissions required by this module during the runtime.
        /// </summary>
        /// <param name="sessionConfig">ARCore Extensions Config.</param>
        /// <returns>
        /// The array containing the Runtime Permissions’ names required by this module.
        /// </returns>
        public override string[] GetRuntimePermissions(ARCoreExtensionsConfig sessionConfig)
        {
            if (UseLocation(sessionConfig))
            {
                return new string[]
                {
                    "android.permission.ACCESS_FINE_LOCATION",
                };
            }

            return Array.Empty<string>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Checking whether it needs to be included into the application. If it returns true,
        /// all related content, including AndroidManifest, proguard rules and Android external
        /// dependencies would be injected during build time. The default values for new fields in
        /// ARCoreExtensionsProjectSettings should guarantee the associated module to return false.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <param name="buildTarget">Target build platform.</param>
        /// <returns>The boolean shows whether the module is enabled.</returns>
        public override bool IsEnabled(ARCoreExtensionsProjectSettings settings,
            UnityEditor.BuildTarget buildTarget)
        {
            if (settings.GeospatialEnabled &&
                (buildTarget == UnityEditor.BuildTarget.Android ||
                (settings.IsIOSSupportEnabled && buildTarget == UnityEditor.BuildTarget.iOS)))
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// Checking whether this module is compatible with given ARCoreExtensionsConfig and
        /// ARCoreExtensionsProjectSettings. If it returns false, the
        /// CompatibilityCheckPreprocessBuild will throw a general Build Failure Error.
        /// A feature developer should use this function to log detailed error messages that
        /// also include a recommendation of how to resolve the issue. Note: This method is
        /// called once per session config. Do not log any warning or info messages here to
        /// avoid duplicated and/or conflicted information.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <param name="sessionConfig">ARCore Extensions Config.</param>
        /// <param name="buildTarget">Target build platform.</param>
        /// <returns>The boolean shows whether the ARCoreExtensionsProjectSettings is compatible
        /// with the ARCoreExtensionsConfig.</returns>
        public override bool IsCompatible(
            ARCoreExtensionsProjectSettings settings, ARCoreExtensionsConfig sessionConfig,
            UnityEditor.BuildTarget buildTarget)
        {
            string optionFeaturesSettings =
                "Edit > Project Settings > XR > ARCore Extensions > Optional Features";
            if (sessionConfig.GeospatialMode != GeospatialMode.Disabled &&
                !settings.GeospatialEnabled)
            {
                Debug.LogErrorFormat(
                    "LocationModule is required by GeospatialMode {0}. " +
                    "Navigate to {1} and select Geospatial.",
                    sessionConfig.GeospatialMode,
                    optionFeaturesSettings);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checking whether this module is required with given ARCoreExtensionsConfig.
        /// In CompatibilityCheckPreprocessBuild, it would loop all explicit sessionConfigs
        /// via this funcion. If no session configs require this module but this module is
        /// still enabled, then a warning returned by <see ref="GetEnabledNotRequiredWarning()"/>
        /// will be logged.
        /// </summary>
        /// <param name="sessionConfig">ARCore Extensions Config.</param>
        /// <returns>
        /// Whether the module is Required/NotRequired/Optional by the sessionConfig.
        /// </returns>
        public override ModuleNecessity GetModuleNecessity(ARCoreExtensionsConfig sessionConfig)
        {
            if (UseLocation(sessionConfig))
            {
                return ModuleNecessity.Required;
            }

            return ModuleNecessity.NotRequired;
        }

        /// <summary>
        /// In CompatibilityCheckPreprocessBuild, if no session configs require this
        /// module but this module is still enabled, it would call this function to get
        /// a warning message.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <param name="buildTarget">Target build platform.</param>
        /// <returns>
        /// The warning message if this module is enabled but not required by any sessionConfigs.
        /// </returns>
        public override string GetEnabledNotRequiredWarning(
            ARCoreExtensionsProjectSettings settings, UnityEditor.BuildTarget buildTarget)
        {
            string featureName = "null";

            if (settings.GeospatialEnabled)
            {
                featureName = "Geospatial";
            }

            return string.Format(
                "{0} feature is checked in ARCore Extensions Project Settings, but " +
                "it is not used by any Scenes in Build.\nTo turn off {0}, uncheck it in " +
                "Edit > Project Settings > XR > ARCore Extensions > Optional Features > {0}.",
                featureName);
        }

        /// <summary>
        /// Return the XML snippet needs to be included if location module is enabled.
        /// The string output would be added as a child node of in the ‘manifest’ node
        /// of the customized AndroidManifest.xml. The android namespace would be provided
        /// and feature developers could use it directly.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>The XML string snippet to add as a child of node 'manifest'.</returns>
        public override string GetAndroidManifestSnippet(ARCoreExtensionsProjectSettings settings)
        {
            return @"
                <uses-permission android:name=""android.permission.ACCESS_FINE_LOCATION""/>
                <uses-permission android:name=""android.permission.ACCESS_COARSE_LOCATION""
                    android:minSdkVersion=""31""/>
                <uses-feature
                    android:name=""android.hardware.location.gps"" android:required=""true""/>
                <uses-feature
                    android:name=""android.hardware.location"" android:required=""true""/>";
        }

        /// <summary>
        /// Return the Proguard to include if this module is enabled. The string output will be
        /// added into "proguard-user.txt" directly.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>The proguard rule string snippet.</returns>
        public override string GetProguardSnippet(ARCoreExtensionsProjectSettings settings)
        {
            return @"-keep class com.google.android.gms.common.** { *; }
                    -keep class com.google.android.gms.location.** { *; }
                    -keep class com.google.android.gms.tasks.** { *; }";
        }

        /// <summary>
        /// Return all JarArtifacts to be used in External Dependencies Resolvor while building
        /// Android app.
        /// It will generate an AndroidResolverDependencies.xml file under ProjectSettings folder
        /// and then be used in Gradle build process.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>An array defining all Android dependencies.</returns>
        public override JarArtifact[] GetAndroidDependencies(
            ARCoreExtensionsProjectSettings settings)
        {
            return new JarArtifact[]{
                new JarArtifact( "com.google.android.gms", "play-services-location", "16+")};
        }
#endif // UNITY_EDITOR

        /// <summary>
        /// Checks if the location module should be used if any dependent feature is enabled.
        /// </summary>
        /// <param name="sessionConfig">ARCore Extensions Config.</param>
        /// <returns>True if location should be used; otherwise, return false.</returns>
        private static bool UseLocation(ARCoreExtensionsConfig sessionConfig)
        {
            if (sessionConfig.GeospatialMode != GeospatialMode.Disabled)
            {
                return true;
            }

            return false;
        }
    }
}
