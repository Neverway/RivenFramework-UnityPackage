//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//  Errynei
//  Connorses
//  Soulex
//
// Description
//  The base class for any asset that can be placed in a map.
//  This provides the ID, display name, tags, teams, and the actor lifecycle calls like Start, Update, etc.
//====================================================================================================================//

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ErryLib;

namespace RivenFramework
{
    /// <summary>
    /// Acts as the base identifier for an asset that can be placed in a map
    /// Assigning it an id, what tags it has, what team it's on, and a human-readable display name
    /// All placeable assets must contain this component on their root
    /// </summary>
    [Serializable]
    public class Actor : GUIDComponent, IActorFunctions
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Header("Actor Data")]
            [Tooltip("This ID is how this actor is identified, saved, and loaded from map files")]
            public string id;

            [Tooltip("This is how this actor is listed in things like an asset browser, or in game like in an inventory")]
            public string displayName;

            [Tooltip("This is what tags this actor is associated with, it's used to filter between different kinds of objects when handling things like logic volumes")]
            public List<ActorTag> tags = new List<ActorTag>();

            [Tooltip("The team this actor is on (Leave empty for no team)")]
            public Team team;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            [HideInInspector, Tooltip("The current domain this actor is contained in")]
            public ActorDomain currentDomain { get; private set; }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )======================================================//

        #region----------------( Inherited )--------------------------------------------------------------------------//
            protected virtual void Start() => OnActorCreated();
            protected virtual void OnDestroy() => OnActorDestroyed();
            protected virtual void OnEnable() => OnActorEnabled();
            protected virtual void OnDisable() => OnActorDisabled();
            protected virtual void Update() => OnActorUpdate();
            protected virtual void FixedUpdate() => OnActorFixedUpdate();
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public void MoveActorToDomain(ActorDomain newDomain)
            {
                if (newDomain == null) newDomain = GameInstance.globalActorDomain;
                if (newDomain == currentDomain) return;

                if (currentDomain != null) currentDomain.UnregisterActor(this);
                currentDomain = newDomain;
                if (currentDomain != null) currentDomain.RegisterActor(this);
            }


            public bool HasActorTag(ActorTag _actorTag) => tags != null && tags.Contains(_actorTag);
            public bool HasAnyActorTag(params ActorTag[] _actorTags) => tags != null && tags.Intersect(_actorTags).Any();
            public bool HasAllActorTags(params ActorTag[] _actorTags) => tags != null && _actorTags.All(_tag => tags.Contains(_tag));


            /// <summary>
            /// Finds other actors in the loaded scenes that share this actor's GUID
            /// </summary>
            /// <returns>Returns the other actors with the same GUID (The list is empty if the GUID is unique)</returns>
            public List<Actor> GetConflictingDuplicateInstancesOfActor()
            {
                List<Actor> conflictingActors = new List<Actor>();
                foreach (var actor in FindObjectsOfType<Actor>())
                {
                    if (actor != this && actor.GetGUID() == GetGUID()) conflictingActors.Add(actor);
                }

                return conflictingActors;
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
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
        #endregion

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
}