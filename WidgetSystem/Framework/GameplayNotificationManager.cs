//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using RivenFramework;
using UnityEngine;

public class GameplayNotificationManager : MonoBehaviour
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
    [SerializeField] private GameObject notificationBoxWidget;
    private GI_WidgetManager widgetManager;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        widgetManager = GameInstance.Get<GI_WidgetManager>();
    }

    private void Update()
    {
    
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
    
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
}
