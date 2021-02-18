//-----------------------------------------------------------------------
// <copyright file="ARCloudReferencePoint.cs" company="Google LLC">
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
    using System.Diagnostics.CodeAnalysis;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Deprecated version of <c><see cref="ARCloudAnchor"/></c>.
    /// </summary>
    /// @deprecated Please use ARCloudAnchor instead.
    [SuppressMessage("UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
     Justification = "Match Unity's naming style.")]
    [Obsolete("This class has been renamed to ARCloudAnchor. " +
        "See details in release notes v1.16.0.")]
    public class ARCloudReferencePoint : ARCloudAnchor
    {
        /// <summary>
        /// Gets the Cloud Reference Id associated with this cloud reference point. For newly
        /// created points the Id will be an empty string until the cloud reference point is
        /// in the <c><see cref="CloudReferenceState"/></c>.<c>Success</c> state.
        /// Deprecated version of <c><see cref="ARCloudAnchor.cloudAnchorId"/></c>.
        /// </summary>
        public string cloudReferenceId
        {
            get
            {
                return cloudAnchorId;
            }
        }

        /// <summary>
        /// Gets the <c><see cref="CloudReferenceState"/></c> associated with cloud reference point.
        /// Deprecated version of <c><see cref="ARCloudAnchor.cloudAnchorState"/></c>.
        /// </summary>
        public CloudReferenceState cloudReferenceState
        {
            get
            {
                return (CloudReferenceState)cloudAnchorState;
            }
        }

        /// <summary>
        /// Gets the <c>TrackableId</c> associated with this cloud reference point.
        /// </summary>
        public new TrackableId trackableId
        {
            get
            {
                return base.trackableId;
            }
        }

        /// <summary>
        /// Gets the <c>Pose</c> associated with this cloud reference point.
        /// </summary>
        public new Pose pose
        {
            get
            {
                return base.pose;
            }
        }

        /// <summary>
        /// Gets the <c>TrackingState</c> associated with this cloud reference point.
        /// </summary>
        public new TrackingState trackingState
        {
            get
            {
                return base.trackingState;
            }
        }

        /// <summary>
        /// Unity Update method.
        /// </summary>
        public new void Update()
        {
            base.Update();
        }
    }
}
