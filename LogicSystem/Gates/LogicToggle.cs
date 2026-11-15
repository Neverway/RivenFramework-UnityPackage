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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RivenFramework
{
    public class LogicToggle : Logic
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Space]
            [Tooltip("Output will persist as TRUE as soon as input is TRUE once")]
            public bool stayPowered = false;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            public LogicInput<bool> input = new(false);
            public LogicOutput<bool> output = new(false);
            public LogicInput<bool> reset = new(false);
            [Space]
            [Tooltip("This event will only fire when the output is powered")]
            public UnityEvent onOutputPowered;
            [Tooltip("This event will only fire when the output is unpowered")]
            public UnityEvent onOutputUnpowered;
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
                if (input != null) input.CallOnSourceChanged(Toggle);
                if (reset != null) reset.CallOnSourceChanged(Reset);
            }

            private void Reset()
            {
                output.Set(false);
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private void Toggle()
            {
                //Trigger the toggle only when input signal is TRUE
                if (input.Get() is false) return;
                if (output.Get() && stayPowered) return;

                //Toggle the output state (Always set to TRUE if "stayPowered" is TRUE)
                output.Set(!output);

                //Invoke events for change of power state
                if (output) onOutputPowered.Invoke();
                else onOutputUnpowered.Invoke();
            }
        #endregion
        #endregion
    }
}
