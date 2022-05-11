//-----------------------------------------------------------------------
// <copyright file="ARCoreExtensionsConfig.cs" company="Google LLC">
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
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// Holds settings that are used to configure the ARCore Extensions.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ARCoreExtensionsConfig",
        menuName = "XR/ARCore Extensions Config",
        order = 1)]
    public class ARCoreExtensionsConfig : ScriptableObject
    {
        [Header("Cloud Anchors")]

        /// <summary>
        /// Gets or sets the <c><see cref="CloudAnchorMode"/></c> to use.
        /// </summary>
        [Tooltip("Chooses which Cloud Anchors mode will be used in ARCore Extensions session.")]
        [FormerlySerializedAs("EnableCloudAnchors")]
        public CloudAnchorMode CloudAnchorMode = CloudAnchorMode.Disabled;

        [Header("Geospatial")]

        /// <summary>
        /// Choose the Geospatial API mode, allowing the use of <c><see cref="AREarthManager"/></c>.
        /// </summary>
        [Tooltip("Choose if the ARCore Geospatial API is enabled, allowing Earth localisation " +
            "features. To enable Geospatial Mode, ensure Geospatial is selected in ARCore " +
            "Extensions Project Settings > Optional Features.")]
        public GeospatialMode GeospatialMode = GeospatialMode.Disabled;

        /// <summary>
        /// Gets or sets a value indicating whether the Cloud Anchors are enabled.
        /// </summary>
        /// @deprecated Please use CloudAnchorMode instead.
        [System.Obsolete(
            "This field has been replaced by ARCoreExtensionsConfig.CloudAnchorMode. See " +
            "https://github.com/google-ar/arcore-unity-extensions/releases/tag/v1.20.0")]
        public bool EnableCloudAnchors
        {
            get
            {
                return CloudAnchorMode != CloudAnchorMode.Disabled;
            }

            set
            {
                CloudAnchorMode = value ? CloudAnchorMode.Enabled : CloudAnchorMode.Disabled;
            }
        }

        /// <summary>
        /// ValueType check if two ARCoreExtensionsConfig objects are equal.
        /// </summary>
        /// <param name="other">The other ARCoreExtensionsConfig.</param>
        /// <returns>True if the two ARCoreExtensionsConfig objects are value-type equal,
        /// otherwise false.</returns>
        public override bool Equals(object other)
        {
            ARCoreExtensionsConfig otherConfig = other as ARCoreExtensionsConfig;
            if (otherConfig == null ||
                GeospatialMode != otherConfig.GeospatialMode ||
                CloudAnchorMode != otherConfig.CloudAnchorMode)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return a hash code for this object.
        /// </summary>
        /// <returns>A hash code value.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// ValueType copy from another ARCoreExtensionsConfig object into this one.
        /// </summary>
        /// <param name="otherConfig">The ARCoreExtensionsConfig to copy from.</param>
        public void CopyFrom(ARCoreExtensionsConfig otherConfig)
        {
            CloudAnchorMode = otherConfig.CloudAnchorMode;
            GeospatialMode = otherConfig.GeospatialMode;
        }
    }
}
