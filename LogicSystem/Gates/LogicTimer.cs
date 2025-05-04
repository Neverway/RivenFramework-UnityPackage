//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicTimer : Logic
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicInput<bool> startTimer = new(false);
    public LogicInput<int> timerDuration = new(5);
    public LogicOutput<int> currentTime = new(5);
    public LogicOutput<bool> timerCompleted = new(false);


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
        startTimer.CallOnSourceChanged(BeginCountdown);
        timerDuration.CallOnSourceChanged(StopAllCoroutines);
    }
    

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void BeginCountdown()
    {
        StopAllCoroutines();
        StartCoroutine(nameof(Countdown));
    }

    private IEnumerator Countdown()
    {
        if (startTimer == false) yield break;
        currentTime.Set(timerDuration);
        
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1);
            currentTime.Set(currentTime - 1);
            timerCompleted.Set(currentTime <= 0);
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
