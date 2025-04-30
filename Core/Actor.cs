//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: This "Actor" Class is used to identify the objects placed in a map, so that they can be saved or loaded using their id.
//      It, or a subclass of it, should be present on all objects you wish to place in a map.
// Notes: This class can be inherited from to create subclasses of Actor, like Pawn
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Tooltip("This ID is how this actor is identified, saved, and loaded from map files")]
    public string id;
    [Tooltip("This is how this actor is listed in things like an asset browser, or in game like in an inventory")]
    public string displayName;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
    
    }

    private void Update()
    {
    
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}
