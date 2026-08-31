using MarkupAttributes.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RivenFramework
{
    [CustomEditor(typeof(MonoBehaviour), true), CanEditMultipleObjects]
    public class MarkedUpMonoBehaviourEditor : MarkedUpEditor
    {
        public override void OnInspectorGUI() { }

        public override VisualElement CreateInspectorGUI()
        {
            if (serializedObject == null)
                return new Label("Serialized Object is somehow null?????");
            if (serializedObject.targetObject == null)
                return new Label("Editor is for null object somehow?????");

            var useMarkup = serializedObject.targetObject.GetType().GetCustomAttribute<UseMarkupInspectorAttribute>();
            if (useMarkup != null)
                return GetEditorUsingMarkup();

            return GetEditorUsingVisualElements();
        }

        public VisualElement GetEditorUsingVisualElements()
        {
            VisualElement root = new VisualElement();
            PropertyField prop = new PropertyField();
            prop.Bind(serializedObject);

            InspectorElement.FillDefaultInspector(
                root,
                serializedObject,
                this
            );
            return root;
        }
        public VisualElement GetEditorUsingMarkup() => new IMGUIContainer(base.OnInspectorGUI);
    }
}