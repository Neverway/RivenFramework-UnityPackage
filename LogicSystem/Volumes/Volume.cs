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

public class Volume : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Header("Team Filtering (Not functional right now)")]
    [Tooltip("Depending on which team owns this volumes will change the functionality. For example, pain volumes normally don't affect their own team.")]
    public List<string> unaffectedTeams = new List<string>(); // Which team owns the trigger
    [Tooltip("If enabled, the volume will affect everyone regardless of team")]
    public bool ignoreUnaffectedTeamsFilter;
    [Header("Object Filtering")]
    [Tooltip("If enabled, physics props that are being held won't be effected by the volume (This is used for things like wind boxes)")]
    public bool ignoreHeldObjects = true;
    [Tooltip("If enabled, will remove objects from the list of objects in the volume when that object becomes disabled")]
    public bool disabledObjectsExitVolume = true;

    //=-----------------=
    // Private Variables
    //=-----------------=
    [HideInInspector] public List<Pawn> pawnsInTrigger = new List<Pawn>();
    [HideInInspector] public List<GameObject> propsInTrigger = new List<GameObject>();


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private GI_PawnManager pawnManager;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Update()
    {
        CheckPawnsInTrigger();
        CheckPropsInTrigger();
    }

    protected void OnTriggerEnter(Collider _other)
    {
        // Pawn has entered the volume
        if (_other.CompareTag("Pawn"))
        {
            // Get a reference to the entity component
            var targetEntity = _other.gameObject.GetComponent<Pawn>();
            // Exit if they are not on the affected team
            //if (!IsOnAffectedTeam(targetEntity)) return;
            // Add the entity to the list if they are not already present
            AddPawnToVolume(targetEntity);
        }

        // A physics prop has entered the volume
        if (_other.CompareTag("PhysProp"))
        {
            // Don't register held objects if we are ignoring held objects
            var grabbable = _other.gameObject.GetComponent<Object_Grabbable>();
            if (grabbable && ignoreHeldObjects)
            {
                if (grabbable.isHeld)
                {
                    return;
                }
            }

            // Get a reference to the entity component
            var targetProp = _other.gameObject;
            // Add the entity to the list if they are not already present
            AddPropToVolume(targetProp);
        }
    }
    
    protected void OnTriggerExit(Collider _other)
    {
        // Pawn has entered the volume
        if (_other.CompareTag("Pawn"))
        {
            // Get a reference to the entity component
            var targetEntity = _other.gameObject.GetComponent<Pawn>();
            // Remove the entity to the list if they are not already absent
            RemovePawnFromVolume(targetEntity);
        }

        // A physics prop has entered the trigger
        if (_other.CompareTag("PhysProp"))
        {
            // Get a reference to the entity component
            var targetProp = _other.gameObject;
            // Add the entity to the list if they are not already present
            RemovePropFromVolume(targetProp);
        }
    }
    
    public virtual void OnDisable()
    {
        pawnsInTrigger.Clear();
        propsInTrigger.Clear();
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    protected virtual bool AddPawnToVolume(Pawn _pawn)
    {
        // Ignore null
        if (_pawn is null) return false;
        
        // Add to list if it's not already present
        if (pawnsInTrigger.Contains(_pawn) is false)
        {
            pawnsInTrigger.Add(_pawn);
            OnObjectsInVolumeUpdated(VolumeUpdateType.PawnAdded);
            return true;
        }
        
        // Default return
        return false;
    }
    
    protected virtual bool AddPropToVolume(GameObject _prop)
    {
        // Ignore null
        if (_prop is null) return false;
        
        // Add to list if it's not already present
        if (propsInTrigger.Contains(_prop) is false)
        {
            propsInTrigger.Add(_prop);
            OnObjectsInVolumeUpdated(VolumeUpdateType.PropAdded);
            return true;
        }
        
        // Default return
        return false;
    }

    protected virtual bool RemovePawnFromVolume(Pawn _pawn)
    {
        // Ignore null
        if (_pawn is null) return false;
        
        // Remove from list if it's present
        if (pawnsInTrigger.Contains(_pawn) is true)
        {
            pawnsInTrigger.Remove(_pawn);
            OnObjectsInVolumeUpdated(VolumeUpdateType.PawnRemoved);
            return true;
        }
        
        // Default return
        return false;
    }
    
    protected virtual bool RemovePropFromVolume(GameObject _prop)
    {
        // Ignore null
        if (_prop is null) return false;
        
        // Remove from list if it's present
        if (propsInTrigger.Contains(_prop) is true)
        {
            propsInTrigger.Remove(_prop);
            OnObjectsInVolumeUpdated(VolumeUpdateType.PropRemoved);
            return true;
        }
        
        // Default return
        return false;
    }

    protected virtual void CheckPawnsInTrigger()
    {
        var pawnsToRemove = new List<Pawn>();
        
        foreach (var _pawn in pawnsInTrigger)
        {
            if (_pawn is null)
            {
                pawnsToRemove.Add(_pawn);
                continue;
            }

            // Adding this redundant check since sometimes it was throwing a null ref on the active check ~Liz
            if (_pawn)
            {
                if (_pawn.gameObject.activeInHierarchy is false && disabledObjectsExitVolume)
                {
                    pawnsToRemove.Add(_pawn);
                    continue;
                }
            }
            else
            {
                pawnsToRemove.Add(_pawn);
            }
        }
        
        foreach (var _pawn in pawnsToRemove) RemovePawnFromVolume(_pawn);
    }
    
    protected virtual void CheckPropsInTrigger()
    {
        var propsToRemove = new List<GameObject>();
        
        foreach (var _prop in propsInTrigger)
        {
            if (_prop is null)
            {
                propsToRemove.Add(_prop);
                continue;
            }

            if (_prop.gameObject.activeInHierarchy is false && disabledObjectsExitVolume)
            {
                propsToRemove.Add(_prop);
                continue;
            }
        }
        
        foreach (var _prop in propsToRemove) RemovePropFromVolume(_prop);
    }
    
    protected Pawn GetPlayerInTrigger()
    {
        if (pawnManager is null)
        {
            pawnManager = FindObjectOfType<GI_PawnManager>();
            if (pawnManager is null) return null;
        }
        
        foreach (var _pawn in pawnsInTrigger)
        {
            if (pawnsInTrigger.Contains(pawnManager.localPlayerCharacter.GetComponent<Pawn>()))
            {
                return _pawn;
            }
        }

        return null;
    }
    
    /// <summary>
    /// Override this method in extended classes to react when any change to the volume contents occurs
    /// </summary>
    /// <param name="updateType">flags for what kind of update the volume was</param>
    // Example: To check if Props were updated:
    //      (updateType & VolumeUpdateType.PropAdded) != 0
    protected virtual void OnObjectsInVolumeUpdated(VolumeUpdateType updateType)
    {
    }

    /// <summary>
    /// Used to filter different types of updates from OnObjectsInVolumeUpdated callback
    /// </summary>
    [System.Flags]
    public enum VolumeUpdateType
    {
        NoUpdate = 0,
        PawnAdded = 1 << 0,
        PawnRemoved = 1 << 1,
        PropAdded = 1 << 2,
        PropRemoved = 1 << 3,

        AnythingAdded = PawnAdded | PropAdded,
        AnythingRemoved = PawnRemoved | PropRemoved,

        PawnUpdated = PawnAdded | PawnRemoved,
        PropUpdated = PropAdded | PropRemoved,

        AnyUpdate = PawnUpdated | PropUpdated
    }
    
    //=-----------------=
    // External Functions
    //=-----------------=
}
