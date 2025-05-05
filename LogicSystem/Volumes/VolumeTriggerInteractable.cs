//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeTriggerInteractable : Volume
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Tooltip("If this is false, this trigger can only be activated once")]
    public bool resetsAutomatically = true;
    [Tooltip("If this is false, a little indicator will appear above this volume to show the player it can be interacted with")]
    public bool hideIndicator;
    [Tooltip("If enabled, the indicator will show a speech bubble instead of the interact indicator")]
    public bool useTalkIndicator;
    [Tooltip("If enabled, then the actor who created the interaction volume must also be inside this trigger")]
    public bool requireActivatingActorInside = true;
    [Tooltip("How many seconds to remain powered when pressing interact")]
    public float secondsToStayPowered = 0.2f;
    [Tooltip("This powers logic components when interacted with")]
    public LogicInput<bool> onTriggered;


    //=-----------------=
    // Private Variables
    //=-----------------=
    [Tooltip("A variable to keep track of if this volume has already been trigger")] 
    [HideInInspector] public bool hasBeenTriggered;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    [Tooltip("This is the object that displays the sprite showing this object can be interacted with")]
    [SerializeField] private GameObject interactionIndicator;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private new void OnTriggerEnter(Collider _other)
    {
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}
