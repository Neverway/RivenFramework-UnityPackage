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

public class LogicNot : MonoBehaviour
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
        //TestInputs();
        //input.CallOnSourceChanged(TestInputs);
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void Update()
    {
        output.Set(!input);
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
