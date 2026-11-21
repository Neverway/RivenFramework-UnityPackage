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
    public class VolumeMusicChange : Volume
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            public AudioClip musicTrack;
            public float transitionTime = 1;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            private GI_MusicManager musicManager;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            protected override void OnTriggerEnter(Collider _other)
            {
                base.OnTriggerEnter(_other); // Call the base class method
                if (!GetPlayerInTrigger())
                {
                    return;
                }
                if (_other.gameObject == GetPlayerInTrigger().gameObject)
                {
                        if (!musicManager) musicManager = FindObjectOfType<GI_MusicManager>();
                        musicManager.CrossFadeToTrack(musicTrack, transitionTime);
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
