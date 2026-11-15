//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  A logic component that, when powered via StartTextEvent, starts a dialogue event that plays out a sequence of textboxes
//  This is a bit broken at the moment and basically just translates the logic
//  inputs and outputs to unity events, and assumes the prefab has those unity events wired up to a text event componentWhen powered starts the text event
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RivenFramework
{
    /// <summary>
    /// A logic component that, when powered via StartTextEvent, starts a dialogue event that plays out a sequence of textboxes
    /// </summary>
    public class LogicDialogueEvent : Logic
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            [Tooltip("When powered starts the text event")]
            public LogicInput<bool> startTextEvent = new(false);
            [Tooltip("When powered, stops all currently running text events")]
            public LogicInput<bool> haltTextEvent = new(false);
            [Tooltip("This event is called when the text event is successfully started")]
            public UnityEvent onStartTextEvent;
            [Tooltip("This event is called when the text event is successfully stopped")]
            public UnityEvent onHaltTextEvent;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void Start()
            {
                if (startTextEvent.HasLogicOutputSource is true)
                {
                    startTextEvent.CallOnSourceChanged(StartTextEvent);
                }
                if (haltTextEvent.HasLogicOutputSource is true)
                {
                    haltTextEvent.CallOnSourceChanged(HaltTextEvent);
                }
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private void StartTextEvent()
            {
                if (startTextEvent.Get()) onStartTextEvent.Invoke();
            }

            private void HaltTextEvent()
            {
                if (haltTextEvent.Get()) onHaltTextEvent.Invoke();
            }
        #endregion
        #endregion
    }
}
