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
//  Hurts or heals pawns in this volume
//====================================================================================================================//

using UnityEngine;

namespace RivenFramework
{
    public class VolumePain : Volume
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Tooltip("Negative values will heal pawns")] 
            [SerializeField] private float damageAmount = 10;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )======================================================//

        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void Update()
            {
                // Who gets hurt (or healed) is decided by the volume's "Affected Actors" filter
                foreach (var entity in pawnsInTrigger)
                {
                    if (entity == null) continue;
                    entity.ModifyHealth(-damageAmount);
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