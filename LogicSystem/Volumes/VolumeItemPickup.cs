//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VolumeItemPickup : Volume
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public TriggerFilter triggerFilter;
    public enum TriggerFilter
    {
        All,
        OnlyPlayer
    }

    public UnityEvent OnAttemptPickup;
    
    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private new void OnTriggerEnter(Collider _other)
    { 
        // Call the base class method
        base.OnTriggerEnter(_other);
        switch (triggerFilter)
        {
            case TriggerFilter.All:
                if (pawnsInTrigger.Count != 0)
                {
                    var inventory = pawnsInTrigger[0].GetComponentInChildren<Pawn_Inventory>();
                    if (inventory)
                    {
                        OnAttemptPickup.Invoke();
                        if (inventory.AddItem(transform.GetChild(1).gameObject))
                        {
                            Destroy(gameObject);
                        }
                    }
                }
                break;
            case TriggerFilter.OnlyPlayer:
                if (GetPlayerInTrigger())
                {
                    var inventory = GetPlayerInTrigger().GetComponentInChildren<Pawn_Inventory>();
                    if (inventory)
                    {
                        OnAttemptPickup.Invoke();
                        if (inventory.AddItem(transform.GetChild(1).gameObject))
                        {
                            Destroy(gameObject);
                        }
                    }
                }
                break;
        }
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/

    #endregion
}
