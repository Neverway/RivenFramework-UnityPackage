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

public class VolumeLevelStream : Volume
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("This is the offset that will be applied to objects within this volume when the level changes")]
    [SerializeField] private Vector3 exitPositionOffset;
    [SerializeField] private Vector3 exitRotationOffset;
    [SerializeField] private bool debugDrawExitZone;
    
    
    [Header("Linking")]
    [Tooltip("Drag the corresponding volume from the other loaded scene here, then use 'Align To Link Target' to auto-solve the offset/rotation!")]
    [SerializeField] private Transform linkTarget;

    
    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("This is the empty game object that streamed actors are stored in, (to save them from being destroyed on map changes)")]
    [SerializeField] private VolumeLevelStreamContainer streamContainer;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag("Pawn"))
        {
            var targetEntity = _other.gameObject.GetComponentInParent<Pawn>();
            targetEntity.transform.SetParent(streamContainer.transform);
        }
        if (_other.CompareTag("PhysProp"))
        {
            var targetEntity = _other.gameObject.GetComponentInParent<Actor>();
            targetEntity.transform.SetParent(streamContainer.transform);
        }
    }
    
    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag("Pawn"))
        {
            var targetEntity = _other.gameObject.GetComponentInParent<Pawn>();
            if (targetEntity.transform.parent == streamContainer.transform)
            {
                var anchor = SceneManager.GetActiveScene().GetRootGameObjects()[0];
                targetEntity.transform.SetParent(anchor.transform);
                targetEntity.transform.SetParent(null);
            }
        }
        if (_other.CompareTag("PhysProp"))
        {
            var targetEntity = _other.gameObject.GetComponentInParent<Actor>();
            if (targetEntity.transform.parent == streamContainer.transform)
            {
                var anchor = SceneManager.GetActiveScene().GetRootGameObjects()[0];
                targetEntity.transform.SetParent(anchor.transform);
                targetEntity.transform.SetParent(null);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!debugDrawExitZone) return;
    
        // Compose the exit transform relative to this volume's own orientation
        Quaternion exitRotation = transform.rotation * Quaternion.Euler(exitRotationOffset);
        Vector3 exitPosition = transform.position + (transform.rotation * exitPositionOffset);
    
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(exitPosition, exitRotation, transform.localScale);
    
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    
        // Facing direction of the exit rotation
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(Vector3.zero, Vector3.forward * 1.5f);
    
        Gizmos.matrix = oldMatrix;
    }
    
    [ContextMenu("Align To Link Target")]
    private void AlignToLinkTarget()
    {
        if (linkTarget == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No linkTarget assigned, can't align exit offsets", this);
            return;
        }
    
    #if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Align Exit Offset To Link Target");
    #endif
    
        exitPositionOffset = Quaternion.Inverse(transform.rotation) * (linkTarget.position - transform.position);
    
        Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * linkTarget.rotation;
        exitRotationOffset = relativeRotation.eulerAngles;
    
    #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
    #endif
    
        Debug.Log($"[{gameObject.name}] Aligned exit offset to '{linkTarget.name}' Position: {exitPositionOffset}, Rotation: {exitRotationOffset}", this);
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    public void PrepareForLoad()
    {
        streamContainer.exitPositionOffset = exitPositionOffset;
        streamContainer.exitRotationOffset = exitRotationOffset;
        streamContainer.parentStreamVolume = gameObject;
        streamContainer.PrepareForLoad();
    }

    
    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /*
    
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
            if (!initializedExitZone)
            {
                print($"[{gameObject.name}] OnTriggerStay fired but exitZone not initialized yet");
                return;
            }
            
            // Pawn has entered the volume
            if (_other.CompareTag("Pawn"))
            {
                // Get a reference to the entity component
                var targetEntity = _other.gameObject.GetComponentInParent<Pawn>();
                print($"[{gameObject.name}] Pawn detected: {targetEntity.gameObject.name}, parent is: {(targetEntity.transform.parent == null ? "NULL" : targetEntity.transform.parent.gameObject.name)}");
                
                // Exit if the object is already parented
                if (targetEntity.transform.parent == streamContainer.transform)
                {
                    print($"[{gameObject.name}] Pawn already in container, skipping");
                    return;
                }
                
                // Add the entity to the list if they are not already present
                MoveObjectToStreamContainer(targetEntity.gameObject);
            }

            // A physics prop has entered the volume
            if (_other.CompareTag("PhysProp"))
            {
                // Get a reference to the entity component
                var targetProp = _other.gameObject.GetComponentInParent<Actor>().gameObject;
                print($"[{gameObject.name}] PhysProp detected: {targetProp.gameObject.name}, parent is: {(targetProp.transform.parent == null ? "NULL" : targetProp.transform.parent.gameObject.name)}");
                
                // Exit if the object is already parented
                if (targetProp.transform.parent == streamContainer.transform)
                {
                    print($"[{gameObject.name}] PhysProp already in container, skipping");
                    return;
                }
                
                // Add the entity to the list if they are not already present
                MoveObjectToStreamContainer(targetProp);
            }
        }

        private new void OnTriggerExit(Collider _other)
        {
            if (!initializedExitZone) return;
            
            if (worldLoader.isLoading) return;
            
            // Pawn has entered the volume
            if (_other.CompareTag("Pawn"))
            {
                //print($"{gameObject.name} has triggered a dump");
                // Get a reference to the entity component
                var targetEntity = _other.gameObject.GetComponentInParent<Pawn>();
                
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
        print($"[{gameObject.name}] Attempting to move {_targetObject.name} to stream container");
        _targetObject.transform.SetParent(null);
        
        // Ensure the stream scene is loaded
        if (SceneManager.GetSceneByName(worldLoader.streamingWorldID).isLoaded)
        {
            //print($"{gameObject.name} move event succeded");
            // Move the object to the scene and set its parent properly, so it can be ejected if need be
            print($"[{gameObject.name}] Streaming scene is loaded, moving {_targetObject.name}...");
            SceneManager.MoveGameObjectToScene(_targetObject, SceneManager.GetSceneByName(worldLoader.streamingWorldID));
            _targetObject.transform.SetParent(streamContainer.transform);
            print($"[{gameObject.name}] {_targetObject.name} moved. Container now has {streamContainer.transform.childCount} children. streamContainer scene: {streamContainer.gameObject.scene.name}");
        }
        else
        {
            print($"[{gameObject.name}] FAILED: Streaming scene is NOT loaded!");
        }
    }

    
    
    private bool initializedExitZone;*/
    

    #endregion
}
