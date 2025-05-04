//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Show the loading screen widget
// Notes:
//
//=============================================================================

using UnityEngine;

public class LB_Travel : MonoBehaviour
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
    private GameObject loadingWidget;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        widgetManager = FindObjectOfType<GI_WidgetManager>();
        widgetManager.AddWidget(loadingWidget);
    }


    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}
