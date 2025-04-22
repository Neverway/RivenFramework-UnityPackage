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
    [Tooltip("The radius of the sphere-cast to check if the head is clear, (this value should be smaller than the radius of the body collider to avoid getting false-positives when a pawn is up against a wall)")]
    public float headCheckRadius = 0.25f;
    [Tooltip("The offset from the pawn's origin to start the sphere-cast to check if the head is clear")]
    public Vector3 headCheckOffset = new Vector3(0, 1.75f, 0);
    [Tooltip("The distance of the sphere-cast to check if the head is clear, (This value should normally match, or be greater than, the crouchDistance)")]
    public float headCheckDistance = 0.5f;
    [Tooltip("The amount of height to add/subtract from the collider's height when uncrouching/crouching")]
    public float crouchDistance = 0.5f;
    public Vector3 crouchColliderOffset = new Vector3(0, 0.25f, 0);
}
