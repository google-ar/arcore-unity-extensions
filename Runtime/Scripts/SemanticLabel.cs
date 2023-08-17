//-----------------------------------------------------------------------
// <copyright file="SemanticLabel.cs" company="Google LLC">
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
    /// Defines the labels the Scene Semantics API is able to detect and maps human-readable names
    /// to per-pixel semantic labels. See the <a
    /// href="https://developers.google.com/ar/develop/unity-arf/scene-semantics">Scene
    /// Semantics Developer Guide</a> for more information.
    ///
    /// Use <c><see cref="ARSemanticManager.TryGetSemanticTexture(ref Texture2D)"/></c> to obtain an
    /// image containing these pixels and
    /// <c><see cref="ARSemanticManager.TryGetSemanticConfidenceTexture(ref Texture2D)"/></c>,
    /// and <c><see cref="ARSemanticManager.GetSemanticLabelFraction(SemanticLabel)"/></c>
    /// to query what percentage of the image contains these pixels.
    /// </summary>
    public enum SemanticLabel
    {
        /// <summary>
        /// Pixels with no semantic label available in the API output.
        /// </summary>
        Unlabeled = 0,

        /// <summary>
        /// Pixels of the open sky, including clouds. Thin electrical wires in front
        /// of the sky are included, but leaves/vegetation are not included.
        /// </summary>
        Sky = 1,

        /// <summary>
        /// Pixels of buildings, including houses, garages, etc. Includes all
        /// structures attached to the building, such as signs, solar panels,
        /// scaffolding, etc.
        /// </summary>
        Building = 2,

        /// <summary>
        /// Pixels of non-walkable vegetation, like trees and shrubs. In contrast,
        /// 'terrain' specifies walkable vegetation, like grass.
        /// </summary>
        Tree = 3,

        /// <summary>
        /// Pixels of drivable surfaces for vehicles, including paved, unpaved, dirt,
        /// driveways, crosswalks, etc.
        /// </summary>
        Road = 4,

        /// <summary>
        /// Pixels of sidewalks for pedestrians and cyclists, including associated
        /// curbs.
        /// </summary>
        Sidewalk = 5,

        /// <summary>
        /// Pixels of walkable vegetation areas, including grass, soil, sand,
        /// mountains, etc. In contrast, 'tree' specifies non-walkable vegetation,
        /// like trees and bushes.
        /// </summary>
        Terrain = 6,

        /// <summary>
        /// Pixels of structures that are not buildings, including fences, guardrails,
        /// stand-alone walls, tunnels, bridges, etc.
        /// </summary>
        Structure = 7,

        /// <summary>
        /// Pixels of general temporary and permanent objects and obstacles, including
        /// street signs, traffic signs, free-standing business signs, billboards,
        /// poles, mailboxes, fire hydrants, street lights, phone booths, bus stop
        /// enclosures, cones, parking meters, animals, etc.
        /// </summary>
        Object = 8,

        /// <summary>
        /// Pixels of vehicles, including cars, vans, buses, trucks, motorcycles,
        /// bicycles, trains, etc.
        /// </summary>
        Vehicle = 9,

        /// <summary>
        /// Pixels of humans, including pedestrians and bicycle/motorcycle riders.
        /// </summary>
        Person = 10,

        /// <summary>
        /// Pixels of ground surfaces covered by water, including lakes, rivers, etc.
        /// </summary>
        Water = 11,
    }
}
