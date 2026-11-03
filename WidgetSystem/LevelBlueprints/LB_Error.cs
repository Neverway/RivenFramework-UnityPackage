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
//  The level blueprint for the level error screeb
//  This just makes pressing any button load the _Title level
//====================================================================================================================//

using UnityEngine;

namespace RivenFramework
{
    public class LB_Error : MonoBehaviour
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
                SceneManager.LoadScene("_Title");
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