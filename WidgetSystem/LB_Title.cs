//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Show the title screen widget and unlock the mouse cursor
// Notes: 
//
//=============================================================================

using UnityEngine;

namespace Neverway.Framework.PawnManagement
{
    public class LB_Title : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=


        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=
        private GI_WidgetManager widgetManager;
        [SerializeField] private GameObject titleWidget;


        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Start()
        {
            widgetManager = FindObjectOfType<GI_WidgetManager>();
            widgetManager.AddWidget(titleWidget);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        //=-----------------=
        // Internal Functions
        //=-----------------=


        //=-----------------=
        // External Functions
        //=-----------------=
    }
}