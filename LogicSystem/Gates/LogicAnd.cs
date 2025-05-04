//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicAnd : Logic
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicInput<bool> inputA = new(false);
    public LogicInput<bool> inputB = new(false);
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
        //TestInputs();
        //inputA.CallOnSourceChanged(TestInputs);
        //inputB.CallOnSourceChanged(TestInputs);
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void Update()
    {
        output.Set(inputA && inputB);
    }
    

    //=-----------------=
    // External Functions
    //=-----------------=
}
