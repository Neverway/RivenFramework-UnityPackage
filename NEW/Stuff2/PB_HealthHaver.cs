//==========================================( Neverway 2026 )=========================================================//
// Author
//  Errynei
//
// Contributors
//  Liz M.
//
//====================================================================================================================//

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// A Pawn Behaviour that allows a pawn to take damage, heal, die, become invulnerable, and despawn
/// </summary>
[Serializable]
public class PB_HealthHaver : PawnBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Health")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    public float invulnerabilityTime = 1f;
    public bool despawnOnDeath;
    public float despawnOnDeathDelay = 3f;
    
    
    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public bool isInvulnerable;
    public bool isDead;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public event Action OnPawnHurt;
    public event Action OnPawnHeal;
    public event Action OnPawnDeath;


    #endregion


    #region=======================================( Functions )======================================================= //
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private IEnumerator InvulnerabilityCooldown()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void ModifyHealth(float _value)
    {
        if (isInvulnerable) return;
        pawn.StartCoroutine(InvulnerabilityCooldown());
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

        if (currentHealth + _value <= 0)
        {
            if (isDead) return;
            OnPawnDeath?.Invoke();
            isDead = true;
            if (despawnOnDeath)
            {
                GameObject.Destroy(pawn.gameObject, despawnOnDeathDelay);
            }
        }

        if (currentHealth + _value > maxHealth) currentHealth = maxHealth;
        else if (currentHealth + _value < 0) currentHealth = 0;
        else currentHealth += _value;
    }

    // Instantly sets the pawns health to zero, firing its onDeath event
    public void Kill() => ModifyHealth(-float.MaxValue);


    #endregion
}
