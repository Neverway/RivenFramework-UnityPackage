//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Keeps this object when changing scenes and ensures there is only
//  ever one of them present in a scene
// Notes: 
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    public class GameInstance : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=


        //=-----------------=
        // Private Variables
        //=-----------------=

        //=-----------------=
        // Reference Variables
        //=-----------------=
        private static GameInstance _instance;
        public static GameInstance Instance 
        { 
            get 
            {
                if (_instance == null) throw new NullReferenceException("GameInstance is missing");
                return _instance;
            } 
        }

        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(_instance);
        }
        //=-----------------=
        // Internal Functions
        //=-----------------=


        //=-----------------=
        // External Functions
        //=-----------------=
        /// <summary>
        /// Directly gets a component from the GameInstance of the type provided
        /// </summary>
        /// <typeparam name="T">GameInstance component you wish to retrieve</typeparam>
        /// <returns>The component of type T from GameInstance</returns>
        /// <exception cref="NullReferenceException"></exception>
        public static T Get<T>() where T : MonoBehaviour
        {
            if (_instance == null)
                throw new NullReferenceException($"{nameof(GameInstance)}: Trying to get a component from {nameof(GameInstance)} " +
                    $"but there is no main instance set for it. (Likely has not been instantiated, must have one GameObject with" +
                    $"this component attached in the scene at game start)");

            return _instance.GetComponent<T>();
        }

        /// <summary>
        /// Directly gets a component from the GameInstance of the type provided
        /// </summary>
        /// <typeparam name="T">GameInstance component you wish to retrieve</typeparam>
        /// <param name="gameInstanceModule">outputs component of the given variable type</param>
        /// <returns>True if there was a component of that type on GameInstance</returns>
        public static bool Get<T>(out T gameInstanceModule) where T : MonoBehaviour
        {
            gameInstanceModule = Get<T>();
            return gameInstanceModule != null;
        }

        // Reload Static Fields
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeStaticFields()
        {
            _instance = null;
        }

        public static Coroutine SendCoroutine(IEnumerator coroutine) => Instance.StartCoroutine(coroutine);
    }
}

