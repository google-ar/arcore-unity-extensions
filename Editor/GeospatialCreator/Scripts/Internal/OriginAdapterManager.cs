//-----------------------------------------------------------------------
// <copyright file="OriginAdapterManager.cs" company="Google LLC">
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
    /// Manages provider-specific adapters for ARGeospatialCreatorOrigin objects. Geospatial
    /// Creator currently only supports Cesium as the tile provider and origin point provider, so
    /// this class ensures that the appropriate Cesium adapters are always assigned to the origin,
    /// which achieves two goals: 1) the origin's GeoCoordinate point is always in sync with the
    /// CesiumGeoreference location; and 2) there is a valid interface to the 3d tileset
    /// abstraction.
    /// <p>
    /// This class is used instead of ARGeospatialCreatorOrigin's Update() method to avoid requiring
    /// the Cesium dependency in the GeospatialCreator's runtime assembly references.
    /// </summary>
    [InitializeOnLoad]
    internal class OriginAdapterManager
    {
        private static readonly OriginAdapterManager _instance;

        // Using a singleton class with `InitializeOnLoad` ensures exactly one event handler is
        // active at all times.
        static OriginAdapterManager()
        {
            _instance = new OriginAdapterManager();
        }

        private OriginAdapterManager()
        {
            AddAdaptersForNewOrigins();
            EditorApplication.hierarchyChanged += AddAdaptersForNewOrigins;
        }

        ~OriginAdapterManager()
        {
            EditorApplication.hierarchyChanged -= AddAdaptersForNewOrigins;
        }

        // Adds adapters to any origin in the scene that does not have them. This is called
        // whenever the scene hierarchy changes, so it will always be executed when a new
        // GameObject or component is added.
        public void AddAdaptersForNewOrigins()
        {
            ARGeospatialCreatorOrigin[] allOrigins =
                GameObject.FindObjectsOfType<ARGeospatialCreatorOrigin>();

            foreach (ARGeospatialCreatorOrigin origin in allOrigins)
            {
#if ARCORE_INTERNAL_USE_CESIUM
                if (origin._originComponentAdapter == null)
                {
                    origin._originComponentAdapter =
                        new GeospatialCreatorCesiumAdapter.OriginComponentCesiumAdapter(origin);
                    origin.UpdateOriginFromComponent();
                }

                if (origin._origin3DTilesetAdapter == null)
                {
                    origin._origin3DTilesetAdapter =
                        new GeospatialCreatorCesiumAdapter.Origin3DTilesetCesiumAdapter(origin);
                }
#endif
            }
        }
    }
}
#endif // ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
