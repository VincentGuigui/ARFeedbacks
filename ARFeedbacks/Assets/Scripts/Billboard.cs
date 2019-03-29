// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX.Utilities
{
    /// <summary>
    /// The Billboard class implements the behaviors needed to keep a GameObject oriented towards the user.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        public enum PivotAxiss
        {
            X,
            Y,
            Z,
            XY,
            XZ,
            YZ,
            Free
        }

        /// <summary>
        /// The axis about which the object will rotate.
        /// </summary>
        public PivotAxiss PivotAxis
        {
            get { return pivotAxis; }
            set { pivotAxis = value; }
        }

        [Tooltip("Specifies the axis about which the object will rotate.")]
        [SerializeField]
        private PivotAxiss pivotAxis = PivotAxiss.XY;

        /// <summary>
        /// The target we will orient to. If no target is specified, the main camera will be used.
        /// </summary>
        public Transform TargetTransform => targetTransform;

        [Tooltip("Specifies the target we will orient to. If no target is specified, the main camera will be used.")]
        [SerializeField]
        private Transform targetTransform;

        private void OnEnable()
        {
            if (targetTransform == null)
            {
                targetTransform = Camera.main.transform;
            }
        }

        /// <summary>
        /// Keeps the object facing the camera.
        /// </summary>
        private void Update()
        {
            if (targetTransform == null)
            {
                return;
            }

            // Get a Vector that points from the target to the main camera.
            Vector3 directionToTarget = targetTransform.position - transform.position;

            bool useCameraAsUpVector = true;

            // Adjust for the pivot axis.
            switch (pivotAxis)
            {
                case PivotAxiss.X:
                    directionToTarget.x = 0.0f;
                    useCameraAsUpVector = false;
                    break;

                case PivotAxiss.Y:
                    directionToTarget.y = 0.0f;
                    useCameraAsUpVector = false;
                    break;

                case PivotAxiss.Z:
                    directionToTarget.x = 0.0f;
                    directionToTarget.y = 0.0f;
                    break;

                case PivotAxiss.XY:
                    useCameraAsUpVector = false;
                    break;

                case PivotAxiss.XZ:
                    directionToTarget.x = 0.0f;
                    break;

                case PivotAxiss.YZ:
                    directionToTarget.y = 0.0f;
                    break;

                case PivotAxiss.Free:
                default:
                    // No changes needed.
                    break;
            }

            // If we are right next to the camera the rotation is undefined. 
            if (directionToTarget.sqrMagnitude < 0.001f)
            {
                return;
            }

            // Calculate and apply the rotation required to reorient the object
            if (useCameraAsUpVector)
            {
                transform.rotation = Quaternion.LookRotation(-directionToTarget, Camera.main.transform.up);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(-directionToTarget);
            }
        }
    }
}
