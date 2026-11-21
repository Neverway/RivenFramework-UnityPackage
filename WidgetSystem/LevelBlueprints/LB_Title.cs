//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  Show the title screen widget and unlock the mouse cursor
//====================================================================================================================//

using UnityEngine;

namespace RivenFramework
{
    public class LB_Title : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [SerializeField] private GameObject titleWidget;
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
                widgetManager.AddWidget(titleWidget);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            private void Update()
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
