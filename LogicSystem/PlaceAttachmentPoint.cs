//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using UnityEngine;

namespace RivenFramework
{
public class PlaceAttachmentPoint : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [SerializeField] private float attachDistance=2;
    [SerializeField] private float maxYPos = 0.7f;
    [SerializeField] private float minYPos = -0.7f;
    [SerializeField] private Transform attachmentPointTransform;
    [SerializeField] private Pawn targetPawn;


    //=-----------------=
    // Private Variables
    //=-----------------=
    private void Start()
    {
        //targetPawn = GetComponent<Pawn>();
        //attachmentPointTransform = targetPawn.physObjectAttachmentPoint.gameObject.transform;
    }


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    
    private void FixedUpdate()
    {
        Vector3 forwardPos = transform.forward * attachDistance; // This is the transform of the attachment points parent
        Vector3 playerForward = transform.parent.forward * attachDistance; // This is the transform of the pawn
        Vector2 horizontalPosition = new Vector2 (playerForward.x, playerForward.z);

        Vector3 resultPos;
        // Constrain the y-position of the attachment point
        if (forwardPos.y > 0)
        {
            resultPos = forwardPos;
        }
        else
        {
            if (forwardPos.y < minYPos) forwardPos.y = minYPos;
        
            horizontalPosition = horizontalPosition.normalized * attachDistance;
            resultPos = new Vector3 (horizontalPosition.x, forwardPos.y, horizontalPosition.y);
        }

        RaycastHit[] hits = Physics.SphereCastAll (transform.position, 0.2f, resultPos, resultPos.magnitude);
        float nearestHit = resultPos.magnitude;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.isTrigger)
                {
                    continue;
                }
                if (hit.collider.tag == "PhysProp")
                {
                    continue;
                }
                if (hit.collider.TryGetComponent<Rigidbody> (out var rb))
                {
                    continue;
                }
            }

            if (hit.distance < nearestHit)
            {
                nearestHit = hit.distance;
            }
        }

        if (nearestHit < resultPos.magnitude)
        {
            nearestHit = nearestHit - 0.25f;
            if (nearestHit < 0.35)
            {
                if (targetPawn.physObjectAttachmentPoint.attachedObject != null && targetPawn.physObjectAttachmentPoint.attachedObject.TryGetComponent<Object_PhysPickup> (out var heldObject))
                {
                    heldObject.Drop ();
                }
            }
            resultPos = resultPos.normalized * (nearestHit);
        }

        attachmentPointTransform.position = transform.position + resultPos;
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}
}
