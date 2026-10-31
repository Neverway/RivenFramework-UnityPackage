//==========================================( Neverway 2024 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  If any key is pressed, return to title screen
//====================================================================================================================//

using RivenFramework;
using UnityEngine;

    public class LB_Error : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=


        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=


        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Update()
        {
            if (Input.anyKeyDown)
            {
                FindObjectOfType<GI_WorldLoader>().ForceLoadWorld("_Title");
            }
        }

        //=-----------------=
        // Internal Functions
        //=-----------------=


        //=-----------------=
        // External Functions
        //=-----------------=
    }