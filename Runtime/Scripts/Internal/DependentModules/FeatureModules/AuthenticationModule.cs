//-----------------------------------------------------------------------
// <copyright file="AuthenticationModule.cs" company="Google LLC">
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
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using UnityEngine;

    /// <summary>
    /// The implemented class of Authentication Module.
    /// </summary>
    public class AuthenticationModule : DependentModuleBase
    {
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
            if (buildTarget == UnityEditor.BuildTarget.iOS)
            {
                bool isEnabled = settings.IOSAuthenticationStrategySetting !=
                    IOSAuthenticationStrategy.DoNotUse;
                if (settings.IsIOSSupportEnabled && !isEnabled)
                {
                    Debug.LogWarning(
                        "Cloud Anchor APIs require one of the iOS authentication strategies. " +
                        "If it’s not in use, you can uncheck iOS Support Enabled in " +
                        "Edit > Project Settings > XR > ARCore Extensions " +
                        "so ARCore Extensions won’t import Cloud Anchor iOS cocoapod into " +
                        "your project.");
                }

                return isEnabled;
            }
            else
            {
                return settings.AndroidAuthenticationStrategySetting !=
                    AndroidAuthenticationStrategy.DoNotUse;
            }
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
            if (buildTarget == UnityEditor.BuildTarget.iOS)
            {
                if (sessionConfig.CloudAnchorMode == CloudAnchorMode.Enabled &&
                    settings.IOSAuthenticationStrategySetting ==
                    IOSAuthenticationStrategy.DoNotUse)
                {
                    Debug.LogErrorFormat(
                        "Cloud Anchor authentication is required by CloudAnchorMode {0}. " +
                        "An iOS Authentication Strategy must be set in " +
                        "Edit > Project Settings > XR > ARCore Extensions > " +
                        "iOS Authentication Strategy when CloudAnchorMode is {0}",
                        sessionConfig.CloudAnchorMode);
                    return false;
                }

                return true;
            }
            else
            {
                if (sessionConfig.CloudAnchorMode == CloudAnchorMode.Enabled &&
                    settings.AndroidAuthenticationStrategySetting ==
                    AndroidAuthenticationStrategy.DoNotUse)
                {
                    Debug.LogErrorFormat(
                        "Cloud Anchor authentication is required by CloudAnchorMode {0}. " +
                        "An Android Authentication Strategy must be set in " +
                        "Edit > Project Settings > XR > ARCore Extensions > " +
                        "Android Authentication Strategy when CloudAnchorMode is {0}.",
                        sessionConfig.CloudAnchorMode);
                    return false;
                }

                return true;
            }
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
            if (sessionConfig.CloudAnchorMode != CloudAnchorMode.Disabled)
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
            ARCoreExtensionsProjectSettings settings,
            UnityEditor.BuildTarget buildTarget)
        {
            string featureName = "Cloud Anchor";
            string platformName;
            if (buildTarget == UnityEditor.BuildTarget.iOS)
            {
                platformName = "iOS";
            }
            else
            {
                platformName = "Android";
            }

            return string.Format("{0} Authentication is enabled in ARCore Extensions Project " +
                    "Settings but {0} is not used in any Scenes in Build.\n" +
                    "To turn off authentication, select Do Not Use in Edit > " +
                    "Project Settings > XR > ARCore Extensions > {1} Authentication Strategy.",
                    featureName, platformName);
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
            if (!string.IsNullOrEmpty(settings.AndroidCloudServicesApiKey))
            {
                return string.Format(
                    @"<application>
                        <meta-data
                            android:name=""com.google.android.ar.API_KEY""
                            android:value=""{0}""/>
                        </application>",
                    settings.AndroidCloudServicesApiKey);
            }

            return string.Empty;
        }

        /// <summary>
        /// Return the Proguard to include if this module is enabled. The string output will be
        /// added into "proguard-user.txt" directly.
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>The proguard rule string snippet.</returns>
        public override string GetProguardSnippet(ARCoreExtensionsProjectSettings settings)
        {
            if (settings.AndroidAuthenticationStrategySetting ==
                AndroidAuthenticationStrategy.Keyless)
            {
                return @"-keep class com.google.android.gms.common.** { *; }
                    -keep class com.google.android.gms.auth.** { *; }
                    -keep class com.google.android.gms.tasks.** { *; }";
            }

            return string.Empty;
        }

        /// <summary>
        /// Return the snippet to be used in Play Services Resolver while building Android app.
        /// The string output will be added into a new created file whose name would combine the
        /// module name and "Dependencies.xml".
        /// </summary>
        /// <param name="settings">ARCore Extensions Project Settings.</param>
        /// <returns>The dependencies to be resolved in Play Services Resolver.</returns>
        public override string GetAndroidDependenciesSnippet(
            ARCoreExtensionsProjectSettings settings)
        {
            if (settings.AndroidAuthenticationStrategySetting ==
                AndroidAuthenticationStrategy.Keyless)
            {
                return @"<!-- Required by Keyless authentication, allows dynamite-loaded part of
                    ARCore to access Google Play Services APIs via reflection in order to
                    access authentication APIs. -->
                    <androidPackage
                        spec=""com.google.android.gms:play-services-auth-base:16+"" />";
            }

            return string.Empty;
        }
#endif // UNITY_EDITOR
    }
}
