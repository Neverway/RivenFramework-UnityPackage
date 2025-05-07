//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumePain : Volume
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Tooltip("Negative values will heal pawns")] [SerializeField]
    private float damageAmount = 10;


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
        foreach (var entity in pawnsInTrigger)
        {
            // If no team specified, or self-infliction enabled, hurt everyone (Kinda metal huh)
            if (unaffectedTeams.Count == 0 || ignoreUnaffectedTeamsFilter)
            {
                entity.ModifyHealth(-damageAmount);
            }
            else
            {
                if (ignoreUnaffectedTeamsFilter) return;
                
                // If teams match allow healing only
                if (unaffectedTeams.Contains(entity.currentStats.team) && damageAmount < 0)
                {
                    entity.ModifyHealth(-damageAmount);
                }
                // If teams don't match allow pain only
                if (!unaffectedTeams.Contains(entity.currentStats.team) && damageAmount > 0)
                {
                    entity.ModifyHealth(-damageAmount);
                }
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
