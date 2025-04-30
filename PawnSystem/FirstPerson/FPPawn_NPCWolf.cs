//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.PawnManagement
{
public class FPPawn_NPCWolf : FPPawn
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public bool logBehaviour;


    //=-----------------=
    // Private Variables
    //=-----------------=
    private Vector3 moveDirection;
    private bool isWandering;// Used to avoid constant overwrites of the moveFor function


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private FPPawnActions action = new FPPawnActions();


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public void Awake()
    {
        base.Awake();
        // Get references
        rigidbody = GetComponent<Rigidbody>();
        viewPoint = transform.Find("ViewPoint").gameObject;
    }

    public void FixedUpdate()
    {
        action.Look(this, ((FPPawnStats)currentStats).lookRange);

        // Get my starting stats
        var currentCourage = ((FPPawnStats)currentStats).courage;
        var collectiveCourage = currentCourage + action.GetCollectiveAllyCourage(this, visibleAllies);
        var closestAlly = action.GetClosest(this, visibleAllies);
        var targetEnemy = action.GetClosest(this, visibleHositles);
        
        // If I see an enemy
        if (visibleHositles.Count > 0)
        {
            if (logBehaviour) print($"Seen");
            // I am low on health
            if (((FPPawnStats)currentStats).health <= 25)
            {
                if (logBehaviour) print($"Low {currentCourage}");
                // Courage drops
                currentCourage = 0;
                collectiveCourage = currentCourage + action.GetCollectiveAllyCourage(this, visibleAllies);
            }
            // There is an ally nearby
            if (visibleAllies.Count > 0)
            {
                if (logBehaviour) print($"Ally!");
                // The Ally is within a comfortable range
                if (Vector3.Distance(closestAlly.transform.position, this.transform.position) <= ((FPPawnStats)currentStats).comfortableAllyDistance)
                {
                    if (logBehaviour) print($"Comfort");
                    // Our collective courage is greater or equal to our enemy courage
                    if (collectiveCourage >= ((FPPawnStats)targetEnemy.currentStats).courage)
                    {
                        if (logBehaviour) print($"courage strong {collectiveCourage}!");
                        // If in range
                        if (Vector3.Distance(this.transform.position, targetEnemy.transform.position) <= 3)
                        {
                            if (logBehaviour) print($"attack");
                            // Look at them
                            action.FaceTowardsPosition(this, viewPoint, targetEnemy.transform.position, 5);
                            // Attack enemy
                            action.Jump(this);
                        }
                        else
                        {
                            if (logBehaviour) print($"approach");
                            // Look at them
                            action.FaceTowardsPosition(this, viewPoint, targetEnemy.transform.position, 5);
                            // Approach
                            moveDirection = new Vector3(0, 0, 1);
                        }
                    }
                    // Else
                    else
                    {
                        if (logBehaviour) print($"courage weak {collectiveCourage}!");
                        // Run away
                        // Look at them
                        action.FaceTowardsPosition(this, viewPoint, targetEnemy.transform.position, 5);
                        // Back away
                        moveDirection = new Vector3(0, 0, -1);
                    }
                }
                // The ally is NOT withing a comfortable range
                else
                {
                    if (logBehaviour) print($"uncomfortable.. repositioning!");
                    
                    // Look at them
                    action.FaceTowardsPosition(this, viewPoint, closestAlly.transform.position, 5);
                    // Back away
                    moveDirection = new Vector3(0, 0, 1);
                    // Move to ally
                    //action.MoveTo(closestAlly.transform.position);
                }
            }
            // There is NOT an ally nearby
            else
            {
                if (logBehaviour) print($"alone!");
                // I am NOT cornered
                    // My courage is greater or equal to enemy courage
                if (currentCourage >= ((FPPawnStats)targetEnemy.currentStats).courage)
                {
                    if (logBehaviour) print($"courage!");
                    // If in range
                    if (Vector3.Distance(this.transform.position, targetEnemy.transform.position) <= 3)
                    {
                        if (logBehaviour) print($"attack");
                        // Look at them
                        action.FaceTowardsPosition(this, viewPoint, targetEnemy.transform.position, 5);
                        // Attack enemy
                        action.Jump(this);
                    }
                    else
                    {
                        if (logBehaviour) print($"approach");
                        // Look at them
                        action.FaceTowardsPosition(this, viewPoint, targetEnemy.transform.position, 5);
                        // Approach
                        moveDirection = new Vector3(0, 0, 1);
                    }
                }
                // Else
                else
                {
                    if (logBehaviour) print($"run!");
                    // Run away
                    // Look at them
                    action.FaceTowardsPosition(this, viewPoint, targetEnemy.transform.position, 5);
                    // Back away
                    moveDirection = new Vector3(0, 0, -1);
                }
                // I am cornered
                    // Attack enemy
            }
        }
        else
        {
            if (!isWandering)
            {
                moveDirection = new Vector3(0, 0, 0);
                StartCoroutine(AttemptWander());
            }
        }
        
        action.Move(this, moveDirection, currentStats.movementSpeed);
    }

    
    //=-----------------=
    // Internal Functions
    //=-----------------=-=

    private IEnumerator AttemptWander()
    {
        isWandering = true;
        
        // Assign random duration to wander for
        float wanderDuration = Random.Range(1, 5);
        
        // 20% chance to wander
        bool willMove = Random.Range(0, 100) <= 15;
        
        // Assign random direction to wander
        action.FaceTowardsDirection(this, viewPoint, new Vector2(0, Random.Range(0, 360)));
        
        while (wanderDuration > 0)
        {
            if (willMove) moveDirection = new Vector3(0, 0, 1);
            wanderDuration -= 1f;
            yield return new WaitForSeconds(1);
        }
        isWandering = false;
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
}
