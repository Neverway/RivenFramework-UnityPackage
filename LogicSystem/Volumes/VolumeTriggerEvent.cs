//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
using UnityEngine;

public class VolumeTriggerEvent : Volume
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Header("Interactable Settings")]
    [Tooltip("If this is false, this trigger can only be activated once")]
    public bool resetsAutomatically = true;
    public LogicInput<bool> reset;
    public TriggerFilter triggerFilter;
    public LogicOutput<bool> onOccupied;


    public enum TriggerFilter
    {
        All,
        Pawns,
        Props,
        OnlyPlayer
    }
    //=-----------------=
    // Private Variables
    //=-----------------=
    [Tooltip("A variable to keep track of if this volume has already been trigger")] 
    [HideInInspector] public bool hasBeenTriggered;


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Update()
    {
        if (reset) Reset();
    }

    private new void OnTriggerEnter(Collider _other)
    { 
        // Call the base class method
        base.OnTriggerEnter(_other);
        onOccupied.Set(IsOccupied());
    }

    private new void OnTriggerExit(Collider _other)
    { 
        // Call the base class method
        base.OnTriggerExit(_other);
        onOccupied.Set(IsOccupied());
    }


    //=-----------------=
    // Internal Functions
    //=-----------------=
    private bool IsOccupied()
    {
        if (hasBeenTriggered && resetsAutomatically is false) return false;
        switch (triggerFilter)
        {
            case TriggerFilter.All:
                if (pawnsInTrigger.Count != 0 || propsInTrigger.Count != 0)
                {
                    return true;
                }
                else
                {
                    hasBeenTriggered = true;
                    return false;
                }
            case TriggerFilter.Pawns:
                if (pawnsInTrigger.Count != 0)
                {
                    return true;
                }
                else
                {
                    hasBeenTriggered = true;
                    return false;
                }
            case TriggerFilter.Props:
                if (propsInTrigger.Count != 0)
                {
                    return true;
                }
                else
                {
                    hasBeenTriggered = true;
                    return false;
                }
            case TriggerFilter.OnlyPlayer:
                if (GetPlayerInTrigger())
                {
                    return true;
                }
                else
                {
                    hasBeenTriggered = true;
                    return false;
                }
        }

        return false;
    }

    private void Reset()
    {
        hasBeenTriggered = false;
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
