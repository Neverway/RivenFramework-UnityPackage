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

using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

namespace RivenFramework
{
    public class GameObject_FaceActiveCamera : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [SerializeField] private float updateRate = 0.25f;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            private GI_PawnManager pawnManager;
            private Transform target;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private bool active = true;
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void Start()
            {
                pawnManager = GameInstance.Get<GI_PawnManager>();
                InvokeRepeating(nameof(UpdateBillboard), 0, updateRate);
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public void SetBillboardActive(bool _active)
            {
                active = _active;
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private void UpdateBillboard()
            {
                if (!active) return;
                if (!pawnManager)
                {
                    pawnManager = GameInstance.Get<GI_PawnManager>();
                    return;
                }

                if (pawnManager.localPlayerCharacter && !target)
                {
                    target = pawnManager.localPlayerCharacter.GetComponentInChildren<Camera>().transform;
                    return;
                }

                if (!target) return;
                transform.LookAt(target, target.up);
            }
        #endregion
        #endregion
    }
}
