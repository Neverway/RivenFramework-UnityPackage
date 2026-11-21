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
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RivenFramework
{
    public class VolumeLevelChange : Volume
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            public SceneReference targetScene;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            private GI_WorldLoader worldLoader;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void OnValidate()
            {
                // Todo: This is a temporary implementation! ~Liz
                targetScene.RefreshSceneName();
            }

            protected override void OnTriggerEnter(Collider _other)
            {
                base.OnTriggerEnter(_other);
                if (GetPlayerInTrigger())
                {
                    if (!worldLoader) worldLoader = GameInstance.Get<GI_WorldLoader>();

                    foreach (var streamVolume in FindObjectsOfType<VolumeLevelStream>())
                    {
                        streamVolume.PrepareForLoad();
                    }

                    worldLoader.LoadWorld(targetScene.sceneName);
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
