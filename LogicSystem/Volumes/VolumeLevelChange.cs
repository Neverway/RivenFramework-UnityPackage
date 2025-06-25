//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VolumeLevelChange : Volume
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public SceneReference targetScene;
    public string worldID;
    public bool useIndexInsteadOfID;
    public bool indexBackwards;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private GI_WorldLoader worldLoader;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void OnValidate()
    {
        // Todo: This is a temporary implementation! ~Liz
        targetScene.RefreshSceneName();
    }

    private new void OnTriggerEnter2D(Collider2D _other)
    {
        if (GetPlayerInTrigger())
        {
            if (!worldLoader) worldLoader = FindObjectOfType<GI_WorldLoader>();
            worldLoader.LoadWorld(worldID);
        }
    }

    private new void OnTriggerEnter(Collider _other)
    {
        base.OnTriggerEnter(_other);
        if (GetPlayerInTrigger())
        {
            //if (!_other.GetComponent<Pawn>().isPossessed) return;
            if (!worldLoader) worldLoader = FindObjectOfType<GI_WorldLoader>();
            if (useIndexInsteadOfID)
            {
                int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
                string scenePath = SceneUtility.GetScenePathByBuildIndex(nextSceneIndex);

                if (!string.IsNullOrEmpty(scenePath))
                {
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    worldLoader.LoadWorld(sceneName);
                }
            }
            else if (!useIndexInsteadOfID)
            {
                worldLoader.LoadWorld(targetScene.sceneName);
            }
        }
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}
