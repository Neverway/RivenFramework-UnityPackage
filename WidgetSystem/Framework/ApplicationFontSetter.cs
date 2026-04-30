//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Defines a consistent default font to all text elements and allows
//  them to be replaced by the dyslexia friendly font
// Notes:
//
//=============================================================================

using System;
using TMPro;
using UnityEngine;

public class ApplicationFontSetter : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public TMP_FontAsset currentFont;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    [SerializeField] private TMP_FontAsset defaultFont, dyslexiaAssistFont;
    [SerializeField] private GI_WidgetManager widgetManager;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        widgetManager.OnNewWidgetCreated += AssignFontToNewWidget;
    }


    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void AssignFontToNewWidget()
    {
        foreach (var textElement in widgetManager.lastCreatedWidget.GetComponentsInChildren<TMP_Text>())
        {
            if (textElement.gameObject.GetComponent(typeof(Text_DontOverideFont))) continue;
            textElement.font = currentFont;
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
    public void SetAppFont(bool dyslexiaAssistEnabled)
    {
        currentFont = dyslexiaAssistEnabled ? dyslexiaAssistFont : defaultFont;

        foreach (var textElement in FindObjectsOfType<TMP_Text>())
        {
            if (textElement.gameObject.GetComponent(typeof(Text_DontOverideFont))) continue;
            textElement.font = currentFont;
        }
    }
}