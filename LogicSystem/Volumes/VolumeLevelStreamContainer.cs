//==========================================( Neverway 2025 )=========================================================//
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

public class VolumeLevelStreamContainer : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public Vector3 exitPositionOffset;
    public Vector3 exitRotationOffset;
    public bool initializedExitZone;
    public GameObject parentStreamVolume;
    private bool hasActivated;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private bool subscribedToEjectEvent;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_WorldLoader worldLoader;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void FixedUpdate()
    {
        if (!initializedExitZone) return;
        if (!worldLoader)
        {
            worldLoader = FindObjectOfType<GI_WorldLoader>();
            return;
        }
        
        // Subscribe to eject event
        if (!subscribedToEjectEvent)
        {
            subscribedToEjectEvent = true;
            //print($"{gameObject.name} subscribed to eject event");
            GI_WorldLoader.OnEjectStreamedActors += EjectStreamedActors;
        }
        
        /*if (!parentStreamVolume && !hasActivated && !worldLoader.isLoading)
        {
            print($"[{gameObject.name}] Link to parent is broken, scene must have changed");
            if (SceneManager.GetSceneByName(worldLoader.streamingWorldID).isLoaded)
            {
                print($"[{gameObject.name}] {worldLoader.streamingWorldID} returned isLoaded as true");
                StartCoroutine(EjectStreamedActors());
            }
        }*/
        
        if (initializedExitZone && subscribedToEjectEvent)
        {
            //print($"[{gameObject.name}] Container has {transform.childCount} children, scene: {gameObject.scene.name}");
        }
    }

    private void OnDestroy()
    {
        GI_WorldLoader.OnEjectStreamedActors -= EjectStreamedActors;
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    private void EjectStreamedActors()
    {
        StartCoroutine(EjectStreamedActorsCoroutine());
    }
        
    public IEnumerator EjectStreamedActorsCoroutine()
    {
        if (hasActivated) yield break;
        hasActivated = true;
    
        print($"[{gameObject.name}] Ejecting {transform.childCount} actors...");

        while (transform.childCount != 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform actor = transform.GetChild(i);
            
                // Apply offsets directly to each actor in world space
                actor.position += exitPositionOffset;
                actor.Rotate(exitRotationOffset);
            
                actor.SetParent(null);
                SceneManager.MoveGameObjectToScene(actor.gameObject, SceneManager.GetActiveScene());
                print($"[{actor.name}] ejected to {actor.position}");
            }
        }

        yield return null;
        Destroy(gameObject);
    }


    #endregion
}
