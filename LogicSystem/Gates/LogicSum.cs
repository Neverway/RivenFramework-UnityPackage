//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  Outputs a logic int of the result of two logic int input values given an operation like addition or subtraction
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    /// <summary>
    /// Outputs a logic int of the result of two logic int input values given an operation like addition or subtraction
    /// </summary>
    public class LogicSum : Logic
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Tooltip("The operation to use on values A and B")]
            public CompareOperation compareOperation;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            [Tooltip("The first int input value to compare")]
            public LogicInput<int> inputValueA = new(0);
            [Tooltip("The second int input value to compare")]
            public LogicInput<int> inputValueB = new(0);
            [Tooltip("The int output result of adding A and B")]
            public LogicOutput<int> comparisonOutput = new(0);
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
                Compare();
                //inputValueA.CallOnSourceChanged(Compare);
                //inputValueB.CallOnSourceChanged(Compare);
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            public enum CompareOperation
            {
                Add,
                Subtract,
                Multiply,
                Divide,
                Power,
                Root,
                Modulo,
                Min,
                Max,
            }

            private void Compare()
            {
                switch (compareOperation)
                {
                    case CompareOperation.Add:
                        comparisonOutput.Set(inputValueA.Get()+inputValueB.Get());
                        break;
                    case CompareOperation.Subtract:
                        comparisonOutput.Set(inputValueA-inputValueB);
                        break;
                    case CompareOperation.Multiply:
                        comparisonOutput.Set(inputValueA*inputValueB);
                        break;
                    case CompareOperation.Divide:
                        comparisonOutput.Set(inputValueA.Get()/inputValueB.Get());
                        break;
                    case CompareOperation.Power:
                        comparisonOutput.Set((int)Mathf.Pow(inputValueA.Get(), inputValueB.Get()));
                        break;
                    case CompareOperation.Root:
                        comparisonOutput.Set((int)Mathf.Pow(inputValueA.Get(), 1f/inputValueB.Get()));
                        break;
                    case CompareOperation.Modulo:
                        comparisonOutput.Set(inputValueA.Get() % inputValueB.Get());
                        break;
                    case CompareOperation.Min:
                        comparisonOutput.Set((int)Mathf.Min(inputValueA.Get(), inputValueB.Get()));
                        break;
                    case CompareOperation.Max:
                        comparisonOutput.Set((int)Mathf.Max(inputValueA.Get(), inputValueB.Get()));
                        break;
                }
            }
        #endregion
        #endregion
    }
}
