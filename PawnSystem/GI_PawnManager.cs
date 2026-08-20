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
    private void Awake()
    {
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
