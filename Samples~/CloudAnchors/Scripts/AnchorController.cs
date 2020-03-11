//-----------------------------------------------------------------------
// <copyright file="AnchorController.cs" company="Google">
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

namespace Google.XR.ARCoreExtensions.Samples.CloudAnchors
{
    using Google.XR.ARCoreExtensions;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// A Controller for the Anchor object that handles hosting and resolving the
    /// <see cref="ARCloudAnchor"/>.
    /// </summary>
#pragma warning disable 618
    public class AnchorController : NetworkBehaviour
#pragma warning restore 618
    {
        /// <summary>
        /// The customized timeout duration for resolving request to prevent retrying to resolve
        /// indefinitely.
        /// </summary>
        private const float k_ResolvingTimeout = 10.0f;

        /// <summary>
        /// The Cloud Anchor ID for the hosted anchor's <see cref="ARCloudAnchor"/>.
        /// This variable will be synchronized over all clients.
        /// </summary>
#pragma warning disable 618
        [SyncVar(hook = "_OnChangeId")]
#pragma warning restore 618
        private string m_ClouAnchorId = string.Empty;

        /// <summary>
        /// Indicates whether this script is running in the Host.
        /// </summary>
        private bool m_IsHost = false;

        /// <summary>
        /// Indicates whether an attempt to resolve the Cloud Anchor should be made.
        /// </summary>
        private bool m_ShouldResolve = false;

        /// <summary>
        /// Indicates whether to chekc Cloud Anchor state and update the anchor.
        /// </summary>
        private bool m_ShouldUpdatePoint = false;

        /// <summary>
        /// Record the time since resolving started. If the timeout has passed, display
        /// additional instructions.
        /// </summary>
        private float m_TimeSinceStartResolving = 0.0f;

        /// <summary>
        /// Indicates whether passes the resolving timeout duration or the anchor has been
        /// successfully resolved.
        /// </summary>
        private bool m_PassedResolvingTimeout = false;

        /// <summary>
        /// The anchor mesh object.
        /// In order to avoid placing the Anchor on identity pose, the mesh object should
        /// be disabled by default and enabled after hosted or resolved.
        /// </summary>
        private GameObject m_AnchorMesh;

        /// <summary>
        /// The Cloud Anchor created locally which is used to moniter whether the
        /// hosting or resolving process finished.
        /// </summary>
        private ARCloudAnchor m_CloudAnchor;

        /// <summary>
        /// The Cloud Anchors example controller.
        /// </summary>
        private CloudAnchorsExampleController m_CloudAnchorsExampleController;

        /// <summary>
        /// The AR Anchor Manager in the scene, used to host or resolve a Cloud Anchor.
        /// </summary>
        private ARAnchorManager m_AnchorManager;

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            m_CloudAnchorsExampleController =
                GameObject.Find("CloudAnchorsExampleController")
                .GetComponent<CloudAnchorsExampleController>();
            m_AnchorManager = m_CloudAnchorsExampleController.AnchorManager;
            m_AnchorMesh = transform.Find("AnchorMesh").gameObject;
            m_AnchorMesh.SetActive(false);
        }

        /// <summary>
        /// The Unity OnStartClient() method.
        /// </summary>
        public override void OnStartClient()
        {
            if (m_ClouAnchorId != string.Empty)
            {
                m_ShouldResolve = true;
            }
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (m_IsHost)
            {
                if (m_ShouldUpdatePoint)
                {
                    _UpdateHostedCloudAnchor();
                }
            }
            else
            {
                if (m_ShouldResolve)
                {
                    if (!m_CloudAnchorsExampleController.IsResolvingPrepareTimePassed())
                    {
                        return;
                    }

                    if (!m_PassedResolvingTimeout)
                    {
                        m_TimeSinceStartResolving += Time.deltaTime;

                        if (m_TimeSinceStartResolving > k_ResolvingTimeout)
                        {
                            m_PassedResolvingTimeout = true;
                            m_CloudAnchorsExampleController.OnResolvingTimeoutPassed();
                        }
                    }

                    if (!string.IsNullOrEmpty(m_ClouAnchorId) && m_CloudAnchor == null)
                    {
                        _ResolveCloudAnchorId(m_ClouAnchorId);
                    }
                }

                if (m_ShouldUpdatePoint)
                {
                    _UpdateResolvedCloudAnchor();
                }
            }
        }

        /// <summary>
        /// Command run on the server to set the Cloud Anchor Id.
        /// </summary>
        /// <param name="cloudAnchorId">The new Cloud Anchor Id.</param>
#pragma warning disable 618
        [Command]
#pragma warning restore 618
        public void CmdSetCloudAnchorId(string cloudAnchorId)
        {
            Debug.Log("Update Cloud Anchor Id with: " + cloudAnchorId);
            m_ClouAnchorId = cloudAnchorId;
        }

        /// <summary>
        /// Hosts the user placed cloud anchor and associates the resulting Id with this object.
        /// </summary>
        /// <param name="anchor">The last placed anchor.</param>
        public void HostAnchor(ARAnchor anchor)
        {
            m_IsHost = true;
            m_ShouldResolve = false;
            transform.SetParent(anchor.transform);
            m_AnchorMesh.SetActive(true);

            m_CloudAnchor = m_AnchorManager.HostCloudAnchor(anchor);
            if (m_CloudAnchor == null)
            {
                Debug.LogError("Failed to add Cloud Anchor.");
                m_CloudAnchorsExampleController.OnAnchorHosted(
                    false, "Failed to add Cloud Anchor.");
                m_ShouldUpdatePoint = false;
            }
            else
            {
                m_ShouldUpdatePoint = true;
            }
        }

        /// <summary>
        /// Resolves the Cloud Anchor Id and instantiate a Cloud Anchor on it.
        /// </summary>
        /// <param name="cloudAnchorId">The Cloud Anchor Id to be resolved.</param>
        private void _ResolveCloudAnchorId(string cloudAnchorId)
        {
            m_CloudAnchorsExampleController.OnAnchorInstantiated(false);
            m_CloudAnchor = m_AnchorManager.ResolveCloudAnchorId(cloudAnchorId);
            if (m_CloudAnchor == null)
            {
                Debug.LogErrorFormat("Client could not resolve Cloud Anchor {0}.", cloudAnchorId);
                m_CloudAnchorsExampleController.OnAnchorResolved(
                    false, "Client could not resolve Cloud Anchor.");
                m_ShouldResolve = true;
                m_ShouldUpdatePoint = false;
            }
            else
            {
                m_ShouldResolve = false;
                m_ShouldUpdatePoint = true;
            }
        }

        /// <summary>
        /// Update the anchor if hosting Cloud Anchor is success.
        /// </summary>
        private void _UpdateHostedCloudAnchor()
        {
            if (m_CloudAnchor == null)
            {
                Debug.LogError("No Cloud Anchor.");
                return;
            }

            CloudAnchorState cloudAnchorState = m_CloudAnchor.cloudAnchorState;
            if (cloudAnchorState == CloudAnchorState.Success)
            {
                CmdSetCloudAnchorId(m_CloudAnchor.cloudAnchorId);
                m_CloudAnchorsExampleController.OnAnchorHosted(
                    true, "Successfully hosted Cloud Anchor.");
                m_ShouldUpdatePoint = false;
            }
            else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
            {
                m_CloudAnchorsExampleController.OnAnchorHosted(false,
                    "Fail to host Cloud Anchor with state: " + cloudAnchorState);
                m_ShouldUpdatePoint = false;
            }
        }

        /// <summary>
        /// Update the anchor if resolving Cloud Anchor is success.
        /// </summary>
        private void _UpdateResolvedCloudAnchor()
        {
            if (m_CloudAnchor == null)
            {
                Debug.LogError("No Cloud Anchor.");
                return;
            }

            CloudAnchorState cloudAnchorState = m_CloudAnchor.cloudAnchorState;
            if (cloudAnchorState == CloudAnchorState.Success)
            {
                transform.SetParent(m_CloudAnchor.transform, false);
                m_CloudAnchorsExampleController.OnAnchorResolved(
                    true,
                    "Successfully resolved Cloud Anchor.");
                m_CloudAnchorsExampleController.WorldOrigin = transform;
                m_AnchorMesh.SetActive(true);

                // Mark resolving timeout passed so it won't fire OnResolvingTimeoutPassed event.
                m_PassedResolvingTimeout = true;
                m_ShouldUpdatePoint = false;
            }
            else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
            {
                m_CloudAnchorsExampleController.OnAnchorResolved(
                    false, "Fail to resolve Cloud Anchor with state: " + cloudAnchorState);
                m_ShouldUpdatePoint = false;
            }
        }

        /// <summary>
        /// Callback invoked once the Cloud Anchor Id changes.
        /// </summary>
        /// <param name="newId">New Cloud Anchor Id.</param>
        private void _OnChangeId(string newId)
        {
            if (!m_IsHost && newId != string.Empty)
            {
                m_ClouAnchorId = newId;
                m_ShouldResolve = true;
                m_CloudAnchor = null;
            }
        }
    }
}
