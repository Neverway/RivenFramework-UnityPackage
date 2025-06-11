//==========================================( Neverway 2025 )=========================================================//
// Author
//  Errynei
//
// Contributors
//
//
//====================================================================================================================//

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty assetProp = property.FindPropertyRelative("sceneAsset");
        EditorGUI.PropertyField(position, assetProp, label);
    }
}
