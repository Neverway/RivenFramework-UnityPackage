//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

public class GameObject_FaceActiveCamera : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] private float updateRate = 0.25f;
    private bool active = true;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_PawnManager pawnManager;
    private Transform target;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        pawnManager = FindObjectOfType<GI_PawnManager>();
        InvokeRepeating(nameof(UpdateBillboard), 0, updateRate);
    }

    private void UpdateBillboard()
    {
        if (!active) return;
        if (!pawnManager)
        {
            pawnManager = FindObjectOfType<GI_PawnManager>();
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


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void SetBillboardActive(bool _active)
    {
        active = _active;
    }


    #endregion
}