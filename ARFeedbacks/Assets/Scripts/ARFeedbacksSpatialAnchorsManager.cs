// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Microsoft.Azure.SpatialAnchors;

namespace ARFeedbacks
{
    public class ARFeedbacksSpatialAnchorsManager : AzureSpatialAnchorsBase
    {
#if !UNITY_EDITOR
        public AnchorExchanger anchorExchanger = new AnchorExchanger();
#endif

#if UNITY_ANDROID || UNITY_IOS
        private static bool _runningOnHoloLens = false;
#else
        private static bool _runningOnHoloLens = true;
#endif

        public string BaseSharingUrl = "";
        private readonly List<GameObject> otherSpawnedObjects = new List<GameObject>();
        public List<GameObject> AttachableThumbnailObjects;
        public List<GameObject> AttachableObjects;
        private int anchorsLocated = 0;
        private int anchorsExpected = 0;
        private readonly List<string> localAnchorIds = new List<string>();
        private string _anchorKeyToFind;
        private long? _anchorNumberToFind;

        protected override void OnCloudAnchorLocated(AnchorLocatedEventArgs args)
        {
            base.OnCloudAnchorLocated(args);

            CloudSpatialAnchor nextCsa = args.Anchor;
            this.currentCloudAnchor = args.Anchor;
            this.QueueOnUpdate(new Action(() =>
            {
                this.anchorsLocated++;
                this.currentCloudAnchor = nextCsa;
                Pose anchorPose = Pose.identity;

#if UNITY_ANDROID || UNITY_IOS
                anchorPose = this.currentCloudAnchor.GetAnchorPose();
#endif
                // HoloLens: The position will be set based on the unityARUserAnchor that was located.


                GameObject nextObject = this.SpawnObjectFromAnchor(anchorPose.position, anchorPose.rotation, this.currentCloudAnchor);
                this.otherSpawnedObjects.Add(nextObject);

                if (this.anchorsLocated >= this.anchorsExpected)
                {
                    this.CloudManager.EnableProcessing = false;
                }
            }));
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        public override void Start()
        {
            this.GameObjectSelector = (anchor) => AttachableObjects.FirstOrDefault(o => o.name == anchor.AppProperties["name"]);
            base.Start();

            Debug.Log(">>MRCloud Demo Script Start");

            if (!SanityCheckAccessConfiguration())
            {
                return;
            }

            if (string.IsNullOrEmpty(this.BaseSharingUrl))
            {
                this.feedbackBox.text = "Need to set the BaseSharingUrl on the AzureSpatialAnchors object in your scene.";
                return;
            }
            else
            {
                Uri result;
                if (!Uri.TryCreate(this.BaseSharingUrl, UriKind.Absolute, out result))
                {
                    this.feedbackBox.text = "BaseSharingUrl, on the AzureSpatialAnchors object in your scene, is not a valid url";
                    return;
                }
                else
                {
                    this.BaseSharingUrl = $"{result.Scheme}://{result.Host}/api/anchors";
                }
            }

#if !UNITY_EDITOR
            anchorExchanger.WatchKeys(this.BaseSharingUrl);
#endif

            this.feedbackBox.text = "Scan the room then click on Load";

            Debug.Log("MRCloud Demo script started");
        }

        private bool _isPlacingObject = false;
        protected override bool IsPlacingObject()
        {
            return _isPlacingObject;
        }

        protected async override void OnSaveCloudAnchorSuccessful()
        {
            base.OnSaveCloudAnchorSuccessful();
            this.feedbackBox.text = "Emotion saved";
            long anchorNumber = -1;

            this.localAnchorIds.Add(this.currentCloudAnchor.Identifier);

#if !UNITY_EDITOR
            anchorNumber = (await this.anchorExchanger.StoreAnchorKey(currentCloudAnchor.Identifier));
#endif

            this.QueueOnUpdate(new Action(() =>
            {
                Pose anchorPose = Pose.identity;

#if UNITY_ANDROID || UNITY_IOS
                anchorPose = this.currentCloudAnchor.GetAnchorPose();
#endif
                // HoloLens: The position will be set based on the unityARUserAnchor that was located.

                this.SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);
            }));
        }

        protected override void OnSaveCloudAnchorFailed(Exception exception)
        {
            base.OnSaveCloudAnchorFailed(exception);
            spawnedObject = null;
            ObjectToAttach = null;
        }

        private void ResetAttachableThumbnailsScale()
        {
            foreach (var o in AttachableThumbnailObjects)
            {
                o.transform.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        }
       
        public void LoadSharedAnchors()
        {
            LogAndDisplay("Creating session...");
            this.CloudManager.ResetSessionStatusIndicators();
            this.anchorsLocated = 0;
            this.CloudManager.EnableProcessing = true;
            this.CloudManager.SetGraphEnabled(true);
            this.CloudManager.ResetAnchorIdsToLocate();
            this.CloudManager.SetNearbyAnchor(this.currentCloudAnchor, 1000, 500);
            LogAndDisplay("Create Watcher...");
            this.CloudManager.CreateWatcher();
            LogAndDisplay("Loading previous anchors..");
        }

        protected override void CleanupSpawnedObjects()
        {
            base.CleanupSpawnedObjects();

            for (int index = 0; index < this.otherSpawnedObjects.Count; index++)
            {
                Destroy(this.otherSpawnedObjects[index]);
            }

            this.otherSpawnedObjects.Clear();
        }

        public void SelectAndPlaceObject()
        {
            _isPlacingObject = true;
            ResetAttachableThumbnailsScale();
            EventSystem.current.currentSelectedGameObject.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            ObjectToAttach = AttachableObjects.FirstOrDefault(o => o.name == EventSystem.current.currentSelectedGameObject.name);
            this.CloudManager.EnableProcessing = true;
            this.feedbackBox.text = "Place your emotion";
        }

        protected override void OnSelectObjectInteraction(Vector3 hitPoint, object target)
        {
            base.OnSelectObjectInteraction(hitPoint, target);
            this.feedbackBox.text = "Saving emotion...";
            this.SaveCurrentObjectAnchorToCloud();
            this.feedbackBox.text = "Emotion spawning...";

        }
        public void AddNewObjectWithAnchor()
        {
            ResetAttachableThumbnailsScale();
            spawnedObject = null;
            ObjectToAttach = null;
            _isPlacingObject = false;
        }
    }
}
