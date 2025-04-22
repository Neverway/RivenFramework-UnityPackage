//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FPS_PlayerController", menuName = "ScriptableObjects/FPS_PlayerController")]
public class FPS_PlayerController : PawnController
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private Vector3 moveDirection;
    private Vector2 lookRotation;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private FPS_ActionController action = new FPS_ActionController();
    private Rigidbody rigidbody;
    private GameObject viewPoint;
    private InputActions.FPSActions inputActions;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public override void PawnAwake(Pawn _pawn)
    {
        // Setup inputs
        inputActions = new InputActions().FPS;
        inputActions.Enable();
        
        // Get references
        rigidbody = _pawn.GetComponent<Rigidbody>();
        viewPoint = _pawn.transform.Find("ViewPoint").gameObject;
    }

    public override void PawnUpdate(Pawn _pawn)
    {
        if (_pawn.isPaused) return;
        UpdateMovement(_pawn);
        UpdateRotation(_pawn);
        
        // Jumping
        if (inputActions.Jump.WasPressedThisFrame()) action.Jump(_pawn, rigidbody);
        
        // Crouching
        if (inputActions.Crouch.IsPressed())
        {
            Debug.Log("Wants to crouch");
            action.Crouch(_pawn, true);
        }
        else
        {
            action.Crouch(_pawn, false);
        }
    }

    public override void PawnFixedUpdate(Pawn _pawn)
    {
        if (_pawn.isPaused) return;
        ApplyMovement(_pawn);
        ApplyRotation(_pawn);
    }
    

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void UpdateMovement(Pawn _pawn)
    {
        moveDirection = new Vector3(inputActions.Move.ReadValue<Vector2>().x, 0, inputActions.Move.ReadValue<Vector2>().y);
    }
    private void ApplyMovement(Pawn _pawn)
    {
        action.Move(_pawn, rigidbody, moveDirection);
    }

    private void UpdateRotation(Pawn _pawn)
    {
        // Get the look speed
        float horizontalLookSpeed = 3; // = applicationSettings.currentSettingsData.horizontalLookSpeed
        float verticalLookSpeed = 2; // = applicationSettings.currentSettingsData.verticalLookSpeed
        
        // Separate multipliers for mouse and joystick
        float mouseMultiplier = 0.01f;
        float joystickMultiplier = 0.2f;

        // Determine the input method (mouse or joystick)
        bool isUsingMouse = false;
        if (inputActions.LookAxis.IsInProgress())
        {
            if (inputActions.LookAxis.activeControl.device.name == "Mouse")
            {
                isUsingMouse = true;
            }
        }

        // Apply the appropriate multiplier
        var multiplier = isUsingMouse ? mouseMultiplier : joystickMultiplier;
        
        // Store the rotation values
        lookRotation.x -= inputActions.LookAxis.ReadValue<Vector2>().y * (20 * verticalLookSpeed) * multiplier;
        lookRotation.y += inputActions.LookAxis.ReadValue<Vector2>().x * (20 * horizontalLookSpeed) * multiplier;
        lookRotation.x = Mathf.Clamp(lookRotation.x, -90f, 90f);
    }
    private void ApplyRotation(Pawn _pawn)
    {
        action.Look(_pawn, viewPoint, lookRotation);
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
