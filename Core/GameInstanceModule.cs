using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    [Todo("Implement GameInstanceModule system", Owner = "Errynei")]
    public abstract class GameInstanceModule : MonoBehaviour
    {
        public void OnGameStart() { }
        public void OnGameUpdate() { }
    }
}
