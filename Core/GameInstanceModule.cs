using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RivenFramework
{
    [Serializable]
    public class GameInstanceModule
    {
        public virtual void OnGameStart() { }
        public virtual void OnGameUpdate() { }
    }





    [Serializable]
    [GIModuleColor(GIModuleColors.Blue)]
    public class GI_DoCrazyShit : GameInstanceModule
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
    [GIModuleColor(GIModuleColors.Magenta)]
    public class GI_DoMoreCrazyShit : GameInstanceModule
    {
        public int someField;
        [Polymorphic, SerializeReference] public Actor actor;
    }
}
