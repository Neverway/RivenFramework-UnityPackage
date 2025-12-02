//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Attached to an object on a pawn to be used like a socket for other
//      objects, such as held physics props, swords on backs, guns on hips, etc.
// Notes: This was originally created to keep track of held physics objects 
//
//=============================================================================

using System;
using UnityEngine;

    public class Pawn_AttachmentPoint : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        [Tooltip("When trying to pickup an object if it's over this mass, the object will be dragged instead")]
        public float pickupMassLimit = 20f;

        public bool heldObjectLooselyPinned;


        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=
        [Tooltip("The object that is attached to this point, this is set, not assigned, don't touch this")]
        public GameObject attachedObject;
        public Rigidbody attachedRigidbody;
        [Tooltip("The configurable joint that's used when picking up light advanced phys props")]
        public ConfigurableJoint pickupJoint;
        [Tooltip("The configurable joint that's used when dragging heavy advanced phys props")]
        public ConfigurableJoint dragJoint;
        [Tooltip("When dragging or pulling on objects, this line renderer is used to represent the distance between the attachment point and the anchor point")]
        public LineRenderer dragLineRenderer;


        //=-----------------=
        // Mono Functions
        //=-----------------=
        public void Update()
        {
            if (dragJoint.connectedBody)
            {
                dragLineRenderer.enabled = true;
                dragLineRenderer.SetPosition(0, transform.position);
                dragLineRenderer.SetPosition(1, dragJoint.connectedBody.position+dragJoint.connectedAnchor);
            }
            else if (pickupJoint.connectedBody && pickupJoint.connectedBody.isKinematic)
            {
                dragLineRenderer.enabled = true;
                dragLineRenderer.SetPosition(0, transform.position);
                dragLineRenderer.SetPosition(1, pickupJoint.connectedBody.position+pickupJoint.connectedAnchor);
            }
            else
            {
                dragLineRenderer.enabled = false;
            }
        }


        //=-----------------=
        // Internal Functions
        //=-----------------=


        //=-----------------=
        // External Functions
        //=-----------------=
        /// <summary>
        /// Returns true if something is already being held in this attachment point
        /// (Simple function to make code more readable)
        /// </summary>
        public bool IsOccupied()
        {
            return attachedObject != null;
        }
        
        /// <summary>
        /// Attach a physics pickup to this attachment point
        /// </summary>
        /// <param name="_targetObject">The root of the physics pickup</param>
        /// <param name="_targetRigidbody">The rigidbody of the physics pickup</param>
        public void Attach(GameObject _targetObject, Rigidbody _targetRigidbody = null)
        {
            dragLineRenderer.enabled = false;
            
            // Find the rigidbody of the physics pickup
            attachedRigidbody = _targetObject.GetComponent<Rigidbody>();
            if (_targetRigidbody) attachedRigidbody = _targetRigidbody;

            // Check mass limitations
            if (heldObjectLooselyPinned || attachedRigidbody.mass > pickupMassLimit)
            {
                dragJoint.connectedBody = attachedRigidbody;
            }
            else
            {
                pickupJoint.connectedBody = attachedRigidbody;
            }
            
            attachedObject = _targetObject;

        }
        
        /// <summary>
        /// Detach a physics pickup from this attachment point
        /// </summary>
        public void Detach()
        {
            attachedObject = null;
            pickupJoint.connectedBody = null;
            dragJoint.connectedBody = null;
            dragLineRenderer.enabled = false;
        }

        public bool IsObjectOverweight(GameObject _targetObject)
        {
            if (attachedObject != null)
            {
                return true;
            }
            
            // Check mass limitations
            if (attachedRigidbody.mass > pickupMassLimit)
            {
                return true;
            }

            return false;
        }
    }