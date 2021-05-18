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
        /// Reflection function used by 'DisplayCondition' for property
        /// 'AndroidCloudServicesApiKey'.
        /// </summary>
        /// <returns>Display condition for 'AndroidCloudServicesApiKey'.</returns>
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
        /// Reflection function used by 'DynamicHelp' for property
        /// 'AndroidAuthenticationStrategySetting'.
        /// </summary>
        /// <returns>Help info for 'AndroidAuthenticationStrategySetting'.</returns>
        public HelpAttribute GetAndroidStrategyHelpInfo()
        {
            if (AndroidAuthenticationStrategySetting == AndroidAuthenticationStrategy.ApiKey)
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
        /// Reflection function used by 'DisplayCondition' for property
        /// 'IOSAuthenticationStrategySetting'.
        /// </summary>
        /// <returns>Display condition for 'IOSAuthenticationStrategySetting'.</returns>
        public bool IsIosStrategyDisplayed()
        {
            return IsIOSSupportEnabled;
        }

        /// <summary>
        /// Reflection function used by 'DisplayCondition' for property 'IOSCloudServicesApiKey'.
        /// </summary>
        /// <returns>Display condition for 'IOSCloudServicesApiKey'.</returns>
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
        /// Reflection function used by 'DynamicHelp' for property 'IOSAuthenticationStrategy'.
        /// </summary>
        /// <returns>Help info for 'IOSAuthenticationStrategy'.</returns>
        public HelpAttribute GetIosStrategyHelpInfo()
        {
            if (IOSAuthenticationStrategySetting == IOSAuthenticationStrategy.ApiKey)
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
                    "Authentication Token is selected as the Cloud Anchor Authentication. " +
                    "To authenticate with the Google Cloud Anchor Service, use " +
                    "ARAnchorManager.SetAuthToken(string) in runtime.",
                    HelpAttribute.HelpMessageType.Info);
            }
            else
            {
                return null;
            }
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
