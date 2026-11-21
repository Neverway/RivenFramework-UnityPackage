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
    public class WB_NotificationBox : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [SerializeField] private Transform root;
            [SerializeField] private GameObject keyhintWidget;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private float timeTillDeath;
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
            public void DisplayKeyHint(float _duration, string _keyhintText, Sprite _keyhintImage)
            {
                StopCoroutine(DeathTimer());
                StartCoroutine(DeathTimer());
                var keyhint = Instantiate(keyhintWidget, root);
                keyhint.GetComponent<WB_NotificationBox_Keyhint>().SetKeyHint(_keyhintText, _keyhintImage);
                Destroy(keyhint, _duration);
            }

            public void DisplayKeyHint(float _duration, string _keyhintText, string _targetActionMap, string _targetAction)
            {
                StopCoroutine(DeathTimer());
                StartCoroutine(DeathTimer());
                var keyhint = Instantiate(keyhintWidget, root);
                keyhint.transform.SetAsFirstSibling();
                keyhint.GetComponent<WB_NotificationBox_Keyhint>().SetKeyHint(_keyhintText, _targetActionMap, _targetAction);
                Destroy(keyhint, _duration);
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private IEnumerator DeathTimer()
            {
                yield return new WaitForSeconds(timeTillDeath);
                //Destroy(gameObject);
            }
        #endregion
        #endregion
    }
}
