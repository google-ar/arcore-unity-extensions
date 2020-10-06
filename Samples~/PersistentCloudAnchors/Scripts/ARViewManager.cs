//-----------------------------------------------------------------------
// <copyright file="ARViewManager.cs" company="Google LLC">
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

namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// A manager component that helps with hosting and resolving Cloud Anchors.
    /// </summary>
    public class ARViewManager : MonoBehaviour
    {
        /// <summary>
        /// The main controller for Persistent Cloud Anchors sample.
        /// </summary>
        public PersistentCloudAnchorsController Controller;

        /// <summary>
        /// The 3D object that represents a Cloud Anchor.
        /// </summary>
        public GameObject CloudAnchorPrefab;

        /// <summary>
        /// The game object that includes <see cref="MapQualityIndicator"/> to visualize
        /// map quality result.
        /// </summary>
        public GameObject MapQualityIndicatorPrefab;

        /// <summary>
        /// The UI element that displays the instructions to guide hosting experience.
        /// </summary>
        public GameObject InstructionBar;

        /// <summary>
        /// The UI panel that allows the user to name the Cloud Anchor.
        /// </summary>
        public GameObject NamePanel;

        /// <summary>
        /// The UI element that displays warning message for invalid input name.
        /// </summary>
        public GameObject InputFieldWarning;

        /// <summary>
        /// The input field for naming Cloud Anchor.
        /// </summary>
        public InputField NameField;

        /// <summary>
        /// The instruction text in the top instruction bar.
        /// </summary>
        public Text InstructionText;

        /// <summary>
        /// The debug text in bottom snack bar.
        /// </summary>
        public Text DebugText;

        /// <summary>
        /// The button to save the typed name.
        /// </summary>
        public Button SaveButton;

        /// <summary>
        /// The button to save current cloud anchor id into clipboard.
        /// </summary>
        public Button ShareButton;

        /// <summary>
        /// The time between enters AR View and ARCore session starts to host or resolve.
        /// </summary>
        private const float _startPrepareTime = 3.0f;

        /// <summary>
        /// The timer to indicate whether the AR View has passed the start prepare time.
        /// </summary>
        private float _timeSinceStart;

        /// <summary>
        /// True if the app is in the process of returning to home page due to an invalid state,
        /// otherwise false.
        /// </summary>
        private bool _isReturning;

        /// <summary>
        /// The MapQualityIndicator that attaches to the placed object.
        /// </summary>
        private MapQualityIndicator _qualityIndicator = null;

        /// <summary>
        /// The history data that represents the current hosted Cloud Anchor.
        /// </summary>
        private CloudAnchorHistory _hostedCloudAnchor;

        /// <summary>
        /// An ARAnchor indicating the 3D object has been placed on a flat surface and
        /// is waiting for hosting.
        /// </summary>
        private ARAnchor _anchor = null;

        /// <summary>
        /// A list of Cloud Anchors that have been created but are not yet ready to use.
        /// </summary>
        private List<ARCloudAnchor> _pendingCloudAnchors = new List<ARCloudAnchor>();

        /// <summary>
        /// A list for caching all Cloud Anchors.
        /// </summary>
        private List<ARCloudAnchor> _cachedCloudAnchors = new List<ARCloudAnchor>();

        private Color _activeColor;

        /// <summary>
        /// Get the camera pose for the current frame.
        /// </summary>
        /// <returns>The camera pose of the current frame.</returns>
        public Pose GetCameraPose()
        {
            return new Pose(Controller.MainCamera.transform.position,
                Controller.MainCamera.transform.rotation);
        }

        /// <summary>
        /// Callback handling the validaton of the input field.
        /// </summary>
        /// <param name="inputString">The current value of the input field.</param>
        public void OnInputFieldValueChanged(string inputString)
        {
            // Cloud Anchor name should only contains: letters, numbers, hyphen(-), underscore(_).
            var regex = new Regex("^[a-zA-Z0-9-_]*$");
            InputFieldWarning.SetActive(!regex.IsMatch(inputString));
            SetSaveButtonActive(!InputFieldWarning.activeSelf && inputString.Length > 0);
        }

        /// <summary>
        /// Callback handling "Ok" button click event for input field.
        /// </summary>
        public void OnSaveButtonClicked()
        {
            _hostedCloudAnchor.Name = NameField.text;
            Controller.SaveCloudAnchorHistory(_hostedCloudAnchor);

            DebugText.text = string.Format("Saved Cloud Anchor:\n{0}.", _hostedCloudAnchor.Name);
            ShareButton.gameObject.SetActive(true);
            NamePanel.SetActive(false);
        }

        /// <summary>
        /// Callback handling "Share" button click event.
        /// </summary>
        public void OnShareButtonClicked()
        {
            GUIUtility.systemCopyBuffer = _hostedCloudAnchor.Id;
            DebugText.text = "Copied cloud id: " + _hostedCloudAnchor.Id;
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            _activeColor = SaveButton.GetComponentInChildren<Text>().color;
        }

        /// <summary>
        /// The Unity OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            _timeSinceStart = 0.0f;
            _isReturning = false;
            _anchor = null;
            _qualityIndicator = null;
            _pendingCloudAnchors.Clear();
            _cachedCloudAnchors.Clear();

            InstructionBar.SetActive(true);
            NamePanel.SetActive(false);
            InputFieldWarning.SetActive(false);
            ShareButton.gameObject.SetActive(false);
            UpdatePlaneVisibility(true);

            switch (Controller.Mode)
            {
                case PersistentCloudAnchorsController.ApplicationMode.Ready:
                    ReturnToHomePage("Invalid application mode, returning to home page...");
                    break;
                case PersistentCloudAnchorsController.ApplicationMode.Hosting:
                case PersistentCloudAnchorsController.ApplicationMode.Resolving:
                    InstructionText.text = "Detecting flat surface...";
                    DebugText.text = "ARCore is preparing for " + Controller.Mode;
                    break;
            }
        }

        /// <summary>
        /// The Unity OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            if (_qualityIndicator != null)
            {
                Destroy(_qualityIndicator.gameObject);
                _qualityIndicator = null;
            }

            if (_anchor != null)
            {
                Destroy(_anchor.gameObject);
                _anchor = null;
            }

            if (_pendingCloudAnchors.Count > 0)
            {
                foreach (var anchor in _pendingCloudAnchors)
                {
                    Destroy(anchor.gameObject);
                }

                _pendingCloudAnchors.Clear();
            }

            if (_cachedCloudAnchors.Count > 0)
            {
                foreach (var anchor in _cachedCloudAnchors)
                {
                    Destroy(anchor.gameObject);
                }

                _cachedCloudAnchors.Clear();
            }

            UpdatePlaneVisibility(false);
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            // Give ARCore some time to prepare for hosting or resolving.
            if (_timeSinceStart < _startPrepareTime)
            {
                _timeSinceStart += Time.deltaTime;
                if (_timeSinceStart >= _startPrepareTime)
                {
                    UpdateInitialInstruction();
                }

                return;
            }

            ARCoreLifecycleUpdate();
            if (_isReturning)
            {
                return;
            }

            if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Resolving)
            {
                ResolvingCloudAnchors();
            }
            else if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Hosting)
            {
                // Perform hit test and place an anchor on the hit test result.
                if (_anchor == null)
                {
                    // If the player has not touched the screen then the update is complete.
                    Touch touch;
                    if (Input.touchCount < 1 ||
                        (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
                    {
                        return;
                    }

                    // Ignore the touch if it's pointing on UI objects.
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        return;
                    }

                    // Perform hit test and place a pawn object.
                    PerformHitTest(touch.position);
                }

                HostingCloudAnchor();
            }

            UpdatePendingCloudAnchors();
        }

        private void PerformHitTest(Vector2 touchPos)
        {
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            Controller.RaycastManager.Raycast(
                touchPos, hitResults, TrackableType.PlaneWithinPolygon);

            // If there was an anchor placed, then instantiate the corresponding object.
            var planeType = PlaneAlignment.HorizontalUp;
            if (hitResults.Count > 0)
            {
                ARPlane plane = Controller.PlaneManager.GetPlane(hitResults[0].trackableId);
                if (plane == null)
                {
                    Debug.LogWarningFormat("Failed to find the ARPlane with TrackableId {0}",
                        hitResults[0].trackableId);
                    return;
                }

                planeType = plane.alignment;
                var hitPose = hitResults[0].pose;
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    // Point the hitPose rotation roughly away from the raycast/camera
                    // to match ARCore.
                    hitPose.rotation.eulerAngles =
                            new Vector3(0.0f, Controller.MainCamera.transform.eulerAngles.y, 0.0f);
                }

                _anchor = Controller.AnchorManager.AddAnchor(hitPose);
            }

            if (_anchor != null)
            {
                Instantiate(CloudAnchorPrefab, _anchor.transform);

                // Attach map quality indicator to this anchor.
                var indicatorGO =
                    Instantiate(MapQualityIndicatorPrefab, _anchor.transform);
                _qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
                _qualityIndicator.DrawIndicator(planeType, Controller.MainCamera);

                InstructionText.text = " To save this location, walk around the object to " +
                    "capture it from different angles";
                DebugText.text = "Waiting for sufficient mapping quaility...";

                // Hide plane generator so users can focus on the object they placed.
                UpdatePlaneVisibility(false);
            }
        }

        private void HostingCloudAnchor()
        {
            // There is no anchor for hosting.
            if (_anchor == null)
            {
                return;
            }

            // There is a pending or finished hosting task.
            if (_cachedCloudAnchors.Count > 0 || _pendingCloudAnchors.Count > 0)
            {
                return;
            }

            // Update map quality:
            int qualityState = 2;
            // Can pass in ANY valid camera pose to the mapping quality API.
            // Ideally, the pose should represent users’ expected perspectives.
            FeatureMapQuality quality =
                Controller.AnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
            DebugText.text = "Current mapping quality: " + quality;
            qualityState = (int)quality;
            _qualityIndicator.UpdateQualityState(qualityState);

            // Hosting instructions:
            var cameraDist = (_qualityIndicator.transform.position -
                Controller.MainCamera.transform.position).magnitude;
            if (cameraDist < _qualityIndicator.Radius * 1.5f)
            {
                InstructionText.text = "You are too close, move backward.";
                return;
            }
            else if (cameraDist > 10.0f)
            {
                InstructionText.text = "You are too far, come closer.";
                return;
            }
            else if (_qualityIndicator.ReachTopviewAngle)
            {
                InstructionText.text =
                    "You are looking from the top view, move around from all sides.";
                return;
            }
            else if (!_qualityIndicator.ReachQualityThreshold)
            {
                InstructionText.text = "Save the object here by capturing it from all sides.";
                return;
            }

            // Start hosting:
            InstructionText.text = "Processing...";
            DebugText.text = "Mapping quality has reached sufficient threshold, " +
                "creating Cloud Anchor.";
            DebugText.text = string.Format(
                "FeatureMapQuality has reached {0}, triggering CreateCloudAnchor.",
                Controller.AnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose()));

            // Creating a Cloud Anchor with lifetime = 1 day.
            // This is configurable up to 365 days when keyless authentication is used.
            ARCloudAnchor cloudAnchor = Controller.AnchorManager.HostCloudAnchor(_anchor, 1);
            if (cloudAnchor == null)
            {
                Debug.LogFormat("Failed to create a Cloud Anchor.");
                OnAnchorHostedFinished(false);
            }
            else
            {
                _pendingCloudAnchors.Add(cloudAnchor);
            }
        }

        private void ResolvingCloudAnchors()
        {
            // No Cloud Anchor for resolving.
            if (Controller.ResolvingSet.Count == 0)
            {
                return;
            }

            // There are pending or finished resolving tasks.
            if (_pendingCloudAnchors.Count > 0 || _cachedCloudAnchors.Count > 0)
            {
                return;
            }

            // ARCore session is not ready for resolving.
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                return;
            }

            Debug.LogFormat("Attempting to resolve {0} Cloud Anchor(s): {1}",
                Controller.ResolvingSet.Count,
                string.Join(",", new List<string>(Controller.ResolvingSet).ToArray()));
            foreach (string cloudId in Controller.ResolvingSet)
            {
                ARCloudAnchor cloudAnchor =
                    Controller.AnchorManager.ResolveCloudAnchorId(cloudId);
                if (cloudAnchor == null)
                {
                    Debug.LogFormat("Faild to resolve Cloud Anchor " + cloudId);
                    OnAnchorResolvedFinished(false, cloudId);
                }
                else
                {
                    _pendingCloudAnchors.Add(cloudAnchor);
                }
            }

            Controller.ResolvingSet.Clear();
        }

        private void UpdatePendingCloudAnchors()
        {
            foreach (var cloudAnchor in _pendingCloudAnchors)
            {
                if (cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
                {
                    if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Hosting)
                    {
                        Debug.LogFormat("Succeed to host the Cloud Anchor: {0}.",
                            cloudAnchor.cloudAnchorId);
                        int count = Controller.LoadCloudAnchorHistory().Collection.Count;
                        _hostedCloudAnchor = new CloudAnchorHistory("CloudAnchor" + count,
                            cloudAnchor.cloudAnchorId);
                        OnAnchorHostedFinished(true, cloudAnchor.cloudAnchorId);
                    }
                    else if (Controller.Mode ==
                        PersistentCloudAnchorsController.ApplicationMode.Resolving)
                    {
                        Debug.LogFormat("Succeed to resolve the Cloud Anchor: {0}",
                            cloudAnchor.cloudAnchorId);
                        OnAnchorResolvedFinished(true, cloudAnchor.cloudAnchorId);
                        Instantiate(CloudAnchorPrefab, cloudAnchor.transform);
                    }

                    _cachedCloudAnchors.Add(cloudAnchor);
                }
                else if (cloudAnchor.cloudAnchorState != CloudAnchorState.TaskInProgress)
                {
                    if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Hosting)
                    {
                        Debug.LogFormat("Failed to host the Cloud Anchor with error {0}.",
                            cloudAnchor.cloudAnchorState);
                        OnAnchorHostedFinished(false, cloudAnchor.cloudAnchorState.ToString());
                    }
                    else if (Controller.Mode ==
                       PersistentCloudAnchorsController.ApplicationMode.Resolving)
                    {
                        Debug.LogFormat("Failed to resolve the Cloud Anchor {0} with error {1}.",
                            cloudAnchor.cloudAnchorId, cloudAnchor.cloudAnchorState);
                        OnAnchorResolvedFinished(false, cloudAnchor.cloudAnchorId,
                            cloudAnchor.cloudAnchorState.ToString());
                    }

                    _cachedCloudAnchors.Add(cloudAnchor);
                }
            }

            _pendingCloudAnchors.RemoveAll(
                x => x.cloudAnchorState != CloudAnchorState.TaskInProgress);
        }

        private void OnAnchorHostedFinished(bool success, string response = null)
        {
            if (success)
            {
                InstructionText.text = "Finish!";
                Invoke("DoHideInstructionBar", 1.5f);
                DebugText.text =
                    string.Format("Succeed to host the Cloud Anchor: {0}.", response);

                // Display name panel and hide instruction bar.
                NameField.text = _hostedCloudAnchor.Name;
                NamePanel.SetActive(true);
                SetSaveButtonActive(true);
            }
            else
            {
                InstructionText.text = "Host failed.";
                DebugText.text = "Failed to host a Cloud Anchor" + (response == null ? "." :
                    "with error " + response + ".");
            }
        }

        private void OnAnchorResolvedFinished(bool success, string cloudId, string response = null)
        {
            if (success)
            {
                InstructionText.text = "Resolve success!";
                DebugText.text =
                    string.Format("Succeed to resolve the Cloud Anchor: {0}.", cloudId);
            }
            else
            {
                InstructionText.text = "Resolve failed.";
                DebugText.text = "Failed to resolve Cloud Anchor: " + cloudId +
                    (response == null ? "." : "with error " + response + ".");
            }
        }

        private void UpdateInitialInstruction()
        {
            switch (Controller.Mode)
            {
                case PersistentCloudAnchorsController.ApplicationMode.Hosting:
                    // Initial instruction for hosting flow:
                    InstructionText.text = "Tap to place an object.";
                    DebugText.text = "Tap a vertical or horizontal plane...";
                    return;
                case PersistentCloudAnchorsController.ApplicationMode.Resolving:
                    // Initial instruction for resolving flow:
                    InstructionText.text =
                        "Look at the location you expect to see the AR experience appear.";
                    DebugText.text = string.Format("Attempting to resolve {0} anchors...",
                        Controller.ResolvingSet.Count);
                    return;
                default:
                    return;
            }
        }

        private void UpdatePlaneVisibility(bool visible)
        {
            foreach (var plane in Controller.PlaneManager.trackables)
            {
                plane.gameObject.SetActive(visible);
            }
        }

        private void ARCoreLifecycleUpdate()
        {
            // Only allow the screen to sleep when not tracking.
            var sleepTimeout = SleepTimeout.NeverSleep;
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                sleepTimeout = SleepTimeout.SystemSetting;
            }

            Screen.sleepTimeout = sleepTimeout;

            if (_isReturning)
            {
                return;
            }

            // Return to home page if ARSession is in error status.
            if (ARSession.state != ARSessionState.Ready &&
                ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                ReturnToHomePage("ARCore encountered an error state " + ARSession.state +
                    ". Please start the app again.");
            }
        }

        private void ReturnToHomePage(string reason)
        {
            Debug.Log("Returning home for reason: " + reason);
            if (_isReturning)
            {
                return;
            }

            _isReturning = true;
            DebugText.text = reason;
            Invoke("DoReturnToHomePage", 3.0f);
        }

        private void DoReturnToHomePage()
        {
            Controller.SwitchToHomePage();
        }

        private void DoHideInstructionBar()
        {
            InstructionBar.SetActive(false);
        }

        private void SetSaveButtonActive(bool active)
        {
            SaveButton.enabled = active;
            SaveButton.GetComponentInChildren<Text>().color = active ? _activeColor : Color.gray;
        }
    }
}
