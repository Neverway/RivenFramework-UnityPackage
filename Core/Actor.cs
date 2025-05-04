//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: This "Actor" Class is used to identify the objects placed in a map, so that they can be saved or loaded using their id.
//      It, or a subclass of it, should be present on all objects you wish to place in a map.
// Notes: This class can be inherited from to create subclasses of Actor, like Pawn
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Actor : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Header("Actor Data")]
    [Tooltip("This ID is how this actor is identified, saved, and loaded from map files")]
    public string id;
    [Tooltip("This is how this actor is listed in things like an asset browser, or in game like in an inventory")]
    public string displayName;
    [Tooltip("This is a unique id to this individual actor, it's used to differentiate between instances of the same type of object")]
    public string uniqueId;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
    }

    private void Update()
    {
    
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    [ContextMenu("Generate ID & Name")]
    private void GenerateIDAndName()
    {
        GenerateID();
        GenerateDisplayName();
    }
    
    [ContextMenu("Generate ID")]
    private void GenerateID()
    {
        // Generate a UUID
        id = gameObject.name;
    }
    
    [ContextMenu("Generate Display Name")]
    private void GenerateDisplayName()
    {
        displayName = Regex.Replace(gameObject.name, "([a-z])([A-Z])", "$1 $2");
    }
    
    [ContextMenu("Generate UUID")]
    private void GenerateUID()
    {
        // Generate a UUID
        uniqueId = Guid.NewGuid().ToString();

        // Check if it's taken
        if (CheckUUID() is false)
        {
            Debug.Log("UUID was already taken");
            GenerateUID();
        }
    }

    [ContextMenu("Check UUID")]
    private bool CheckUUID()
    {
        foreach (var actor in FindObjectsOfType<Actor>())
        {
            if (actor == this) continue;
            if (actor.uniqueId == uniqueId)
            {
                return false;
            }
        }

        return true;
    }


    //=-----------------=
    // External Functions
    //=-----------------=

    [ContextMenu("Check UUID")]
    public List<Actor> GetConflictingActors()
    {
        List<Actor> conflictingActors = new List<Actor>();
        foreach (var actor in FindObjectsOfType<Actor>())
        {
            if (actor.uniqueId == uniqueId)
            {
                conflictingActors.Add(this);
            }
        }

        return conflictingActors;
    }
}
