//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
[GIModuleColor(_color: GIModuleColors.Blue)]
public class GI_MapLoader : GameInstanceModule
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Project Configuration")]
    [Tooltip("When a level load occurs, this is how long to wait before the new map begins loading.")]
    [SerializeField] public float delayBeforeWorldChange = 0.25f;
    [Tooltip("The minimum amount of time to stay on the loading screen to avoid flashing the load screen too quickly.")]
    public float minimumRequiredLoadTime;
    
    [Header("Debugging")]
    [Tooltip("If true, a level load is in progress.")]
    public bool isLoading;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public static event Action OnWorldLoaded;
    public static event Action OnEjectStreamedActors;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
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

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
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
        GameInstance.SendCoroutine(LoadWorldCoroutine(_worldName));
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


    //=----Reload Static Fields----=
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeStaticFields()
    {
        OnWorldLoaded = null;
        OnEjectStreamedActors = null;
    }


    #endregion
}
