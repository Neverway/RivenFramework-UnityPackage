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
using RivenFramework;
using UnityEngine;
using UnityEngine.Events;

namespace RivenFramework
{
    public class VolumeTriggerEvent : Volume
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Header("Interactable Settings")] [Tooltip("If this is false, this trigger can only be activated once")]
            public bool resetsAutomatically = true;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            public LogicInput<bool> reset;
            public LogicOutput<bool> onOccupied;
            [Tooltip(
                "This event will only fire when something first enters (does not refire for subsequent entries until unoccupied)")]
            public UnityEvent onFirstOccupied;
            [Tooltip("This event will only fire when last one leaves")]
            public UnityEvent onFirstUnoccupied;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            [Tooltip("A variable to keep track of if this volume has already been trigger")] [HideInInspector]
            public bool hasBeenTriggered;
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            protected override void Update()
            {
                base.Update();
                if (reset) Reset();
            }

            [Todo ("We should update the state of this volume when it's disabled. If an occupant is non-crushable (doesn't get disabled in rift) then it should be removed from the list.")]
            private void OnDisable ()
            {
                base.OnDisable();
            }

            protected override void OnTriggerEnter(Collider _other)
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

            protected override void OnTriggerExit(Collider _other)
            {
                base.OnTriggerExit(_other);
                bool isOccupied = IsOccupied (); //This boolean prevents us from needing to call IsOccupied() twice. - Connor
                if (isOccupied == false) onFirstUnoccupied.Invoke();
                onOccupied.Set(isOccupied);
            }

            private void Reset()
            {
                hasBeenTriggered = false;
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private bool IsOccupied()
            {
                if (hasBeenTriggered && resetsAutomatically == false) return false;
                return pawnsInTrigger.Count != 0 || propsInTrigger.Count != 0;
            }

            private bool CanFireEvents()
            {
                if (hasBeenTriggered && !resetsAutomatically) return false;
                return true;
            }
        #endregion
        #endregion
    }
}
