//==========================================( Neverway 2024 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  Show the loading screen widget
//====================================================================================================================//

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
