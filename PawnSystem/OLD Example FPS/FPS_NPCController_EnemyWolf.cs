//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using Framework.PawnManagement;
using UnityEngine;
/*
public class FPS_NPCController_EnemyWolf : PawnController
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
    private FPS_ActionList action = new FPS_ActionList();
    private Rigidbody rigidbody;
    private GameObject viewPoint;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public void Awake()
    {
        // Get references
        rigidbody = GetComponent<Rigidbody>();
        viewPoint = transform.Find("ViewPoint").gameObject;
    }

    public void FixedUpdate()
    {
        action.Look(this, ((FPS_StatsData)currentStats).lookRange);

        // Get my starting stats
        var currentCourage = ((FPS_StatsData)currentStats).courage;
        var collectiveCourage = 0;//currentCourage + action.GetCollectiveAllyCourage(_pawn);
        var closestAlly = action.GetClosest(this, action.visibleAllies);
        var targetEnemy = action.GetClosest(this, action.visibleHositles);
        
        // If I see an enemy
        if (action.visibleHositles.Count > 0)
        {
            // I am low on health
            if (action.currentHealth <= 25)
            {
                // Courage drops
                currentCourage -= 25;
            }
            // There is an ally nearby
            if (action.visibleAllies.Count > 0)
            {
                // The Ally is within a comfortable range
                if (Vector3.Distance(closestAlly.transform.position, this.transform.position) > ((FPS_StatsData)currentStats).comfortableAllyDistance)
                {
                    // Our collective courage is greater or equal to our enemy courage
                    if (collectiveCourage >= ((FPS_StatsData)currentStats).courage)
                    {
                        // If in range
                        if (Vector3.Distance(this.transform.position, targetEnemy.transform.position) <= 3)
                        {
                            // Attack enemy
                            action.Jump(this);
                        }
                        else
                        {
                            // Look at them
                            action.FaceTowardsPosition(this, viewPoint, targetEnemy.transform.position, 5);
                            // Approach
                            action.Move(this, new Vector3(Random.Range(-1, 1),0,1), ((FPS_StatsData)currentStats).movementSpeed);
                        }
                    }
                    // Else
                    else
                    {
                        // Run away
                        // Look at them
                        action.FaceTowardsPosition(this, viewPoint, targetEnemy.transform.position, 5);
                        // Back away
                        action.Move(this, new Vector3(0,0,-1), ((FPS_StatsData)currentStats).movementSpeed);
                    }
                }
                // The ally is NOT withing a comfortable range
                else
                {
                    // Move to ally
                    action.MoveTo(closestAlly.transform.position);
                }
            }
            // There is NOT an ally nearby
            else
            {
                // I am NOT cornered
                    // My courage is greater or equal to enemy courage
                if (currentCourage >= ((FPS_StatsData)currentStats).courage)
                {
                    // If in range
                    if (Vector3.Distance(this.transform.position, targetEnemy.transform.position) <= 3)
                    {
                        // Attack enemy
                        action.Jump(this);
                    }
                    else
                    {
                        // Look at them
                        action.FaceTowardsPosition(this, viewPoint, targetEnemy.transform.position, 5);
                        // Approach
                        action.Move(this, new Vector3(Random.Range(-1, 1),0,1), ((FPS_StatsData)currentStats).movementSpeed);
                    }
                }
                // Else
                else
                {
                    // Run away
                    // Look at them
                    action.FaceTowardsPosition(this, viewPoint, targetEnemy.transform.position, 5);
                    // Back away
                    action.Move(this, new Vector3(0,0,-1), ((FPS_StatsData)currentStats).movementSpeed);
                }
                // I am cornered
                    // Attack enemy
            }
        }
    }

    
    //=-----------------=
    // Internal Functions
    //=-----------------=
    private Transform GetClosestTarget(Pawn _pawn)
    {
        Transform closestTarget = null;
        var actorsInRange = Physics.OverlapSphere(_pawn.transform.position, 5f);
        foreach (var actor in actorsInRange)
        {
            if (actor.GetComponent<Pawn>())
            {
                if (actor.GetComponent<Pawn>() == _pawn) continue;
                if (closestTarget is not null)
                {
                    if (Vector3.Distance(_pawn.transform.position, actor.transform.position) <=
                        Vector3.Distance(_pawn.transform.position, closestTarget.transform.position))
                    {
                        closestTarget = actor.transform;
                    }
                }
                else
                {
                    closestTarget = actor.transform;
                }
            }
        }

        return closestTarget;
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}*/
