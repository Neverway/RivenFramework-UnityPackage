//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PC_FirstPerson : PawnController, IUsePawnBehaviour<PB_HealthHaver>
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    protected PB_HealthHaver healthBehaviour;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public override void OnActorUpdate()
    {
        base.OnActorUpdate();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    public void StabSelf()
    {
        Debug.Log("* Squelchy Stab!");
        healthBehaviour.ModifyHealth(-10);
    }

    public void UnstabSelf()
    {
        Debug.Log("* Squelchy UNStab!");
        healthBehaviour.ModifyHealth(10);
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion

    public void AssignPawnBehaviour(PB_HealthHaver _behaviour) => healthBehaviour = _behaviour;
}
