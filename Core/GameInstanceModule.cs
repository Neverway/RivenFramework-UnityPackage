using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    [Todo("Implement GameInstanceModule system", "Errynei")]
    public abstract class GameInstanceModule : MonoBehaviour
    {
        public virtual void OnGameStart() { }
        public virtual void OnModuleUpdate() { }
    }
}
