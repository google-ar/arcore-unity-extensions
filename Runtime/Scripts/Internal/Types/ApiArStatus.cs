//-----------------------------------------------------------------------
// <copyright file="ApiArStatus.cs" company="Google">
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

namespace Google.XR.ARCoreExtensions.Internal
{
    // ARCore status, results returned by many ARCore C-API functions.
    internal enum ApiArStatus
    {
        Success = 0,
        ErrorInvalidArgument = -1,
        ErrorFatal = -2,
        ErrorSessionPaused = -3,
        ErrorSessionNotPaused = -4,
        ErrorNotTracking = -5,
        ErrorTextureNotSet = -6,
        ErrorMissingGlContext = -7,
        ErrorUnsupportedConfiguration = -8,
        ErrorCameraPermissionNotGranted = -9,
        ErrorDeadlineExceeded = -10,
        ErrorResourceExhausted = -11,
        ErrorNotYetAvailable = -12,
        ErrorCameraNotAvailable = -13,
        ErrorCloudAnchorsNotConfigured = -14,
        ErrorInternetPermissionNotGranted = -15,
        ErrorAnchorNotSupportedForHosting = -16,
        ErrorImageInsufficientQuality = -17,
        ErrorDataInvalidFormat = -18,
        ErrorDatatUnsupportedVersion = -19,
        UnavailableArCoreNotInstalled = -100,
        UnavailableDeviceNotCompatible = -101,
        UnavailableApkTooOld = -103,
        UnavailableSdkTooOld = -104,
        UnavailableUserDeclinedInstall = -105,
    }
}
