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

    [Space]
    [Tooltip("Output will persist as TRUE as soon as input is TRUE once")]
    public bool stayPowered = false;

    [Space]
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
        print("Toggling");
        //Trigger the toggle only when input signal is TRUE
        if (input.Get() is false) return;

        //Toggle the output state (Always set to TRUE if "stayPowered" is TRUE)
        output.Set(!output || stayPowered);

        //Invoke events for change of power state
        if (output) onOutputPowered.Invoke();
        else onOutputUnpowered.Invoke();
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
