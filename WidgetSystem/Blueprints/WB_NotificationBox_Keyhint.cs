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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RivenFramework
{
    public class WB_NotificationBox_Keyhint : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [SerializeField] private Image_KeyHint keyHint;
            [SerializeField] private TMP_Text keyhintText;
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
            private void Start()
            {

            }

            private void Update()
            {

            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public void SetKeyHint(string _keyhintText, Sprite _keyhintImage)
            {
                keyhintText.text = _keyhintText;
                keyHint.enabled = false;
                keyHint.gameObject.GetComponent<Image>().sprite = _keyhintImage;
                GetComponent<Animator>().Play("WB_NotificationBox_Keyhint_appear");
            }

            public void SetKeyHint(string _keyhintText, string _targetActionMap, string _targetAction)
            {
                keyhintText.text = _keyhintText;
                keyHint.enabled = true;
                keyHint.targetActionMap = _targetActionMap;
                keyHint.targetAction = _targetAction;
                GetComponent<Animator>().Play("WB_NotificationBox_Keyhint_appear");
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
