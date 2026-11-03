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

using RivenFramework;
using UnityEditor;
using UnityEngine;

namespace RivenFramework
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
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
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            [Todo("Make this property drawer assign SceneReference.sceneName whenever SceneReference.sceneAsset is modified", Owner = "Errynei")]
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                SerializedProperty assetProp = property.FindPropertyRelative("sceneAsset");
                EditorGUI.PropertyField(position, assetProp, label);
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion
    }
}