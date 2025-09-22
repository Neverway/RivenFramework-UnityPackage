//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Keeps this object when changing scenes and ensures there is only
//  ever one of them present in a scene
// Notes: 
//
//=============================================================================

using System;
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
        //[IsDomainReloaded] 
        private static GameInstance instance;

        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(instance); //should be DontDestroyOnLoad(gameobject);
        }
        //=-----------------=
        // Internal Functions
        //=-----------------=


        //=-----------------=
        // External Functions
        //=-----------------=
        /// <summary>
        /// Directly gets a component from the GameInstance instance of the type provided
        /// </summary>
        /// <typeparam name="T">GameInstance component you wish to retrieve</typeparam>
        /// <returns>The component of type T from GameInstance</returns>
        /// <exception cref="NullReferenceException"></exception>
        public static T Get<T>() where T : MonoBehaviour
        {
            if (instance == null)
                throw new NullReferenceException($"Trying to get GameInstance component, but there is no GameInstance. " +
                    $"(or it is not stored in {nameof(GameInstance)}.{nameof(instance)}");

            return instance.GetComponent<T>();
        }

        // Reload Static Fields
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeStaticFields()
        {
            instance = null;
        }
    }
}

