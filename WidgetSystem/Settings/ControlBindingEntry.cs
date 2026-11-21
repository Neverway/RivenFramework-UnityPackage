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

using TMPro;
using UnityEngine;

namespace RivenFramework
{
    public class ControlBindingEntry : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            public TMP_Text text;
            public Image_KeyHint[] keyHints;
            public bool isComposite;
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
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public void SendRebindRequest()
            {
                Debug.Log($"[{this.name}] Executing function 'SendRebindRequest()'");
                FindObjectOfType<WB_Settings_Controls>().Rebind(keyHints[0].targetActionMap, keyHints[0].targetAction, isComposite);
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
