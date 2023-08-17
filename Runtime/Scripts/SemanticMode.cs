//-----------------------------------------------------------------------
// <copyright file="SemanticMode.cs" company="Google LLC">
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
    /// <summary>
    /// Describes the desired behavior of Scene Semantics. Scene Semantics uses a machine learning
    /// model to label each pixel from the camera feed with a <c><see cref="SemanticLabel"/></c>.
    /// See the <a href="https://developers.google.com/ar/develop/unity-arf/scene-semantics">Scene
    /// Semantics Developer Guide</a> for more information.
    ///
    /// The Scene Semantics API is currently able to distinguish between outdoor labels specified by
    /// <c><see cref="SemanticLabel"/></c>. Usage indoors is currently unsupported and may yield
    /// unreliable results.
    ///
    /// <p>A small number of ARCore supported devices do not support the Scene Semantics API. Use
    /// <c><see cref="ARSemanticManager.IsSemanticModeSupported(SemanticMode)"/></c>
    /// to query for support for Scene Semantics. Affected devices are also indicated on the <a
    /// href="https://developers.google.com/ar/devices">ARCore supported devices page</a>.
    ///
    /// <p>The default value is <c><see cref="SemanticMode.Disabled"/></c>.
    /// </summary>
    public enum SemanticMode
    {
        /// <summary>
        /// The Scene Semantics API is disabled. Calls to
        /// <c><see cref="ARSemanticManager.TryGetSemanticTexture(ref Texture2D)"/></c>,
        /// <c><see cref="ARSemanticManager.TryGetSemanticConfidenceTexture(ref Texture2D)"/></c>,
        /// and <c><see cref="ARSemanticManager.GetSemanticLabelFraction(SemanticLabel)"/></c> will
        /// not return valid results.
        ///
        /// This is the default mode.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// The Scene Semantics API is enabled. Calls to
        /// <c><see cref="ARSemanticManager.TryGetSemanticTexture(ref Texture2D)"/></c>,
        /// <c><see cref="ARSemanticManager.TryGetSemanticConfidenceTexture(ref Texture2D)"/></c>,
        /// and <c><see cref="ARSemanticManager.GetSemanticLabelFraction(SemanticLabel)"/></c> will
        /// return valid results.
        /// </summary>
        Enabled = 1,
    }
}
