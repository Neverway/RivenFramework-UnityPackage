//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeKill : Volume
{
    //=-----------------=
    // Public Variables
    //=-----------------=


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
    private new void OnTriggerEnter(Collider _other)
    {
        // R.I.P "Connorses has a hat" debug log message 2024-2025 ~Liz
        
        base.OnTriggerEnter(_other); // Call the base class method
        if (_other.CompareTag("Pawn"))
        {
            _other.GetComponent<Pawn>().Kill();
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
