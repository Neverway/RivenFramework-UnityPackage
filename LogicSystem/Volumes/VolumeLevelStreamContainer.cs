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
using UnityEngine;

public class VolumeLevelStreamContainer : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public Vector3 exitOffset;
    public bool initializedExitZone;
    public GameObject parentStreamVolume;
    private bool hasActivated;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Update()
    {
        if (!initializedExitZone) return;
        if (!parentStreamVolume && !hasActivated)
        {
            hasActivated = true;
            print($"link to parent has been lost, scene must have changed! Ejecting...");
            StartCoroutine(EjectStreamedActors());
        }
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public IEnumerator EjectStreamedActors()
    {
        transform.position += exitOffset;
        while (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject actor = transform.GetChild(i).gameObject;
                actor.transform.SetParent(null);
            }
        }
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }


    #endregion
}
