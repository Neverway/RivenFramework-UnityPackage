//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicComparator : Logic
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicInput<int> inputValueA = new(0);
    public LogicInput<int> inputValueB = new(0);
    public LogicOutput<bool> comparisonOutput = new(false);
    public CompareOperation compareOperation;
    public enum CompareOperation
    {
        EqualTo=0,
        LessThan=1,
        GreaterThan=2,
        NotEqualTo=3,
        GreaterThanOrEqualTo=4,
        LessThanOrEqualTo=5
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
            case CompareOperation.EqualTo:
                comparisonOutput.Set(inputValueA.Get()==inputValueB.Get());
                break;
            case CompareOperation.LessThan:
                comparisonOutput.Set(inputValueA<inputValueB);
                break;
            case CompareOperation.GreaterThan:
                comparisonOutput.Set(inputValueA>inputValueB);
                break;
            case CompareOperation.NotEqualTo:
                comparisonOutput.Set(inputValueA.Get()!=inputValueB.Get());
                break;
            case CompareOperation.GreaterThanOrEqualTo:
                comparisonOutput.Set(inputValueA>=inputValueB);
                break;
            case CompareOperation.LessThanOrEqualTo:
                comparisonOutput.Set(inputValueA<=inputValueB);
                break;
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
