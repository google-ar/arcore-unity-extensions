//-----------------------------------------------------------------------
// <copyright file="GeoTilesReference.cs" company="Google LLC">
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

#if UNITY_2021_3_OR_NEWER

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Internal
{
    using System;
#if ARCORE_INTERNAL_USE_CESIUM
    using CesiumForUnity;
#endif
#if ARCORE_INTERNAL_USE_UNITY_MATH
    using Unity.Mathematics;
#endif
    using UnityEngine;

    public class GeoTilesReferencePoint
    {
        public double Latitude;
        public double Longitude;
        public double Height;

        public GeoTilesReferencePoint()
        {
            Latitude = 0;
            Longitude = 0;
            Height = 0;
        }

        public GeoTilesReferencePoint(double latitude, double longitude, double height)
        {
            Latitude = latitude;
            Longitude = longitude;
            Height = height;
        }
    }

    public class GeoTilesReference
    {
#if ARCORE_INTERNAL_USE_CESIUM
        private CesiumGeoreference geoReference = null;
#endif

        public GeoTilesReference(GameObject gameObject)
        {
#if ARCORE_INTERNAL_USE_CESIUM
            geoReference = GetReference(gameObject);
#endif
        }

        public GeoTilesReferencePoint GetTilesReferencePoint(GameObject gameObject)
        {
#if ARCORE_INTERNAL_USE_CESIUM
            if (geoReference == null)
            {
                geoReference = GetReference(gameObject);
                if (geoReference == null)
                {
                    // higher level code can use cached values if they have one.
                    return null;
                }
            }
            GeoTilesReferencePoint RefPoint = new GeoTilesReferencePoint(
                geoReference.latitude,
                geoReference.longitude,
                geoReference.height
            );
            return RefPoint;
#else
            throw new Exception("Missing dependency: Cesium 1.0.0+ not available.");
#endif
        }

        public void SetGameLayer(GameObject gameObject, int _layer)
        {
#if ARCORE_INTERNAL_USE_CESIUM
            if (geoReference == null)
            {
                geoReference = GetReference(gameObject);
                if (geoReference == null)
                {
                    return;
                }
            }
            SetGameLayerRecursive(geoReference.gameObject, _layer);
#endif
        }

#if ARCORE_INTERNAL_USE_CESIUM
        private void SetGameLayerRecursive(GameObject _go, int _layer)
        {
            _go.layer = _layer;
            foreach (Transform child in _go.transform)
            {
                child.gameObject.layer = _layer;

                Transform _HasChildren = child.GetComponentInChildren<Transform>();
                if (_HasChildren != null)
                    SetGameLayerRecursive(child.gameObject, _layer);
            }
        }

        private CesiumGeoreference GetReference(GameObject gameObject)
        {
            // First check parent, then global, then error out.
            // :TODO b/276777888 & b/268498440 to make this more flexible
            CesiumGeoreference geoRef = gameObject.GetComponentInParent<CesiumGeoreference>();
            if (geoRef == null)
            {
                geoRef = FindFirstGeoreference();
            }
            if (geoRef == null)
            {
                throw new InvalidOperationException(
                    "Can't find CesiumGeoreference used to calculate latitude and longitude");
            }
            return geoRef;
        }

        private CesiumGeoreference FindFirstGeoreference()
        {
            CesiumForUnity.CesiumGeoreference[] georeferences =
                UnityEngine.Object.FindObjectsOfType<CesiumGeoreference>(true);
            for (int i = 0; i < georeferences.Length; i++)
            {
                CesiumGeoreference georeference = georeferences[i];
                if (georeference != null)
                {
                    return georeference;
                }
            }
            return null;
        }
#endif
    }

    public class CesiumWgs84Ellipsoid
    {
        public static double3 EarthCenteredEarthFixedToLongitudeLatitudeHeight(double3 ecef)
        {
#if ARCORE_INTERNAL_USE_CESIUM && ARCORE_INTERNAL_USE_UNITY_MATH
            return CesiumForUnity.CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(
                ecef
            );
#else
            throw new Exception("Missing dependencies: Cesium 1.0.0+ and/or com.unity.mathematics 1.2.0+ not available.");
#endif
        }
    }
}

#endif // UNITY_X_OR_NEWER
