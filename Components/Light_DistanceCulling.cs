//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Optimizes real-time lights by slowly fading them out when the local
//  player is too far away
// Notes:
//
//=============================================================================

using System.Collections;
using UnityEngine;

namespace Neverway.Framework.PawnManagement
{
    [RequireComponent(typeof(Light))]
    public class Light_DistanceCulling : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        [Tooltip("If enabled, the light will be disabled when the local player is out of range")]
        [SerializeField] private bool cullWhenOutOfRange;
        [Tooltip("The range of the light is used to determine the range of culling, this multiplier expands that range")]
        [SerializeField] private float rangeMultiplier = 1;
        [Tooltip("If enabled, the light will fade out instead of cutting off")]
        [SerializeField] private bool fadeLightWhenCulled = true;
        [Tooltip("The duration, in seconds, it takes to fade the light in and out")]
        [SerializeField] private float fadeSpeed = 0.2f;
        [Tooltip("If enabled, a sphere will be drawn around the light representing the culling range of the light")]
        [SerializeField] private bool debugDrawCullRange;
        
        
        //=-----------------=
        // Private Variables
        //=-----------------=
        // The original intensity of the light before we began fading it out
        private float storedLightIntensity;
        
        
        //=-----------------=
        // Reference Variables
        //=-----------------=
        private Light targetLight;
        private Transform localPlayer;
        private GameInstance gameInstance;
        
        
        
        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Start()
        {
            targetLight = GetComponent<Light>();
            gameInstance = FindObjectOfType<GameInstance>();
            storedLightIntensity = targetLight.intensity;
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

        private void Update()
        {
            if (GetLocalPlayer() is false || cullWhenOutOfRange is false) return;
            if (fadeLightWhenCulled)
            {
                // Light is out of range & intensity is full
                if (LightIsInActiveRange() is false && targetLight.intensity >= storedLightIntensity)
                {
                    // Fadeout
                    StartCoroutine(FadeLight("out"));
                }
                // Light is in range & intensity is zero
                if (LightIsInActiveRange() && targetLight.intensity <= 0f)
                {
                    // Fadein
                    StartCoroutine(FadeLight("in"));
                }
            }
            else
            {
                targetLight.enabled = LightIsInActiveRange();
            }
        }

        
        //=-----------------=
        // Internal Functions
        //=-----------------=
        private bool GetLocalPlayer()
        {
            if (localPlayer is null)
            {
                if (gameInstance is null)
                {
                    gameInstance = FindObjectOfType<GameInstance>();
                    if (gameInstance is null)
                    {
                        return false;
                    }
                }

                if (gameInstance.localPlayerCharacter is null)
                {
                    return false;
                }

                localPlayer = gameInstance.localPlayerCharacter.transform;
            }

            return true;
        }

        private bool LightIsInActiveRange()
        {
            return Vector3.Distance(transform.position, localPlayer.position) <= targetLight.range * rangeMultiplier;
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
        
        
        //=-----------------=
        // External Functions
        //=-----------------=
        
        
    }
}
