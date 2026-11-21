//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  Defines a consistent default font to all text elements and allows them to be replaced by the dyslexia friendly
//  font
//====================================================================================================================//

using System;
using TMPro;
using UnityEngine;

namespace RivenFramework
{
    public class ApplicationFontSetter : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            public TMP_FontAsset currentFont;
            [SerializeField] private TMP_FontAsset defaultFont, dyslexiaAssistFont;
            [SerializeField] private GI_WidgetManager widgetManager;
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
                widgetManager.OnNewWidgetCreated += AssignFontToNewWidget;
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public void SetAppFont(bool dyslexiaAssistEnabled)
            {
                currentFont = dyslexiaAssistEnabled ? dyslexiaAssistFont : defaultFont;

                foreach (var textElement in FindObjectsOfType<TMP_Text>())
                {
                    if (textElement.gameObject.GetComponent(typeof(Text_DontOverideFont))) continue;
                    textElement.font = currentFont;
                }
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private void AssignFontToNewWidget()
            {
                foreach (var textElement in widgetManager.lastCreatedWidget.GetComponentsInChildren<TMP_Text>())
                {
                    if (textElement.gameObject.GetComponent(typeof(Text_DontOverideFont))) continue;
                    textElement.font = currentFont;
                }
            }
        #endregion
        #endregion
    }
}
