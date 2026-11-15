//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  Outputs the opposite of whatever the input value is
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    /// <summary>
    /// Outputs the opposite of whatever the input value is
    /// </summary>
    /// <summary>
    /// The input value to invert (This can also just be used as an ambient power source if the input is left disconnected)
    /// </summary>
    public class LogicNot : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            [Tooltip("The input value to invert")]
            public LogicInput<bool> input = new(false);
            [Tooltip("The resulting inverted output value")]
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
                //input.CallOnSourceChanged(TestInputs);
            }

            private void Update()
            {
                output.Set(!input);
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
