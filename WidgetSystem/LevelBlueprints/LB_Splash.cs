//==========================================( Neverway 2027 )=========================================================//
// Author
//
// Contributors
//  Liz M.
//  Errynei
//  Connorses
//  Soulex
//
// Description
//  The level blueprint for the splash screen
//  This just makes pressing any button skip the splash screen and load the _Title level
//====================================================================================================================//

using UnityEngine;

namespace RivenFramework
{
    public class LB_Splash : MonoBehaviour
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
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
            if (Input.anyKeyDown)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion
    }
}