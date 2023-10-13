//-----------------------------------------------------------------------
// <copyright file="ARCoreExtensionsProjectSettings.cs" company="Google LLC">
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

#if UNITY_EDITOR
namespace Google.XR.ARCoreExtensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using Google.XR.ARCoreExtensions;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;

    /// <summary>
    /// Android Authentication Strategy.
    /// </summary>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules",
                     "SA1602:EnumerationItemsMustBeDocumented",
     Justification = "Internal.")]
    public enum AndroidAuthenticationStrategy
    {
        [DisplayName("Do Not Use")]
        DoNotUse = 0,
        [DisplayName("Keyless (recommended)")]
        Keyless = 1,
        [DisplayName("API Key")]
        ApiKey = 2,
    }

    /// <summary>
    /// IOS Authentication Strategy.
    /// </summary>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules",
                     "SA1602:EnumerationItemsMustBeDocumented",
     Justification = "Internal.")]
    public enum IOSAuthenticationStrategy
    {
        [DisplayName("Do Not Use")]
        DoNotUse = 0,
        [DisplayName("Authentication Token (recommended)")]
        AuthenticationToken = 1,
        [DisplayName("API Key")]
        ApiKey = 2,
    }

    /// <summary>
    /// Settings of ARCore Extensions.
    /// </summary>
    public class ARCoreExtensionsProjectSettings
    {
        /// <summary>
        /// ARCore Extensions version.
        /// </summary>
        [HideInInspector]
        public string Version;

        /// <summary>
        /// IOS support setting.
        /// </summary>
        [DisplayName("iOS Support Enabled")]
        [DynamicHelp("GetIOSSupportHelpInfo")]
        public bool IsIOSSupportEnabled;

        /// <summary>
        /// Android Authentication Strategy.
        /// </summary>
        [DisplayName("Android Authentication Strategy")]
        [DynamicHelp("GetAndroidStrategyHelpInfo")]
        public AndroidAuthenticationStrategy AndroidAuthenticationStrategySetting =
            AndroidAuthenticationStrategy.DoNotUse;

        /// <summary>
        /// Android Api Key.
        /// </summary>
        [DisplayName("Android API Key")]
        [DisplayCondition("IsAndroidApiKeyFieldDisplayed")]
        public string AndroidCloudServicesApiKey;

        /// <summary>
        /// IOS Authentication Strategy.
        /// </summary>
        [DisplayName("iOS Authentication Strategy")]
        [DisplayCondition("IsIosStrategyDisplayed")]
        [DynamicHelp("GetIosStrategyHelpInfo")]
        public IOSAuthenticationStrategy IOSAuthenticationStrategySetting =
            IOSAuthenticationStrategy.DoNotUse;

        /// <summary>
        /// IOS Api Key.
        /// </summary>
        [DisplayName("iOS API Key")]
        [DisplayCondition("IsIosApiKeyFieldDisplayed")]
        public string IOSCloudServicesApiKey;

        [Header("Optional Features")]

        /// <summary>
        /// Indicates whether Cloud Anchor is enabled for this project.
        /// </summary>
        [DisplayName("Cloud Anchors")]
        [DynamicHelp("GetCloudAnchorHelpInfo")]
        public bool CloudAnchorEnabled;

        /// <summary>
        /// Indicates whether the project is built with the ARCore Geospatial API. When this is
        /// checked, includes libraries required for the Geospatial API to function in your build.
        /// </summary>
        [DisplayName("Geospatial")]
        [DynamicHelp("GetGeospatialHelpInfo")]
        public bool GeospatialEnabled;

        /// <summary>
        /// Indicates if ARCore Geospatial Creator features should be available in the Unity Editor.
        /// When this is checked, the scripting symbol will be defined which enables the features.
        /// </summary>
        /// <remarks> We use the original "GeospatialEditor" feature name for the field here to retain
        /// backwards compatibility. </remarks>
        [DisplayName("Geospatial Creator")]
        [DynamicHelp("GetGeospatialCreatorHelpInfo")]
        public bool GeospatialEditorEnabled;

        /// <summary>
        /// Indicates whether the project is built with the ARCore Semantics API for iOS. When this
        /// is checked, includes libraries required for the Scene Semantics API to function in your
        /// build.
        /// </summary>
        [DisplayName("Semantics on iOS")]
        [DisplayCondition("IsSemanticsIosFieldDisplayed")]
        public bool SemanticsIosEnabled;

        private const string _projectSettingsPath =
            "ProjectSettings/ARCoreExtensionsProjectSettings.json";

        private static ARCoreExtensionsProjectSettings _instance = null;

        /// <summary>
        /// Gets singleton instance of ARCoreExtensionsProjectSettings.
        /// </summary>
        public static ARCoreExtensionsProjectSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ARCoreExtensionsProjectSettings();
                    _instance.Load();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Get scripting define symbols for iOS supported features and their status.
        /// </summary>
        /// <returns>A dictionary contains Scripting Define Symbols for iOS features and
        /// whether it's enabled.</returns>
        public Dictionary<string, bool> GetIOSSymbolsStatus()
        {
            return new Dictionary<string, bool>
            {
                { "CLOUDANCHOR_IOS_SUPPORT", CloudAnchorEnabled },
                { "GEOSPATIAL_IOS_SUPPORT", GeospatialEnabled },
                { "SEMANTICS_IOS_SUPPORT", SemanticsIosEnabled },
            };
        }

        /// <summary>
        /// Get the filenames of all available CocoaPod templates and their status.
        /// </summary>
        /// <returns>An array of all available CocoaPod templates and whether it's enabled.
        /// </returns>
        public Dictionary<string, bool> GetIOSDependenciesStatus()
        {
            return new Dictionary<string, bool>
            {
                { "ARCoreiOSCloudAnchorDependencies", CloudAnchorEnabled },
                { "ARCoreiOSGeospatialDependencies", GeospatialEnabled },
                { "ARCoreiOSSemanticsDependencies", SemanticsIosEnabled },
            };
        }

        /// <summary>
        /// Loads previous settings.
        /// </summary>
        public void Load()
        {
            // Default settings.
            IsIOSSupportEnabled = false;
            AndroidAuthenticationStrategySetting = AndroidAuthenticationStrategy.DoNotUse;
            IOSAuthenticationStrategySetting = IOSAuthenticationStrategy.DoNotUse;
            AndroidCloudServicesApiKey = string.Empty;
            IOSCloudServicesApiKey = string.Empty;

            if (File.Exists(_projectSettingsPath))
            {
                ARCoreExtensionsProjectSettings settings =
                    JsonUtility.FromJson<ARCoreExtensionsProjectSettings>(
                        File.ReadAllText(_projectSettingsPath));
                foreach (FieldInfo fieldInfo in this.GetType().GetFields())
                {
                    fieldInfo.SetValue(this, fieldInfo.GetValue(settings));
                }

                // Set the initial value in previous settings to keep Cloud Anchors enabled.
                int[] versions = string.IsNullOrEmpty(Version) ? new int[0] :
                    Array.ConvertAll(
                        Version.Split('.'),
                        s => { int.TryParse(s, out int num); return num; });
                if (versions.Length == 3 && versions[0] == 1 && versions[1] <= 30)
                {
                    CloudAnchorEnabled = true;
                }
            }

            if (!string.IsNullOrEmpty(AndroidCloudServicesApiKey))
            {
                AndroidAuthenticationStrategySetting = AndroidAuthenticationStrategy.ApiKey;
            }

            if (!string.IsNullOrEmpty(IOSCloudServicesApiKey))
            {
                IOSAuthenticationStrategySetting = IOSAuthenticationStrategy.ApiKey;
            }

            // Update the settings version as needed.
            Version = VersionInfo.Version;
        }

        /// <summary>
        /// Saves current settings.
        /// </summary>
        public void Save()
        {
            try
            {
                File.WriteAllText(_projectSettingsPath, JsonUtility.ToJson(this));
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Unable to save ARCoreExtensionsProjectSettings, '{0}'", e);
            }
        }

        /// <summary>
        /// Reflection function used by <see cref="DynamicHelpAttribute"/> for property
        /// <see cref="IsIOSSupportEnabled"/>.
        /// </summary>
        /// <returns>Help info for <see cref="IsIOSSupportEnabled"/>.</returns>
        public HelpAttribute GetIOSSupportHelpInfo()
        {
            if (IsIOSSupportEnabled && !GetIOSSymbolsStatus().ContainsValue(true))
            {
                return new HelpAttribute(
                    "iOS support is enabled but no iOS feature is in use, " +
                    "this will include unnecessary dependencies in your application. " +
                    "You may select the desired features from 'Optional Features' or " +
                    "turn off iOS support.",
                    HelpAttribute.HelpMessageType.Warning);
            }

            return null;
        }

        /// <summary>
        /// Reflection function used by <see cref="DisplayConditionAttribute"/> for property
        /// <see cref="AndroidCloudServicesApiKey"/>.
        /// </summary>
        /// <returns>Display condition for <see cref="AndroidCloudServicesApiKey"/>.</returns>
        public bool IsAndroidApiKeyFieldDisplayed()
        {
            if (AndroidAuthenticationStrategySetting == AndroidAuthenticationStrategy.ApiKey)
            {
                return true;
            }
            else
            {
                AndroidCloudServicesApiKey = string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Reflection function used by <see cref="DisplayConditionAttribute"/> for property
        /// <see cref="IOSCloudServicesApiKey"/>.
        /// </summary>
        /// <returns>Display condition for <see cref="IOSCloudServicesApiKey"/>.</returns>
        public bool IsIosApiKeyFieldDisplayed()
        {
            if (!IsIOSSupportEnabled)
            {
                return false;
            }

            if (IOSAuthenticationStrategySetting == IOSAuthenticationStrategy.ApiKey)
            {
                return true;
            }
            else
            {
                IOSCloudServicesApiKey = string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Reflection function used by <see cref="DisplayConditionAttribute"/> for property
        /// <see cref="SemanticsIosEnabled"/>.
        /// </summary>
        /// <returns>Display condition for <see cref="SemanticsIosEnabled"/>.</returns>
        public bool IsSemanticsIosFieldDisplayed()
        {
            return IsIOSSupportEnabled;
        }

        /// <summary>
        /// Reflection function used by <see cref="DynamicHelpAttribute"/> for property
        /// <see cref="AndroidAuthenticationStrategySetting"/>.
        /// </summary>
        /// <returns>Help info for <see cref="AndroidAuthenticationStrategySetting"/>.</returns>
        public HelpAttribute GetAndroidStrategyHelpInfo()
        {
            if (AndroidAuthenticationStrategySetting == AndroidAuthenticationStrategy.ApiKey &&
                CloudAnchorEnabled)
            {
                return new HelpAttribute(
                    "Persistent Cloud Anchors will not be available on Android when 'API Key'" +
                    " authentication strategy is selected.",
                    HelpAttribute.HelpMessageType.Warning);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reflection function used by <see cref="DisplayConditionAttribute"/> for property
        /// <see cref="IOSAuthenticationStrategySetting"/>.
        /// </summary>
        /// <returns>Display condition for <see cref="IOSAuthenticationStrategySetting"/>.</returns>
        public bool IsIosStrategyDisplayed()
        {
            return IsIOSSupportEnabled;
        }

        /// <summary>
        /// Reflection function used by <see cref="DynamicHelpAttribute"/> for property
        /// <see cref="IOSAuthenticationStrategySetting"/>.
        /// </summary>
        /// <returns>Help info for <see cref="IOSAuthenticationStrategySetting"/>.</returns>
        public HelpAttribute GetIosStrategyHelpInfo()
        {
            if (IOSAuthenticationStrategySetting == IOSAuthenticationStrategy.ApiKey &&
                CloudAnchorEnabled)
            {
                return new HelpAttribute(
                    "Persistent Cloud Anchors will not be available on iOS when 'API Key'" +
                    " authentication strategy is selected.",
                    HelpAttribute.HelpMessageType.Warning);
            }
            else if (IOSAuthenticationStrategySetting ==
                IOSAuthenticationStrategy.AuthenticationToken)
            {
                return new HelpAttribute(
                    "Authentication Token is selected as iOS Authentication. " +
                    "To authenticate with the Google Cloud Service, use " +
                    "ARAnchorManager.SetAuthToken(string) at runtime.",
                    HelpAttribute.HelpMessageType.Info);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reflection function used by <see cref="DynamicHelpAttribute"/> for property
        /// <see cref="CloudAnchorEnabled"/>.
        /// </summary>
        /// <returns>Help info for <see cref="CloudAnchorEnabled"/>.</returns>
        public HelpAttribute GetCloudAnchorHelpInfo()
        {
            if (!CloudAnchorEnabled)
            {
                return null;
            }

            if (AndroidAuthenticationStrategySetting == AndroidAuthenticationStrategy.DoNotUse
                || (IsIOSSupportEnabled &&
                IOSAuthenticationStrategySetting == IOSAuthenticationStrategy.DoNotUse))
            {
                return new HelpAttribute(
                    "When using Cloud Anchors, an authentication strategy is required.",
                    HelpAttribute.HelpMessageType.Error);
            }

            return new HelpAttribute(
                string.Format(
                    "When using Cloud Anchors, {0} on Android{1}.",
                    AndroidAuthenticationStrategySetting ==
                        AndroidAuthenticationStrategy.Keyless ?
                        "add authentication dependencies" : "inject API Key to the manifest",
                    IsIOSSupportEnabled ?
                        ", and import CloudAnchors CocoaPod on iOS" : string.Empty),
                HelpAttribute.HelpMessageType.None);
        }

        /// <summary>
        /// Reflection function used by <see cref="DynamicHelpAttribute"/> for property
        /// <see cref="GeospatialEnabled"/>.
        /// </summary>
        /// <returns>Help info for <see cref="GeospatialEnabled"/>.</returns>
        public HelpAttribute GetGeospatialHelpInfo()
        {
            if (!GeospatialEnabled)
            {
                return null;
            }

            if (AndroidAuthenticationStrategySetting == AndroidAuthenticationStrategy.DoNotUse ||
                (IOSAuthenticationStrategySetting == IOSAuthenticationStrategy.DoNotUse &&
                IsIOSSupportEnabled))
            {
                return new HelpAttribute(
                    "When using the Geospatial API, an authentication strategy is required.",
                    HelpAttribute.HelpMessageType.Error);
            }
            else
            {
                return new HelpAttribute(
                    string.Format(
                        "When using the Geospatial API, add {0}location dependencies on Android{1}. " +
                        "Note: precise location permission is required at runtime, " +
                        "otherwise, enabling the Geospatial API may fail with a permission not "+
                        "granted error.",
                        AndroidAuthenticationStrategySetting ==
                            AndroidAuthenticationStrategy.Keyless ?
                            "authentication and " : string.Empty,
                        IsIOSSupportEnabled ?
                            ", and import Geospatial CocoaPod on iOS" : string.Empty),
                    HelpAttribute.HelpMessageType.None);
            }
        }

        /// <summary>
        /// Reflection function used by <see cref="DynamicHelpAttribute"/> for property
        /// <see cref="GeospatialEditorEnabled"/>.
        /// </summary>
        /// <returns>Help info for <see cref="GeospatialEnabled"/>.</returns>
        public HelpAttribute GetGeospatialCreatorHelpInfo()
        {
#if !UNITY_2021_3_OR_NEWER
            if (GeospatialEditorEnabled)
            {
                return new HelpAttribute(
                    "The Geospatial Creator requires Unity 2021.3 or later.",
                    HelpAttribute.HelpMessageType.Error);
            }
#endif
            if (GeospatialEditorEnabled && !GeospatialEnabled)
            {
                return new HelpAttribute(
                    "The \"Geospatial\" optional feature must be enabled to use the Geospatial Creator.",
                    HelpAttribute.HelpMessageType.Error);
            }
            return null;
        }
    }

    /// <summary>
    /// This attribute controls whether to display the field or not. The function name
    /// would be input as the parameter to this attribute. Note, the function must return
    /// the type bool, take no parameters, and be a member of ARCoreProjectSettings.
    /// </summary>
    public class DisplayConditionAttribute : Attribute
    {
        /// <summary>
        /// Reflection function that return the type bool, take no parameters,
        /// and be a member of ARCoreProjectSettings.
        /// </summary>
        public readonly string CheckingFunction;

        /// <summary>
        /// Initializes a new instance of the `DisplayCondition` class.
        /// </summary>
        /// <param name="checkingFunction">Reflection function.</param>
        public DisplayConditionAttribute(string checkingFunction)
        {
            CheckingFunction = checkingFunction;
        }
    }

    /// <summary>
    /// This attribute would affect the field displayed in the ProjectSettingGUI.
    /// It could be used for either a field or an enum. If this attribute isn’t provided,
    /// then the default field name would be the field name.
    /// </summary>
    public class DisplayNameAttribute : Attribute
    {
        /// <summary>
        /// Display string in the GUI.
        /// </summary>
        public readonly string DisplayString;

        /// <summary>
        /// Initializes a new instance of the `DisplayName` class.
        /// </summary>
        /// <param name="displayString">Display string in the GUI.</param>
        public DisplayNameAttribute(string displayString)
        {
            DisplayString = displayString;
        }
    }

    /// <summary>
    /// This attribute is used to control the enum ranges provided for popup.
    /// The function must be a member of ARCoreProjectSettings, return the type
    /// System.Array, and take no parameters.
    /// </summary>
    public class EnumRangeAttribute : Attribute
    {
        /// <summary>
        /// Reflection function that return the type System.Array, take no parameters,
        /// and be a member of ARCoreProjectSettings.
        /// </summary>
        public readonly string CheckingFunction;

        /// <summary>
        /// Initializes a new instance of the `EnumRange` class.
        /// </summary>
        /// <param name="checkingFunction">Reflection function.</param>
        public EnumRangeAttribute(string checkingFunction)
        {
            CheckingFunction = checkingFunction;
        }
    }
}
#endif // UNITY_EDITOR
