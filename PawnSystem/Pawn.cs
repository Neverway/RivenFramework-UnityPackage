//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using UnityEngine;

public class Pawn : Actor
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Header("Pawn Data")]
    [Tooltip("If true, the pawn will not be able to perform any actions")]
    public bool isPaused;
    [Tooltip("If true, the pawn will be considered dead and will be queued for despawning if enabled")]
    public bool isDead;
    [Tooltip("If enabled, the pawn will be despawned when it dies after the despawnOnDeathDelay time passes")]
    public bool despawnOnDeath;
    [Tooltip("How long after a pawn dies until it is despawned (if despawnOnDeath is enabled)")]
    public float despawnOnDeathDelay=3f;


    //=-----------------=
    // Private Variables
    //=-----------------=
    private bool isInvulnerable;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    public PawnStats defaultStats;
    public PawnStats currentStats;
    public PawnActions action;
    public Pawn_AttachmentPoint physObjectAttachmentPoint;

    public event Action OnPawnHurt;
    public event Action OnPawnHeal;
    public event Action OnPawnDeath;

    //=-----------------=
    // Mono Functions
    //=-----------------=


    //=-----------------=
    // Internal Functions
    //=-----------------=
    private IEnumerator InvulnerabilityCooldown()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(((PawnStats)currentStats).invulnerabilityTime);
        isInvulnerable = false;
    }
    

    //=-----------------=
    // External Functions
    //=-----------------=
    public void ModifyHealth(float _value)
    {
        if (isInvulnerable) return;
        StartCoroutine(InvulnerabilityCooldown());
        switch (_value)
        {
            case > 0:
                OnPawnHeal?.Invoke();
                isDead = false;
                break;
            case < 0:
                if (isDead) return;
                OnPawnHurt?.Invoke();
                break;
        }

        if (currentStats.health + _value <= 0)
        {
            if (isDead) return;
            OnPawnDeath?.Invoke();
            isDead = true;
            if (despawnOnDeath)
            {
                Destroy(gameObject, despawnOnDeathDelay);
            }
        }

        if (currentStats.health + _value > currentStats.health) currentStats.health = currentStats.health;
        else if (currentStats.health + _value < 0) currentStats.health = 0;
        else currentStats.health += _value;
    }
    
    public void Kill()
    {
        // Instantly sets the pawns health to zero, firing its onDeath event
        ModifyHealth(-999999);
    }
}
