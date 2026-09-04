//==========================================( Neverway 2026 )=========================================================//
// Author
//  Errynei
//
// Contributors
//  Liz M.
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ErryLib;
using UnityEngine;

/// <summary>
/// Acts as the base identifier for an asset that can be placed in a map
/// Assigning it an id, what groups it's a part of, and a human-readable display name
/// All placeable assets must contain this component on their root
/// </summary>
public class ActorV2 : GUIDComponent, IActorFunctions
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Actor Data")]
    [Tooltip("This ID is how this actor is identified, saved, and loaded from map files")]
    public string id;
    [Tooltip("This is how this actor is listed in things like an asset browser, or in game like in an inventory")]
    public string displayName;
    [Tooltip("This is what groups this actor is associated with, it's used to filter between different kinds of objects when handling things like logic volumes")]
    public List<ActorGroup> groups;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start() => OnActorCreated();
    private void OnDestroy() => OnActorDestroyed();
    private void OnEnable() => OnActorEnabled(); 
    private void OnDisable() => OnActorDisabled(); 
    private void Update() => OnActorUpdate(); 
    private void FixedUpdate() => OnActorFixedUpdate();


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    public virtual void OnActorCreated() { }
    public virtual void OnActorDestroyed() { }
    public virtual void OnActorEnabled() { }
    public virtual void OnActorDisabled() { }
    public virtual void OnActorUpdate() { }
    public virtual void OnActorFixedUpdate() { }
    
    
    
    [ContextMenu("Generate ID & Name")]
    private void GenerateIDAndName()
    {
        GenerateID();
        GenerateDisplayName();
    }

    [ContextMenu("Generate ID")]
    private void GenerateID()
    {
        id = gameObject.name;
    }

    [ContextMenu("Generate Display Name")]
    private void GenerateDisplayName()
    {
        displayName = Regex.Replace(gameObject.name, "([a-z])([A-Z])", "$1 $2");
        displayName = Regex.Replace(displayName, "^[^_]*_", "");
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public bool IsInAnyOfGroups(List<ActorGroup> _groups)
    {
        return groups.Intersect(_groups).Any();
    }

    [ContextMenu("Check UUID")]
    public List<ActorV2> GetConflictingDuplicateInstancesOfActor()
    {
        List<ActorV2> conflictingActors = new List<ActorV2>();
        foreach (var actor in FindObjectsOfType<ActorV2>())
        {
            if (actor.GetGUID() == GetGUID())
            {
                conflictingActors.Add(this);
            }
        }

        return conflictingActors;
    }


    #endregion
}

/// <summary>
/// The basic logic functions an actor can call
/// </summary>
public interface IActorFunctions
{
    public void OnActorCreated();
    public void OnActorDestroyed();
    public void OnActorEnabled();
    public void OnActorDisabled();
    public void OnActorUpdate();
    public void OnActorFixedUpdate();
}

[Serializable]
public abstract class Filter<T>
{
    public abstract bool PassesFilter(T _objectToCheck);
}

[Serializable]
public class ActorFilter_IsNamed : Filter<ActorV2>
{
    public string name;

    public override bool PassesFilter(ActorV2 _actor)
    {
        return _actor.displayName == name;
    }
}

[Serializable]
public class ActorFilter_IsID : Filter<ActorV2>
{
    public string id;

    public override bool PassesFilter(ActorV2 _actor)
    {
        return _actor.id == id;
    }
}

[Serializable]
public class ActorFilter_IsInAnyGroup : Filter<ActorV2>
{
    public List<ActorGroup> groups;

    public override bool PassesFilter(ActorV2 _actor)
    {
        return _actor.IsInAnyOfGroups(groups);
    }
}
