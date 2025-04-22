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
using UnityEngine.Serialization;

public class Pawn : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public PawnController controller;
    public PawnStats stats;
    public PawnAppearance appearance;

    public bool isPaused;
    public bool wasPaused; // used to restore a paused state when all pawns isPaused state is modified by the game instance


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Awake()
    {
        controller.PawnAwake(this);
    }

    private void Update()
    {
        controller.PawnUpdate(this);
    }

    private void FixedUpdate()
    {
        controller.PawnFixedUpdate(this);
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}