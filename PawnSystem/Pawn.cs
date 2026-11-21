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

using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

namespace RivenFramework
{
    public class Pawn : Actor
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        [Header("Pawn Data")]
        [Tooltip("If true, the pawn will be considered dead and will be queued for despawning if enabled")]
        public bool isDead;
        [Tooltip("If enabled, the pawn will be despawned when it dies after the despawnOnDeathDelay time passes")]
        public bool despawnOnDeath;
        [Tooltip("How long after a pawn dies until it is despawned (if despawnOnDeath is enabled)")]
        public float despawnOnDeathDelay = 3f;
        [Tooltip("This is used for calculating delta offsets for pawns on rotating platforms")]
        [HideInInspector] public float platformYOffset;

        public bool isPlayerControlled;


        [Tooltip("Whether or not the pawn is being paused from some source")]
        public bool IsPaused => pauseTokens.Count > 0;


        //=-----------------=
        // Private Variables
        //=-----------------=
        private bool isInvulnerable;
        private HashSet<object> pauseTokens = new HashSet<object>();

        //=-----------------=
        // Reference Variables
        //=-----------------=
        public PawnStats defaultStats;
        public PawnStats currentStats;
        public PawnActions action;
        public Transform viewPoint;
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
            if (_value == 0 || isDead) return;
            if (_value < 0 && isInvulnerable) return;
            
            float newHealth = Mathf.Clamp(currentStats.health + _value, 0, defaultStats.health);
            if (_value > 0 && newHealth == currentStats.health) return;
            
            currentStats.health = newHealth;
            if (_value > 0)
            {
                OnPawnHeal?.Invoke();
                return;
            }
            
            StartCoroutine(InvulnerabilityCooldown());
            
            OnPawnHurt?.Invoke();
            if (newHealth <= 0)
            {
                isDead = true;
                OnPawnDeath?.Invoke();
                if (despawnOnDeath) Destroy(gameObject, despawnOnDeathDelay);
            }
        }

        // Instantly sets the pawns health to zero, firing its onDeath event
        public void Kill() => ModifyHealth(-float.MaxValue);

        public void Pause(out object token) => Pause(token = new());
        public void Pause(object token) => pauseTokens.Add(token);
        public void Unpause(object token) => pauseTokens.Remove(token);
    }
}
