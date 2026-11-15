//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//  Errynei
//  Connorses
//  Soulex
//
// Description
//  Sets the output to being powered only if both inputs are powered
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    /// <summary>
    /// Sets the output to being powered only if both inputs are powered
    /// </summary>
    public class LogicAnd : Logic
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            [Tooltip("The first input to check for power")]
            public LogicInput<bool> inputA = new(false);
            [Tooltip("The second input to check for power")]
            public LogicInput<bool> inputB = new(false);
            [Tooltip("The result of if both inputs are powered")]
            public LogicOutput<bool> output = new(false);
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
                output.Set(inputA && inputB);
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
