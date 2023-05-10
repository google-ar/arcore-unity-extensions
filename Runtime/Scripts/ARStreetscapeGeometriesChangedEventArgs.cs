//-----------------------------------------------------------------------
// <copyright file="ARStreetscapeGeometriesChangedEventArgs.cs" company="Google LLC">
//
// Copyright 2022 Google LLC
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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Event arguments for the
    /// <c><see cref="ARStreetscapeGeometryManager.StreetscapeGeometriesChanged"/></c> event.
    /// </summary>
    public struct ARStreetscapeGeometriesChangedEventArgs
    {
        /// <summary>
        /// Constructs an <c><see cref="ARStreetscapeGeometriesChangedEventArgs"/></c>.
        /// </summary>
        /// <param name="added">The list of <c><see cref="ARStreetscapeGeometry"/></c> added since
        /// the last event.</param>
        /// <param name="updated">The list of <c><see cref="ARStreetscapeGeometry"/></c>
        /// updated since the last event.
        /// </param>
        /// <param name="removed">The list of <c><see cref="ARStreetscapeGeometry"/></c> removed
        /// since the last event.
        /// </param>
        public ARStreetscapeGeometriesChangedEventArgs(
            List<ARStreetscapeGeometry> added,
            List<ARStreetscapeGeometry> updated,
            List<ARStreetscapeGeometry> removed)
        {
            this.Added = added;
            this.Updated = updated;
            this.Removed = removed;
        }

        /// <summary>
        /// Gets the list of <c><see cref="ARStreetscapeGeometry"/></c>s added since the last event.
        /// </summary>
        public List<ARStreetscapeGeometry> Added { get; private set; }

        /// <summary>
        /// Gets the list of <c><see cref="ARStreetscapeGeometry"/></c>s updated since the last
        /// event.
        /// </summary>
        public List<ARStreetscapeGeometry> Updated { get; private set; }

        /// <summary>
        /// Gets the list of <c><see cref="ARStreetscapeGeometry"/></c>s removed since the last
        /// event.
        /// </summary>
        public List<ARStreetscapeGeometry> Removed { get; private set; }

        /// <summary>
        /// Generates a string representation of this <c><see
        /// cref="ARStreetscapeGeometriesChangedEventArgs"/></c>.
        /// </summary>
        /// <returns>A string representation of this <c><see
        /// cref="ARStreetscapeGeometriesChangedEventArgs"/></c>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Added: {0}, Updated: {1}, Removed: {2}",
                Added == null ? 0 : Added.Count,
                Updated == null ? 0 : Updated.Count,
                Removed == null ? 0 : Removed.Count);
        }
    }
}
