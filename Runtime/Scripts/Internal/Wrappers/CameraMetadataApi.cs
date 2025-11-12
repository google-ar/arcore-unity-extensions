//-----------------------------------------------------------------------
// <copyright file="CameraMetadataApi.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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

namespace Google.XR.ARCoreExtensions.Internal
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#endif

    using Marshal = System.Runtime.InteropServices.Marshal;

    internal class CameraMetadataApi
    {
        private const int _maximumTagCountForWarning = 5000;
        private static HashSet<int> _warningTags = new HashSet<int>();

        public static void Release(IntPtr arCameraMetadataHandle)
        {
#if UNITY_ANDROID
            ExternApi.ArImageMetadata_release(arCameraMetadataHandle);
#endif // UNITY_ANDROID
        }

        public static bool TryGetValues(IntPtr sessionHandle, IntPtr cameraMetadataHandle,
            CameraMetadataTag tag, List<CameraMetadataValue> resultList)
        {
            resultList.Clear();
#if UNITY_ANDROID
            uint utag = (uint)tag;
            ArCameraMetadata entry = new ArCameraMetadata();
            ApiArStatus status =
                ExternApi.ArImageMetadata_getConstEntry(sessionHandle,
                                                        cameraMetadataHandle, utag,
                                                        ref entry);
            if (status != ApiArStatus.Success)
            {
                ARDebug.LogErrorFormat(
                    "ArImageMetadata_getConstEntry error with native camera error code: {0}",
                    status);
                return false;
            }

            if (entry.Count > _maximumTagCountForWarning && !_warningTags.Contains((int)tag))
            {
                Debug.LogWarningFormat(
                    "TryGetValues for tag {0} has {1} values. Accessing tags with a large " +
                    "number of values may impede performance.", tag, entry.Count);
                _warningTags.Add((int)tag);
            }

            for (int i = 0; i < entry.Count; i++)
            {
                switch (entry.Type)
                {
                    case ArCameraMetadataType.Byte:
                        sbyte byteValue = (sbyte)Marshal.PtrToStructure(
                            MarshalingHelper.GetPtrToUnmanagedArrayElement<sbyte>(entry.Value, i),
                            typeof(sbyte));
                        resultList.Add(new CameraMetadataValue(byteValue));
                        break;
                    case ArCameraMetadataType.Int32:
                        int intValue = (int)Marshal.PtrToStructure(
                            MarshalingHelper.GetPtrToUnmanagedArrayElement<int>(entry.Value, i),
                            typeof(int));
                        resultList.Add(new CameraMetadataValue(intValue));
                        break;
                    case ArCameraMetadataType.Float:
                        float floatValue = (float)Marshal.PtrToStructure(
                            MarshalingHelper.GetPtrToUnmanagedArrayElement<float>(entry.Value, i),
                            typeof(float));
                        resultList.Add(new CameraMetadataValue(floatValue));
                        break;
                    case ArCameraMetadataType.Int64:
                        long longValue = (long)Marshal.PtrToStructure(
                            MarshalingHelper.GetPtrToUnmanagedArrayElement<long>(entry.Value, i),
                            typeof(long));
                        resultList.Add(new CameraMetadataValue(longValue));
                        break;
                    case ArCameraMetadataType.Double:
                        double doubleValue = (double)Marshal.PtrToStructure(
                            MarshalingHelper.GetPtrToUnmanagedArrayElement<double>(entry.Value, i),
                            typeof(double));
                        resultList.Add(new CameraMetadataValue(doubleValue));
                        break;
                    case ArCameraMetadataType.Rational:
                        CameraMetadataRational rationalValue =
                            (CameraMetadataRational)Marshal.PtrToStructure(
                                MarshalingHelper
                                .GetPtrToUnmanagedArrayElement<CameraMetadataRational>(
                                    entry.Value, i),
                                typeof(CameraMetadataRational));
                        resultList.Add(new CameraMetadataValue(rationalValue));
                        break;
                    default:
                        return false;
                }
            }

            return true;
#else
            return false;
#endif // UNITY_ANDROID
        }

        public static bool GetAllCameraMetadataTags(
            IntPtr sessionHandle, IntPtr cameraMetadataHandle, List<CameraMetadataTag> resultList)
        {
#if UNITY_ANDROID
            IntPtr tagsHandle = IntPtr.Zero;
            int tagsCount = 0;
            ExternApi.ArImageMetadata_getAllKeys(
                sessionHandle, cameraMetadataHandle, ref tagsCount, ref tagsHandle);
            if (tagsCount == 0 || tagsHandle == IntPtr.Zero)
            {
                ARDebug.LogError(
                    "ArImageMetadata_getAllKeys error with empty metadata keys list.");
                return false;
            }

            for (int i = 0; i < tagsCount; i++)
            {
                resultList.Add((CameraMetadataTag)Marshal.PtrToStructure(
                    MarshalingHelper.GetPtrToUnmanagedArrayElement<int>(tagsHandle, i),
                    typeof(int)));
            }

            return true;
#else
            return false;
#endif // UNITY_ANDROID
        }

        private struct ExternApi
        {
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImageMetadata_release(IntPtr metadata);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImageMetadata_getAllKeys(
                IntPtr session, IntPtr image_metadata, ref int out_number_of_tags,
                ref IntPtr out_tags);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArImageMetadata_getConstEntry(
                IntPtr session, IntPtr image_metadata, uint tag,
                ref ArCameraMetadata out_metadata_entry);
        }
    }
}
