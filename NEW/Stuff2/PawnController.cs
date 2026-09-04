//==========================================( Neverway 2026 )=========================================================//
// Author
//  Errynei
//
// Contributors
//  Liz M.
//
//====================================================================================================================//

/// <summary>
/// The class instance that controls a pawn and it's behaviours,
/// like a first-person player, an npc with wolf AI, an npc with bird AI, a first-person network player, etc.
/// </summary>
public abstract class PawnController : IActorFunctions
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public PawnV2 ControlledPawn;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/

    public virtual void OnActorCreated()
    {
    }

    public virtual void OnActorDestroyed()
    {
    }

    public virtual void OnActorEnabled()
    {
    }

    public virtual void OnActorDisabled()
    {
    }

    public virtual void OnActorUpdate()
    {
    }

    public virtual void OnActorFixedUpdate()
    {
    }
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
        
    public virtual void OnStartControl() { }

    public virtual void OnStopControl() { }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
        
    public void StartControl(PawnV2 _pawn)
    {
        PawnBehaviourUtils.ControlPawnWithTargetPawnController(_pawn, this);
    }
        
    public void StopControl()
    {
        PawnBehaviourUtils.RemoveControlFromPawn(ControlledPawn);
    }


    #endregion
}
