//==========================================( Neverway 2027 )=========================================================//
// Author
//  Errynei
//
// Contributors
//  Liz M.
//
// Description
//
//====================================================================================================================//

using System;
using UnityEditor;
using UnityEngine;

namespace RivenFramework
{
    public class LogicValueLinkToolSettings : ScriptableObject
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            public bool showToolButton = true;
            public bool showLinksWhenNotUsingTool = true;
            public bool selectTransformWhenNotUsingTool = true;
            [Space]
            public Texture2D toolbarIcon;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            public event Action OnSettingsValidate;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public static LogicValueLinkToolSettings _instance;
            public static LogicValueLinkToolSettings Instance => (_instance != null ? _instance : _instance = LoadOrCreateSettings());
            [HideInInspector] public bool EditorToolUsesSceneGUI => showToolButton || showLinksWhenNotUsingTool;
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            public void OnValidate()
            {
                OnSettingsValidate?.Invoke();
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            [MenuItem("Neverway/Tools/Logic Value Link Tool/Settings")]
            private static void SelectSettings()
            {
                Selection.activeObject = _instance;
            }

            private static LogicValueLinkToolSettings LoadOrCreateSettings()
            {
                string path;
                string[] settings = AssetDatabase.FindAssets($"t:{nameof(LogicValueLinkToolSettings)}");
                if (settings.Length > 0)
                {
                    path = AssetDatabase.GUIDToAssetPath(settings[0]);
                    return AssetDatabase.LoadAssetAtPath<LogicValueLinkToolSettings>(path);
                }

                LogicValueLinkToolSettings newSettings = ScriptableObject.CreateInstance<LogicValueLinkToolSettings>();
                try 
                {
                    MonoScript script = MonoScript.FromScriptableObject(newSettings);
                    path = AssetDatabase.GetAssetPath(script);

                    path = path.Substring(0, path.LastIndexOf('/'));
                    path += "/LogicValueLinkToolSettings.asset";

                    AssetDatabase.CreateAsset(newSettings, path);
                    AssetDatabase.SaveAssets();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not find nor create {nameof(LogicValueLinkToolSettings)} Asset, using defaults as fallback");
                    Debug.LogException(e);
                }

                return newSettings;
            }
        #endregion
        #endregion
    }
}
