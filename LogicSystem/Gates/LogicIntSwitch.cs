//==========================================( Neverway 2024 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicIntSwitch : Logic
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicInput<bool> powered = new(false);
    public LogicInput<int> unpoweredValue = new(0);
    public LogicInput<int> poweredValue = new(0);
    public LogicOutput<int> output = new(0);
    


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=

    private void Update()
    {
        if (powered.Get()) output.Set(poweredValue);
        else output.Set(unpoweredValue);
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    


    //=-----------------=
    // External Functions
    //=-----------------=
}
