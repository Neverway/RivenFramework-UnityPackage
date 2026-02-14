//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RivenFramework
{
public class GI_WorldLoader : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [SerializeField] public float delayBeforeWorldChange = 0.25f;
    [Tooltip("The minimum amount of time to wait on the loading screen between level changes")]
    [SerializeField] public float minimumRequiredLoadTime = 1f;
    [Tooltip("The ID of the level that we will wait on during loading (It's the loading screen level)")]
    [SerializeField] private string loadingWorldID = "_Travel";
    [Tooltip("The ID of the level that we store objects in between level changes to avoid them being unloaded")]
    public string streamingWorldID = "_Streaming";
    [Tooltip("An exposed value used for referencing if the game is currently in the process of loading the level")]
    public bool isLoading;
    public static event Action OnWorldLoaded;
    public static event Action OnEjectStreamedActors;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private Image loadingBar;
    private Scene streamingScene;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Update()
    {
        // Make sure the streaming world is loaded, so we can store actors there if needed
        if (!SceneManager.GetSceneByName(streamingWorldID).isLoaded)
        {
            //print("Streaming world wasn't found, initializing...");
            
            // [Sanity Check] Store the currently active scene
            var activeScene = SceneManager.GetActiveScene();
            
            // Load the streaming world
            SceneManager.LoadScene(streamingWorldID, LoadSceneMode.Additive);
            streamingScene = SceneManager.GetSceneByName(streamingWorldID);
            
            // [Sanity Check] Ensure the active scene is still the original scene and not the streaming world
            if (SceneManager.GetActiveScene() != activeScene)
            {
                print($" =^@ m @^= WorldLoader sanity check failed: active scene is {SceneManager.GetActiveScene().name}, active scene should be {activeScene.name}. Resetting...");
                SceneManager.SetActiveScene(activeScene);
            }
        }
    }
    

    //=-----------------=
    // Internal Functions
    //=-----------------=
    /// <summary>
    /// When the level is loaded, this will remove any objects we wanted to keep during a level change, from the streaming level
    /// </summary>
    private void EjectStreamedActors()
    {
        foreach (var actor in SceneManager.GetSceneByName(streamingWorldID).GetRootGameObjects())
        {
            print($"found actor {actor.name}");
            SceneManager.MoveGameObjectToScene(actor.gameObject, SceneManager.GetActiveScene());
        }
    }

    private IEnumerator LoadWorldCoroutine(string _worldName)
    {
        // WARNING OLD GARBAGE BELOW //
        // Load the transition level over top everything else
        //SceneManager.LoadScene(loadingWorldID, LoadSceneMode.Additive);
        
        // The following should execute on the loading screen scene
        //var loadingBarObject = GameObject.FindWithTag("LoadingBar");
        //if (loadingBarObject) loadingBar = loadingBarObject.GetComponent<Image>();
        
        /*AsyncOperation loadAsync = SceneManager.LoadSceneAsync(_worldName, LoadSceneMode.Additive);
        while (!loadAsync.isDone)
        {
            //print(loadAsync.progress);
            //if (loadingBar) loadingBar.fillAmount = loadAsync.progress;
            yield return null;
        }*/
        
        //print(unloadAsync.progress);
        //if (loadingBar) loadingBar.fillAmount = unloadAsync.progress;
        // END OF OLD GARBAGE (Carry on :3) //
        //Debug.Log("awaiting to load world");
        isLoading = true;
        yield return new WaitForSeconds(delayBeforeWorldChange);
        //Debug.Log("loading world: " + _worldName);
        
        // BEWARE THOSE WHO MAY CODE HERE!
        // Traveler, if you find yourself here, I can only say I wish you Good Luck!
        // You may be wondering why we unload the level before loading the next one.
        // Let me explain it to you like this:
        // Imagine you have a room filled with fire. Now you want to load a room full of puppies.
        // If you load the puppies first, they will be set on fire.
        // Please don't set fire to puppies.
        // ~Past Liz
        
        // I have terrible news... there's a catch.
        // Let me explain.
        // Imagine you have a congo of kittens. Now the kittens are walking through a loading stream volume.
        // The kittens want to go to the room full of puppies.
        // If the room full of puppies takes too long to load,
        // The kittens will be streamed,
        // and promptly fall into the void.
        // Please don't drop kittens into the void.
        // ~Future Liz
        
        // I tried freezing the theoretical kittens by setting timescale to 0 and then restoring once
        // the theoretical room full of puppies is done loading.
        
        // Store what scene is the current one
        var previousScene = SceneManager.GetActiveScene();
        //print($"Current scene is {previousScene.name}");

        var originalTimescale = Time.timeScale;
        Time.timeScale = 0;
        
        // Load target level
        //print($"Loading next scene {_worldName}...");
        AsyncOperation loadAsync = SceneManager.LoadSceneAsync(_worldName, LoadSceneMode.Additive);
        while (!loadAsync.isDone) { yield return new WaitForEndOfFrame(); }
        //print($"Loaded next scene");;
        
        // Assign the new scene to be the active scene
        //print($"Setting the active scene to {_worldName}. Active scene was {SceneManager.GetActiveScene().name}");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_worldName));
        
        // Empty the stream world into the active scene
        EjectStreamedActors();
        // Call the eject event so containers will empty into their current scene (Needs to be done a bit after eject)
        print("Called ejection");
        OnEjectStreamedActors?.Invoke();
        
        // Begin async unload of previous level
        //print($"Unloading previous scene {previousScene.name}...");
        AsyncOperation unloadAsync = SceneManager.UnloadSceneAsync(previousScene);
        while (!unloadAsync.isDone) { yield return new WaitForEndOfFrame(); }
        //print($"Unloaded previous scene");

        Time.timeScale = originalTimescale;

        // Finish up by setting any external flags
        isLoading = false;
        //Debug.Log("completed world loading");
        if (OnWorldLoaded is not null) OnWorldLoaded.Invoke();
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


    //=-----------------=
    // External Functions
    //=-----------------=
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
    }
}
}
