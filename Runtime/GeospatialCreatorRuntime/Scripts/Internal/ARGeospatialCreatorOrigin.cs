//-----------------------------------------------------------------------
// <copyright file="ARGeospatialCreatorOrigin.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Internal
{
    using System;

    using Google.XR.ARCoreExtensions.Internal;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

    /// <summary>
    /// Provides a Geospatial Creator Origin that has both a lat/lon and gamespace coordinates. This is
    /// the reference point used by AR Anchors made in the Geospatial Creator to resolve their
    /// location in gamespace.
    /// </summary>
#if ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
    [AddComponentMenu("XR/Geospatial Creator/AR Geospatial Creator Origin")]
#endif
    public class ARGeospatialCreatorOrigin : ARGeospatialCreatorObject
    {
#if UNITY_EDITOR
        // Updated in the Editor assembly from the GeospatialOriginUpdater. Can be null if there's
        // no origin specified.
        internal GeoCoordinate OriginPoint;
#endif
    }
}

