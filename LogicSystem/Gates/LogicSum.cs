//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicSum : Logic
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicInput<int> inputValueA = new(0);
    public LogicInput<int> inputValueB = new(0);
    public LogicOutput<int> comparisonOutput = new(0);
    public CompareOperation compareOperation;
    public enum CompareOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Power,
        Root,
        Modulo,
        Min,
        Max,
    }


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
        Compare();
        //inputValueA.CallOnSourceChanged(Compare);
        //inputValueB.CallOnSourceChanged(Compare);
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void Compare()
    {
        switch (compareOperation)
        {
            case CompareOperation.Add:
                comparisonOutput.Set(inputValueA.Get()+inputValueB.Get());
                break;
            case CompareOperation.Subtract:
                comparisonOutput.Set(inputValueA-inputValueB);
                break;
            case CompareOperation.Multiply:
                comparisonOutput.Set(inputValueA*inputValueB);
                break;
            case CompareOperation.Divide:
                comparisonOutput.Set(inputValueA.Get()/inputValueB.Get());
                break;
            case CompareOperation.Power:
                comparisonOutput.Set((int)Mathf.Pow(inputValueA.Get(), inputValueB.Get()));
                break;
            case CompareOperation.Root:
                comparisonOutput.Set((int)Mathf.Pow(inputValueA.Get(), 1f/inputValueB.Get()));
                break;
            case CompareOperation.Modulo:
                comparisonOutput.Set(inputValueA.Get() % inputValueB.Get());
                break;
            case CompareOperation.Min:
                comparisonOutput.Set((int)Mathf.Min(inputValueA.Get(), inputValueB.Get()));
                break;
            case CompareOperation.Max:
                comparisonOutput.Set((int)Mathf.Max(inputValueA.Get(), inputValueB.Get()));
                break;
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
