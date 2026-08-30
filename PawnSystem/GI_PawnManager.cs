//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    public class GI_PawnManager : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        public GameObject defaultPawn;
        public GameObject localPlayerCharacter;


        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=
        public static GI_PawnManager Instance { get; private set; }


        //=-----------------=
        // Mono Functions
        //=-----------------=
        [Todo("Using Awake on GameInstance components does not work", Owner = "Errynei")]
        private void Awake()
        {
            Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Instance = this;
        }


        //=-----------------=
        // Internal Functions
        //=-----------------=


        //=-----------------=
        // External Functions
        //=-----------------=
    }
}
