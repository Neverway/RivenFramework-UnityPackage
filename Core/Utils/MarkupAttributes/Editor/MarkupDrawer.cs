using MarkupAttributes;
using MarkupAttributes.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static MarkupAttributes.Editor.MarkupGUI;

namespace RivenFramework
{
    [CustomPropertyDrawer(typeof(MarkupAttribute))]
    [Todo("I would really love to get this working, but MarkedUpAttributes for PropertyDrawers is Tricky :(", Owner = "Errynei")]
    public class MarkupDrawer : PropertyDrawer
    {
        //public override void OnGUI(
        //Rect position,
        //SerializedProperty property,
        //GUIContent label)
        //{
        //    // Draw the actual property.
        //    EditorGUI.PropertyField(position, property, label, true);
        //
        //    // Draw additional visual content, if desired.
        //    // Usually this requires allocating extra height.
        //}
        //
        //public override float GetPropertyHeight(
        //    SerializedProperty property,
        //    GUIContent label)
        //{
        //    return EditorGUI.GetPropertyHeight(property, label, true) + 50;
        //}



        //public override float GetHeight()
        //{
        //    return base.GetHeight() + 2 * ((ColorLineAttribute)attribute).padding;
        //}
        //
        //public override void OnGUI(Rect position)
        //{
        //    // Drawing logic for the decorative line
        //    float width = ((ColorLineAttribute)attribute).width;
        //    float height = ((ColorLineAttribute)attribute).height;
        //    float x = position.x + (position.width / 2) - (width / 2);
        //    float y = position.y + ((ColorLineAttribute)attribute).padding;
        //
        //    // Draw the line
        //    EditorGUI.DrawRect(new Rect(x, y, width, height), Color.black);
        //}
        
        MarkupGUI.GroupsStack groupsStack = new MarkupGUI.GroupsStack();
        Object referencedObject = null;
        Editor editor = null;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            referencedObject = property.objectReferenceValue;

            if (referencedObject != null)
            {
                editor = MarkedUpEditor.CreateEditor(referencedObject);
            }

            return new IMGUIContainer(() =>
            {
                groupsStack.Clear();

                MarkupGUI.DrawEditorInline(property, editor, InlineEditorMode.Box);

                groupsStack.EndAll();
                //groupsStack
                //MarkupGUI.DrawEditorInline(property, null, InlineEditorMode.Box);
            });
        }
    }
}
