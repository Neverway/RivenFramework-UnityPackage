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
    [SerializeField] private Vector3 exitOffset;
    [SerializeField] private bool debugDrawExitZone;
    private bool initializedExitZone;
    

    //=-----------------=
    // Reference Variables
    //=-----------------=
    private GI_WorldLoader worldLoader;
    private VolumeLevelStreamContainer streamContainer;


    //=-----------------=
    // Mono Functions
    //=-----------------=
        private void Start()
        {
            worldLoader = FindObjectOfType<GI_WorldLoader>();
            streamContainer = transform.GetComponentInChildren<VolumeLevelStreamContainer>();
            streamContainer.exitOffset = exitOffset;
            streamContainer.parentStreamVolume = gameObject;
            streamContainer.transform.SetParent(null);
            worldLoader = FindObjectOfType<GI_WorldLoader>();
        }

        private void LateUpdate()
        {
            if (SceneManager.GetSceneByName(worldLoader.streamingWorldID).IsValid())
            {
                streamContainer.initializedExitZone = true;
                initializedExitZone = true;
                SceneManager.MoveGameObjectToScene(streamContainer.gameObject, SceneManager.GetSceneByName(worldLoader.streamingWorldID));
                streamContainer.transform.SetParent(null);
            }
        }

        private void OnDrawGizmos()
        {
            if (!debugDrawExitZone) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position+exitOffset, transform.localScale);
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
            if (!worldLoader) worldLoader = FindObjectOfType<GI_WorldLoader>();
            
            // Pawn has entered the volume
            if (_other.CompareTag("Pawn"))
            {
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
    private void MoveObjectToStreamContainer(GameObject _targetObject)
    {
        // Clear its parent to avoid random bugs
        _targetObject.transform.SetParent(null);
        
        // Ensure the stream scene is loaded
        if (SceneManager.GetSceneByName(worldLoader.streamingWorldID).IsValid())
        {
            // Move the object to the scene and set its parent properly, so it can be ejected if need be
            SceneManager.MoveGameObjectToScene(_targetObject, SceneManager.GetSceneByName(worldLoader.streamingWorldID));
            _targetObject.transform.SetParent(streamContainer.transform);
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
