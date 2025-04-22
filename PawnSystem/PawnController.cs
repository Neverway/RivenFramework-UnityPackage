//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PawnController : ScriptableObject
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public abstract void PawnAwake(Pawn _pawn);
    public abstract void PawnUpdate(Pawn _pawn);
    public abstract void PawnFixedUpdate(Pawn _pawn);


    //=-----------------=
    // Private Variables
    //=-----------------=


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
}