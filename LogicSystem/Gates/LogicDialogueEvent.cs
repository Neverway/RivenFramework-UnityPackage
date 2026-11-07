//==========================================( Neverway 2024 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RivenFramework
{
    public class LogicDialogueEvent : Logic
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        public LogicInput<bool> startTextEvent = new(false);
        public LogicInput<bool> haltTextEvent = new(false);


        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=
        public UnityEvent onStartTextEvent;
        public UnityEvent onHaltTextEvent;


        //=-----------------=
        // Mono Functions
        //=-----------------=
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

        //=-----------------=
        // Internal Functions
        //=-----------------=
        private void StartTextEvent()
        {
            if (startTextEvent.Get()) onStartTextEvent.Invoke();
        }
        private void HaltTextEvent()
        {
            if (haltTextEvent.Get()) onHaltTextEvent.Invoke();
        }


        //=-----------------=
        // External Functions
        //=-----------------=
    }
}
