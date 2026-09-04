//==========================================( Neverway 2025 )=========================================================//
// Author
//  Errynei
//
// Contributors
//
//
//====================================================================================================================//

using RivenFramework;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferenceDrawer : PropertyDrawer
{
    [Todo("Make this property drawer assign SceneReference.sceneName whenever SceneReference.sceneAsset is modified", Owner = "Errynei")]
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty assetProp = property.FindPropertyRelative("sceneAsset");
        EditorGUI.PropertyField(position, assetProp, label);
    }
}
