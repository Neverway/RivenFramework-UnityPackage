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

public class VolumeLevelStreamContainer : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public Vector3 exitPositionOffset;
    public Vector3 exitRotationOffset;
    public GameObject parentStreamVolume;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private Vector3 cachedExitWorldPosition;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_WorldLoader worldLoader;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        GI_WorldLoader.OnWorldLoaded += EjectActors;
    }

    private void OnDestroy()
    {
        GI_WorldLoader.OnWorldLoaded -= EjectActors;
    }



    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Cache the position of the streaming container before the old scene unloads
    /// </summary>
    public void PrepareForLoad()
    {
        //cachedExitWorldPosition = transform.position + exitPositionOffset;
    }

    /// <summary>
    /// Move all the actors out of the stream container and to the new scene once the new scene is loaded
    /// </summary>
    private void EjectActors()
    {
        print($"[{gameObject.name}] Ejecting to {cachedExitWorldPosition}, childCount={transform.childCount}");
        
        // Move the container and its contents to the exit poisiton
        transform.position += exitPositionOffset;

        // Put the children into a list so we can modify them without the list breaking
        List<GameObject> childActors = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            childActors.Add(transform.GetChild(i).gameObject);
        }
        
        // Eject the children into the new scene
        StartCoroutine(EjectChildrenIntoNewLevel(childActors));
    }

    private IEnumerator EjectChildrenIntoNewLevel(List<GameObject> childActors)
    {
        var anchor = SceneManager.GetActiveScene().GetRootGameObjects()[0];
        List<ValueTuple<Rigidbody, bool, Vector3, Vector3>> rigidbodyStates = new List<(Rigidbody, bool, Vector3, Vector3)>();
        
        for (int i = 0; i < childActors.Count; i++)
        {
            var actor = childActors[i].gameObject;
            actor.transform.SetParent(anchor.transform);
            actor.transform.SetParent(null);
            print($"[{actor.gameObject.name}] EJECTED");

            // Get all rigidbody components on this actor
            var rigidbodies = actor.GetComponentsInChildren<Rigidbody>();
            for (int j = 0; j < rigidbodies.Length; j++)
            {
                // Store their current kinematic state and velocity
                rigidbodyStates.Add((rigidbodies[j], rigidbodies[j].isKinematic, rigidbodies[j].velocity, rigidbodies[j].angularVelocity));
                // Set all rigidbody components to kinematic
                rigidbodies[j].isKinematic = true;
            }
            
            // Move those little bastards to their offset transforms
            //actor.root.position = cachedExitWorldPosition;
            //actor.root.Rotate(exitRotationOffset);
        }
        
        worldLoader = GameInstance.Get<GI_WorldLoader>();
        
        // Wait for the worldloader to finish loading the scene
        if (worldLoader.isLoading)
        {
            yield return new WaitForEndOfFrame();
        }
            
        // Restore kinematic states and velocity
        for (int j = 0; j < rigidbodyStates.Count; j++)
        {
            rigidbodyStates[j].Item1.isKinematic = rigidbodyStates[j].Item2;
            rigidbodyStates[j].Item1.velocity = rigidbodyStates[j].Item3;
            rigidbodyStates[j].Item1.angularVelocity = rigidbodyStates[j].Item4;
        }
        
        yield return new WaitForEndOfFrame();
        
        //Destroy(gameObject);
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
