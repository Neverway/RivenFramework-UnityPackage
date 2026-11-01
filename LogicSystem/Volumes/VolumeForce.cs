//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//  Errynei
//  Connorses
//  Soulex
//
// Description
//  Applies a physics force to actors within this volume
//====================================================================================================================//

using UnityEngine;

namespace RivenFramework
{
    public class VolumeForce : Volume
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
            [SerializeField] private float actorForceIntensity;
            [SerializeField] private float propForceIntensity;
            [SerializeField] private ForceMode forceMode;
            [SerializeField] private ForceMode2D forceMode2d;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            [Tooltip("Uses the forward vector from this object to determine the direction to apply the force")]
            [SerializeField] private Transform forceDirection;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )======================================================//

        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void FixedUpdate()
            {
                // Push Pawns
                foreach (var entity in pawnsInTrigger)
                {
                    if (entity is null) continue;
                    var rigidbody2D = entity.gameObject.GetComponent<Rigidbody2D>();
                    var rigidbody = entity.gameObject.GetComponent<Rigidbody>();

                    // Who gets pushed is decided by the volume's "Affected Actors" filter
                    if (rigidbody2D)
                    {
                        rigidbody2D.AddForce(forceDirection.transform.forward * actorForceIntensity, forceMode2d);
                    }
                    if (rigidbody)
                    {
                        rigidbody.AddForce(forceDirection.transform.forward * actorForceIntensity, forceMode);
                    }
                }
                // Push Props
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
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion
    }
}