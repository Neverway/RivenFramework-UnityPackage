//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FPS_Stats", menuName = "ScriptableObjects/FPS_Stats")]
public class FPS_Stats : PawnStats
{
    public float movementSpeed = 10;
    public float airMovementMultiplier = 0.25f;
    public float crouchMovementMultiplier = 0.25f;
    
    [Header("Ground Detection & Jumping")]
    public LayerMask groundMask;
    public float groundCheckRadius = 0.25f;
    public Vector3 groundCheckOffset;
    public float jumpForce = 10;

    [Header("Head Detection & Crouching")]
    public float headCheckRadius = 0.25f;
    public Vector3 headCheckOffset;
    public Vector3 crouchColliderOffset;
    public float crouchHeight = 1;
}
