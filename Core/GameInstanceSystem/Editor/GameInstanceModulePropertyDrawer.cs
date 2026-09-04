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

        private VisualElement root;


        private string moduleName = "Null Module";
        private Color moduleColor = GIModuleColorAttribute.defaultColor;

        //Header Elements
        private Label moduleNameLabel;
        private VisualElement moduleHeader;
        private Toggle foldout;
        //Body
        private VisualElement foldoutContent;
        private VisualElement propertyValuesContainer;



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
            //If no visualtree is found, use the default property field for this property
            if (!TryGetVisualTreeAsset(out VisualTreeAsset tree))
                return new PropertyField(property);

            //Use the visualtree to create the root layout of this property
            root = tree.Instantiate();

            //Get all UIElement references
            foldout = root.Q<Toggle>("Toggle_ModuleFoldout");
            foldoutContent = root.Q<VisualElement>("FoldoutContent");
            moduleNameLabel = root.Q<Label>("Label_ModuleName");
            moduleHeader = root.Q<VisualElement>("ModuleHeader");
            propertyValuesContainer = root.Q<VisualElement>("PropertyValues");

            //If the value of the field is actually NOT null
            if (property.boxedValue != null)
            {
                //Make the text nicer. Example, convert "GI_SomeModuleName" to "Some Module Name"
                moduleName = GetModuleNameFromType(property.type);

                //Setup Foldout
                foldout.RegisterCallback<ChangeEvent<bool>>(SetIsFoldedOut);
                GetIsFoldedOut();

                //Setup Property field inside of propertyValuesContainer
                {
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
                    propertyValuesContainer.Add(propValues);
                }

                //Setup Module color
                {
                    try
                    {
                        GIModuleColorAttribute headerColor = property.boxedValue.GetType().GetCustomAttribute<GIModuleColorAttribute>();
                        if (headerColor != null)
                            moduleColor = headerColor.color;
                    }
                    catch
                    {
                        Debug.LogWarning("Could not get property type for some reason????");
                    }
                }
            }

            //Apply module name
            moduleNameLabel.text = moduleName;

            //Apply module color
            moduleHeader.style.backgroundColor = moduleColor;
            moduleColor.a *= 0.35f;
            foldoutContent.style.backgroundColor = moduleColor;

            return root;
        }

        public static readonly string EDITORPREFS_ISFOLDEDOUT_PREFIX = "RivenFramework_ModuleFoldout ";
        public string EditorPrefs_IsFoldedOutKey => EDITORPREFS_ISFOLDEDOUT_PREFIX + moduleName;
        public void SetIsFoldedOut(ChangeEvent<bool> foldoutChangeEvent) => SetIsFoldedOut(foldoutChangeEvent.newValue);
        public void SetIsFoldedOut(bool isFoldedOut)
        {
            EditorPrefs.SetBool(EditorPrefs_IsFoldedOutKey, isFoldedOut);
            foldoutContent.style.display = isFoldedOut ? DisplayStyle.Flex : DisplayStyle.None;
        }
        public bool GetIsFoldedOut()
        {
            bool isFoldedOut = EditorPrefs.GetBool(EditorPrefs_IsFoldedOutKey, false);
            foldoutContent.style.display = isFoldedOut ? DisplayStyle.Flex : DisplayStyle.None;
            foldout.SetValueWithoutNotify(isFoldedOut);
            return isFoldedOut;
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
