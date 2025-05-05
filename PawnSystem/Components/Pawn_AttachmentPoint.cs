//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Attached to an object on a pawn to be used like a socket for other
//      objects, such as held physics props, swords on backs, guns on hips, etc.
// Notes: This was originally created to keep track of held physics objects 
//
//=============================================================================

using UnityEngine;

    public class Pawn_AttachmentPoint : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        [Tooltip("The object that is attached to this point, this is set, not assigned, don't touch this")]
        public GameObject attachedObject;


        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=


        //=-----------------=
        // Mono Functions
        //=-----------------=
        

        //=-----------------=
        // Internal Functions
        //=-----------------=


        //=-----------------=
        // External Functions
        //=-----------------=
    }