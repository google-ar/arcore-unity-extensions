//-----------------------------------------------------------------------
// <copyright file="GeospatialCreatorMenuUtils.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Editor.Internal
{
    using System;
    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
    using UnityEditor;
    using UnityEngine;

    internal static class GeospatialCreatorMenuUtils
    {
        private static int _createdAnchorCount = 1;

#if ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
        [MenuItem("GameObject/XR/AR Geospatial Creator Origin", false, 30)]
#endif
        private static void CreateOrigin(MenuCommand menuCommand)
        {
            GameObject origin = CreateObject(
                menuCommand,
                "AR Geospatial Creator Origin",
                typeof(ARGeospatialCreatorOrigin));
            origin.tag = "EditorOnly";
        }

#if ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
        [MenuItem("GameObject/XR/AR Geospatial Creator Anchor", false, 40)]
#endif
        private static void CreateAnchor(MenuCommand menuCommand)
        {
            CreateObject(
                menuCommand,
                $"AR Geospatial Creator Anchor {_createdAnchorCount}",
                typeof(ARGeospatialCreatorAnchor));
            _createdAnchorCount++;
        }

        private static GameObject CreateObject(
            MenuCommand menuCommand,
            string name,
            params Type[] types)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject(name, types);
            GameObjectUtility.EnsureUniqueNameForSibling(gameObject);
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);

            // Make Undo work and make the object the one shown in the inspector
            Undo.RegisterCompleteObjectUndo(gameObject, $"Create Object: {gameObject.name}");
            Selection.activeGameObject = gameObject;

            return gameObject;
        }
    }
}
