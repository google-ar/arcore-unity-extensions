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

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Internal
{
    /// <summary>
    /// Specifies how the ARGeospatialCreatorAnchor's Altitude and AlttudeOffset fields will be
    /// interpreted.</summary>
    public enum AnchorAltitudeType
    {
        /// <summary>
        /// Altitude specifies the altitude of the anchor in meters for WGS84. AltitudeOffset is
        /// not used.</summary>
        WGS84,

        /// <summary>
        /// Altitude specifies the relative altitude above/below the terrain, in meters. If the
        /// anchor does not appear to render at the correct height in the Editor, adjust the
        /// anchor's EditorAltitude property.</summary>
        Terrain,

        /// <summary> Altitude specifies the relative altitude above/below the rooftop, in meters.
        /// If the anchor does not appear to render at the correct height in the Editor, adjust the
        /// anchor's EditorAltitude property.</summary>
        Rooftop
    }
}
