//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RivenFramework
{
    public class VolumeItemPickup : Volume
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            public TriggerFilter triggerFilter;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            public UnityEvent OnAttemptPickup;
            public LogicOutput<bool> OnSuccessfulPickup;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
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
                                    OnSuccessfulPickup.Set(true);
                                    gameObject.SetActive(false);
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
                                    OnSuccessfulPickup.Set(true);
                                    gameObject.SetActive(false);
                                }
                            }
                        }
                        break;
                }
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            public enum TriggerFilter
            {
                All,
                OnlyPlayer
            }
        #endregion
        #endregion
    }
}
