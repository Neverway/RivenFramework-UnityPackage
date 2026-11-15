//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  Someone is gonna have to explain nand gates to me again ~Liz
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    /// <summary>
    /// Someone is gonna have to explain nand gates to me again ~Liz
    /// </summary>
    public class LogicNand : Logic
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            public LogicInput<bool> inputA = new(false);
            public LogicInput<bool> inputB = new(false);
            public LogicOutput<bool> output = new(false);
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
                //TestInputs();
                //inputA.CallOnSourceChanged(TestInputs);
                //inputB.CallOnSourceChanged(TestInputs);
            }

            private void Update()
            {
                output.Set(!(inputA && inputB));
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
