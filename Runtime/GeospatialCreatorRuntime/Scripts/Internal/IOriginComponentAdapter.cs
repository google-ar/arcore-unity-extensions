//-----------------------------------------------------------------------
// <copyright file="IOriginComponentAdapter.cs" company="Google LLC">
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
    using UnityEngine;

    // Used when an ARGeospatialCreatorOrigin's location is determined by or maintained within a
    // subcomponent. For example, the default way to configure an origin is to add a
    // CesiumGeoreference as a subcomponent, and couple the lat/lon/alt of the Origin object to
    // that of the CesiumGeoreference subcomponent. The adapter pattern allows us to avoid
    // exposing the implementation details of the CesiumGeoreference to the
    // ARGeospatialCreatorOrigin class.
    internal interface IOriginComponentAdapter
    {
        // Retrieves the origin location from the subcomponent. Could be null if the subcomponent
        // does not exist or if the origin is not defined.
        GeoCoordinate GetOriginFromComponent();

        // Sets the origin location in the subcomponent. This is invoked by
        // ARGeospatialCreatorOrigin#SetOriginPoint to ensure the origin and its coupled
        // subcomponent stay in sync.
        void SetComponentOrigin(GeoCoordinate newOrigin);
    }
}
