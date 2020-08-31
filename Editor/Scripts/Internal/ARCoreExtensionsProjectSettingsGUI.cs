//-----------------------------------------------------------------------
// <copyright file="ARCoreExtensionsProjectSettingsGUI.cs" company="Google LLC">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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
    using UnityEditor;
    using UnityEngine;

    internal class ARCoreExtensionsProjectSettingsGUI
    {
        // These labels are made public so they can be discovered automatically by
        // the provider using GetSearchKeywordsFromGUIContentProperties().
        public static readonly GUIContent IsIOSSupportEnabled =
            new GUIContent("iOS Support Enabled");

        public static readonly GUIContent CloudAnchorAPIKeys =
            new GUIContent("Cloud Anchor API Keys");

        public static readonly GUIContent Android = new GUIContent("Android");
        public static readonly GUIContent IOS = new GUIContent("iOS");

        private static float _groupLabelIndent = 15;
        private static float _groupFieldIndent =
            EditorGUIUtility.labelWidth - _groupLabelIndent;
        private static bool _foldoutCloudAnchorAPIKeys = true;

        internal static void OnGUI(bool renderForStandaloneWindow)
        {
            bool newIOSEnabled = EditorGUILayout.Toggle(IsIOSSupportEnabled,
                ARCoreExtensionsProjectSettings.Instance.IsIOSSupportEnabled);
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            _foldoutCloudAnchorAPIKeys =
                EditorGUILayout.Foldout(_foldoutCloudAnchorAPIKeys, CloudAnchorAPIKeys);
            if (_foldoutCloudAnchorAPIKeys)
            {
                // Draw text field for Android Cloud Anchor API Key.
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(_groupLabelIndent);
                EditorGUILayout.LabelField(Android, GUILayout.Width(_groupFieldIndent));
                ARCoreExtensionsProjectSettings.Instance.AndroidCloudServicesApiKey =
                    EditorGUILayout.TextField(
                        ARCoreExtensionsProjectSettings.Instance.AndroidCloudServicesApiKey);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                // Draw text field for iOS Cloud Anchor API Key.
                if (newIOSEnabled)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(_groupLabelIndent);
                    EditorGUILayout.LabelField(IOS, GUILayout.Width(_groupFieldIndent));
                    ARCoreExtensionsProjectSettings.Instance.IOSCloudServicesApiKey =
                        EditorGUILayout.TextField(
                            ARCoreExtensionsProjectSettings.Instance.IOSCloudServicesApiKey);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                }
                else
                {
                    ARCoreExtensionsProjectSettings.Instance.IOSCloudServicesApiKey = string.Empty;
                }
            }

            if (GUI.changed)
            {
                if (newIOSEnabled != ARCoreExtensionsProjectSettings.Instance.IsIOSSupportEnabled)
                {
                    ARCoreExtensionsProjectSettings.Instance.IsIOSSupportEnabled = newIOSEnabled;
                    IOSSupportHelper.SetARCoreIOSSupportEnabled(newIOSEnabled);
                }

                ARCoreExtensionsProjectSettings.Instance.Save();
            }
        }
    }
}
