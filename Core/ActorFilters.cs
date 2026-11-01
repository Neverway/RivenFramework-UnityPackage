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
//  System for filtering actors by different properties, like display name, id, tag, team, etc.
//====================================================================================================================//

using UnityEngine;
using System;
using System.Collections.Generic;

namespace RivenFramework
{
    /// <summary>
    /// A list of filters and how they are checked to determin if a target passes a filter check
    /// </summary>
    [Serializable]
    public class ActorFilterSet
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Tooltip("Must Pass All: every filter has to pass, Passes Any: only one filter has to pass")]
            public Mode mode = Mode.MustPassAll;

            [Tooltip("Which filters to check (If empty, all actors pass the filter)")]
            [SerializeReference, Polymorphic] public List<Filter<Actor>> filters = new List<Filter<Actor>>();
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public enum Mode
            {
                MustPassAll,
                PassesAny
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )======================================================//

        #region----------------( Inherited )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            /// <summary>
            /// Checks an actor against every filter in this set
            /// </summary>
            /// <param name="_actor">The actor to check</param>
            /// <returns>Returns true if the actor passes</returns>
            public bool Passes(Actor _actor)
            {
                if (filters == null || filters.Count == 0) return true;
                if (_actor == null) return false;

                bool anyFilterUsed = false;
                foreach (var filter in filters)
                {
                    if (filter == null) continue;
                    anyFilterUsed = true;
                    bool passed = filter.PassesFilter(_actor);
                    if (mode == Mode.MustPassAll && !passed) return false;
                    if (mode == Mode.PassesAny && passed) return true;
                }

                if (!anyFilterUsed) return true;
                return mode == Mode.MustPassAll;
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion
    }
    
    /// <summary>
    /// Passes actors if their display name matches
    /// </summary>
    [Serializable]
    public class ActorFilter_IsNamed : Filter<Actor>
    {
        #region====================================( Variables )==========================================================//

        #region----------------( Inspector )------------------------------------------------------------------------------//
            [Tooltip("The display name to match")]
            public string displayName;
        #endregion

        #region----------------( Reference )------------------------------------------------------------------------------//
        #endregion

        #region----------------( External )-------------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )-------------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )==========================================================//

        #region----------------( Inherited )------------------------------------------------------------------------------//
            public override bool PassesFilter(Actor _actor)
            {
                return _actor != null && _actor.displayName == displayName;
            }
        #endregion

        #region----------------( External )-------------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )-------------------------------------------------------------------------------//
        #endregion

        #endregion
    }

    /// <summary>
    /// Passes the actor with a specific ID
    /// </summary>
    [Serializable]
    public class ActorFilter_IsID : Filter<Actor>
    {
        #region====================================( Variables )==========================================================//

        #region----------------( Inspector )------------------------------------------------------------------------------//
            [Tooltip("The actor ID to match")]
            public string id;
        #endregion

        #region----------------( Reference )------------------------------------------------------------------------------//
        #endregion

        #region----------------( External )-------------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )-------------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )==========================================================//

        #region----------------( Inherited )------------------------------------------------------------------------------//
            public override bool PassesFilter(Actor _actor)
            {
                return _actor != null && _actor.id == id;
            }
        #endregion

        #region----------------( External )-------------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )-------------------------------------------------------------------------------//
        #endregion

        #endregion
    }

    /// <summary>
    /// Passes actors that have at least one of the listed tags
    /// </summary>
    [Serializable]
    public class ActorFilter_HasAnyActorTag : Filter<Actor>
    {
        #region====================================( Variables )==========================================================//

        #region----------------( Inspector )------------------------------------------------------------------------------//
            [Tooltip("Actors need at least one of these tags")]
            public List<ActorTag> tags = new List<ActorTag>();
        #endregion

        #region----------------( Reference )------------------------------------------------------------------------------//
        #endregion

        #region----------------( External )-------------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )-------------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )==========================================================//

        #region----------------( Inherited )------------------------------------------------------------------------------//
            public override bool PassesFilter(Actor _actor)
            {
                return _actor != null && _actor.HasAnyActorTag(tags.ToArray());
            }
        #endregion

        #region----------------( External )-------------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )-------------------------------------------------------------------------------//
        #endregion

        #endregion
    }

    /// <summary>
    /// Passes actors that have every one of the listed tags
    /// </summary>
    [Serializable]
    public class ActorFilter_HasAllActorTags : Filter<Actor>
    {
        #region====================================( Variables )==========================================================//

        #region----------------( Inspector )------------------------------------------------------------------------------//
            [Tooltip("Actors need all of these tags")]
            public List<ActorTag> tags = new List<ActorTag>();
        #endregion

        #region----------------( Reference )------------------------------------------------------------------------------//
        #endregion

        #region----------------( External )-------------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )-------------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )==========================================================//

        #region----------------( Inherited )------------------------------------------------------------------------------//
            public override bool PassesFilter(Actor _actor)
            {
                return _actor != null && _actor.HasAllActorTags(tags.ToArray());
            }
        #endregion

        #region----------------( External )-------------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )-------------------------------------------------------------------------------//
        #endregion

        #endregion
    }

    /// <summary>
    /// Passes actors that are on a specific team
    /// </summary>
    [Serializable]
    public class ActorFilter_IsOnTeam : Filter<Actor>
    {
        #region====================================( Variables )==========================================================//

        #region----------------( Inspector )------------------------------------------------------------------------------//
            [Tooltip("The team the actor must be on")]
            public Team team;
        #endregion

        #region----------------( Reference )------------------------------------------------------------------------------//
        #endregion

        #region----------------( External )-------------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )-------------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )==========================================================//

        #region----------------( Inherited )------------------------------------------------------------------------------//
            public override bool PassesFilter(Actor _actor)
            {
                return _actor != null && _actor.team == team;
            }
        #endregion

        #region----------------( External )-------------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )-------------------------------------------------------------------------------//
        #endregion

        #endregion
    }

    /// <summary>
    /// Passes actors whose team has one of the chosen relations to a team (For example, a pain volume that should not hurt its own team uses Opposed and Neutral)
    /// </summary>
    [Serializable]
    public class ActorFilter_TeamRelation : Filter<Actor>
    {
        #region====================================( Variables )========================================================== //

        #region----------------( Inspector )------------------------------------------------------------------------------ //

        [Tooltip(
            "The team to compare against, usually the team that owns the volume (If empty, every actor counts as Neutral)")]
        public Team relativeTo;

        [Tooltip("The actor passes if its team relates to the team above in any of these ways")]
        public List<TeamRelation> relations = new List<TeamRelation> { TeamRelation.Opposed, TeamRelation.Neutral };

        #endregion

        #region----------------( Reference )------------------------------------------------------------------------------ //

        #endregion

        #region----------------( External )------------------------------------------------------------------------------- //

        #endregion

        #region----------------( Internal )------------------------------------------------------------------------------- //

        #endregion

        #endregion

        #region====================================( Functions )========================================================== //

        #region----------------( Inherited )------------------------------------------------------------------------------ //

        public override bool PassesFilter(Actor _actor)
        {
            if (_actor == null) return false;
            TeamRelation relation = relativeTo != null ? relativeTo.GetRelationTo(_actor.team) : TeamRelation.Neutral;
            return relations != null && relations.Contains(relation);
        }

        #endregion

        #region----------------( External )------------------------------------------------------------------------------- //

        #endregion

        #region----------------( Internal )------------------------------------------------------------------------------- //

        #endregion

        #endregion
    }
}