//==========================================( Neverway 2027 )=========================================================//
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
using UnityEngine;

namespace RivenFramework
{
    /// <summary>
    /// Allows the pawn the local player is currently controlling to pass the filter
    /// </summary>
    [Serializable]
    public class PawnFilter_IsLocalPlayer : ActorFilter
    {
        public override bool PassesFilter(Actor _actor)
        {
            if (_actor == null) return false;
            if (!GameInstance.TryGet(out GI_PawnManager pawnManager)) return false;
            return pawnManager.localPlayerCharacter == _actor.gameObject;
        }
    }
}
