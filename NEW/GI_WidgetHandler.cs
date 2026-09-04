//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

[Serializable]
[GIModuleColor(_color: GIModuleColors.Blue)]
public class GI_WidgetHandler : GameInstanceModule
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Project Configuration")]
    [Tooltip("This is the list of user interface widgets that can be shown and hidden")]
    public List<WidgetBlueprint> widgets;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private const string CANVAS_GAMEOBJECT_TAG = "UserInterface";
    private GameObject _canvas;
    private GameObject Canvas
    {
        get
        {
            if (_canvas == null)
                _canvas = GameObject.FindWithTag(CANVAS_GAMEOBJECT_TAG);
            return _canvas;
        }
        
    }
    [Header("Debugging")]
    public Action OnNewWidgetCreated;
    public WidgetBlueprint lastCreatedWidget;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
