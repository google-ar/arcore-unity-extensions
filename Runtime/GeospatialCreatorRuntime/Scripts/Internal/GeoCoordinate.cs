//-----------------------------------------------------------------------
// <copyright file="GeoCoordinate.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary> Immutable class representing a specific lat/lon/altitude.</summary>
    internal sealed class GeoCoordinate
    {
        private double _latitude;
        private double _longitude;
        private double _altitude;

        public GeoCoordinate(double latitude, double longitude, double altitude)
        {
            _latitude = latitude;
            _longitude = longitude;
            _altitude = altitude;
        }

        public double Latitude
        {
            get { return _latitude; }
        }

        public double Longitude
        {
            get { return _longitude; }
        }

        public double Altitude
        {
            get { return _altitude; }
        }
    }
}

