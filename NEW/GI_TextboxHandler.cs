//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using MarkupAttributes;
using RivenFramework;
using UnityEngine;

[Serializable]
[GIModuleColor(_color: GIModuleColors.Blue)]
public class GI_TextboxHandler : GameInstanceModule
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Project Configuration")]
    public float normalTextTypeDelay;
    public float skippingTextTypeDelay;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [Header("Debugging")]
    [ErryBox] public TextEvent currentTextEvent;
    public bool HasActiveTextEvent => textEventActive;
    public bool textEventActive;

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    public bool currentlyPrinting;
    public string currentTextContent;
    public float currentTextTypeDelay;
    public int currentFrame;
    public bool performingRegularMarkup, performingSpecialMarkup;
    public int markupStartIndex;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_WidgetManager widgetManager;
    private WB_Textbox textbox;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
