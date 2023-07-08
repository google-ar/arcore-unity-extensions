//-----------------------------------------------------------------------
// <copyright file="GeospatialObjectTracker.cs" company="Google LLC">
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

#if ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED

namespace Google.XR.ARCoreExtensions.GeospatialCreator.Editor.Internal
{
    using System;
    using System.Collections.Generic;

    using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class to track specific ARGeospatialCreatorObject objects in the scene, so they can be
    /// assigned Editor-only Update() behaviors.
    /// </summary>
    /// <remarks>
    /// This is useful if the Editor-only behaviors rely on assemblies that we do not want included
    /// in the runtime. If there's no special dependencies required, the object's Update() method
    /// can be implemented with "#if UNITY_EDITOR ... #endif" guards instead.
    /// </remarks>
    internal class GeospatialObjectTracker<T> where T : ARGeospatialCreatorObject
    {
        private Func<T, Action> _updateActionFactory;
        private Dictionary<T, Action> _trackedObjects;
        private bool _isTracking;

        /// <summary>
        /// Create a GeospatialObjectTracker that will assign an Editor-specific Update behavior
        /// to all objects of type T in a scene.
        /// </summary>
        /// <param name="updateActionFactory">
        /// The factory function that returns an Update action for a given object of type T. The
        /// Action returned by the function will be invoked whenever the Editor calls the Update
        /// method on the given object.
        /// </param>
        public GeospatialObjectTracker(Func<T, Action> updateActionFactory)
        {
            _trackedObjects = new Dictionary<T, Action>();
            _updateActionFactory = updateActionFactory;
            _isTracking = false;
        }

        ~GeospatialObjectTracker()
        {
            if (_isTracking)
            {
                StopTracking();
            }
        }

        public void StartTracking()
        {
            if (_isTracking)
            {
                StopTracking();
            }

            UpdateTrackedObjects();
            EditorApplication.hierarchyChanged += UpdateTrackedObjects;
            _isTracking = true;
        }

        public void StopTracking()
        {
            EditorApplication.hierarchyChanged -= UpdateTrackedObjects;
            _isTracking = false;
            foreach (KeyValuePair<T, Action> entry in _trackedObjects)
            {
                entry.Key.OnEditorUpdate -= entry.Value;
            }

            _trackedObjects.Clear();
        }

        /// <summary>
        /// Handles EditorApplication.Update() events to check if GameObjects of type T have been
        /// added to or removed from the scene. For new objects, a a new update Action is created
        /// and added to that object's OnEditorUpdate delegates. For removed objects, any
        /// previously-assigned delegate is removed.
        /// </summary>
        private void UpdateTrackedObjects()
        {
            HashSet<T> currentObjects = new HashSet<T>(GameObject.FindObjectsOfType<T>());
            if (currentObjects.SetEquals(_trackedObjects.Keys))
            {
                // no added or removed objects, so there's nothing to do
                return;
            }

            HashSet<T> newObjects = new HashSet<T>(currentObjects);
            newObjects.ExceptWith(_trackedObjects.Keys);
            HashSet<T> removedObjects = new HashSet<T>(_trackedObjects.Keys);
            removedObjects.ExceptWith(currentObjects);

            foreach (T obj in newObjects)
            {
                Action updateAction = _updateActionFactory(obj);
                obj.OnEditorUpdate += updateAction;
                _trackedObjects.Add(obj, updateAction);
            }

            foreach (T obj in removedObjects)
            {
                Action updateAction;
                if (_trackedObjects.TryGetValue(obj, out updateAction))
                {
                    obj.OnEditorUpdate -= updateAction;
                    _trackedObjects.Remove(obj);
                }
            }
        }
    }
}
#endif // ARCORE_INTERNAL_GEOSPATIAL_CREATOR_ENABLED
