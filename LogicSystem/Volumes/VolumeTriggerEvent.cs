//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.Events;

public class VolumeTriggerEvent : Volume
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Header("Interactable Settings")] [Tooltip("If this is false, this trigger can only be activated once")]
    public bool resetsAutomatically = true;

    public LogicInput<bool> reset;
    public TriggerFilter triggerFilter;
    public LogicOutput<bool> onOccupied;

    [Tooltip(
        "This event will only fire when something first enters (does not refire for subsequent entries until unoccupied)")]
    public UnityEvent onFirstOccupied;

    [Tooltip("This event will only fire when last one leaves")]
    public UnityEvent onFirstUnoccupied;


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
    [Tooltip("A variable to keep track of if this volume has already been trigger")] [HideInInspector]
    public bool hasBeenTriggered;


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private new void Update()
    {
        base.Update();
        if (reset) Reset();
    }

    private new void OnTriggerEnter(Collider _other)
    {
        bool wasOccupied = IsOccupied();
        base.OnTriggerEnter(_other);
        bool isOccupied = IsOccupied (); //This boolean prevents us from needing to call IsOccupied() twice. - Connor
        if (isOccupied && wasOccupied == false)
        {
            hasBeenTriggered = true;
            onFirstOccupied.Invoke();
        }
        onOccupied.Set(isOccupied);
    }

    private new void OnTriggerExit(Collider _other)
    {
        base.OnTriggerExit(_other);
        bool isOccupied = IsOccupied (); //This boolean prevents us from needing to call IsOccupied() twice. - Connor
        if (isOccupied == false) onFirstUnoccupied.Invoke();
        onOccupied.Set(isOccupied);
    }


    //=-----------------=
    // Internal Functions
    //=-----------------=
    private bool IsOccupied()
    {
        if (hasBeenTriggered && resetsAutomatically == false)
        {
            return false;
        }
        switch (triggerFilter)
        {
            case TriggerFilter.All: return pawnsInTrigger.Count != 0 || propsInTrigger.Count != 0;
            case TriggerFilter.Pawns: return pawnsInTrigger.Count != 0;
            case TriggerFilter.Props: return propsInTrigger.Count != 0;
            case TriggerFilter.OnlyPlayer: return GetPlayerInTrigger() != null;
        }

        return false;
    }

    private bool CanFireEvents()
    {
        if (hasBeenTriggered && !resetsAutomatically) return false;
        return true;
    }

    private void Reset()
    {
        hasBeenTriggered = false;
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
