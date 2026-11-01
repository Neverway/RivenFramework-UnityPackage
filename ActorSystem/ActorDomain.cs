//==========================================( Neverway 2027 )=========================================================//
// Author
//  
//
// Contributors
//  Liz M.
//  Errynei
//  Connorses
//  Soulex
//
// Description
//  
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    /// <summary>
    /// Contains actors, and passes any ActorFunction call on it down to 
    /// </summary>
    public class ActorDomain : Actor
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public HashSet<Actor> actors = new HashSet<Actor>();
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )======================================================//

        #region----------------( Inherited )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static ActorDomain CreateNewGlobalDomain()
            {
                ActorDomain domain = CreateNewInScene();
                DontDestroyOnLoad(domain.gameObject);
                return domain;
            }
            
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static ActorDomain CreateNewInScene()
            {
                ActorDomain domain = new GameObject("Actor Domain").AddComponent<ActorDomain>();
                domain.NewGUID();

                return domain;
            }
            
            public void RegisterActor(Actor actor) => actors.Add(actor);
            
            public void UnregisterActor(Actor actor) => actors.Remove(actor);
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion
    }
}