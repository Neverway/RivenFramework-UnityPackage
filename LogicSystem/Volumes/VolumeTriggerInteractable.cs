//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeTriggerInteractable : Volume
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Header("Interactable Settings")]
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
    public LogicOutput<bool> onTriggered;


    //=-----------------=
    // Private Variables
    //=-----------------=
    [Tooltip("A variable to keep track of if this volume has already been trigger")] 
    [HideInInspector] public bool hasBeenTriggered;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    [Tooltip("This is the object that displays the sprite showing this object can be interacted with")]
    [HideInInspector] [SerializeField] private GameObject interactionIndicator;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        interactionIndicator = transform.GetChild(0).gameObject;
    }

    private new void OnTriggerEnter(Collider _other)
    { 
        // Call the base class method
        base.OnTriggerEnter(_other);
        
        SetInteractionIndicatorState();

        // Check for interaction
        var interaction = _other.GetComponent<VolumeTriggerInteraction>();
        if (interaction)
        {
            Interact(interaction);
        }
    }

    private new void OnTriggerExit(Collider _other)
    {/*
        base.OnTriggerExit(_other); // Call the base class method
        SetInteractionIndicatorState();

        // Disable the indicator if the player left
        // (Shouldn't `SetInteractionIndicatorState();` already take care of this?? ~Liz)
        if (_other.CompareTag("Pawn") && targetEntity.isPossessed)
        {
            //targetEntity.isNearInteractable = false;
        }*/
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void SetInteractionIndicatorState()
    {
        if (interactionIndicator.activeInHierarchy)
            interactionIndicator.GetComponent<Animator>().Play(useTalkIndicator ? "talk" : "use");

        if (GetPlayerInTrigger() && !hideIndicator)
        {
            interactionIndicator.SetActive(true);
        }
        else
        {
            interactionIndicator.SetActive(false);
        }
    }

    private void Interact(VolumeTriggerInteraction _interaction)
    {
        if (hasBeenTriggered && !resetsAutomatically || requireActivatingActorInside && !pawnsInTrigger.Contains(_interaction.owningPawn))
        {
            return;
        }

        //onInteract.Invoke();
        // Dear future me, please keep in mind that this will not be called unless the onInteractSignal is set. I don't know if I intended for it to work that way. (P.S. I am using "-" for empty activations) ~Past Liz M.
        // Dear past me, you are a fool and a coward. I fixed it. ~Future Liz M.
        // Dear past and future me, you are both clowns. Those systems were bad and are now deprecated. ~Future Future Liz M.

        // Flip the current activation state
        StartCoroutine(WaitUnpower());
        //previousIsPoweredState = isPowered;

        // Update connected devices
        hasBeenTriggered = true;
    }

    IEnumerator WaitUnpower()
    {
        yield return null;/*
        isPowered = true;
        yield return null;
        yield return new WaitForSeconds(secondsToStayPowered);
        isPowered = false;*/
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
