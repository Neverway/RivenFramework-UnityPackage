//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicDebugIndicator : Logic
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicInput<bool> input = new(false);
    public LogicOutput<bool> output = new(false);
    public Sprite indicatorOn, indicatorOff;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private SpriteRenderer spriteRenderer;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void Update()
    {
        if (input)
        {
            output.Set(true);
            spriteRenderer.sprite = indicatorOn;
        }
        else
        {
            output.Set(false);
            spriteRenderer.sprite = indicatorOff;
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
