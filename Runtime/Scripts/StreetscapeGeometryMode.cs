//-----------------------------------------------------------------------
// <copyright file="StreetscapeGeometryMode.cs" company="Google LLC">
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
    /// Describes the desired behavior of the Geospatial Streetscape Geometry API.
    /// The Streetscape Geometry API provides polygon meshes of terrain, buildings,
    /// and other structures in a radius surrounding the device. See the <a
    /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/streetscape-geometry">Streetscape
    /// Geometry Developer Guide</a> for additional information.
    ///
    /// When Streetscape Geometry is enabled, <c><see cref="ARStreetscapeGeometryManager"/></c>
    /// can be used.
    ///
    /// The Streetscape Geometry API requires both
    /// <c><see cref="StreetscapeGeometryMode"/></c> to be set to
    /// <c><see cref="StreetscapeGeometryMode.Enabled"/></c> and
    /// <c><see cref="GeospatialMode"/></c> to be set to
    /// <c><see cref="GeospatialMode.Enabled"/></c>.
    ///
    /// The default value is <c><see cref="StreetscapeGeometryMode.Disabled"/></c>. Use
    /// <c><see cref="ARCoreExtensionsConfig.StreetscapeGeometryMode"/></c> to set the desired mode.
    /// </summary>
    public enum StreetscapeGeometryMode
    {
        /// <summary>
        /// The Streetscape Geometry API is disabled.
        ///
        /// This is the default mode.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// The Streetscape Geometry API is enabled.
        /// <c><see cref="ARStreetscapeGeometryManager"/></c> can be used.
        ///
        /// Use
        /// <c><see cref="ARCoreExtensionsConfig.StreetscapeGeometryMode"/></c> to set this mode.
        /// </summary>
        Enabled = 1,
    }
}
