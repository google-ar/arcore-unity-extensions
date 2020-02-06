//-----------------------------------------------------------------------
// <copyright file="SceneUtils.cs" company="Google">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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

namespace Google.XR.ARCoreExtensions.Editor.Internal
{
    using UnityEditor;

    internal static class SceneUtils
    {
        [MenuItem("GameObject/XR/ARCore Extensions", false, 10)]
        private static void CreateARCoreExtensions()
        {
            ObjectFactory.CreateGameObject("ARCore Extensions", typeof(ARCoreExtensions));
        }
    }
}
