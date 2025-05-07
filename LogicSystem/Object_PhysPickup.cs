//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
public class Object_PhysPickup : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Tooltip("If the prop somehow gets this far away from it's connected attachment point, it will be dropped")]
    public float breakAwayDistance = 3;
    public Vector3 holdPositionOffset;
    public Vector3 holdRotationOffset;

    [Tooltip("This is the radius used to check for obstructions to the attachment point. Set this to 1/2 the depth of the prop's collider for best results.")]
    public float checkRadius = 0.25f;
    
    [Tooltip("These are the layers the phys prop will collide with while being held")]
    public LayerMask layerMask;


    //=-----------------=
    // Private Variables
    //=-----------------=
    public bool isHeld;
    private bool wasGravityEnabled;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    [Tooltip("A reference to this prop's rigidbody (we are assuming this component is attached to the same object that has the rigidbody, and getting the reference in start)")]
    private Rigidbody propRigidbody;
    [Tooltip("A reference to the pawn that is holding this prop")]
    private Pawn holdingPawn;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        propRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (isHeld)
        {
            MoveToHoldingPosition();
        }
    }

    private void OnTriggerEnter(Collider _other)
    {
        // Check to see if an interaction trigger has activated us
        var interaction = _other.GetComponent<VolumeTriggerInteraction>();
        if (interaction)
        {
            holdingPawn = interaction.owningPawn;
            ToggleHeld();
        }
    }
    
    private void OnDisable()
    {
        Drop();
    }
    

    //=-----------------=
    // Internal Functions
    //=-----------------=
    public void Pickup()
    {
        // If we are already being held, exit
        if (isHeld) return;
        
        // If the target is already holding something, exit
        if (holdingPawn.physObjectAttachmentPoint.attachedObject != null) return;
        
        // Okie doke, we are good to go, let's pickup the object
        isHeld = true;
        // Assign this prop to the attachment point of our target
        holdingPawn.physObjectAttachmentPoint.attachedObject = this.gameObject;
        // Store whether gravity was enabled before we got picked up
        // (When a physics prop is picked up, we disable its gravity, so this value keeps track of if the object had gravity to begin with)
        wasGravityEnabled = propRigidbody.useGravity;
    }

    public void Drop()
    {
        // If we are not being held, exit
        if (isHeld is false) return;
        
        isHeld = false;
        // Restore gravity if it was enabled before pickup
        propRigidbody.useGravity = wasGravityEnabled;
        // Clear ourselves from the attachment point
        holdingPawn.physObjectAttachmentPoint.attachedObject = null;
        holdingPawn = null;
    }

    public void ToggleHeld()
    {
        if (isHeld)
        {
            Drop();
        }
        else if (isHeld is false)
        {
            Pickup();
        }
    }

    public void MoveToHoldingPosition()
    {
        // Initialize values
        var viewTransform = holdingPawn.viewPoint.position;
        var attachmentTransform = holdingPawn.physObjectAttachmentPoint.transform.position;
        var directionFromAttachmentToView = attachmentTransform - viewTransform;
        var distanceFromAttachmentToView = Vector3.Distance(attachmentTransform, holdingPawn.viewPoint.position);
        Vector3 targetPosition;
        
        // Check to see if attachment point is clear of obstructions
        if (Physics.SphereCast(viewTransform, checkRadius, directionFromAttachmentToView, out RaycastHit hit, distanceFromAttachmentToView, layerMask))
        {
            // It's not clear, so we'll want to move the target position to hold the prop backwards until it is free
            targetPosition = (hit.point - (directionFromAttachmentToView.normalized * 0.5f));
        }
        else
        {
            // It is clear, so set the target position to the attachment point
            targetPosition = (attachmentTransform);
        }

        // Snap the prop to match the target position
        propRigidbody.MovePosition(targetPosition+holdPositionOffset);
        // Snap the prop to match the attachments rotation
        var targetRotation = holdingPawn.physObjectAttachmentPoint.transform.rotation;
        transform.rotation = new Quaternion(targetRotation.x+holdRotationOffset.x, targetRotation.y+holdRotationOffset.y, targetRotation.z+holdRotationOffset.z, targetRotation.w);
        
        // Remove any existing velocity, so it doesn't bug out while holding it
        propRigidbody.velocity = Vector3.zero;
        propRigidbody.angularVelocity = Vector3.zero;
        propRigidbody.useGravity = false;

        // Drop the object if it's too far away
        if (Vector3.Distance(gameObject.transform.position, holdingPawn.physObjectAttachmentPoint.transform.position) > breakAwayDistance)
        {
            Drop();
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
}
