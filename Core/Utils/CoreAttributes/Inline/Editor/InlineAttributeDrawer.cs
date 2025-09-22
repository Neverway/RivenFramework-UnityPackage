using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InlineAttribute))]
public class InlineAttributeDrawer : PropertyDrawer
{
    private bool foldout = true;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InlineAttribute inlineAttr = (InlineAttribute)attribute;

        if (property.propertyType != SerializedPropertyType.ObjectReference)
        {
            EditorGUI.LabelField(position, label.text, "Inline attribute can only be used on object references.");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // Calculate positions
        Rect objectFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        if (inlineAttr.showFoldout && property.objectReferenceValue != null)
        {
            // Show foldout arrow
            Rect foldoutRect = new Rect(position.x, position.y, 15, EditorGUIUtility.singleLineHeight);
            foldout = EditorGUI.Foldout(foldoutRect, foldout, GUIContent.none);

            // Adjust object field position to account for foldout
            objectFieldRect.x += 15;
            objectFieldRect.width -= 15;
        }

        // Draw the object field
        UnityEngine.Object oldValue = property.objectReferenceValue;
        UnityEngine.Object newValue = EditorGUI.ObjectField(objectFieldRect, label, oldValue, GetPropertyType(), true);

        // Handle object creation if null and createIfNull is true
        if (newValue == null && oldValue != null)
        {
            property.objectReferenceValue = null;
        }
        else if (newValue != oldValue)
        {
            property.objectReferenceValue = newValue;
        }

        // Create new instance if null and createIfNull is enabled
        if (inlineAttr.createIfNull && property.objectReferenceValue == null &&
            !typeof(MonoBehaviour).IsAssignableFrom(GetPropertyType()) &&
            !typeof(ScriptableObject).IsAssignableFrom(GetPropertyType()))
        {
            // Only create for regular serializable classes, not MonoBehaviours or ScriptableObjects
            try
            {
                property.objectReferenceValue = Activator.CreateInstance(GetPropertyType()) as UnityEngine.Object;
            }
            catch
            {
                // Ignore creation failures
            }
        }

        // Draw inline properties if object exists and foldout is open
        if (property.objectReferenceValue != null && (!inlineAttr.showFoldout || foldout))
        {
            EditorGUI.indentLevel++;

            SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
            SerializedProperty prop = serializedObject.GetIterator();

            float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (prop.NextVisible(true))
            {
                do
                {
                    // Skip the script reference
                    if (prop.propertyPath == "m_Script") continue;

                    Rect propRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUI.GetPropertyHeight(prop));
                    EditorGUI.PropertyField(propRect, prop, true);

                    yOffset += EditorGUI.GetPropertyHeight(prop) + EditorGUIUtility.standardVerticalSpacing;
                }
                while (prop.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        InlineAttribute inlineAttr = (InlineAttribute)attribute;

        if (property.propertyType != SerializedPropertyType.ObjectReference)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight;

        // Add height for inline properties if object exists and is expanded
        if (property.objectReferenceValue != null && (!inlineAttr.showFoldout || foldout))
        {
            SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
            SerializedProperty prop = serializedObject.GetIterator();

            if (prop.NextVisible(true))
            {
                do
                {
                    if (prop.propertyPath == "m_Script") continue;
                    height += EditorGUI.GetPropertyHeight(prop) + EditorGUIUtility.standardVerticalSpacing;
                }
                while (prop.NextVisible(false));
            }
        }

        return height;
    }

    private Type GetPropertyType()
    {
        // Get the type of the field this property drawer is applied to
        string[] pathParts = fieldInfo.FieldType.ToString().Split('.');
        return fieldInfo.FieldType;
    }
}