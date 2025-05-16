//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LogicToggle : Logic
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicInput<bool> input = new(false);
    public LogicOutput<bool> output = new(false);
    
    [Tooltip("This event will only fire when the output is powered")]
    public UnityEvent onOutputPowered;
    [Tooltip("This event will only fire when the output is unpowered")]
    public UnityEvent onOutputUnpowered;


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
        if (input.Get() is false) return;
        output.Set(!output);
        if (output) onOutputPowered.Invoke();
        else onOutputUnpowered.Invoke();
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
