//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: This class holds a list of actions that any FPS pawn can implement
// Notes:
//
//=============================================================================

using UnityEngine;

public class FPS_ActionController
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private RaycastHit slopeHit;
    private bool isCrouching;


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
    public void Move(Pawn _pawn, Rigidbody _rigidbody, Vector3 _direction, float _speed=0)
    {
        if (_speed is 0) _speed = ((FPS_Stats)_pawn.stats).movementSpeed;
        
        // Make sure that the axis passed for the direction are always relative to the direction the pawn is facing
        var localMoveDirection = _pawn.transform.right * _direction.x + _pawn.transform.up * _direction.y + _pawn.transform.forward * _direction.z;
        var currentVelocity = _rigidbody.velocity;
        
        // Get desired velocities
        var desiredGroundVelocity = localMoveDirection.normalized * _speed;
        IsOnSlope(_pawn); // Calculate IsOnSlope to get the result of slopeHit
        var slopMoveDirection = Vector3.ProjectOnPlane(localMoveDirection, slopeHit.normal);
        var desiredSlopeVelocity = slopMoveDirection * _speed;
        var desiredAirVelocity = localMoveDirection.normalized * (_speed * ((FPS_Stats)_pawn.stats).airMovementMultiplier);
        var desiredCrouchVelocity = localMoveDirection.normalized * (_speed * ((FPS_Stats)_pawn.stats).crouchMovementMultiplier);
        
        // Define acceleration rates
        var groundAccelerationRate = 0.1f;
        var slopeAccelerationRate = 0.25f;
        var airAccelerationRate = 0.2f;
        
        // Ground Movement
        if (IsOnGround(_pawn) && !IsOnSlope(_pawn))
        {
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredGroundVelocity.x && desiredGroundVelocity.x > 0f || currentVelocity.x > desiredGroundVelocity.x && desiredGroundVelocity.x < 0f )
            {
                _rigidbody.velocity += new Vector3(desiredGroundVelocity.x*groundAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredGroundVelocity.y && desiredGroundVelocity.y > 0f || currentVelocity.y > desiredGroundVelocity.y && desiredGroundVelocity.y < 0f )
            {
                _rigidbody.velocity += new Vector3(0, desiredGroundVelocity.y*groundAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredGroundVelocity.z && desiredGroundVelocity.z > 0f || currentVelocity.z > desiredGroundVelocity.z && desiredGroundVelocity.z < 0f )
            {
                _rigidbody.velocity += new Vector3(0, 0, desiredGroundVelocity.z*groundAccelerationRate);
            }
        }
        // Slope Movement
        else if (IsOnGround(_pawn) && IsOnSlope(_pawn))
        {
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredSlopeVelocity.x && desiredSlopeVelocity.x > 0f || currentVelocity.x > desiredSlopeVelocity.x && desiredSlopeVelocity.x < 0f )
            {
                _rigidbody.velocity += new Vector3(desiredSlopeVelocity.x*slopeAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredSlopeVelocity.y && desiredSlopeVelocity.y > 0f || currentVelocity.y > desiredSlopeVelocity.y && desiredSlopeVelocity.y < 0f )
            {
                _rigidbody.velocity += new Vector3(0, desiredSlopeVelocity.y*slopeAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredSlopeVelocity.z && desiredSlopeVelocity.z > 0f || currentVelocity.z > desiredSlopeVelocity.z && desiredSlopeVelocity.z < 0f )
            {
                _rigidbody.velocity += new Vector3(0, 0, desiredSlopeVelocity.z*slopeAccelerationRate);
            }
        }
        // Crouch Movement
        else if (IsOnGround(_pawn) && isCrouching)
        {
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredCrouchVelocity.x && desiredCrouchVelocity.x > 0f || currentVelocity.x > desiredCrouchVelocity.x && desiredCrouchVelocity.x < 0f )
            {
                _rigidbody.velocity += new Vector3(desiredCrouchVelocity.x*groundAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredCrouchVelocity.y && desiredCrouchVelocity.y > 0f || currentVelocity.y > desiredCrouchVelocity.y && desiredCrouchVelocity.y < 0f )
            {
                _rigidbody.velocity += new Vector3(0, desiredCrouchVelocity.y*groundAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredCrouchVelocity.z && desiredCrouchVelocity.z > 0f || currentVelocity.z > desiredCrouchVelocity.z && desiredCrouchVelocity.z < 0f )
            {
                _rigidbody.velocity += new Vector3(0, 0, desiredCrouchVelocity.z*groundAccelerationRate);
            }
        }
        // Air Movement
        else
        {
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredAirVelocity.x && desiredAirVelocity.x > 0f || currentVelocity.x > desiredAirVelocity.x && desiredAirVelocity.x < 0f )
            {
                _rigidbody.velocity += new Vector3(desiredAirVelocity.x*airAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredAirVelocity.y && desiredAirVelocity.y > 0f || currentVelocity.y > desiredAirVelocity.y && desiredAirVelocity.y < 0f )
            {
                _rigidbody.velocity += new Vector3(0, desiredAirVelocity.y*airAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredAirVelocity.z && desiredAirVelocity.z > 0f || currentVelocity.z > desiredAirVelocity.z && desiredAirVelocity.z < 0f )
            {
                _rigidbody.velocity += new Vector3(0, 0, desiredAirVelocity.z*airAccelerationRate);
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
    /// Make the pawn turn a specified amount
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_direction">The direction to rotate in (x-axis is left/right, y-axis is up/down)</param>
    public void Look(Pawn _pawn, GameObject _viewPoint, Vector2 _direction)
    {
        _viewPoint.transform.localRotation = Quaternion.Euler(_direction.x, 0, 0); // Rotate the head for up/down
        _pawn.transform.rotation = Quaternion.Euler(0, _direction.y, 0); // Rotate the body for left/right
    }
    
    /// <summary>
    /// Make the pawn look at a specified point
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_position"></param>
    /// <param name="_speed"></param>
    public void LookAt(Pawn _pawn, GameObject _viewPoint, Vector3 _position, float _speed)
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
    public void Jump(Pawn _pawn, Rigidbody _rigidbody)
    {
        if (IsOnGround(_pawn) is false) return;
        _rigidbody.AddForce(Vector3.up * ((FPS_Stats)_pawn.stats).jumpForce, ForceMode.Impulse);
    }
    
    /// <summary>
    /// Make the pawn crouch by reducing its capsule collider height (and also trigger Move to change to a crouching movement speed)
    /// </summary>
    /// <param name="_pawn"></param>
    /// <param name="_enable"></param>
    public void Crouch(Pawn _pawn, bool _enable)
    {
        Debug.Log(IsHeadClear(_pawn));
        if (_enable && isCrouching is false)
        {
            var collider = _pawn.GetComponent<CapsuleCollider>();
            collider.height -= ((FPS_Stats)_pawn.stats).crouchDistance;
            collider.center += ((FPS_Stats)_pawn.stats).crouchColliderOffset;
            isCrouching = true;
        }
        if (_enable is false && isCrouching && IsHeadClear(_pawn))
        {
            var collider = _pawn.GetComponent<CapsuleCollider>();
            collider.height += ((FPS_Stats)_pawn.stats).crouchDistance;
            collider.center -= ((FPS_Stats)_pawn.stats).crouchColliderOffset;
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
    
    public bool IsHeadClear(Pawn _pawn)
    {
        RaycastHit hit;
        if (Physics.SphereCast(_pawn.transform.position + ((FPS_Stats)_pawn.stats).headCheckOffset, ((FPS_Stats)_pawn.stats).headCheckRadius, _pawn.transform.up, out hit, ((FPS_Stats)_pawn.stats).headCheckDistance, ((FPS_Stats)_pawn.stats).groundMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        return true;
    }
    
    public bool IsOnGround(Pawn _pawn)
    {
        return Physics.CheckSphere(_pawn.transform.position - ((FPS_Stats)_pawn.stats).groundCheckOffset, ((FPS_Stats)_pawn.stats).groundCheckRadius, ((FPS_Stats)_pawn.stats).groundMask, QueryTriggerInteraction.Ignore);
    }

    public bool IsOnSlope(Pawn _pawn)
    {
        if (Physics.Raycast(_pawn.transform.position, Vector3.down, out slopeHit, ((FPS_Stats)_pawn.stats).groundCheckOffset.y, ((FPS_Stats)_pawn.stats).groundMask, QueryTriggerInteraction.Ignore))
        {
            return slopeHit.normal != Vector3.up;
        }

        return false;
    }
}
