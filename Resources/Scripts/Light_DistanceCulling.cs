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
            float randomScanStaggerOffset = Random.Range(0f, 0.15f);
            InvokeRepeating(nameof(CheckCulling), randomScanStaggerOffset, 0.1f);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(CheckCulling));
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

        private void CheckCulling()
        {
            if (GetLocalPlayer() is false || cullWhenOutOfRange is false) return;
            bool inRange = LightIsInActiveRange();
            
            if (fadeLightWhenCulled)
            {
                // Light is out of range & intensity is full
                if (inRange is false && targetLight.intensity >= storedLightIntensity)
                {
                    // Fadeout
                    StartCoroutine(FadeLight("out"));
                }
                // Light is in range & intensity is zero
                if (inRange && targetLight.intensity <= 0f)
                {
                    // Fadein
                    StartCoroutine(FadeLight("in"));
                }
            }
            else
            {
                targetLight.enabled = inRange;
            }
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

        private bool LightIsInActiveRange()
        {
            if (!localPlayer)
            {
                GetLocalPlayer();
                return false;
            }

            return (transform.position - localPlayer.position).sqrMagnitude <= cachedCullRange;
        }

        private IEnumerator FadeLight(string _fadeDirection)
        {
            float timeElapsed = 0;
            if (_fadeDirection is "in")
            {
                targetLight.enabled = true;
                while (timeElapsed < fadeSpeed)
                {
                    targetLight.intensity = Mathf.Lerp(0, storedLightIntensity, timeElapsed / fadeSpeed);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
                
                targetLight.intensity = storedLightIntensity;
            }
            else if (_fadeDirection is "out")
            {
                while (timeElapsed < fadeSpeed)
                {
                    targetLight.intensity = Mathf.Lerp(storedLightIntensity, 0, timeElapsed / fadeSpeed);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
                targetLight.enabled = false;
                targetLight.intensity = 0;
            }
        }

        private void RecalculateCullRange()
        {
            float range = targetLight.range * rangeMultiplier;
            cachedCullRange = range * range;
        }


        //=-----------------=
        // External Functions
        //=-----------------=


    }
