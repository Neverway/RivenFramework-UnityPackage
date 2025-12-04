//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VolumePlatformMagnet : Volume
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
    public GameObject reparentContainer;


    //=-----------------=
    // Mono Functions
    //=-----------------=
        private void Awake()
        {
        }

        private new void OnTriggerStay(Collider _other)
        {
            
            // Pawn has entered the volume
            if (_other.CompareTag("Pawn"))
            {
                // Get a reference to the entity component
                var targetEntity = _other.gameObject.GetComponent<Pawn>();
                
                // Exit if the object is already parented
                if (targetEntity.transform.parent == reparentContainer.transform) return;
                
                // Add the entity to the list if they are not already present
                ReparentToPlatform(targetEntity.gameObject);
            }

            // A physics prop has entered the volume
            if (_other.CompareTag("PhysProp"))
            {
                // Get a reference to the entity component
                var targetProp = _other.gameObject.GetComponentInParent<Actor>().gameObject;
                
                // Exit if the object is already parented
                if (targetProp.transform.parent == reparentContainer.transform) return;
                
                // Add the entity to the list if they are not already present
                ReparentToPlatform(targetProp);
            }
        }

        private new void OnTriggerExit(Collider _other)
        {
            
            // Pawn has entered the volume
            if (_other.CompareTag("Pawn"))
            {
                //print($"{gameObject.name} has triggered a dump");
                // Get a reference to the entity component
                var targetEntity = _other.gameObject.GetComponent<Pawn>();
                
                targetEntity.transform.SetParent(null);
            }

            // A physics prop has entered the volume
            if (_other.CompareTag("PhysProp"))
            {
                // Get a reference to the entity component
                var targetProp = _other.gameObject.GetComponentInParent<Actor>().gameObject;
                
                targetProp.transform.SetParent(null);
            }
        }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void ReparentToPlatform(GameObject _targetObject)
    {
        //print($"{gameObject.name} has triggered a move event");
        // Clear its parent to avoid random bugs
        _targetObject.transform.SetParent(null);
        _targetObject.transform.SetParent(reparentContainer.transform);
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
