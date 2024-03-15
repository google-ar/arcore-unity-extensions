//-----------------------------------------------------------------------
// <copyright file="AnchorAltitudeType.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.GeospatialCreator
{
    /// <summary>
    /// Specifies how the <c><see cref="ARGeospatialCreatorAnchor"/></c>'s
    /// <c><see cref="ARGeospatialCreatorAnchor.Altitude"/></c> and
    /// <c><see cref="ARGeospatialCreatorAnchor.EditorAltitudeOverride"/></c> properties will be
    /// interpreted.</summary>
    public enum AnchorAltitudeType
    {
        /// <summary>
        /// The anchor represents a <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/anchors#wgs84_anchors">WGS84
        /// anchor</a>.
        /// The anchor's <c><see cref="ARGeospatialCreatorAnchor.Altitude"/></c> specifies the
        /// altitude of the anchor in meters for WGS84.
        ///
        /// <c><see cref="ARGeospatialCreatorAnchor.EditorAltitudeOverride"/></c> is not used.
        /// </summary>
        WGS84,

        /// <summary>
        /// The anchor represents a <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/anchors#terrain_anchors">Terrain
        /// anchor</a>.
        /// The anchor's <c><see cref="ARGeospatialCreatorAnchor.Altitude"/></c> specifies the
        /// relative altitude above or below the terrain, in meters.
        ///
        /// If the anchor does not appear to render at the correct height in the Editor, set the
        /// anchor's
        /// <c><see cref="ARGeospatialCreatorAnchor.UseEditorAltitudeOverride"/></c> property to
        /// <c>true</c> and use
        /// <c><see cref="ARGeospatialCreatorAnchor.EditorAltitudeOverride"/></c>
        /// to adjust the anchor's visual altitude in the Editor. At runtime, the anchor will ignore
        /// these values and use an altitude relative to the terrain at that anchor's horizontal
        /// location.
        /// </summary>
        Terrain,

        /// <summary>
        /// The anchor represents a <a
        /// href="https://developers.google.com/ar/develop/unity-arf/geospatial/anchors#rooftop_anchors">Rooftop
        /// anchor</a>.
        /// The anchor's <c><see cref="ARGeospatialCreatorAnchor.Altitude"/></c> specifies the
        /// relative altitude above or below to a rooftop at that anchor's horizontal location, in
        /// meters.
        ///
        /// If the anchor does not appear to render at the correct height in the Editor, set the
        /// anchor's
        /// <c><see cref="ARGeospatialCreatorAnchor.UseEditorAltitudeOverride"/></c> property to
        /// <c>true</c> and use
        /// <c><see cref="ARGeospatialCreatorAnchor.EditorAltitudeOverride"/></c>
        /// to adjust the anchor's visual altitude in the Editor. At runtime, the anchor will ignore
        /// these values and use an altitude relative to a rooftop at that anchor's horizontal
        /// location.
        /// </summary>
        Rooftop
    }
}
