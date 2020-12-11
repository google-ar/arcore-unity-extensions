//-----------------------------------------------------------------------
// <copyright file="EnumFlagsAttributeDrawer.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
    using System;
    using Google.XR.ARCoreExtensions.Internal;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// EnumFlagsAttribute drawer that draws a mask field and calculate the int value based on
    /// current enum values.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        /// <summary>
        /// Override Unity OnGUI to make a custom GUI for the property with EnumFlagsAttribute.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnumFlagsAttribute flagsAttribute = attribute as EnumFlagsAttribute;

            string[] itemNames = Enum.GetNames(flagsAttribute.EnumType);
            int[] itemValues = Enum.GetValues(flagsAttribute.EnumType) as int[];

            property.intValue =
                 EditorGUI.MaskField(position, label, property.intValue, itemNames);

            if (property.intValue == -1)
            {
                int maskValue = 0;
                foreach (int itemValue in itemValues)
                {
                    maskValue |= itemValue;
                }

                property.intValue = maskValue;
            }
        }
    }
}
