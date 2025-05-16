//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeForce : Volume
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [SerializeField] private float actorForceIntensity;
    [SerializeField] private float propForceIntensity;
    [SerializeField] private ForceMode forceMode;
    [SerializeField] private ForceMode2D forceMode2d;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    [Tooltip("Uses the forward vector from this object to determine the direction to apply the force")]
    [SerializeField] private Transform forceDirection;


    //=-----------------=
    // Mono Functions
    //=-----------------=
        private void FixedUpdate()
        {
            // Push actors
            foreach (var entity in pawnsInTrigger)
            {
                if (entity is null) continue;
                var rigidbody2D = entity.gameObject.GetComponent<Rigidbody2D>();
                var rigidbody = entity.gameObject.GetComponent<Rigidbody>();

                // If no team specified, push everyone
                if (unaffectedTeams.Count == 0)
                {
                    if (rigidbody2D)
                    {
                        rigidbody2D.AddForce(forceDirection.transform.forward * actorForceIntensity, forceMode2d);
                    }
                    if (rigidbody)
                    {
                        rigidbody.AddForce(forceDirection.transform.forward * actorForceIntensity, forceMode);
                    }
                }
                // If teams match and self-infliction enabled
                if (unaffectedTeams.Contains(entity.currentStats.team) && ignoreUnaffectedTeamsFilter)
                {
                    if (rigidbody2D)
                    {
                        rigidbody2D.AddForce(forceDirection.transform.forward * actorForceIntensity, forceMode2d);
                    }
                    if (rigidbody)
                    {
                        rigidbody.AddForce(forceDirection.transform.forward * actorForceIntensity, forceMode);
                    }
                }
                // If teams don't match
                if (!unaffectedTeams.Contains(entity.currentStats.team))
                {
                    if (rigidbody2D)
                    {
                        rigidbody2D.AddForce(forceDirection.transform.forward * actorForceIntensity, forceMode2d);
                    }
                    if (rigidbody)
                    {
                        rigidbody.AddForce(forceDirection.transform.forward * actorForceIntensity, forceMode);
                    }
                }
            }

            foreach (var entity in propsInTrigger)
            {
                if (entity is null) continue;
                var rigidbody2D = entity.gameObject.GetComponent<Rigidbody2D>();
                var rigidbody = entity.gameObject.GetComponent<Rigidbody>();

                if (rigidbody)
                {
                    rigidbody.AddForce(forceDirection.transform.forward * propForceIntensity, forceMode);
                }
                else if (rigidbody2D)
                {
                    rigidbody2D.AddForce(forceDirection.transform.forward * propForceIntensity, forceMode2d);
                }
            }
        }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}
