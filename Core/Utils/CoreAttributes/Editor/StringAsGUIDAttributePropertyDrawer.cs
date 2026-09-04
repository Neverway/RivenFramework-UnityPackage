using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using RivenFramework;

[CustomPropertyDrawer(typeof(StringAsGUIDAttribute))]
public class StringAsGUIDAttributePropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Ensure it's a string
        if (property.propertyType != SerializedPropertyType.String)
            return new PropertyField(property);

        var target = property.serializedObject.targetObject;
        var root = new VisualElement { style = { flexDirection = FlexDirection.Row } };

        // Get current value
        var currentValue = fieldInfo.GetValue(target) as string ?? "<no GUID assigned>";

        // Label showing the field name and current value
        var label = new Label($"{property.displayName} : ")
        {
            style = { unityTextAlign = TextAnchor.MiddleLeft, flexShrink = 1 }
        };
        label.style.unityFontStyleAndWeight = FontStyle.Bold;

        var guid = new Label($"{currentValue}")
        {
            style = { unityTextAlign = TextAnchor.MiddleLeft, flexGrow = 1, flexShrink = 1,
            color = new Color(0.8f, 0.8f, 0.8f), backgroundColor = new Color(0.18f, 0.18f, 0.18f),
            marginLeft = 10, marginRight = 10, paddingLeft = 10}
        };
        guid.style.SetBorderWidth(1);
        guid.style.SetBorderRadius(2);
        guid.style.SetBorderColor(new Color(0.3f, 0.3f, 0.3f));

        // Button to generate a new GUID
        var button = new Button(() =>
        {
            // Generate a new GUID string
            string newGuid = System.Guid.NewGuid().ToString();

            // Apply directly via reflection
            fieldInfo.SetValue(target, newGuid);

            // Mark the object dirty so Unity saves it
            EditorUtility.SetDirty(target);

            // Force the SerializedObject to resync
            property.serializedObject.Update();

            // Refresh the label
            guid.text = $"{newGuid}";
        })
        {
            text = "New GUID",
            style = { width = 75 }
        };

        root.Add(label);
        root.Add(guid);
        root.Add(button);
        return root;
    }
}
