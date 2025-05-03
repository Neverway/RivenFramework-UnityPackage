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


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private Image loadingBar;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    

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
            SceneManager.MoveGameObjectToScene(actor.gameObject, SceneManager.GetActiveScene());
        }
    }

    private IEnumerator LoadWorldCoroutine(string _worldName)
    {
        isLoading = true;
        yield return new WaitForSeconds(delayBeforeWorldChange);

        // Load the transition level over top everything else
        SceneManager.LoadScene(loadingWorldID);
        
        // The following should execute on the loading screen scene
        var loadingBarObject = GameObject.FindWithTag("LoadingBar");
        if (loadingBarObject) loadingBar = loadingBarObject.GetComponent<Image>();
        
        // Begin async load of target level
        AsyncOperation loadAsync = SceneManager.LoadSceneAsync(_worldName);
        loadAsync.allowSceneActivation = false;

        // Set loading bar to reflect async progress
        while (loadAsync.progress < 1)
        {
            if (loadingBar) loadingBar.fillAmount = loadAsync.progress;
            yield return new WaitForSeconds(minimumRequiredLoadTime);
        }

        // Eject saved actors from previous
        loadAsync.allowSceneActivation = true;
        EjectStreamedActors();

        // Finish up by setting any external flags
        isLoading = false;
        if (OnWorldLoaded is not null) OnWorldLoaded.Invoke();
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
        StartCoroutine(LoadWorldCoroutine(_worldName));
    }

    /// <summary>
    /// Load a target world immediately, disregarding loading times, transitions, and streamed actors. This is not recommended in most cases!
    /// </summary>
    /// <param name="_worldName">The name of the world to load</param>
    public void ForceLoadWorld(string _worldName)
    {
        SceneManager.LoadScene(_worldName);
    }
}
}
