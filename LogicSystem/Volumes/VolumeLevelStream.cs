//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VolumeLevelStream : Volume
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    [Tooltip("This is the offset that will be applied to objects within this volume when the level changes")]
    [SerializeField] private Vector3 exitPositionOffset;
    [SerializeField] private Vector3 exitRotationOffset;
    [SerializeField] private bool debugDrawExitZone;
    private bool initializedExitZone;
    

    //=-----------------=
    // Reference Variables
    //=-----------------=
    private GI_WorldLoader worldLoader;
    [Tooltip("This is the empty game object that streamed actors are stored in, (to save them from being destroyed on map changes)")]
    private VolumeLevelStreamContainer streamContainer;


    //=-----------------=
    // Mono Functions
    //=-----------------=
        private void Awake()
        {
            worldLoader = FindObjectOfType<GI_WorldLoader>();
        }

        private void FixedUpdate()
        {
            StartCoroutine(InitializeStreamContainer());
        }

        private void OnDrawGizmos()
        {
            if (!debugDrawExitZone) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position+exitPositionOffset, transform.localScale);
        }

        private new void OnTriggerStay(Collider _other)
        {
            if (!initializedExitZone) return;
            
            // Pawn has entered the volume
            if (_other.CompareTag("Pawn"))
            {
                // Get a reference to the entity component
                var targetEntity = _other.gameObject.GetComponent<Pawn>();
                
                // Exit if the object is already parented
                if (targetEntity.transform.parent == streamContainer.transform) return;
                
                // Add the entity to the list if they are not already present
                MoveObjectToStreamContainer(targetEntity.gameObject);
            }

            // A physics prop has entered the volume
            if (_other.CompareTag("PhysProp"))
            {
                // Get a reference to the entity component
                var targetProp = _other.gameObject.GetComponentInParent<Actor>().gameObject;
                
                // Exit if the object is already parented
                if (targetProp.transform.parent == streamContainer.transform) return;
                
                // Add the entity to the list if they are not already present
                MoveObjectToStreamContainer(targetProp);
            }
        }

        private new void OnTriggerExit(Collider _other)
        {
            if (!initializedExitZone) return;
            
            // Pawn has entered the volume
            if (_other.CompareTag("Pawn"))
            {
                //print($"{gameObject.name} has triggered a dump");
                // Get a reference to the entity component
                var targetEntity = _other.gameObject.GetComponent<Pawn>();
                
                targetEntity.transform.SetParent(null);
                SceneManager.MoveGameObjectToScene(targetEntity.gameObject, SceneManager.GetActiveScene());
            }

            // A physics prop has entered the volume
            if (_other.CompareTag("PhysProp"))
            {
                // Get a reference to the entity component
                var targetProp = _other.gameObject.GetComponentInParent<Actor>().gameObject;
                
                targetProp.transform.SetParent(null);
                SceneManager.MoveGameObjectToScene(targetProp.gameObject, SceneManager.GetActiveScene());
            }
        }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private IEnumerator InitializeStreamContainer()
    {
        if (initializedExitZone) yield break;
        // Prepare the streaming container
        streamContainer = transform.GetComponentInChildren<VolumeLevelStreamContainer>();
        if (!streamContainer) yield break;
        streamContainer.exitPositionOffset = exitPositionOffset;
        streamContainer.exitRotationOffset = exitRotationOffset;
        streamContainer.parentStreamVolume = gameObject;
        
        if (SceneManager.GetSceneByName(worldLoader.streamingWorldID).isLoaded)
        {
            streamContainer.initializedExitZone = true;
            initializedExitZone = true;
            streamContainer.transform.SetParent(null);
            SceneManager.MoveGameObjectToScene(streamContainer.gameObject, SceneManager.GetSceneByName(worldLoader.streamingWorldID));
        }
    }
    
    private void MoveObjectToStreamContainer(GameObject _targetObject)
    {
        //print($"{gameObject.name} has triggered a move event");
        // Clear its parent to avoid random bugs
        _targetObject.transform.SetParent(null);
        
        // Ensure the stream scene is loaded
        if (SceneManager.GetSceneByName(worldLoader.streamingWorldID).isLoaded)
        {
            //print($"{gameObject.name} move event succeded");
            // Move the object to the scene and set its parent properly, so it can be ejected if need be
            SceneManager.MoveGameObjectToScene(_targetObject, SceneManager.GetSceneByName(worldLoader.streamingWorldID));
            _targetObject.transform.SetParent(streamContainer.transform);
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
