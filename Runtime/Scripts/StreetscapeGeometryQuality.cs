//-----------------------------------------------------------------------
// <copyright file="StreetscapeGeometryQuality.cs" company="Google LLC">
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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Describes the quality of the mesh data. The values correspond to the levels
    /// of detail (LOD) defined by the <a
    /// href="https://portal.ogc.org/files/?artifact_id=16675">CityGML 2.0 standard</a>.
    ///
    /// Obtained by <c><see cref="ARStreetscapeGeometry.quality"/></c>.
    /// </summary>
    public enum StreetscapeGeometryQuality
    {
        /// <summary>
        /// The quality of the geometry is not defined, e.g. when the
        /// <c><see cref="StreetscapeGeometryType"/></c> is
        /// <c><see cref="StreetscapeGeometryType.Terrain"/></c>.
        /// </summary>
        None = 0,

        /// <summary>
        /// The <c><see cref="StreetscapeGeometryType.Building"/></c> geometry is the building
        /// footprint extruded up to a single flat top. The building contains empty
        /// space above any angled roofs.
        /// </summary>
        BuildingLOD1 = 1,

        /// <summary>
        /// The <c><see cref="StreetscapeGeometryType.Building"/></c> geometry is the building
        /// footprint with rough heightmap. The geometry will closely follow simple
        /// angled roofs. Chimneys and roof vents on top of roofs will poke outside
        /// of the mesh.
        /// </summary>
        BuildingLOD2 = 2,
    }
}
