//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicCounter : Logic
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public int currentValue = 0;
    public LogicInput<bool> inputAdd = new(false);
    public LogicInput<bool> inputSubtract = new(false);
    public LogicInput<int> inputAddAmount = new(0);
    public LogicInput<int> inputSubtractAmount = new(0);
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
    private void Start()
    {
        inputAdd.CallOnSourceChanged(Add);
        inputSubtract.CallOnSourceChanged(Subtract);
        inputAddAmount.CallOnSourceChanged(AddAmount);
        inputSubtractAmount.CallOnSourceChanged(SubtractAmount);
    }

    private void Update()
    {
        output.Set(currentValue);
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void Add()
    {
        if (inputAdd == false) return; 
        currentValue += 1;
    }
    private void Subtract()
    {
        if (inputAdd == false) return; 
        currentValue -= 1;
    }
    private void AddAmount()
    {
        currentValue += inputAddAmount.Get();
    }
    private void SubtractAmount()
    {
        currentValue -= inputAddAmount.Get();
    }
    


    //=-----------------=
    // External Functions
    //=-----------------=
}
