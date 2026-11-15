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
//  Takes two int input values and uses a compare operator to determin if the output of the comparison is true or false
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    /// <summary>
    /// Takes two int input values and uses a compare operator to determin if the output of the comparison is true or false
    /// </summary>
    public class LogicComparator : Logic
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            [Tooltip("The first int value to compare")]
            public LogicInput<int> inputValueA = new(0);
            [Tooltip("The second int value to compare")]
            public LogicInput<int> inputValueB = new(0);
            [Tooltip("The result of the comparison")]
            public LogicOutput<bool> comparisonOutput = new(false);
            [Tooltip("The operator to use for the comparsion")]
            public CompareOperation compareOperation;

            public enum CompareOperation
            {
                EqualTo=0,
                LessThan=1,
                GreaterThan=2,
                NotEqualTo=3,
                GreaterThanOrEqualTo=4,
                LessThanOrEqualTo=5
            }
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
            private void Compare()
            {
                switch (compareOperation)
                {
                    case CompareOperation.EqualTo:
                        comparisonOutput.Set(inputValueA.Get()==inputValueB.Get());
                        break;
                    case CompareOperation.LessThan:
                        comparisonOutput.Set(inputValueA<inputValueB);
                        break;
                    case CompareOperation.GreaterThan:
                        comparisonOutput.Set(inputValueA>inputValueB);
                        break;
                    case CompareOperation.NotEqualTo:
                        comparisonOutput.Set(inputValueA.Get()!=inputValueB.Get());
                        break;
                    case CompareOperation.GreaterThanOrEqualTo:
                        comparisonOutput.Set(inputValueA>=inputValueB);
                        break;
                    case CompareOperation.LessThanOrEqualTo:
                        comparisonOutput.Set(inputValueA<=inputValueB);
                        break;
                }
            }
        #endregion
        #endregion
    }
}
