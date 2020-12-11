//-----------------------------------------------------------------------
// <copyright file="AndroidPermissionsManager.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Manages Android permissions for the Unity application.
    /// </summary>
    public class AndroidPermissionsManager : AndroidJavaProxy
    {
        internal const string _cameraPermission = "android.permission.CAMERA";
        private static AndroidPermissionsManager _instance;
        private static Action<bool> _permissionRequest = null;
        private static AndroidJavaObject _activity = null;
        private static AndroidJavaObject _permissionService = null;

        /// <summary>
        /// Constructs a new AndroidPermissionsManager.
        /// </summary>
        public AndroidPermissionsManager() : base(
            "com.unity3d.plugin.UnityAndroidPermissions$IPermissionRequestResult")
        {
        }

        /// <summary>
        /// Checks if an Android permission is granted to the application.
        /// </summary>
        /// <param name="permissionName">The full name of the Android permission to check (e.g.
        /// android.permission.CAMERA).</param>
        /// <returns><c>true</c> if <c>permissionName</c> is granted to the application, otherwise
        /// <c>false</c>.</returns>
        public static bool IsPermissionGranted(string permissionName)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return true;
            }

            return GetPermissionsService().Call<bool>(
                "IsPermissionGranted", GetUnityActivity(), permissionName);
        }

        /// <summary>
        /// Requests an Android permission from the user.
        /// </summary>
        /// <param name="permissionName">The permission to be requested (e.g.
        /// android.permission.CAMERA).</param>
        /// <param name="onRequestFinished">The callback event when the request got a result.
        /// </param>
        public static void RequestPermission(string permissionName, Action<bool> onRequestFinished)
        {
            if (IsPermissionGranted(permissionName))
            {
                onRequestFinished(true);
                return;
            }

            if (_permissionRequest != null)
            {
                Debug.LogError("Attempted to make simultaneous Android permissions requests.");
                return;
            }

            _permissionRequest = onRequestFinished;
            GetPermissionsService().Call("RequestPermissionAsync", GetUnityActivity(),
                new[] { permissionName }, GetInstance());
        }

        /// <summary>
        /// Callback fired when a permission is granted.
        /// </summary>
        /// <param name="permissionName">The name of the permission that was granted.</param>
        public virtual void OnPermissionGranted(string permissionName)
        {
            OnPermissionResult(permissionName, true);
        }

        /// <summary>
        /// Callback fired when a permission is denied.
        /// </summary>
        /// <param name="permissionName">The name of the permission that was denied.</param>
        public virtual void OnPermissionDenied(string permissionName)
        {
            OnPermissionResult(permissionName, false);
        }

        /// <summary>
        /// Callback fired on an Android activity result (unused part of UnityAndroidPermissions
        /// interface).
        /// </summary>
        public virtual void OnActivityResult()
        {
        }

        internal static AndroidPermissionsManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new AndroidPermissionsManager();
            }

            return _instance;
        }

        private static AndroidJavaObject GetUnityActivity()
        {
            if (_activity == null)
            {
                AndroidJavaClass unityPlayer =
                    new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            return _activity;
        }

        private static AndroidJavaObject GetPermissionsService()
        {
            if (_permissionService == null)
            {
                _permissionService =
                    new AndroidJavaObject("com.unity3d.plugin.UnityAndroidPermissions");
            }

            return _permissionService;
        }

        /// <summary>
        /// Callback fired on an Android permission result.
        /// </summary>
        /// <param name="permissionName">The name of the permission.</param>
        /// <param name="granted">If permission is granted or not.</param>
        private void OnPermissionResult(string permissionName, bool granted)
        {
            if (_permissionRequest == null)
            {
                Debug.LogErrorFormat(
                    "AndroidPermissionsManager received an unexpected permissions result {0}",
                    permissionName);
                return;
            }

            // Cache completion method and reset request state.
            var onRequestFinished = _permissionRequest;
            _permissionRequest = null;

            onRequestFinished(granted);
        }
    }
}
