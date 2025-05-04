//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Gets the speed of a target object in m/s and displays it to a text element
// Notes:
//
//=============================================================================

using TMPro;
using UnityEngine;


namespace Neverway.Framework.PawnManagement
{
    [RequireComponent(typeof(TMP_Text))]
    public class Text_Speedometer : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        [Tooltip("If true, the target of this speedometer will be the local player (which this will auto-locate)")]
        [SerializeField] private bool useLocalPlayer;


        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=
        private TMP_Text velocityText;
        private GameInstance gameInstance;
        [Tooltip("The rigidbody this speedometer will be tracking")]
        [SerializeField] private Rigidbody targetRigidbody;


        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Start()
        {
            velocityText = GetComponent<TMP_Text>();
            gameInstance = FindObjectOfType<GameInstance>();
        }
        
        private void Update()
        {
            if (useLocalPlayer&& GetLocalPlayer() is false) return;

            if (targetRigidbody is not null)
            {
                velocityText.text = "Velocity: " + targetRigidbody.velocity.magnitude.ToString("F2") + " m/s";
            }
        }


        //=-----------------=
        // Internal Functions
        //=-----------------=
        private bool GetLocalPlayer()
        {
            if (targetRigidbody is null)
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

                if (gameInstance.localPlayerCharacter.GetComponent<Rigidbody>() is null)
                {
                    return false;
                }

                targetRigidbody = gameInstance.localPlayerCharacter.GetComponent<Rigidbody>();
            }

            return true;
        }


        //=-----------------=
        // External Functions
        //=-----------------=
    }
}