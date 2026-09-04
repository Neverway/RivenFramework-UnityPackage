//==========================================( Neverway 2026 )=========================================================//
// Author
//  Errynei
//
// Contributors
//  Liz M.
//
//====================================================================================================================//

using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// This is the base class for any actor that can be controlled (Like via the player or the computer)
/// </summary>
public class PawnV2 : ActorV2
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Pawn Params")]
    [SerializeReference, Polymorphic] public PawnController CurrentController;
    [SerializeReference, Polymorphic] public PawnBehaviour[] Behaviours;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public void Awake()
    {
        // Some Errynei black-magic that allows behaviours to nest other behaviours
        foreach (var behaviour in Behaviours)
        {
            foreach (var otherBehaviour in Behaviours)
            {
                if (behaviour == otherBehaviour) continue;
                PawnBehaviourUtils.TryAssignPawnBehaviour(behaviour, otherBehaviour);
            }
        }

        foreach (var behaviour in Behaviours)
            behaviour.pawn = this;
        ForceSetController(CurrentController);
    }    
    
    public override void OnActorCreated()
    {
        foreach (var behaviour in Behaviours)
            behaviour.OnActorCreated();
        CurrentController?.OnActorCreated();
    }

    public override void OnActorDestroyed()
    {
        foreach (var behaviour in Behaviours)
            behaviour.OnActorDestroyed();
        
        CurrentController?.OnActorDestroyed();
    }

    public override void OnActorEnabled()
    {
        foreach (var behaviour in Behaviours)
            behaviour.OnActorEnabled();
        
        CurrentController?.OnActorEnabled();
    }

    public override void OnActorDisabled()
    {
        foreach (var behaviour in Behaviours)
            behaviour.OnActorDisabled();
        
        CurrentController?.OnActorDisabled();
    }

    public override void OnActorUpdate() 
    { 
        foreach (var behaviour in Behaviours)
            behaviour.OnActorUpdate();
        
        CurrentController?.OnActorUpdate();
    }

    public override void OnActorFixedUpdate()
    {
        foreach (var behaviour in Behaviours)
            behaviour.OnActorFixedUpdate();
        
        CurrentController?.OnActorFixedUpdate();
    }
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Returns true if the pawn currently has a controller
    /// </summary>
    public bool IsControlled => CurrentController != null;

    /// <summary>
    /// Sets the controller for this pawn if the pawn is not currently being controlled
    /// </summary>
    public bool TrySetController(PawnController _newController)
    {
        if (IsControlled) return false; // Pawn was already controlled so set failed
        ForceSetController(_newController);
        return true; // Pawn was not controlled so set succeeded

    }

    /// <summary>
    /// Sets the controller for this pawn regardless of if the pawn is currently being controlled
    /// </summary>
    public void ForceSetController(PawnController _newController)
    {
        PawnBehaviourUtils.ControlPawnWithTargetPawnController(this, _newController);
    }
        
    /// <summary>
    /// Removes the current controller for this pawn
    /// </summary>
    public void RemoveCurrentController()
    {
        PawnBehaviourUtils.RemoveControlFromPawn(this);
    }


    #endregion
}
