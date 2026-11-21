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

using RivenFramework;
using UnityEngine;

namespace RivenFramework
{
    public class GameplayNotificationManager : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [SerializeField] private GameObject notificationBoxWidget;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            private GI_WidgetManager widgetManager;
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
                widgetManager = GameInstance.Get<GI_WidgetManager>();
            }

            private void Update()
            {

            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public void DisplayKeyHint(float _duration, string _keyhintText, Sprite _keyhintImage)
            {
                if (!widgetManager.GetExistingWidget(notificationBoxWidget.name))
                {
                    widgetManager.AddWidget(notificationBoxWidget);
                }
                FindObjectOfType<WB_NotificationBox>().DisplayKeyHint(_duration, _keyhintText, _keyhintImage);
            }

            public void DisplayKeyHint(float _duration, string _keyhintText, string _targetActionMap, string _targetAction)
            {
                if (!widgetManager.GetExistingWidget(notificationBoxWidget.name))
                {
                    widgetManager.AddWidget(notificationBoxWidget);
                }
                FindObjectOfType<WB_NotificationBox>().DisplayKeyHint(_duration, _keyhintText, _targetActionMap, _targetAction);
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
