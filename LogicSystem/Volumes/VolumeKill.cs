//==========================================( Neverway 2024 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    public class VolumeKill : Volume
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
    

        //=-----------------=
        // Internal Functions
        //=-----------------=
        private new void OnTriggerEnter(Collider _other)
        {
            // R.I.P "Connorses has a hat" debug log message 2024-2025 ~Liz
        
            base.OnTriggerEnter(_other); // Call the base class method
            if (_other.CompareTag("Pawn"))
            {
                _other.GetComponentInParent<Pawn>().Kill();
            }
        }


        //=-----------------=
        // External Functions
        //=-----------------=
    }
}
