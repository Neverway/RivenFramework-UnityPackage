//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FPS_NPCController_Enemy", menuName = "ScriptableObjects/FPS_NPCController_Enemy")]
public class FPS_NPCController_Enemy : PawnController
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private Transform target;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private FPS_ActionController action = new FPS_ActionController();
    private Rigidbody rigidbody;
    private GameObject viewPoint;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public override void PawnAwake(Pawn _pawn)
    {
        // Get references
        rigidbody = _pawn.GetComponent<Rigidbody>();
        viewPoint = _pawn.transform.Find("ViewPoint").gameObject;
    }

    public override void PawnUpdate(Pawn _pawn)
    {
    }

    public override void PawnFixedUpdate(Pawn _pawn)
    {
        target = GetClosestTarget(_pawn);

        // If we have a target
        if (target is not null)
        {
            // Look at them
            action.LookAt(_pawn, viewPoint, target.position, 5);
            // If they are too close, move away
            if (Vector3.Distance(target.position, _pawn.transform.position) < 4)
            {
                action.Move(_pawn, rigidbody, new Vector3(0,0,-1), ((FPS_Stats)_pawn.stats).movementSpeed/2);
            }
            // If they are at the optimal distance, attack them
            else
            {
                action.Jump(_pawn, rigidbody);
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
}
