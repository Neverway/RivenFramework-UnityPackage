//==========================================( Neverway 2026 )=========================================================//
// Author
// Liz M.
//
// Contributors
//  Errynei, Connorses, Soulex
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Acts as the base identifier for an asset that can be placed in a map
/// Assigning it an id, what groups it's a part of, and a human-readable display name
/// All placeable assets must contain this component on their root
/// </summary>
[Serializable]
public class Actor : MonoBehaviour
{
    #region========================================( Variables )====================================================== //

    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Actor Data")]
    [Tooltip("This ID is how this actor is identified, saved, and loaded from map files")]
    public string id;
    [Tooltip("This is how this actor is listed in things like an asset browser, or in game like in an inventory")]
    public string displayName;
    [Tooltip("This is a unique id to this individual actor, it's used to differentiate between instances of the same type of object in map")]
    public string uniqueId;
    [Tooltip("This is what groups this actor is associated with, it's used to filter between different kinds of objects when handling things like logic volumes")]
    public List<ActorGroup> groups;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
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
        displayName = Regex.Replace(displayName, "^[^_]*_", "");
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


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/

    
    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public bool IsInAnyOfGroups(List<ActorGroup> _groups)
    {
        return groups.Intersect(_groups).Any();
    }

    [ContextMenu("Check UUID")]
    public List<Actor> GetConflictingDuplicateInstancesOfActor()
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


    #endregion
}

[Serializable]
public abstract class ActorFilter
{
    public abstract bool PassesFilter(Actor _actor);
}

[Serializable]
public class ActorFilter_IsNamed : ActorFilter
{
    public string name;

    public override bool PassesFilter(Actor _actor)
    {
        return _actor.displayName == name;
    }
}

[Serializable]
public class ActorFilter_IsID : ActorFilter
{
    public string id;

    public override bool PassesFilter(Actor _actor)
    {
        return _actor.id == id;
    }
}

[Serializable]
public class ActorFilter_IsInAnyGroup : ActorFilter
{
    public List<ActorGroup> groups;

    public override bool PassesFilter(Actor _actor)
    {
        return _actor.IsInAnyOfGroups(groups);
    }
}

