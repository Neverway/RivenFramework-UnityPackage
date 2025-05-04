//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicToggle : Logic
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicInput<bool> input = new(false);
    public LogicOutput<bool> output = new(false);


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        input.CallOnSourceChanged(Toggle);
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void Toggle()
    {
        output.Set(!output);
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
