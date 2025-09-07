//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VolumeLevelStreamContainer : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public Vector3 exitOffset;
    public bool initializedExitZone;
    public GameObject parentStreamVolume;
    private bool hasActivated;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_WorldLoader worldLoader;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void FixedUpdate()
    {
        if (!initializedExitZone) return;
        if (!worldLoader) worldLoader = FindObjectOfType<GI_WorldLoader>();
        if (!parentStreamVolume && !hasActivated)
        {
            print($"[{gameObject.name}] Link to parent is broken, scene must have changed");
            if (SceneManager.GetSceneByName(worldLoader.streamingWorldID).isLoaded)
            {
                hasActivated = true;
                print($"[{gameObject.name}] {worldLoader.streamingWorldID} returned isLoaded as true");
                StartCoroutine(EjectStreamedActors());
            }
        }
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public IEnumerator EjectStreamedActors()
    {
        print($"[{gameObject.name}] Ejecting actors...");
        transform.position += exitOffset;
        yield return new WaitForEndOfFrame();
        while (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject actor = transform.GetChild(i).gameObject;
                actor.transform.SetParent(null);
            }
        }
        yield return new WaitForEndOfFrame();
        print($"[{gameObject.name}] My job is done here, self-deleting!");
        Destroy(gameObject);
    }


    #endregion
}
