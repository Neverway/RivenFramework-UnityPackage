//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Optimizes real-time lights by slowly fading them out when the local
//  player is too far away, and reporting to the GI_LightShadowBudgetManager so
//  only a limited number of in-range lights can cast real-time shadows at the same time
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using RivenFramework;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Light))]
    public class Light_DistanceCulling : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        [Header("Range Culling")]
        [Tooltip("If enabled, the light will be disabled when the local player is out of range")]
        [SerializeField] private bool cullWhenOutOfRange;
        [Tooltip("The range of the light is used to determine the range of culling, this multiplier expands that range")]
        [SerializeField] private float rangeMultiplier = 1;
        [Tooltip("If enabled, the light will fade out instead of cutting off")]
        [SerializeField] private bool fadeLightWhenCulled = true;
        [Tooltip("The duration, in seconds, it takes to fade the light in and out")]
        [SerializeField] private float fadeSpeed = 0.2f;
        [Tooltip("If enabled, a sphere will be drawn around the light representing the culling range of the light in the editor")]
        [SerializeField] private bool debugDrawCullRange;
        
        [Header("Shadow Budgeting")]
        [Tooltip("If enabled, this light will never cast shadows, so it won't take up any available shadow caster slots and won't harm performance as much")]
        [SerializeField] private bool neverCastShadows;
        [Tooltip("If enabled, this light will always cast shadows and won't take up a slot in the available shadow caster slots (Once again, use sparingly, shadows are effin expensive man!)")]
        [SerializeField] private bool alwaysCastShadows;
        [Tooltip("The higher this value, the higher the importance of this light when being ranked for available shadow caster slots (So set this value higher for lights you wanna keep casting shadows, but use is sparingly!)")]
        [SerializeField] private float shadowPriorityBias;
        
        
        //=-----------------=
        // Private Variables
        //=-----------------=
        // The original intensity of the light before we began fading it out
        private float storedLightIntensity;
        // The original intensity of the lights shadows before we began fading it out
        private float storedShadowStrength;
        // The original shadow type this light uses before it may have been culled out
        private LightShadows storedShadowType;
        // Stored value of the range this light culls at so it doesn't get recalculated constantly
        private float cachedCullRange;
        
        
        
        //=-----------------=
        // Reference Variables
        //=-----------------=
        private Light targetLight;
        private Coroutine intensityFadeRoutine;
        private Coroutine shadowFadeRoutine;
        private Transform localPlayer;
        // GI_LightShadowBudgetManager stuffs
        public bool CanCastShadows => !neverCastShadows && storedShadowType != LightShadows.None;
        public bool IsInRange { get; private set; }
        
        
        
        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Start()
        {
            targetLight = GetComponent<Light>();
            storedLightIntensity = targetLight.intensity;
            storedShadowStrength = targetLight.shadowStrength;
            storedShadowType = targetLight.shadows;
            RecalculateCullRange();
        }

        private void OnEnable()
        {
            if (GI_LightShadowBudgetManager.Instance) GI_LightShadowBudgetManager.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (GI_LightShadowBudgetManager.Instance) GI_LightShadowBudgetManager.Instance.Unregister(this);
        }

        private void OnDrawGizmosSelected()
        {
            if (!debugDrawCullRange) return;
            // This get component needs to be here for the editor to get the light
            if (!targetLight) targetLight = GetComponent<Light>();
            Gizmos.color = new Color(0.9f,0.5f,0.0f,0.25f);
            Gizmos.DrawSphere(transform.position, targetLight.range * rangeMultiplier);
            Gizmos.color = new Color(0.9f,0.5f,0.0f,0.4f);
            Gizmos.DrawWireSphere(transform.position, targetLight.range * rangeMultiplier);
        }


        
        //=-----------------=
        // Internal Functions
        //=-----------------=
        private void RecalculateCullRange()
        {
            float r = targetLight.range * rangeMultiplier;
            cachedCullRange = r * r;
        }

        private void StartIntensityFade(bool fadeIn)
        {
            if (intensityFadeRoutine != null) StopCoroutine(intensityFadeRoutine);
            intensityFadeRoutine = StartCoroutine(IntensityFadeRoutine(fadeIn));
        }

        private IEnumerator IntensityFadeRoutine(bool fadeIn)
        {
            float timeElapsed = 0f;

            if (fadeIn)
            {
                targetLight.enabled = true;
                while (timeElapsed < fadeSpeed)
                {
                    targetLight.intensity = Mathf.Lerp(0f, storedLightIntensity, timeElapsed / fadeSpeed);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
                targetLight.intensity = storedLightIntensity;
            }
            else
            {
                while (timeElapsed < fadeSpeed)
                {
                    targetLight.intensity = Mathf.Lerp(storedLightIntensity, 0f, timeElapsed / fadeSpeed);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
                targetLight.enabled = false;
                targetLight.intensity = 0f;
            }
        }

        private IEnumerator ShadowFadeRoutine(bool enable, float fadeDuration)
        {
            float timeElapsed = 0f;

            if (enable)
            {
                targetLight.shadows = storedShadowType;
                targetLight.shadowStrength = 0f;
                while (timeElapsed < fadeDuration)
                {
                    targetLight.shadowStrength = Mathf.Lerp(0f, storedShadowStrength, timeElapsed / fadeDuration);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
                targetLight.shadowStrength = storedShadowStrength;
            }
            else
            {
                float startStrength = targetLight.shadowStrength;
                while (timeElapsed < fadeDuration)
                {
                    targetLight.shadowStrength = Mathf.Lerp(startStrength, 0f, timeElapsed / fadeDuration);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
                targetLight.shadowStrength = 0f;
                targetLight.shadows = LightShadows.None;
            }
        }


        //=-----------------=
        // External Functions
        //=-----------------=
        public bool EvaluateCulling(Vector3 playerPos)
        {
            if (!cullWhenOutOfRange)
            {
                IsInRange = true;
                return true;
            }

            IsInRange = (transform.position - playerPos).sqrMagnitude <= cachedCullRange;

            if (fadeLightWhenCulled)
            {
                if (!IsInRange && targetLight.intensity >= storedLightIntensity)
                    StartIntensityFade(false);
                if (IsInRange && targetLight.intensity <= 0f)
                    StartIntensityFade(true);
            }
            else
            {
                targetLight.enabled = IsInRange;
            }

            return IsInRange;
        }    
        
        public float GetShadowImportanceScore(Vector3 playerPos)
        {
            if (alwaysCastShadows) return float.MaxValue;

            float distSq = Mathf.Max((transform.position - playerPos).sqrMagnitude, 0.01f);
            float sizeWeight = targetLight.range * Mathf.Max(targetLight.intensity, 0.01f);
            return (sizeWeight * shadowPriorityBias) / distSq;
        }
        
        public void SetShadowCasting(bool enable, float fadeDuration)
        {
            if (enable && !CanCastShadows) return;
            if (shadowFadeRoutine != null) StopCoroutine(shadowFadeRoutine);
            shadowFadeRoutine = StartCoroutine(ShadowFadeRoutine(enable, fadeDuration));
        }


    }
