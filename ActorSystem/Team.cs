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
//  
//====================================================================================================================//

using UnityEngine;
using System.Collections.Generic;

namespace RivenFramework
{
    /// <summary>
    /// A team that an actor belongs to, along with who it sees as allies and opponents
    /// </summary>
    [CreateAssetMenu(fileName = "NewTeam", menuName = "Neverway/Actors/Team")]
    public class Team : ScriptableObject
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Tooltip("Teams this team will see as friendly (Being allied is one-way, so don't forget to ally the other team too if you want the feeling to be mutual! <3)")]
            public List<Team> allies = new List<Team>();

            [Tooltip("Teams this team will see as hostile")]
            public List<Team> opponents = new List<Team>();
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )======================================================//

        #region----------------( Inherited )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            /// <summary>
            /// Works out how another team relates to this one from this team's point of view
            /// </summary>
            /// <param name="_other">The team to compare against (Null counts as nuetral)</param>
            /// <returns>Returns Self for same team, Allied, or Opposed if applicable, and Neutral otherwise</returns>
            public TeamRelation GetRelationTo(Team _other)
            {
                if (_other == null) return TeamRelation.Neutral;
                if (_other == this) return TeamRelation.Self;
                if (allies != null && allies.Contains(_other)) return TeamRelation.Allied;
                if (opponents != null && opponents.Contains(_other)) return TeamRelation.Opposed;
                return TeamRelation.Neutral;
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion
    }
    
    /// <summary>
    /// How one team sees another team
    /// </summary>
    public enum TeamRelation
    {
        Self,
        Allied,
        Opposed,
        Neutral
    }
}