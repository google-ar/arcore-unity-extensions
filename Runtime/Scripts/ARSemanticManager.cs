//-----------------------------------------------------------------------
// <copyright file="ARSemanticManager.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions
{
    using System;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;

    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Provides access to the Scene Semantics API.
    /// </summary>
    public class ARSemanticManager : MonoBehaviour
    {
        /// <summary>
        /// Checks whether the provided <c><see cref="SemanticMode"/></c> is supported on this
        /// device with the selected camera configuration. The current list of supported devices
        /// is documented on the <a href="https://developers.google.com/ar/devices">
        /// ARCore supported devices</a> page.
        /// </summary>
        /// <param name="mode">The desired semantic mode.</param>
        /// <returns>
        /// Indicates whether the given mode is supported on this device.
        /// It will return <c><see cref="FeatureSupported.Unknown"/></c> if the session is still
        /// under initialization.
        /// </returns>
        public FeatureSupported IsSemanticModeSupported(SemanticMode mode)
        {
#if UNITY_IOS
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero)
            {
                return FeatureSupported.Unknown;
            }

            return SessionApi.IsSemanticModeSupported(
                ARCoreExtensions._instance.currentARCoreSessionHandle, mode);
#else // UNITY_IOS
            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero)
            {
                return FeatureSupported.Unknown;
            }

            return SessionApi.IsSemanticModeSupported(
                ARCoreExtensions._instance.currentARCoreSessionHandle, mode);
#endif // UNITY_IOS
        }

        /// <summary>
        /// Attempts to get a texture representing the semantic image for the current frame.
        /// The texture format is <c><see cref="TextureFormat.R8"/></c>. Each pixel in the
        /// image is an 8-bit unsigned integer representing a semantic class label.
        /// <c><see cref="SemanticLabel"/></c> is the list of possible pixel labels. See the <a
        /// href="https://developers.google.com/ar/develop/unity-arf/scene-semantics">Scene
        /// Semantics Developer Guide</a> for more information.
        ///
        /// In order to obtain a valid result from this function, you must set the session's
        /// <c><see cref="SemanticMode"/></c> to <c><see cref="SemanticMode.Enabled"/></c>.
        /// Use <c><see cref="ARSemanticManager.IsSemanticModeSupported(SemanticMode)"/></c>
        /// to query for support for Scene Semantics.
        ///
        /// The width of the semantic image is currently 256 pixels. The height of the
        /// image depends on the device and will match its display aspect ratio.
        ///
        /// </summary>
        /// <param name="texture">
        /// The semantic image <c><see cref="Texture2D"/></c> to be filled.</param>
        /// <returns>True if the semantic image texture was filled. Otherwise, false.</returns>
        public bool TryGetSemanticTexture(ref Texture2D texture)
        {
#if UNITY_IOS
            if (ARCoreExtensions._instance.ARCoreExtensionsConfig.SemanticMode !=
                SemanticMode.Enabled)
            {
                Debug.LogWarning(
                    "Semantic image texture is not available when SemanticMode is not enabled.");
                return false;
            }

            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                ARCoreExtensions._instance.CameraManager == null)
            {
                return false;
            }

            if (!ARCoreExtensions.TryGetLatestFrame(out XRCameraFrame frame))
            {
                return false;
            }

            if (CachedData.TryGetCachedData(
                    CachedData.SemanticsTexture, out texture) &&
                CachedData.TryGetCachedData(
                    CachedData.SemanticsTimestamp, out long timestamp) &&
                texture != null && timestamp == frame.timestampNs)
            {
                return true;
            }

            IntPtr imageHandle = FrameApi.AcquireSemanticImage(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                IOSSupportManager.Instance.ARCoreFrameHandle);

            if (imageHandle == IntPtr.Zero)
            {
                return false;
            }

            ImageApi.UpdateTexture(
                ARCoreExtensions._instance.currentARCoreSessionHandle, imageHandle,
                TextureFormat.R8, ref texture);
            ImageApi.Release(imageHandle);
            CachedData.SetCachedData(CachedData.SemanticsTexture, texture);
            CachedData.SetCachedData(CachedData.SemanticsTimestamp, frame.timestampNs);

            return true;
#else // UNITY_IOS
            if (ARCoreExtensions._instance.ARCoreExtensionsConfig.SemanticMode !=
                SemanticMode.Enabled)
            {
                Debug.LogWarning(
                    "Semantic image texture is not available when SemanticMode is not enabled.");
                return false;
            }

            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                ARCoreExtensions._instance.CameraManager == null)
            {
                return false;
            }

            if (!ARCoreExtensions.TryGetLatestFrame(out XRCameraFrame frame))
            {
                return false;
            }

            if (CachedData.TryGetCachedData(
                    CachedData.SemanticsTexture, out texture) &&
                CachedData.TryGetCachedData(
                    CachedData.SemanticsTimestamp, out long timestamp) &&
                texture != null && timestamp == frame.timestampNs)
            {
                return true;
            }

            IntPtr imageHandle = FrameApi.AcquireSemanticImage(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                frame.FrameHandle());

            if (imageHandle == IntPtr.Zero)
            {
                return false;
            }

            ImageApi.UpdateTexture(
                ARCoreExtensions._instance.currentARCoreSessionHandle, imageHandle,
                TextureFormat.R8, ref texture);
            ImageApi.Release(imageHandle);
            CachedData.SetCachedData(CachedData.SemanticsTexture, texture);
            CachedData.SetCachedData(CachedData.SemanticsTimestamp, frame.timestampNs);

            return true;
#endif // UNITY_IOS
        }

        /// <summary>
        /// Attempts to get a texture representing the semantic confidence image corresponding to
        /// the current frame. The texture format is <c><see cref="TextureFormat.Alpha8"/></c>,
        /// with each pixel representing the estimated confidence of the corresponding pixel in
        /// the semantic image. See the <a
        /// href="https://developers.google.com/ar/develop/unity-arf/scene-semantics">Scene
        /// Semantics Developer Guide</a> for more information.
        ///
        /// The confidence value is between 0 and 255, inclusive, with 0
        /// representing the lowest confidence and 255 representing the highest confidence in
        /// the semantic class prediction.
        ///
        /// In order to obtain a valid result from this function, you must set the session's
        /// <c><see cref="SemanticMode"/></c> to <c><see cref="SemanticMode.Enabled"/></c>.
        /// Use <c><see cref="ARSemanticManager.IsSemanticModeSupported(SemanticMode)"/></c>
        /// to query for support for Scene Semantics.
        ///
        /// The size of the semantic confidence image is the same size as the image
        /// obtained by <c><see cref="ARSemanticManager.TryGetSemanticTexture(ref Texture2D)"/></c>.
        ///
        /// </summary>
        /// <param name="texture">
        /// The semantic confidence image <c><see cref="Texture2D"/></c> to be filled.</param>
        /// <returns>True if the semantic confidence image texture was filled.
        /// Otherwise, false.</returns>
        public bool TryGetSemanticConfidenceTexture(ref Texture2D texture)
        {
#if UNITY_IOS
            if (ARCoreExtensions._instance.ARCoreExtensionsConfig.SemanticMode !=
                SemanticMode.Enabled)
            {
                Debug.LogWarning(
                    "Semantic confidence image texture is not available when" +
                    " SemanticMode is not enabled.");
                return false;
            }

            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                ARCoreExtensions._instance.CameraManager == null)
            {
                return false;
            }

            if (!ARCoreExtensions.TryGetLatestFrame(out XRCameraFrame frame))
            {
                return false;
            }

            if (CachedData.TryGetCachedData(
                    CachedData.SemanticsConfidenceTexture, out texture) &&
                CachedData.TryGetCachedData(
                    CachedData.SemanticsConfidenceTimestamp, out long timestamp) &&
                texture != null && timestamp == frame.timestampNs)
            {
                return true;
            }

            IntPtr imageHandle = FrameApi.AcquireSemanticConfidenceImage(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                IOSSupportManager.Instance.ARCoreFrameHandle);

            if (imageHandle == IntPtr.Zero)
            {
                return false;
            }

            ImageApi.UpdateTexture(
                ARCoreExtensions._instance.currentARCoreSessionHandle, imageHandle,
                TextureFormat.Alpha8, ref texture);
            ImageApi.Release(imageHandle);
            CachedData.SetCachedData(CachedData.SemanticsConfidenceTexture, texture);
            CachedData.SetCachedData(CachedData.SemanticsConfidenceTimestamp, frame.timestampNs);

            return true;
#else // UNITY_IOS
            if (ARCoreExtensions._instance.ARCoreExtensionsConfig.SemanticMode !=
                SemanticMode.Enabled)
            {
                Debug.LogWarning(
                    "Semantic confidence image texture is not available when" +
                    " SemanticMode is not enabled.");
                return false;
            }

            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                ARCoreExtensions._instance.CameraManager == null)
            {
                return false;
            }

            if (!ARCoreExtensions.TryGetLatestFrame(out XRCameraFrame frame))
            {
                return false;
            }

            if (CachedData.TryGetCachedData(
                    CachedData.SemanticsConfidenceTexture, out texture) &&
                CachedData.TryGetCachedData(
                    CachedData.SemanticsConfidenceTimestamp, out long timestamp) &&
                texture != null && timestamp == frame.timestampNs)
            {
                return true;
            }

            IntPtr imageHandle = FrameApi.AcquireSemanticConfidenceImage(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                frame.FrameHandle());

            if (imageHandle == IntPtr.Zero)
            {
                return false;
            }

            ImageApi.UpdateTexture(
                ARCoreExtensions._instance.currentARCoreSessionHandle, imageHandle,
                TextureFormat.Alpha8, ref texture);
            ImageApi.Release(imageHandle);
            CachedData.SetCachedData(CachedData.SemanticsConfidenceTexture, texture);
            CachedData.SetCachedData(CachedData.SemanticsConfidenceTimestamp, frame.timestampNs);

            return true;
#endif // UNITY_IOS
        }

        /// <summary>
        /// Retrieves the percentage of pixels in the most recent semantics image that are
        /// @p queryLabel. This call is more efficient than retrieving the image and
        /// performing a pixel-wise search for the detected label.
        ///
        /// </summary>
        /// <param name="queryLabel">The <c><see cref="SemanticLabel"/></c> to search for
        /// within the semantic image for this frame.</param>
        /// <returns>
        /// The fraction of pixels in the most recent semantic image that contains the
        /// query label.  This value is in the range 0.0 to 1.0.  If no pixels are
        /// present with that label, or if an invalid label is provided, this call
        /// returns 0.0.
        /// </returns>
        public float GetSemanticLabelFraction(SemanticLabel queryLabel)
        {
#if UNITY_IOS
            if (ARCoreExtensions._instance.ARCoreExtensionsConfig.SemanticMode !=
                SemanticMode.Enabled)
            {
                Debug.LogWarning(
                    "Get semantic label fraction is not available when" +
                    " SemanticMode is not enabled.");
                return 0.0f;
            }

            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                ARCoreExtensions._instance.CameraManager == null)
            {
                return 0.0f;
            }

            if (!ARCoreExtensions.TryGetLatestFrame(out XRCameraFrame frame))
            {
                return 0.0f;
            }

            return FrameApi.GetSemanticLabelFraction(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                IOSSupportManager.Instance.ARCoreFrameHandle,
                queryLabel.ToApiSemanticLabel());
#else // UNITY_IOS
            if (ARCoreExtensions._instance.ARCoreExtensionsConfig.SemanticMode !=
                SemanticMode.Enabled)
            {
                Debug.LogWarning(
                    "Get semantic label fraction is not available when" +
                    " SemanticMode is not enabled.");
                return 0.0f;
            }

            if (ARCoreExtensions._instance.currentARCoreSessionHandle == IntPtr.Zero ||
                ARCoreExtensions._instance.CameraManager == null)
            {
                return 0.0f;
            }

            if (!ARCoreExtensions.TryGetLatestFrame(out XRCameraFrame frame))
            {
                return 0.0f;
            }

            return FrameApi.GetSemanticLabelFraction(
                ARCoreExtensions._instance.currentARCoreSessionHandle,
                frame.FrameHandle(), queryLabel.ToApiSemanticLabel());
#endif // UNITY_IOS
        }
    }
}
