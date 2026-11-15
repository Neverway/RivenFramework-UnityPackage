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

namespace RivenFramework
{
    public class LogicTimer : Logic
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            public LogicInput<bool> startTimer = new(false);
            public LogicInput<int> timerDuration = new(5);
            public LogicOutput<int> currentTime = new(5);
            public LogicOutput<bool> timerCompleted = new(false);
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private Coroutine timerRoutine;
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void Start()
            {
                startTimer.CallOnSourceChanged(BeginCountdown);
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private void BeginCountdown()
            {
                if (timerRoutine != null) return;
                timerRoutine = StartCoroutine(Countdown());
            }

            private IEnumerator Countdown()
            {
                timerCompleted.Set(false);

                currentTime.Set(timerDuration);

                while (currentTime > 0)
                {
                    yield return new WaitForSeconds(1);
                    currentTime.Set(currentTime - 1);
                }

                timerCompleted.Set(true);

                timerRoutine = null;
            }
        #endregion
        #endregion
    }
}
