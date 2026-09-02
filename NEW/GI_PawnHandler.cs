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
using RivenFramework;
using UnityEngine;

[Serializable]
[GIModuleColor(_color: GIModuleColors.Blue)]
public class GI_PawnHandler : GameInstanceModule
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Header("References")]
    [Tooltip("This is the prefab for the main view camera object that the game renders to. One should always be present while the game is running.")]
    public GameObject viewCameraPrefab;
    
    [Header("Debugging")]
    [Tooltip("This is a reference to the main camera the local player sees from. If this is currently null, a new viewCameraPrefab object will be created and fill in this field.")]
    public Camera viewCamera;
    [Tooltip("This is a reference to the pawn in the scene that is currently being controlled by the local player.")]
    public Pawn localPlayerPawn;
    [Tooltip("A list of all pawns that are currently loaded.")]
    public List<Pawn> cachedPawns;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
