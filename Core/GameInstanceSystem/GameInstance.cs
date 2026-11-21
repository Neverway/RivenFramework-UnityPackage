//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  Keeps this object when changing scenes and ensures there is only ever one of them present in a scene
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using MarkupAttributes;
using UnityEngine;

namespace RivenFramework
{
    public class GameInstance : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [SerializeReference] public GameInstanceModule[] modules;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public static ActorDomain globalActorDomain;
            public static GameInstance Instance 
            { 
                get 
                {
                    if (_instance == null) 
                        throw new NullReferenceException("GameInstance is missing");
                    return _instance;
                } 
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            //protected Dictionary<Type, GameInstanceModule> moduleDictionary;
            private static GameInstance _instance;
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void Awake()
            {
                if (_instance != null)
                {
                    Destroy(gameObject);
                    return;
                }

                _instance = this;
                DontDestroyOnLoad(_instance);
                InitializeGameInstance();
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
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
            public static bool TryGet<T>(out T gameInstanceModule) where T : MonoBehaviour
            {
                gameInstanceModule = _instance != null ? _instance.GetComponent<T>() : null;
                return gameInstanceModule != null;
            }

            public static Coroutine SendCoroutine(IEnumerator coroutine) => Instance.StartCoroutine(coroutine);
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private void InitializeGameInstance()
            {
                globalActorDomain = ActorDomain.CreateNewGlobalDomain();
                globalActorDomain.gameObject.name = "Global Actor Domain";

                foreach (GameInstanceModule module in modules)
                {

                }
            }

            // Reload Static Fields
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            static void InitializeStaticFields()
            {
                _instance = null;
            }
        #endregion
        #endregion
    }
}
