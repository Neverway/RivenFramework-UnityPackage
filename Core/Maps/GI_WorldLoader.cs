//==========================================( Neverway 2027 )=========================================================//
// Author
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

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RivenFramework
{
    public class GI_WorldLoader : MonoBehaviour
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Tooltip("The amount of time to wait when a load level function is called before the level actually begins loading")]
            [SerializeField] public float delayBeforeWorldChange = 0.25f;
            [Tooltip("The minimum amount of time to wait on the loading screen between level changes")]
            [SerializeField] public float minimumRequiredLoadTime = 1f;
            [Tooltip("The ID of the level that we will wait on during loading (It's the loading screen level)")]
            [SerializeField] private string loadingWorldID = "_Travel";
            [Tooltip("The ID of the level that we store objects in between level changes to avoid them being unloaded")]
            public string streamingWorldID = "_Streaming";
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            private Image loadingBar;
            private Scene streamingScene;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            [Tooltip("An exposed value used for referencing if the game is currently in the process of loading the level")]
            public bool isLoading;
            public static event Action OnWorldLoaded;
            public static event Action OnEjectStreamedActors;
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )======================================================//

        #region----------------( Inherited )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            /// <summary>
            /// Load a target world (targeted by name) asynchronously, respecting loading times, transitions, and streamed actors
            /// </summary>
            /// <param name="_worldName">The name of the world to load</param>
            public void LoadWorld(string _worldName)
            {
                if (_worldName == SceneManager.GetActiveScene().name) Debug.LogWarning("Target world is the same as the loaded world, this causes strange issues please dont do this!!!!");
            
                if (isLoading)
                {
                    Debug.LogWarning("failed to load world: " + _worldName + " already loading");
                    return;
                }
                if (DoesSceneExist(_worldName) is false)
                {
                    Debug.LogError($"Could not find scene '{_worldName}'");
                    ForceLoadWorld("_Error");
                    return;
                }
                StartCoroutine(LoadWorldCoroutine(_worldName));
            }

            /// <summary>
            /// Load a target world immediately, disregarding loading times, transitions, and streamed actors. This is not recommended in most cases!
            /// </summary>
            /// <param name="_worldName">The name of the world to load</param>
            public void ForceLoadWorld(string _worldName)
            {
                if (DoesSceneExist(_worldName) is false && _worldName != "_Error")
                {
                    ForceLoadWorld("_Error");
                }
                SceneManager.LoadScene(_worldName);
                OnWorldLoaded?.Invoke();
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private IEnumerator LoadWorldCoroutine(string _worldName)
            {
                isLoading = true;
                yield return new WaitForSeconds(delayBeforeWorldChange);
        
                var previousScene = SceneManager.GetActiveScene();

                var originalTimescale = Time.timeScale;
                Time.timeScale = 0;
        
                AsyncOperation loadAsync = SceneManager.LoadSceneAsync(_worldName, LoadSceneMode.Additive);
                while (!loadAsync.isDone) { yield return new WaitForEndOfFrame(); }
        
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(_worldName));
        
                //print("Called ejection");
                //yield return StartCoroutine(WaitForEjectCompletion());
        
                AsyncOperation unloadAsync = SceneManager.UnloadSceneAsync(previousScene);
                while (!unloadAsync.isDone) { yield return new WaitForEndOfFrame(); }

                Time.timeScale = originalTimescale;

                isLoading = false;
                
                OnWorldLoaded?.Invoke();
            }

            // This code was expertly copied from @Yagero on github.com
            // https://gist.github.com/yagero/2cd50a12fcc928a6446539119741a343
            // (Seriously though, this function is a lifesaver, so thanks!)
            public static bool DoesSceneExist(string _targetSceneID)
            {
                if (string.IsNullOrEmpty(_targetSceneID)) return false;

                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    var lastSlash = scenePath.LastIndexOf("/");
                    var sceneName = scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1);

                    if (string.Compare(_targetSceneID, sceneName, true) == 0) return true;
                }

                return false;
            }
        
            private IEnumerator WaitForEjectCompletion()
            {
                OnEjectStreamedActors?.Invoke();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
            }
            
            
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void InitializeStaticFields()
            {
                OnWorldLoaded = null;
                OnEjectStreamedActors = null;
            }
        #endregion

        #endregion
    }
}