//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RivenFramework
{
    [Serializable]
    public class GameInstanceModule
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
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
            public virtual void OnGameStart() { }

            public virtual void OnGameUpdate() { }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }

    [Serializable]
    [GIModuleColor(GIModuleColors.Magenta)]
    public class GSDFRHSDFI_DoCrazyShit : GameInstanceModule
    {
        public int someField;
        public int someField2;
        public string enterTextHere;

        public List<Actor> actorList;
        public override void OnGameStart() 
        { 

        }
    }

    [Serializable]
    [GIModuleColor(GIModuleColors.Orange)]
    public class GI_DoMoreCrazyShit : GameInstanceModule
    {
        public int someField;
        [Polymorphic, SerializeReference] public Actor actor;


        public void SomeMethod()
        {

        }
    }

    public class BelongsToConnorsAttribute : Attribute
    {

    }
}
