//-----------------------------------------------------------------------
// <copyright file="GeospatialCreatorEnabledWizard.cs" company="Google LLC">
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
    using System.Text;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEditor;
    using UnityEditor.PackageManager;
    using UnityEditor.PackageManager.Requests;
    using UnityEngine;
    using UnityEngine.UIElements;

    internal class GeospatialCreatorEnabledWizard : EditorWindow
    {
        private const string _cesiumId = "com.cesium.unity";
        private const string _unityMathId = "com.unity.mathematics";

        private static readonly Version _cesiumMinVersion = new Version("1.1.0");
        private static readonly Version _unityMathMinVersion = new Version("1.2.0");
        private static readonly Vector2 _wizardWindowSize = new Vector2(350, 400);

        private static GeospatialCreatorEnabledWizard _instance;

        private ListRequest _listInstalledPackagesRequest = null;
        private bool _isMissingCesium = true;
        private bool _isMissingUnityMath = true;

        public static GeospatialCreatorEnabledWizard ShowWizard()
        {
            if (_instance == null)
            {
                _instance =
                    ScriptableObject.CreateInstance(typeof(GeospatialCreatorEnabledWizard))
                    as GeospatialCreatorEnabledWizard;
                _instance.ShowUtility();

                _instance.titleContent = new GUIContent("Geospatial Creator");
                _instance.minSize = _wizardWindowSize;
                _instance.maxSize = _instance.minSize;
            }
            else
            {
                _instance.ShowUtility();
            }

            return _instance;
        }

        public void OnDestroy()
        {
            _instance = null;
        }

        public void OnFocus()
        {
            // Automatically recheck the dependencies since they may have
            // changed while the wizard bas out of focus.
            CheckDependencies();
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical();
            if (_listInstalledPackagesRequest != null)
            {
                IsCheckingDependenciesGUI();
            }
            else if (_isMissingCesium || _isMissingUnityMath)
            {
                IsMissingDependenciesGUI();
            }
            else
            {
                FinishGUI();
            }

            GUILayout.EndVertical();
        }

        private static bool MeetsVersionRequirement(Version toCheck, Version minVersion)
        {
            return (toCheck.CompareTo(minVersion) >= 0);
        }

        // Draw the GUI elements for the title, fixed product summary, and Quickstart button
        private void HeaderGUI()
        {
            GUILayout.Label("Geospatial Creator", WizardStyles.TitleLabel);
            GUILayout.Label(
                "\nThe Geospatial Creator helps you build AR apps by showing where"
                    + " your content will be placed in the real world.",
                WizardStyles.BodyLabel);

            GUIContent openQuickstartContent = new GUIContent(
                "Open Geospatial Creator Quickstart Guide",
                "Open the Quickstart webpage for Geospatial Creator in a browser.");
            if (GUILayout.Button(openQuickstartContent))
            {
                Application.OpenURL(GeospatialCreatorHelper.QuickstartUrl);
            }

            EditorGUILayout.Space();
        }

        // Draw the GUI elements for the bottom button bar, which includes an optionally-visible
        // "Cancel" button and a generic action button that is enabled if
        // isActionButtonEnabled=true and executes action() when clicked.
        private void FooterGUI(
            bool isCancelButtonVisible,
            bool isActionButtonEnabled,
            string actionButtonText,
            Action action)
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            if (isCancelButtonVisible)
            {
                if (GUILayout.Button("Cancel"))
                {
                    ARCoreExtensionsProjectSettings.Instance.GeospatialEditorEnabled = false;
                    ARCoreExtensionsProjectSettings.Instance.Save();
                    Close();
                }
            }

            bool prevEnabled = GUI.enabled;
            GUI.enabled = isActionButtonEnabled;
            if (GUILayout.Button(actionButtonText))
            {
                action();
            }

            GUI.enabled = prevEnabled;
            GUILayout.EndHorizontal();
        }

        // Draw the GUI elements displayed while waiting for the dependencies check to finish
        private void IsCheckingDependenciesGUI()
        {
            HeaderGUI();
            GUILayout.Label(
                "Checking product dependencies, please wait...",
                WizardStyles.BoldBodyLabel);
            FooterGUI(true, false, "Recheck dependencies", () => { });
        }

        // Draw the GUI elements displayed when any dependencies are missing from the project.
        private void IsMissingDependenciesGUI()
        {
            StringBuilder dependenciesText =
                new StringBuilder("The project is missing the following required dependencies:\n");
            if (_isMissingCesium)
            {
                dependenciesText.Append(
                    String.Format("\n   • {0} {1}+", _cesiumId, _cesiumMinVersion.ToString()));
            }

            if (_isMissingUnityMath)
            {
                dependenciesText.Append(
                    String.Format(
                        "\n   • {0} {1}+",
                        _unityMathId,
                        _unityMathMinVersion.ToString()));
            }

            dependenciesText.Append("\n\nSee the Quickstart Guide for more information.");

            HeaderGUI();
            GUILayout.Label(dependenciesText.ToString(), WizardStyles.BoldBodyLabel);

            FooterGUI(true, true, "Recheck dependencies", () =>
            {
                CheckDependencies();
            });
        }

        private void FinishGUI()
        {
            HeaderGUI();
            string finishButtonText = "Finish";
            GUILayout.Label(
                String.Format(
                    "Your project is ready. Click \"{0}\" to begin using the Geospatial Creator!",
                    finishButtonText),
                WizardStyles.BoldBodyLabel);

            FooterGUI(false, true, finishButtonText, () =>
            {
                GeospatialCreatorHelper.ConfigureScriptingSymbols(true);
                ARCoreExtensionsProjectSettings.Instance.GeospatialEditorEnabled = true;
                ARCoreExtensionsProjectSettings.Instance.Save();

                // :TODO (b/277333333): Consider waiting for scripts to reload before closing
                Close();
            });
        }

        private void CheckDependencies()
        {
            Debug.Log("Geospatial Creator is checking for required dependencies...");

            // Remove the listener, if it is exists, so we can't have multiple instances checking
            // at once. This is a no-op if we weren't currently listening.
            _listInstalledPackagesRequest = null;
            EditorApplication.update -= ListPackagesRequestProgress;

            // assume we're missing them until we know otherwise.
            _isMissingCesium = true;
            _isMissingUnityMath = true;

            _listInstalledPackagesRequest = Client.List(
                /*offlineMode=*/ true, /*includeIndirectDependencies=*/ true);
            EditorApplication.update += ListPackagesRequestProgress;
        }

        private void ListPackagesRequestProgress()
        {
            if (!_listInstalledPackagesRequest.IsCompleted)
            {
                return;
            }

            foreach (var package in _listInstalledPackagesRequest.Result)
            {
                if (package.name == _cesiumId)
                {
                    _isMissingCesium =
                        !MeetsVersionRequirement(new Version(package.version), _cesiumMinVersion);
                }
                else if (package.name == _unityMathId)
                {
                    _isMissingUnityMath =
                        !MeetsVersionRequirement(
                            new Version(package.version), _unityMathMinVersion);
                }

                if (!_isMissingCesium && !_isMissingUnityMath)
                {
                    break;
                }
            }

            EditorApplication.update -= ListPackagesRequestProgress;
            _listInstalledPackagesRequest = null;
            Repaint();
        }

        private static class WizardStyles
        {
            public static readonly GUIStyle BodyLabel;
            public static readonly GUIStyle BoldBodyLabel;
            public static readonly GUIStyle TitleLabel;

            static WizardStyles()
            {
                BodyLabel = new GUIStyle(EditorStyles.largeLabel);
                BodyLabel.wordWrap = true;

                BoldBodyLabel = new GUIStyle(BodyLabel);
                BoldBodyLabel.fontStyle = FontStyle.Bold;

                TitleLabel = new GUIStyle(EditorStyles.boldLabel);
                TitleLabel.fontSize = 20;
                TitleLabel.alignment = TextAnchor.MiddleCenter;
            }
        }
    }
}
