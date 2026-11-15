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
//  Keeps track of an int output value and gives inputs for adding and subtracting the current value
//====================================================================================================================//

using UnityEngine;

namespace RivenFramework
{
    /// <summary>
    /// Keeps track of an int output value and gives inputs for adding and subtracting the current value
    /// </summary>
    public class LogicCounter : Logic
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            [Tooltip("The current value for the output")]
            public int currentValue = 0;
            [Tooltip("When powered, adds the value in AddAmount to the current value")]
            public LogicInput<bool> inputAdd = new(false);
            [Tooltip("When powered, subtracts the value in SubtractAmount to the current value")]
            public LogicInput<bool> inputSubtract = new(false);
            [Tooltip("How much to add when inputAdd is powered")]
            public LogicInput<int> inputAddAmount = new(0);
            [Tooltip("How much to subtract when inputSubtract is powered")]
            public LogicInput<int> inputSubtractAmount = new(0);
            [Tooltip("The logic output of the current value")]
            public LogicOutput<int> output = new(0);
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
                if (inputAdd.HasLogicOutputSource) inputAdd.CallOnSourceChanged(Add);
                if (inputSubtract.HasLogicOutputSource) inputSubtract.CallOnSourceChanged(Subtract);
                if (inputAddAmount.HasLogicOutputSource) inputAddAmount.CallOnSourceChanged(AddAmount);
                if (inputSubtractAmount.HasLogicOutputSource) inputSubtractAmount.CallOnSourceChanged(SubtractAmount);
            }

            private void Update()
            {
                output.Set(currentValue);
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private void Add()
            {
                if (inputAdd == false) return; 
                currentValue += 1;
            }

            private void Subtract()
            {
                if (inputAdd == false) return; 
                currentValue -= 1;
            }

            private void AddAmount()
            {
                currentValue += inputAddAmount.Get();
            }

            private void SubtractAmount()
            {
                currentValue -= inputAddAmount.Get();
            }
        #endregion
        #endregion
    }
}
