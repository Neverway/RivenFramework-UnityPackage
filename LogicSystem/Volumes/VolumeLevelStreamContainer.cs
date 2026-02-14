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
        
        // Adjust container to its offset
        transform.position += exitPositionOffset;
        transform.Rotate(exitRotationOffset);
        yield return new WaitForEndOfFrame();
        
        // Empty the container into the streaming world then dump into the active scene
        // The while loop is here since the for loop doesn't finish in time due to... witchcraft probably
        // (Sorry Errynei, the while loop has to stay) ~Liz
        while (transform.childCount != 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject actor = transform.GetChild(i).gameObject;
                print($"[{actor.name}] ejected");
                actor.transform.SetParent(null);
            }
        }
        
        // I don't think this 'wait' is necessary, but I am terrified of the consequences of removing it! ~Liz
        yield return new WaitForFixedUpdate();
        
        //print($"[{gameObject.name}] My job is done here, self-deleting!");
        Destroy(gameObject); // <= Bye bye :3
    }


    #endregion
}
