//-----------------------------------------------------------------------
// <copyright file="GeospatialOriginUpdater.cs" company="Google LLC">
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

#if ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Editor.Internal
{
    using System;
    using Google.XR.ARCoreExtensions.GeospatialCreator;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Handles Editor Updates for ARGeospatialCreatorOrigin objects. Specifically, it ensures the
    /// Origin's GeoCoordinate point is in sync with the CesiumGeoreference location. This class is
    /// used instead of implementing ARGeospatialCreatorOrigin's Update() method to avoid requiring
    /// the Cesium dependency in the GeospatialCreator's runtime assembly references.
    /// </summary>
    [InitializeOnLoad]
    internal class GeospatialOriginUpdater
    {
        private static GeospatialObjectTracker<ARGeospatialCreatorOrigin> tracker;

        private readonly ARGeospatialCreatorOrigin _origin;

        // Use a static initializer, plus the InitializeOnLoad attribute, to ensure objects in the
        // scene are always being tracked.
        static GeospatialOriginUpdater()
        {
            Func<ARGeospatialCreatorOrigin, Action> actionFactory = origin =>
                (new GeospatialOriginUpdater(origin)).EditorUpdate;

            var tracker = new GeospatialObjectTracker<ARGeospatialCreatorOrigin>(actionFactory);
            tracker.StartTracking();
        }

        public GeospatialOriginUpdater(ARGeospatialCreatorOrigin origin)
        {
            _origin = origin;
#if ARCORE_INTERNAL_USE_CESIUM
            _origin._originComponentAdapter =
                new GeospatialCreatorCesiumAdapter.OriginComponentCesiumAdapter(origin);
            _origin.UpdateOriginFromComponent();
            _origin._origin3DTilesetAdapter =
                new GeospatialCreatorCesiumAdapter.Origin3DTilesetCesiumAdapter(origin);
#endif
        }

        private void EditorUpdate()
        {
            _origin.UpdateOriginFromComponent();
        }

    }
}
#endif // ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
