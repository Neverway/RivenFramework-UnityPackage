//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Attached to an object on a pawn to be used like a socket for other
//      objects, such as held physics props, swords on backs, guns on hips, etc.
// Notes: This was originally created to keep track of held physics objects 
//
//=============================================================================

using UnityEngine;

    public class Pawn_AttachmentPoint : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        [Tooltip("The object that is attached to this point, this is set, not assigned, don't touch this")]
        public GameObject attachedObject;
        [Tooltip("When trying to pickup an object if it's over this mass, the object will be dragged instead")]
        public float pickupMassLimit = 20f;


        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=
        [Tooltip("Advanced phys pickups uses a spring joint to drag and hold objects")]
        public ConfigurableJoint connectionJoint;


        //=-----------------=
        // Mono Functions
        //=-----------------=


        //=-----------------=
        // Internal Functions
        //=-----------------=


        //=-----------------=
        // External Functions
        //=-----------------=
        /// <summary>
        /// Returns true if something is already being held in this attachment point
        /// (Simple function to make code more readable)
        /// </summary>
        public bool IsOccupied()
        {
            return attachedObject != null;
        }
        
        /// <summary>
        /// Attach a physics pickup to this attachment point
        /// </summary>
        /// <param name="_targetObject">The root of the physics pickup</param>
        /// <param name="_targetRigidbody">The rigidbody of the physics pickup</param>
        public void Attach(GameObject _targetObject, Rigidbody _targetRigidbody = null)
        {
            // Find the rigidbody of the physics pickup
            var objectRigidbody = _targetObject.GetComponent<Rigidbody>();
            if (_targetRigidbody) objectRigidbody = _targetRigidbody;

            // Check mass limitations
            if (objectRigidbody.mass < pickupMassLimit)
            {
                
            }
            
            attachedObject = _targetObject;
            connectionJoint.connectedBody = objectRigidbody;

        }
        
        /// <summary>
        /// Detach a physics pickup from this attachment point
        /// </summary>
        public void Detach()
        {
            attachedObject = null;
            connectionJoint.connectedBody = null;
        }
    }