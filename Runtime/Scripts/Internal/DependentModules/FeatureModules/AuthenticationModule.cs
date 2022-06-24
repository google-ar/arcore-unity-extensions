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
                return settings.IOSAuthenticationStrategySetting !=
                    IOSAuthenticationStrategy.DoNotUse;
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
            string optionFeaturesSettings =
                "Edit > Project Settings > XR Plug-in Management > ARCore Extensions > " +
                "Optional Features";
            if (sessionConfig.CloudAnchorMode != CloudAnchorMode.Disabled &&
                !settings.CloudAnchorEnabled)
            {
                Debug.LogErrorFormat(
                    "Cloud Anchors feature is required by CloudAnchorMode {0}. " +
                    "It must be set in {1} > Cloud Anchors.",
                    sessionConfig.CloudAnchorMode,
                    optionFeaturesSettings);
                return false;
            }
            else if (sessionConfig.GeospatialMode != GeospatialMode.Disabled && !settings.GeospatialEnabled)
            {
                Debug.LogErrorFormat(
                    "Geospatial feature is required by GeospatialMode {0}. " +
                    "It must be set in {1} > Geospatial.",
                    sessionConfig.GeospatialMode,
                    optionFeaturesSettings);
                return false;
            }

            if (buildTarget == UnityEditor.BuildTarget.iOS)
            {
                string requirement = string.Empty;
                string iosAuthSettings =
                    "Edit > Project Settings > XR Plug-in Management > ARCore Extensions > " +
                    "iOS Authentication Strategy";
                if (sessionConfig.CloudAnchorMode == CloudAnchorMode.Enabled &&
                    settings.IOSAuthenticationStrategySetting ==
                    IOSAuthenticationStrategy.DoNotUse)
                {
                    requirement = string.Format(
                        "A valid authentication strategy is required by CloudAnchorMode {0}.",
                        sessionConfig.CloudAnchorMode);
                }
                else if (
                    sessionConfig.GeospatialMode == GeospatialMode.Enabled &&
                    settings.IOSAuthenticationStrategySetting == IOSAuthenticationStrategy.DoNotUse)
                {
                    requirement = string.Format(
                        "A valid authentication strategy is required by GeospatialMode {0}.",
                        sessionConfig.GeospatialMode);
                }

                if (string.IsNullOrEmpty(requirement))
                {
                    return true;
                }
                else
                {
                    Debug.LogErrorFormat("{0} It must be set in {1}.",
                        requirement, iosAuthSettings);
                    return false;
                }
            }
            else
            {
                string requirement = string.Empty;
                string androidAuthSettings =
                    "Edit > Project Settings > XR Plug-in Management > ARCore Extensions > " +
                    "Android Authentication Strategy";
                if (sessionConfig.CloudAnchorMode == CloudAnchorMode.Enabled &&
                    settings.AndroidAuthenticationStrategySetting ==
                    AndroidAuthenticationStrategy.DoNotUse)
                {
                    requirement = string.Format(
                        "A valid authentication strategy is required by CloudAnchorMode {0}.",
                        sessionConfig.CloudAnchorMode);
                }
                else if (
                    sessionConfig.GeospatialMode == GeospatialMode.Enabled &&
                    settings.AndroidAuthenticationStrategySetting ==
                    AndroidAuthenticationStrategy.DoNotUse)
                {
                    requirement = string.Format(
                        "A valid authentication strategy is required by GeospatialMode {0}.",
                        sessionConfig.GeospatialMode);
                }

                if (string.IsNullOrEmpty(requirement))
                {
                    return true;
                }
                else
                {
                    Debug.LogErrorFormat("{0} It must be set in {1}.",
                        requirement, androidAuthSettings);
                    return false;
                }
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

            if (sessionConfig.GeospatialMode != GeospatialMode.Disabled)
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
            string platformName = buildTarget == UnityEditor.BuildTarget.iOS ? "iOS" : "Android";
            return string.Format(
                    "{0} Authentication is enabled in ARCore Extensions Project Settings " +
                    "but the feature is not used by any Scenes in Build.\n" +
                    "To turn off authentication, select Do Not Use in Edit > " +
                    "Project Settings > XR Plug-in Management > ARCore Extensions > {0} " +
                    "Authentication Strategy.",
                    platformName);
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
            if (settings.AndroidAuthenticationStrategySetting ==
                AndroidAuthenticationStrategy.Keyless)
            {
                return new JarArtifact[]{
                    new JarArtifact( "com.google.android.gms", "play-services-auth-base", "16+")};
            }
            else
            {
                return null;
            }
        }
#endif // UNITY_EDITOR
    }
}
