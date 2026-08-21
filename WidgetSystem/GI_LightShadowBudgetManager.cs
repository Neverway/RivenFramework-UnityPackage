//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Handles real-time light shadow culling stuffs
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GI_LightShadowBudgetManager : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public static GI_LightShadowBudgetManager Instance { get; private set; }
    [Header("Budget")]
    public int maxShadowCasters = 6;
    [SerializeField] private bool autoAdjustBudget = false;
    [SerializeField] private int minShadowCasters = 2;
    [SerializeField] private int maxShadowCastersCeiling = 12;
    [SerializeField] private float targetFrameTimeMs = 16.6f;
    
    [Header("Timing")]
    [SerializeField] private float reevaluateRankingInterval = 0.15f;
    [SerializeField] private float shadowFadeDuration = 0.25f;
    [SerializeField, Range(1f, 2f)] private float rankSwapMargin = 1.15f;
    
    
    //=-----------------=
    // Private Variables
    //=-----------------=
    private readonly List<Light_DistanceCulling> registeredLights = new List<Light_DistanceCulling>();
    private readonly HashSet<Light_DistanceCulling> currentShadowCasters = new HashSet<Light_DistanceCulling>();
    private readonly Queue<float> recentFrameTimes = new Queue<float>();
    private Transform localPlayer;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private Coroutine evaluationLoopRoutine;
    
    
    
    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (evaluationLoopRoutine != null) StopCoroutine(evaluationLoopRoutine);
        evaluationLoopRoutine = StartCoroutine(EvaluationLoop());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        recentFrameTimes.Clear();
    }

    //=-----------------=
    // External Functions
    //=-----------------=
    public void Register(Light_DistanceCulling light)
    {
        if (!registeredLights.Contains(light))
            registeredLights.Add(light);
    }

    public void Unregister(Light_DistanceCulling light)
    {
        registeredLights.Remove(light);
        currentShadowCasters.Remove(light);
    }


    //=-----------------=
    // Internal Functions
    //=-----------------=
    private bool GetLocalPlayer()
    {
        if (localPlayer) return true;
        var manager = GI_PawnManager.Instance;
        if (manager == null || !manager.localPlayerCharacter) return false;
        localPlayer = manager.localPlayerCharacter.transform;
        return localPlayer != null;
    }

    private IEnumerator EvaluationLoop()
    {
        var wait = new WaitForSeconds(reevaluateRankingInterval);
        while (true)
        {
            if (GetLocalPlayer())
            {
                EvaluateAllLights();
                if (autoAdjustBudget) AdjustBudgetBasedOnFrameTime();
            }
            yield return wait;
        }
    }

    private void EvaluateAllLights()
    {
        // Cull first, ask questions later
        var candidates = new List<(Light_DistanceCulling light, float score)>(registeredLights.Count);

        for (int i = 0; i < registeredLights.Count; i++)
        {
            var light = registeredLights[i];
            if (!light) continue;

            bool inRange = light.EvaluateCulling(localPlayer.position);
            if (!inRange || !light.CanCastShadows) continue;

            float score = light.GetShadowImportanceScore(localPlayer.position);
            if (currentShadowCasters.Contains(light)) score *= rankSwapMargin;
            
            candidates.Add((light, score));
        }
        
        // Top scorer's get a shadow slot
        var desired = new HashSet<Light_DistanceCulling>();
        for (int i = 0; i < candidates.Count && i < maxShadowCasters; i++)
        {
            desired.Add(candidates[i].light);
        }
        
        // Apply it ba-by!
        for (int i = 0; i < registeredLights.Count; i++)
        {
            var light = registeredLights[i];
            if (!light) continue;

            bool shouldCast = desired.Contains(light);
            bool isCasting = currentShadowCasters.Contains(light);

            if (shouldCast && !isCasting)
            {
                currentShadowCasters.Add(light);
                light.SetShadowCasting(true, shadowFadeDuration);
            }
            else if (!shouldCast && isCasting)
            {
                currentShadowCasters.Remove(light);
                light.SetShadowCasting(false, shadowFadeDuration);
            }
        }
    }

    private void AdjustBudgetBasedOnFrameTime()
    {
        recentFrameTimes.Enqueue(Time.unscaledDeltaTime * 1000f);
        if (recentFrameTimes.Count > 10) recentFrameTimes.Dequeue();

        float average = 0f;
        foreach (var time in recentFrameTimes)
        {
            average += time;
        }
        average /= recentFrameTimes.Count;
        
        if (average > targetFrameTimeMs * 1.15f && maxShadowCasters > minShadowCasters)
            maxShadowCasters--;
        else if (average < targetFrameTimeMs * 0.85f && maxShadowCasters < maxShadowCastersCeiling)
            maxShadowCasters++;
    }
    
    
    //=-----------------=
    // External Functions
    //=-----------------=
}
