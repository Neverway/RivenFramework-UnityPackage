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

public class PC_FirstPerson_StabbingThinker : PC_FirstPerson
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public override void OnActorCreated()
    {
        base.OnActorCreated();
        ControlledPawn.StartCoroutine(ThinkingAboutStabbingSelf());
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private IEnumerator ThinkingAboutStabbingSelf()
    {
        while (healthBehaviour.isDead == false)
        {
            Debug.Log("I wonder if I should stab myself... ^3- ");
            yield return new WaitForSeconds(Random.Range(0, 7));
            int decided = Random.Range(0, 4);
            if (decided < 2)
            {
                Debug.Log("YIPEE TIME TO STAB MYSELF! XD ");
                StabSelf();
            }
            else if (decided == 2)
            {
                Debug.Log("Nah, the weather isn't really good for that right now. _-_");
            }
            else
            {
                healthBehaviour.ModifyHealth(10);
                Debug.Log("YIPPE STABBING TIME- OH NO THAT WAS MY UNSTABBING KNIFE!!!!! NOOOO I FEEL SO MUCH BETTER NOWWWWWWW..... -M-");
                UnstabSelf();
            }
        }
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
