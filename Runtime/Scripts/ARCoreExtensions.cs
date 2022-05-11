//-----------------------------------------------------------------------
// <copyright file="ARCoreExtensions.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Google.XR.ARCoreExtensions.Internal;
    using Unity.Collections;
    using UnityEngine;

#if UNITY_ANDROID
    using UnityEngine.XR.ARCore;
#endif // UNITY_ANDROID
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;


    /// <summary>
    /// ARCore Extensions, this script allows an app to specify and provide access to
    /// AR Foundation object instances that should be used by ARCore Extensions.
    /// </summary>
    public class ARCoreExtensions : MonoBehaviour
    {
        /// <summary>
        /// AR Foundation <c>ARSession</c> used by the scene.
        /// </summary>
        public ARSession Session;

        /// <summary>
        /// AR Foundation <c>ARSessionOrigin</c> used by the scene.
        /// </summary>
        public ARSessionOrigin SessionOrigin;

        /// <summary>
        /// AR Foundation <c>ARCameraManager</c> used in the ARSessionOrigin.
        /// </summary>
        public ARCameraManager CameraManager;

        /// <summary>
        /// Supplementary configuration to define features and options for the
        /// ARCore Extensions.
        /// </summary>
        public ARCoreExtensionsConfig ARCoreExtensionsConfig;

        /// <summary>
        /// The <c><see cref="ARCoreExtensionsCameraConfigFilter"/></c> to define the set of
        /// properties desired or required by the app to run.
        /// </summary>
        [Tooltip("Configuration options to select the camera mode and features.")]
        public ARCoreExtensionsCameraConfigFilter CameraConfigFilter;

        /// <summary>
        /// The callback event that allows a camera configuration to be selected from
        /// a list of valid configurations.
        /// The callback should be registered before the ARCore session is enabled
        /// (e.g. Unity's Awake() method) to ensure it is triggered on the first
        /// frame update.
        /// </summary>
        [HideInInspector]
        public OnChooseXRCameraConfigurationEvent OnChooseXRCameraConfiguration;

        /// <summary>
        /// Selects a camera configuration for the ARCore session to use.
        /// </summary>
        /// <param name="supportedConfigurations">A list of supported camera configurations.
        /// The size is dependent on <c><see cref="CameraConfigFilter"/></c> settings.
        /// The GPU texture resolutions are the same in all configs.
        /// Currently, most devices provide GPU texture resolution of 1920 x 1080, but devices
        /// might provide higher or lower resolution textures, depending on device capabilities.
        /// The CPU image resolutions returned are VGA, 720p, and a resolution matching
        /// the GPU texture, typically the native resolution of the device.</param>
        /// <returns>The index of the camera configuration in <c>supportedConfigurations</c> to be
        /// used for the ARCore session. If the return value is not a valid index
        /// (e.g. the value -1), then no camera configuration will be set and the ARCore session
        /// will use the previously selected camera configuration or a default configuration
        /// if no previous selection exists.</returns>
        public delegate int OnChooseXRCameraConfigurationEvent(
            List<XRCameraConfiguration> supportedConfigurations);

#if UNITY_ANDROID
        internal const int _androidSSDKVersion = 31;
        private static AndroidJavaClass _versionInfo;

        private string _currentPermissionRequest = null;

        private HashSet<string> _requiredPermissionNames = new HashSet<string>();

        private ARCoreSessionSubsystem _arCoreSubsystem;

        private ARCoreExtensionsConfig _cachedConfig = null;

        private ARCoreCameraSubsystem _arCoreCameraSubsystem;

        private ARCoreExtensionsCameraConfigFilter _cachedFilter = null;

#endif
        internal static ARCoreExtensions _instance { get; private set; }

        internal IntPtr currentARCoreSessionHandle
        {
            get
            {
                if (_instance == null || _instance.Session == null)
                {
                    Debug.LogError("ARCore Extensions not found or not configured. Please " +
                        "include an ARCore Extensions game object in your scene. " +
                        "GameObject -> XR -> ARCore Extensions");
                    return IntPtr.Zero;
                }

#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
                return IOSSupportManager.Instance.ARCoreSessionHandle;
#else
                return _instance.Session.SessionHandle();
#endif
            }
        }

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        public void Awake()
        {
            if (_instance)
            {
                Debug.LogError("ARCore Extensions is already initialized. You may only " +
                    "have one instance in your scene at a time.");
            }

            _instance = this;
#if UNITY_ANDROID
            _versionInfo = new AndroidJavaClass("android.os.Build$VERSION");
#endif
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        public void Start()
        {
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            IOSSupportManager.Instance.UpdateARSession(Session);
            IOSSupportManager.Instance.UpdateCameraManager(CameraManager);
#endif
        }

        /// <summary>
        /// Unity's OnEnable method.
        /// </summary>
        public void OnEnable()
        {
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            IOSSupportManager.Instance.SetEnabled(true);
#endif // UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
#if UNITY_ANDROID
            if (_instance.Session == null)
            {
                Debug.LogError("ARSession is required by ARCoreExtensions!");
                return;
            }

            _arCoreSubsystem = (ARCoreSessionSubsystem)Session.subsystem;
            if (_arCoreSubsystem == null)
            {
                Debug.LogError(
                    "No active ARCoreSessionSubsystem is available in this session, Please " +
                    "ensure that a valid loader configuration exists in the XR project settings.");
            }
            else
            {
                _arCoreSubsystem.beforeSetConfiguration += BeforeConfigurationChanged;
            }

            _arCoreCameraSubsystem = (ARCoreCameraSubsystem)CameraManager.subsystem;
            if (_arCoreCameraSubsystem == null)
            {
                Debug.LogError(
                    "No active ARCoreCameraSubsystem is available in this session, Please " +
                    "ensure that a valid loader configuration exists in the XR project settings.");
            }
            else
            {
                _arCoreCameraSubsystem.beforeGetCameraConfiguration += BeforeGetCameraConfiguration;
            }
#endif // UNITY_ANDROID

            CachedData.Reset();
        }

        /// <summary>
        /// Unity's OnDisable method.
        /// </summary>
        public void OnDisable()
        {
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            IOSSupportManager.Instance.SetEnabled(false);
#endif // UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
#if UNITY_ANDROID
            if (_arCoreSubsystem != null)
            {
                _arCoreSubsystem.beforeSetConfiguration -= BeforeConfigurationChanged;
            }

            if (_arCoreCameraSubsystem != null)
            {
                _arCoreCameraSubsystem.beforeGetCameraConfiguration -= BeforeGetCameraConfiguration;
            }
#endif // UNITY_ANDROID

            CachedData.Reset();
        }

        /// <summary>
        /// Unity's OnDestroy method.
        /// </summary>
        public void OnDestroy()
        {
#if UNITY_IOS && ARCORE_EXTENSIONS_IOS_SUPPORT
            IOSSupportManager.Instance.ResetInstanceAndSession();
#endif

            if (_instance)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Unity's Update method.
        /// </summary>
        public void Update()
        {
#if UNITY_ANDROID
            if (_requiredPermissionNames.Count > 0)
            {
                RequestPermission();
            }

            if (Session.SessionHandle() == IntPtr.Zero)
            {
                return;
            }

            // Update camera config filter
            if (CameraConfigFilter != null && !CameraConfigFilter.Equals(_cachedFilter))
            {
                _cachedFilter =
                    ScriptableObject.CreateInstance<ARCoreExtensionsCameraConfigFilter>();
                _cachedFilter.CopyFrom(CameraConfigFilter);

                // Extensions will attempt to select the camera config based on the filter
                // if it's in use, otherwise, relies on AR Foundation's default behavior.
                SelectCameraConfig();
            }

            // Update session configuration.
            if (ARCoreExtensionsConfig != null && !ARCoreExtensionsConfig.Equals(_cachedConfig))
            {
                _cachedConfig = ScriptableObject.CreateInstance<ARCoreExtensionsConfig>();
                _cachedConfig.CopyFrom(ARCoreExtensionsConfig);

                List<IDependentModule> modules = DependentModulesManager.GetModules();
                foreach (var module in modules)
                {
                    string[] permissions = module.GetRuntimePermissions(_cachedConfig);
                    foreach (var permission in permissions)
                    {
                        if (!AndroidPermissionsManager.IsPermissionGranted(permission))
                        {
                            _requiredPermissionNames.Add(permission);
                        }
                    }
                }

                if (_requiredPermissionNames.Count > 0)
                {
                    RequestPermission();
                    return;
                }

                _arCoreSubsystem.SetConfigurationDirty();
            }
#endif
        }
#if UNITY_ANDROID

        /// <summary>
        /// Unity OnValidate.
        /// </summary>
        public void OnValidate()
        {

            if (ARCoreExtensionsConfig != null && CameraManager != null &&
                CameraManager.requestedFacingDirection != CameraFacingDirection.World &&
                ARCoreExtensionsConfig.GeospatialMode == GeospatialMode.Enabled)
            {
                Debug.LogErrorFormat(
                    "Geospatial Mode {0} is incompatible with user-facing (selfie) camera. " +
                    "Choose 'World' camera in ARCameraManager instead.",
                    ARCoreExtensionsConfig.GeospatialMode);
            }
        }

        private void RequestPermission()
        {
            // Waiting for current request.
            if (!AndroidPermissionsManager.IsPermissionGranted(
                    AndroidPermissionsManager._cameraPermission) ||
                !string.IsNullOrEmpty(_currentPermissionRequest))
            {
                return;
            }

            string[] requestPermissions;
            _currentPermissionRequest = _requiredPermissionNames.First();
            if (_versionInfo.GetStatic<int>("SDK_INT") >= _androidSSDKVersion &&
                _currentPermissionRequest.Equals(AndroidPermissionsManager._fineLocationPermission))
            {
                requestPermissions = new[] {
                    _currentPermissionRequest,
                    AndroidPermissionsManager._coarseLocationPermission,
                };
            }
            else
            {
                requestPermissions = new[] { _currentPermissionRequest };
            }

            AndroidPermissionsManager.RequestPermission(
                requestPermissions, OnPermissionRequestFinish);
        }

        private void OnPermissionRequestFinish(string permissionName, bool isGranted)
        {
            Debug.LogFormat("{0} {1}.",
                isGranted ? "Granted" : "Denied", permissionName);
            if (_currentPermissionRequest != permissionName)
            {
                return;
            }

            _requiredPermissionNames.Remove(_currentPermissionRequest);
            _currentPermissionRequest = null;
            _arCoreSubsystem.SetConfigurationDirty();
        }

        private void BeforeConfigurationChanged(ARCoreBeforeSetConfigurationEventArgs eventArgs)
        {
            if (_cachedConfig == null)
            {
                return;
            }

            if (eventArgs.arSession.AsIntPtr() != IntPtr.Zero &&
                eventArgs.arConfig.AsIntPtr() != IntPtr.Zero)
            {
                ConfigApi.UpdateSessionConfig(
                    eventArgs.arSession.AsIntPtr(), eventArgs.arConfig.AsIntPtr(), _cachedConfig);
            }
        }

        private void BeforeGetCameraConfiguration(
            ARCoreBeforeGetCameraConfigurationEventArgs eventArgs)
        {
            if (CameraConfigFilter == null)
            {
                return;
            }

            if (eventArgs.session.AsIntPtr() != IntPtr.Zero &&
                eventArgs.filter.AsIntPtr() != IntPtr.Zero)
            {
                CameraConfigFilterApi.UpdateFilter(
                    eventArgs.session.AsIntPtr(), eventArgs.filter.AsIntPtr(), CameraConfigFilter);

                // Update the filter cache to avoid overwriting user's selection in case the
                // GetConfiguration() is called by the user instead of the SelectCameraConfig().
                if (!CameraConfigFilter.Equals(_cachedFilter))
                {
                    _cachedFilter =
                        ScriptableObject.CreateInstance<ARCoreExtensionsCameraConfigFilter>();
                    _cachedFilter.CopyFrom(CameraConfigFilter);
                }
            }
        }

        private void SelectCameraConfig()
        {
            if (CameraManager == null)
            {
                return;
            }

            using (var configurations = CameraManager.GetConfigurations(Allocator.Temp))
            {
                if (configurations.Length == 0)
                {
                    Debug.LogWarning(
                        "Unable to choose a custom camera configuration " +
                        "because none are available.");
                    return;
                }

                int configIndex = 0;
                if (OnChooseXRCameraConfiguration != null)
                {
                    configIndex = OnChooseXRCameraConfiguration(configurations.ToList());
                }

                if (configIndex < 0 || configIndex >= configurations.Length)
                {
                    Debug.LogWarning(
                        "Failed to find a valid config index with " +
                        "the custom selection function.");
                    return;
                }

                CameraManager.currentConfiguration = configurations[configIndex];
            }
        }
#endif // UNITY_ANDROID
    }
}
