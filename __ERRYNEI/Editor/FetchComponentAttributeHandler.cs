using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FetchComponentAttribute))]
public class FetchComponentAttributeHandler : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Type defaultType = GetFieldType(property);
        FetchComponentAttribute fetchComponentData = 
            property.GetFieldInfo().GetAttribute<FetchComponentAttribute>();

        if (property.serializedObject.targetObject is Component homeComponent)
        {
            if(property.isArray)
            {
                Debug.Log("AHA");
                Component[] fetchedArray = fetchComponentData.FetchArray(homeComponent, defaultType);

                property.arraySize = fetchedArray.Length;

                for (int i = 0; i < fetchedArray.Length; i++)
                    property.GetArrayElementAtIndex(i).objectReferenceValue = fetchedArray[i];
            }
            else
                property.objectReferenceValue = fetchComponentData.Fetch(homeComponent, defaultType);
        }
        else
        {
            Debug.LogWarning("The FetchComponent attribute only works on fields of Component classes, not a " + 
                property.serializedObject.targetObject.GetType().HumanName());
        }

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PropertyField(position, property, true);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 
        EditorGUI.GetPropertyHeight(property, true);

    public Type GetFieldType(SerializedProperty property)
    {
        Type type = property.GetFieldInfo().FieldType;

        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType && typeof(IList).IsAssignableFrom(type))
            return type.GetGenericArguments()[0];

        return type;
    }
}