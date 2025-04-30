//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.PawnManagement
{
public class FPPawn_Player : FPPawn
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
    private FPPawnActions action = new FPPawnActions();
    private InputActions.FirstPersonActions inputActions;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public void Awake()
    {
        base.Awake();
        Debug.Log("test");
        // Setup inputs
        inputActions = new InputActions().FirstPerson;
        inputActions.Enable();
        
        // Enable the view camera
        action.EnableViewCamera(this, true);
    }

    public void Update()
    {
        if (isPaused) return;
        UpdateMovement();
        UpdateRotation();
        
        // Jumping
        if (inputActions.Jump.WasPressedThisFrame()) action.Jump(this);
        
        // Crouching
        if (inputActions.Crouch.IsPressed())
        {
            Debug.Log("Wants to crouch");
            action.Crouch(this, true);
        }
        else
        {
            action.Crouch(this, false);
        }
    }

    public void FixedUpdate()
    {
        if (isPaused) return;
        ApplyMovement();
        ApplyRotation();
    }
    

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void UpdateMovement()
    {
        moveDirection = new Vector3(inputActions.Move.ReadValue<Vector2>().x, 0, inputActions.Move.ReadValue<Vector2>().y);
    }
    private void ApplyMovement()
    {
        action.Move(this, moveDirection);
    }

    private void UpdateRotation()
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
    private void ApplyRotation()
    {
        action.FaceTowardsDirection(this, viewPoint, lookRotation);
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
}
