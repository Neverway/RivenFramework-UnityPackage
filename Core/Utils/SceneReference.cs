//==========================================( Neverway 2027 )=========================================================//
// Author
//  Errynei
//
// Contributors
//  Liz M.
//  Connorses
//  Soulex
//
// Description
//
//====================================================================================================================//

using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RivenFramework
{
    [Serializable]
    public struct SceneReference
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
            #if UNITY_EDITOR
            public SceneAsset sceneAsset;
            #endif
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            [HideInInspector] public string sceneName;
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion

        #region====================================( Functions )======================================================//

        #region----------------( Inherited )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public void RefreshSceneName()
            {
                #if UNITY_EDITOR
                if (sceneAsset == null)
                {
                    sceneName = null;
                    return;
                }
                sceneName = sceneAsset.name;
                #endif
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion
    }
}