//==========================================( Neverway 2026 )=========================================================//
// Author
// Liz M.
//
// Contributors
//  Errynei, Connorses, Soulex
//
//====================================================================================================================//

using ErryLib;
using RivenFramework;
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
public class Actor : GUIDComponent
{
    #region========================================( Variables )====================================================== //

    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [field: Header("Actor Data")]
    [Tooltip("This ID is how this actor is identified, saved, and loaded from map files")]
    public string id { get; private set; }

    [Tooltip("This is how this actor is listed in things like an asset browser, or in game like in an inventory")]
    public string displayName { get; private set; }

    [Tooltip("This is what tags this actor is associated with, it's used to filter between different kinds of objects when handling things like logic volumes")]
    private List<ActorTag> actorTags;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    
    [HideInInspector, Tooltip("The current domain this actor is contained in")]
    public ActorDomain currentDomain { get; private set; }




    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/

    [ContextMenu("Generate Display Name")]
    private void GenerateDisplayName()
    {
        displayName = Regex.Replace(gameObject.name, "([a-z])([A-Z])", "$1 $2");
        displayName = Regex.Replace(displayName, "^[^_]*_", "");
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void MoveActorToDomain(ActorDomain newDomain)
    {
        if (newDomain == currentDomain) return;
        if (newDomain == null) newDomain = GameInstance.globalActorDomain;

        currentDomain.UnregisterActor(this);
        currentDomain = newDomain;
        currentDomain.RegisterActor(this);
    }


    public bool HasActorTag(ActorTag _actorTag) => actorTags.Contains(_actorTag);
    public bool HasAnyActorTag(params ActorTag[] _actorTags) => actorTags.Intersect(_actorTags).Any();
    public bool HasAllActorTags(params ActorTag[] _actorTags) => !actorTags.Except(_actorTags).Any();


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
public class ActorFilter_HasAnyActorTag : ActorFilter
{
    public List<ActorTag> tags;

    public override bool PassesFilter(Actor _actor)
    {
        return _actor.HasAnyActorTag(tags.ToArray());
    }
}

[Serializable]
public class ActorFilter_HasAllActorTags : ActorFilter
{
    public List<ActorTag> tags;

    public override bool PassesFilter(Actor _actor)
    {
        return _actor.HasAllActorTags(tags.ToArray());
    }
}