//==========================================( Neverway 2026 )=========================================================//
// Author
//  Liz M.
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
using UnityEngine.SceneManagement;

public class VolumeLevelChange : Volume
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public SceneReference targetScene;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_WorldLoader worldLoader;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void OnValidate()
    {
        // Todo: This is a temporary implementation! ~Liz
        targetScene.RefreshSceneName();
    }

    private new void OnTriggerEnter(Collider _other)
    {
        base.OnTriggerEnter(_other);
        if (GetPlayerInTrigger())
        {
            if (!worldLoader) worldLoader = GameInstance.Get<GI_WorldLoader>();

            foreach (var streamVolume in FindObjectsOfType<VolumeLevelStream>())
            {
                streamVolume.PrepareForLoad();
            }

            worldLoader.LoadWorld(targetScene.sceneName);
        }
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
