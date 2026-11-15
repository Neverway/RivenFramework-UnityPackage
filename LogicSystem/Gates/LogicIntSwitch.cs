//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  Switches the output value between two int logic values based on if the input is powered
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    /// <summary>
    /// Switches the output value between two int logic values based on if the input is powered
    /// </summary>
    public class LogicIntSwitch : Logic
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            [Tooltip("Input on wether to use the powered or unpowered value")]
            public LogicInput<bool> powered = new(false);
            [Tooltip("The int value to output when this is not powered")]
            public LogicInput<int> unpoweredValue = new(0);
            [Tooltip("The int value to output when this is powered")]
            public LogicInput<int> poweredValue = new(0);
            [Tooltip("The current int output value")]
            public LogicOutput<int> output = new(0);
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void Update()
            {
                if (powered.Get()) output.Set(poweredValue);
                else output.Set(unpoweredValue);
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
