//-----------------------------------------------------------------------
// <copyright file="IOSPostProcessBuild.cs" company="Google LLC">
//
// Copyright 2026 Google LLC
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
    using System.IO;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEditor.iOS.Xcode;
    using UnityEngine;

    /// <summary>
    /// Internal script to perform iOS build post-processing.
    /// Adds -ObjC linker flag which is required for Swift Package Manager based builds
    /// to correctly link static libraries with categories.
    /// </summary>
    public static class IOSPostProcessBuild
    {
        /// <summary>
        /// Callback method to be executed after the build process is complete.
        /// Executes after other post-process steps (e.g. EDM4U runs around 40-50).
        /// </summary>
        /// <param name="buildTarget">The build target.</param>
        /// <param name="pathToBuiltProject">The path to the built project.</param>
        [PostProcessBuild(999)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS)
            {
                return;
            }

            Debug.Log("ARCore Extensions: Adding -ObjC flag to Other Linker Flags...");

            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            string targetGuid = project.GetUnityFrameworkTargetGuid();

            // Add -ObjC to ensure categories in static libraries are linked.
            project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");

            project.WriteToFile(projectPath);
        }
    }
}
