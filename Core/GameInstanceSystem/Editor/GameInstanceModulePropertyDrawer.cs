using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RivenFramework
{
    [CustomPropertyDrawer(typeof(GameInstanceModule), true)]
    public class GameInstanceModulePropertyDrawer : PropertyDrawer
    {
        private static VisualTreeAsset treeAsset;
        public static bool TryGetVisualTreeAsset(out VisualTreeAsset tree, [CallerFilePath] string filePath = "")
        {
            tree = treeAsset;
            if (tree != null) return true;

            try
            {
                string dataPath = Path.GetFullPath(Application.dataPath);
                if (filePath.StartsWith(dataPath))
                {
                    string uxmlPath = $"Assets{filePath.Substring(dataPath.Length, filePath.Length - dataPath.Length - 3)}.uxml";
                    tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
                    treeAsset = tree;
                }
            }
            catch 
            {
                Debug.LogError($"Uxml file expected but could not find or load. Expected location: {filePath.Substring(filePath.Length - 3)}.uxml");
                return false; 
            }

            return tree != null;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!TryGetVisualTreeAsset(out VisualTreeAsset tree))
                return new PropertyField(property);

            VisualElement root = tree.Instantiate();
            
            if (property.boxedValue == null)
            {
                root.Q<VisualElement>("PropertyValues");
            }


            //Property Values -----------------------------------------------------------------------------------------

            PropertyField propValues = new PropertyField();
            propValues.BindProperty(property);
            propValues.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                //Get rid of the toggle label section
                propValues.Q<Toggle>().style.display = DisplayStyle.None;

                //Always show the "Flex" section of the foldout, and remove the margin..
                //   ..so its as if the foldout was never there!
                var content = propValues.Q<VisualElement>("unity-content");
                content.style.display = DisplayStyle.Flex;
                content.style.marginLeft = 0;

            });
            root.Q<VisualElement>("PropertyValues").Add(propValues);


            //Foldout -------------------------------------------------------------------------------------------------

            Toggle foldout = root.Q<Toggle>("Toggle_ModuleFoldout");
            VisualElement foldoutContent = root.Q<VisualElement>("FoldoutContent");
            foldout.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                foldoutContent.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            foldoutContent.style.display = foldout.enabledSelf ? DisplayStyle.None : DisplayStyle.Flex;


            //Module Name ---------------------------------------------------------------------------------------------

            Label moduleName = root.Q<Label>("Label_ModuleName");
            //Make the text nicer. Example, convert "GI_SomeModuleName" to "Some Module Name"
            moduleName.text = GetModuleNameFromType(property.type);


            //Module Color --------------------------------------------------------------------------------------------
            VisualElement moduleHeader = root.Q<VisualElement>("ModuleHeader");

            Color moduleColor = GIModuleColorAttribute.defaultColor;
            try
            {
                GIModuleColorAttribute headerColor = property.boxedValue.GetType().GetCustomAttribute<GIModuleColorAttribute>();
                //GIModuleColorAttribute headerColor = fieldInfo.FieldType.GetCustomAttribute<GIModuleColorAttribute>();
                if (headerColor != null)
                    moduleColor = headerColor.color;
            }
            catch
            {
                Debug.LogWarning("Could not get property type for some reason????");
            }

            moduleHeader.style.backgroundColor = moduleColor;
            moduleColor.a *= 0.35f;
            foldoutContent.style.backgroundColor = moduleColor;

            // --------------------------------------------------------------------------------------------------------
            return root;
        }


        public static string GetModuleNameFromType(Type moduleType)
            => GetModuleNameFromType(moduleType.Name);

        public static string GetModuleNameFromType(string typeName)
        {
            if (typeName.StartsWith("managedReference<") && typeName.EndsWith(">"))
                typeName = typeName.Substring("managedReference<".Length, typeName.Length - "managedReference<".Length - ">".Length);
            int underscoreIndex = typeName.IndexOf('_');
            if (underscoreIndex > 0)
            {
                bool capsOnlyBeforeUnderscore = true;
                for (int i = 0; i < underscoreIndex; i++)
                    capsOnlyBeforeUnderscore &= char.IsUpper(typeName[i]);
                if (capsOnlyBeforeUnderscore)
                    typeName = typeName.Substring(underscoreIndex + 1);
            }
            //if (typeName.StartsWith("GI_"))
            //    typeName = typeName.Substring("GI_".Length);
            //if (typeName.StartsWith("GIM_"))
            //    typeName = typeName.Substring("GIM_".Length);
            typeName = ObjectNames.NicifyVariableName(typeName);
            return typeName;
        }
    }
}
