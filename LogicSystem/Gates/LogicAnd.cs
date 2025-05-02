//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using Neverway.Framework.LogicValueSystem;
using UnityEngine;

public class LogicAnd : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public List<LogicInput<bool>> inputs;
    public LogicOutput<bool> output;


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
    
    }

    private void Update()
    {
        output.Set(TestInputs());
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    /// <summary>
    /// Returns true if all inputs are powered
    /// </summary>
    /// <returns></returns>
    private bool TestInputs()
    {
        foreach (var input in inputs)
        {
            if (input == false)
            {
                return false;
            }
        }

        return true;
    }
    

    //=-----------------=
    // External Functions
    //=-----------------=
}
