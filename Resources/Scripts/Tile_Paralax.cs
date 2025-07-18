//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Simulate parallax scrolling on a tilemap for use with orthographic cameras.
// Notes: This script adjusts the scale and position of the tilemap based on the camera's movement.
//
//=============================================================================

using RivenFramework;
using UnityEngine;

    public class Tile_Parallax : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        [Tooltip("How much the tiles will shift from their starting position")]
        [SerializeField] private Vector2 parallaxAmount;
        [Tooltip("Which gamemode should the parallaxing be active during")]
        //[SerializeField] private string activeGameMode = "Topdown2D";


        //=-----------------=
        // Private Variables
        //=-----------------=
        private Vector3 originalPosition;
        private Vector3 originalScale;
        private Vector3 parallaxScale;


        //=-----------------=
        // Reference Variables
        //=-----------------=
        private GI_PawnManager pawnManager;
        private Transform localPlayer;


        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Start()
        {
            // Calculate the new local scale based on the parallax amount
            originalPosition = transform.position;
            originalScale = transform.localScale;
            parallaxScale = new Vector3(
                originalScale.x + (parallaxAmount.x / 2), 
                originalScale.y + (parallaxAmount.y / 2), 
                originalScale.z);
        }

        private void Update()
        {
            if (GetLocalPlayer() is false) return;

            // Only set the scale to the parallax scale if the correct gamemode is active
            //if (pawnManager.GetActiveGamemode("Topdown2D"))
            //{
                transform.localScale = parallaxScale;
            /*}
            else
            {
                transform.localScale = originalScale;
                transform.position = originalPosition;
            }*/

            // Calculate the distance to move based on the parallax amount
            Vector3 position = localPlayer.position;
            Vector2 distance = new Vector2(position.x * -parallaxAmount.x, position.y * -parallaxAmount.y);
            Vector3 newPosition = new Vector3(distance.x, distance.y, originalPosition.z);

            // Set the new position
            transform.position = newPosition;
        }


        //=-----------------=
        // Internal Functions
        //=-----------------=
        private bool GetLocalPlayer()
        {
            if (localPlayer is null)
            {
                if (pawnManager is null)
                {
                    pawnManager = FindObjectOfType<GI_PawnManager>();
                    if (pawnManager is null)
                    {
                        return false;
                    }
                }

                if (pawnManager.localPlayerCharacter is null)
                {
                    return false;
                }

                localPlayer = pawnManager.localPlayerCharacter.transform;
            }

            return true;
        }


        //=-----------------=
        // External Functions
        //=-----------------=
    }