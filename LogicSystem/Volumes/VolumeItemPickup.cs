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
            [Todo_PoorlyCoded("Assuming the item object for an item pickup is the first child of the item pickup is probably a crappy way to handle this.")]
            private new void OnTriggerEnter(Collider _other)
            {
                base.OnTriggerEnter(_other);
                foreach (var pawn in pawnsInTrigger)
                {
                    var inventory = pawn.GetComponentInChildren<Pawn_Inventory>();
                    if (!inventory) continue;
                    OnAttemptPickup.Invoke();
                    // TODO find a better way to assign item objects as the functional item instead of assuming its the first child of this volume
                    // The reasoning here was that an item in a pawns inventory or hand shouldn't be any different than that same item sitting around to be picked up
                    // So the object that is the child of this volume IS the same object that pawns use. When a pawn picks up the item they just control the calling of the items functions
                    // As an example think of a gun. A gun can be shot well someone is holding it, but it can also still go off when falling off a table.
                    // A situation where this would be uses would also be something like the portal gun on the pedestal in portal 1.
                    // The portal gun on the pedestal can still fire portals even if it's not being held, so why waste resources coding two different items that do the same thing
                    if (inventory.AddItem(transform.GetChild(1).gameObject))
                    {
                        OnSuccessfulPickup.Set(true);
                        gameObject.SetActive(false);
                        return;
                    }
                }
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
