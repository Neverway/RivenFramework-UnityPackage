//==========================================( Neverway 2025 )=========================================================//
// Author
//  Errynei
//
// Contributors
//
//
//====================================================================================================================//

using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct SceneReference
{
#if UNITY_EDITOR
    public SceneAsset sceneAsset;
#endif
    [HideInInspector] public string sceneName;

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
}