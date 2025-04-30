//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPPawnActions : PawnActions
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private RaycastHit slopeHit;
    private bool isCrouching;
    private GameObject viewCamera;


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
    /// <summary>
    /// Make the pawn move, using velocity, in a specified direction
    /// </summary>
    /// <param name="_pawn">A reference to the owning pawn</param>
    /// <param name="_rigidbody">A reference to the owning rigidbody</param>
    /// <param name="_direction">The direction to move in (x-axis is left/right, y-axis is forward/backward, and z-axis is up/down (which is only really used for flying enemies))</param>
    /// <param name="_speed">The speed to move the pawn at (set this to 0 to just use the stats movement speed)</param>
    public void Move(FPPawn _pawn, Vector3 _direction, float _speed=0)
    {
        if (_speed == 0)
        {
            _speed = ((FPPawnStats)_pawn.currentStats).movementSpeed;
        }

        var rigidbody = _pawn.GetComponent<Rigidbody>();
        
        // Make sure that the axis passed for the direction are always relative to the direction the pawn is facing
        var localMoveDirection = _pawn.transform.right * _direction.x + _pawn.transform.up * _direction.y + _pawn.transform.forward * _direction.z;
        var currentVelocity = rigidbody.velocity;
        
        // Get desired velocities
        var desiredGroundVelocity = localMoveDirection.normalized * _speed;
        IsOnSlope(_pawn); // Calculate IsOnSlope to get the result of slopeHit
        var slopMoveDirection = Vector3.ProjectOnPlane(localMoveDirection, slopeHit.normal);
        var desiredSlopeVelocity = slopMoveDirection * _speed;
        var desiredAirVelocity = localMoveDirection.normalized * (_speed * ((FPPawnStats)_pawn.currentStats).airMovementMultiplier);
        var desiredCrouchVelocity = localMoveDirection.normalized * (_speed * ((FPPawnStats)_pawn.currentStats).crouchMovementMultiplier);
        
        // Define acceleration rates
        var groundAccelerationRate = ((FPPawnStats)_pawn.currentStats).groundAccelerationRate;
        var slopeAccelerationRate = ((FPPawnStats)_pawn.currentStats).slopeAccelerationRate;
        var airAccelerationRate = ((FPPawnStats)_pawn.currentStats).airAccelerationRate;
        
        // Ground Movement
        if (IsOnGround(_pawn) && !IsOnSlope(_pawn) && !isCrouching)
        {
            rigidbody.useGravity = true;
            rigidbody.drag = ((FPPawnStats)_pawn.currentStats).groundDrag;
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredGroundVelocity.x && desiredGroundVelocity.x > 0f || currentVelocity.x > desiredGroundVelocity.x && desiredGroundVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredGroundVelocity.x*groundAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredGroundVelocity.y && desiredGroundVelocity.y > 0f || currentVelocity.y > desiredGroundVelocity.y && desiredGroundVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredGroundVelocity.y*groundAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredGroundVelocity.z && desiredGroundVelocity.z > 0f || currentVelocity.z > desiredGroundVelocity.z && desiredGroundVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredGroundVelocity.z*groundAccelerationRate);
            }
        }
        // Crouch Movement
        else if (IsOnGround(_pawn) && !IsOnSlope(_pawn) && isCrouching)
        {
            rigidbody.useGravity = true;
            Debug.Log("Crouching");
            rigidbody.drag = ((FPPawnStats)_pawn.currentStats).groundDrag;
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredCrouchVelocity.x && desiredCrouchVelocity.x > 0f || currentVelocity.x > desiredCrouchVelocity.x && desiredCrouchVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredCrouchVelocity.x*groundAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredCrouchVelocity.y && desiredCrouchVelocity.y > 0f || currentVelocity.y > desiredCrouchVelocity.y && desiredCrouchVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredCrouchVelocity.y*groundAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredCrouchVelocity.z && desiredCrouchVelocity.z > 0f || currentVelocity.z > desiredCrouchVelocity.z && desiredCrouchVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredCrouchVelocity.z*groundAccelerationRate);
            }
        }
        // Slope Movement
        else if (IsOnGround(_pawn) && IsOnSlope(_pawn))
        {
            rigidbody.useGravity = false;
            rigidbody.drag = ((FPPawnStats)_pawn.currentStats).slopeDrag;
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredSlopeVelocity.x && desiredSlopeVelocity.x > 0f || currentVelocity.x > desiredSlopeVelocity.x && desiredSlopeVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredSlopeVelocity.x*slopeAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredSlopeVelocity.y && desiredSlopeVelocity.y > 0f || currentVelocity.y > desiredSlopeVelocity.y && desiredSlopeVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredSlopeVelocity.y*slopeAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredSlopeVelocity.z && desiredSlopeVelocity.z > 0f || currentVelocity.z > desiredSlopeVelocity.z && desiredSlopeVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredSlopeVelocity.z*slopeAccelerationRate);
            }
        }
        // Air Movement
        else
        {
            rigidbody.useGravity = true;
            rigidbody.drag = ((FPPawnStats)_pawn.currentStats).airDrag;
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredAirVelocity.x && desiredAirVelocity.x > 0f || currentVelocity.x > desiredAirVelocity.x && desiredAirVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredAirVelocity.x*airAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredAirVelocity.y && desiredAirVelocity.y > 0f || currentVelocity.y > desiredAirVelocity.y && desiredAirVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredAirVelocity.y*airAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredAirVelocity.z && desiredAirVelocity.z > 0f || currentVelocity.z > desiredAirVelocity.z && desiredAirVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredAirVelocity.z*airAccelerationRate);
            }
        }
    }
    
    /// <summary>
    /// TODO Make the pawn move in a direct path to a specified position
    /// </summary>
    /// <param name="_position"></param>
    public void MoveTo(Vector3 _position)
    {
        
    }
    
    /// <summary>
    /// TODO Make the pawn path-find it's way to a specified position
    /// </summary>
    /// <param name="_position"></param>
    public void MoveToSmart(Vector3 _position)
    {
        
    }
    
    /// <summary>
    /// Make the pawn turn to face a specified amount
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_direction">The direction to rotate in (x-axis is left/right, y-axis is up/down)</param>
    public void FaceTowardsDirection(FPPawn _pawn, GameObject _viewPoint, Vector2 _direction)
    {
        _viewPoint.transform.localRotation = Quaternion.Euler(_direction.x, 0, 0); // Rotate the head for up/down
        _pawn.transform.rotation = Quaternion.Euler(0, _direction.y, 0); // Rotate the body for left/right
    }
    
    /// <summary>
    /// Make the pawn face at a specified point
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_position"></param>
    /// <param name="_speed"></param>
    public void FaceTowardsPosition(FPPawn _pawn, GameObject _viewPoint, Vector3 _position, float _speed)
    {
        var vectorToTarget = _pawn.transform.position - _position;

        // Rotate the body for left/right
        var bodyLookRotation = Mathf.Atan2(vectorToTarget.x, vectorToTarget.z) * Mathf.Rad2Deg;
        _pawn.transform.rotation = Quaternion.Euler(0, bodyLookRotation+180, 0);
        
        // Rotate the head for up/down
        var headLookRotation = Quaternion.LookRotation(vectorToTarget, _pawn.transform.up).eulerAngles;
        var desiredRotation = new Vector3(-headLookRotation.x, headLookRotation.y + 180, headLookRotation.z);
        _viewPoint.transform.eulerAngles = desiredRotation;
    }
    
    /// <summary>
    /// Make the pawn jump using a force applied to the rigidbody
    /// </summary>
    /// <param name="_pawn">A reference to the pawn to get its jump force & IsOnGround state</param>
    /// <param name="_rigidbody"></param>
    public void Jump(FPPawn _pawn)
    {
        if (IsOnGround(_pawn) is false) return;
        var rigidbody = _pawn.GetComponent<Rigidbody>();
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
        rigidbody.AddForce(Vector3.up * ((FPPawnStats)_pawn.currentStats).jumpForce, ForceMode.Impulse);
    }
    
    /// <summary>
    /// Make the pawn crouch by reducing its capsule collider height (and also trigger Move to change to a crouching movement speed)
    /// </summary>
    /// <param name="_pawn"></param>
    /// <param name="_enable"></param>
    public void Crouch(FPPawn _pawn, bool _enable)
    {
        if (_enable && isCrouching is false)
        {
            var collider = _pawn.GetComponent<CapsuleCollider>();
            collider.height -= ((FPPawnStats)_pawn.currentStats).crouchDistance;
            collider.center += ((FPPawnStats)_pawn.currentStats).crouchColliderOffset;
            isCrouching = true;
        }
        if (_enable is false && isCrouching && IsHeadClear(_pawn))
        {
            var collider = _pawn.GetComponent<CapsuleCollider>();
            collider.height += ((FPPawnStats)_pawn.currentStats).crouchDistance;
            collider.center -= ((FPPawnStats)_pawn.currentStats).crouchColliderOffset;
            isCrouching = false;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void Interact()
    {
        
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="_action"></param>
    public void UseItem(int _action = 0)
    {
        
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void SwitchItem()
    {
        
    }
    
    public bool IsHeadClear(FPPawn _pawn)
    {
        RaycastHit hit;
        if (Physics.SphereCast(_pawn.transform.position + ((FPPawnStats)_pawn.currentStats).headCheckOffset, ((FPPawnStats)_pawn.currentStats).headCheckRadius, _pawn.transform.up, out hit, ((FPPawnStats)_pawn.currentStats).headCheckDistance, ((FPPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        return true;
    }
    
    public bool IsOnGround(FPPawn _pawn)
    {
        // Move the ground check position upwards if the pawn is crouching to account for their change in height
        Vector3 crouchingOffset = new Vector3(0,0,0);
        if (isCrouching) crouchingOffset = new Vector3(0, ((FPPawnStats)_pawn.currentStats).crouchDistance, 0);
        
        return Physics.CheckSphere(_pawn.transform.position - ((FPPawnStats)_pawn.currentStats).groundCheckOffset + crouchingOffset, ((FPPawnStats)_pawn.currentStats).groundCheckRadius, ((FPPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore);
    }

    public bool IsOnSlope(FPPawn _pawn)
    {
        /*
        This function does not account for crouching offsets. Meaning if a pawn is crouched, the slope detection will likely fail and the pawn will slip off the slope.
        This is a bug, but I'm deciding to keep it in since it's super fun to be able to crouch when falling at a slope to slide down it!
        If this needs to be patched out for any reason, update this function to account for the crouch offset. If you're not sure how to do that, check IsOnGround function above. It correctly accounts for the crouch offset.
        Happy sliding! ~Liz
        //*/
        if (Physics.Raycast(_pawn.transform.position, Vector3.down, out slopeHit, ((FPPawnStats)_pawn.currentStats).slopeCheckDistance, ((FPPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore))
        {
            return slopeHit.normal != Vector3.up;
        }

        return false;
    }

    public void EnableViewCamera(FPPawn _pawn, bool _setActive)
    {
        if (viewCamera is null)
        {
            // Try to get a view camera
            viewCamera =_pawn.GetComponentInChildren<Camera>(true).gameObject;
            if (viewCamera is null) return;
        }
        
        viewCamera.SetActive(_setActive);
    }

    /// <summary>
    /// Clears and populates the lists of visible pawns
    /// </summary>
    /// <param name="_pawn"></param>
    /// <param name="_distance"></param>
    public void Look(FPPawn _pawn, float _distance)
    {
        // Clear the list of visible pawns
        _pawn.visiblePawns.Clear();
        _pawn.visibleHositles.Clear();
        _pawn.visibleAllies.Clear();
        foreach (var target in Physics.OverlapSphere(_pawn.transform.position, _distance))
        {
            // Object is pawn
            var targetPawn = target.GetComponent(typeof(FPPawn)) as FPPawn;
            if (targetPawn)
            {
                if (targetPawn.gameObject == _pawn.gameObject) continue;
                // Pawn is not occluded by something
                //if (!Physics.Raycast(_pawn.viewPoint.transform.position, _pawn.transform.position - target.transform.position, 9999, _pawn.currentStats.groundMask))
                //{
                    // Add it to the list of visible pawns
                    _pawn.visiblePawns.Add(targetPawn);
                    // If it's an enemy, add it to the list of visible hostiles
                    if (_pawn.currentStats.opposedTeams.Contains(((FPPawnStats)targetPawn.currentStats).team))
                    {
                        _pawn.visibleHositles.Add(targetPawn);
                    }
                    // If it's a friend, add it to the list of visible allies
                    if (((FPPawnStats)_pawn.currentStats).alliedTeams.Contains(((FPPawnStats)targetPawn.currentStats).team))
                    {
                        _pawn.visibleAllies.Add(targetPawn);
                    }
                //}
            }
        }
    }
    
    public void Listen()
    {
        
    }

    public FPPawn GetClosest(FPPawn _pawn, List<Pawn> _pawns)
    {
        var closestDistance = 999999f;
        FPPawn closestPawn = null;
        foreach (var target in _pawns)
        {
            var distanceToTarget = Vector3.Distance(_pawn.transform.position, target.transform.position);
            if (distanceToTarget <= closestDistance)
            {
                closestDistance = distanceToTarget;
                closestPawn = ((FPPawn)target);
            }
        }

        return closestPawn;
    }

    public float GetCollectiveAllyCourage(FPPawn _pawn, List<Pawn> _pawns)
    {
        float collectiveAllyCourage = 0;
        foreach (var target in _pawns)
        {
            var distanceToTarget = Vector3.Distance(_pawn.transform.position, target.transform.position);
            if (distanceToTarget <= ((FPPawnStats)_pawn.currentStats).comfortableAllyDistance)
            {
                collectiveAllyCourage += ((FPPawnStats)_pawn.currentStats).courage;
            }
        }
        /*foreach (var VARIABLE in COLLECTION)
        {
            Vector3.Distance(closestAlly.transform.position, _pawn.transform.position) > ((FPS_Stats)_pawn.stats).comfortableAllyDistance
        }*/
        return collectiveAllyCourage;
    }
}
