using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RivenFramework
{
    [CustomEditor(typeof(GameInstance))]
    public class GameInstanceEditor : Editor
    {
        [SerializeField]
        public VisualTreeAsset treeAsset;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = treeAsset.Instantiate();
            
            SerializedProperty prop_modules = serializedObject.FindProperty("modules");
            if (prop_modules == null)
            {
                Debug.LogError("Could not find serialized property: modules");
                return root;
            }
            if (prop_modules.isArray)
            {
                for(int i = 0; i < prop_modules.arraySize; i++)
                {
                    SerializedProperty prop_module = prop_modules.GetArrayElementAtIndex(i);
                    PropertyField modulePropField = new PropertyField();
                    modulePropField.BindProperty(prop_module);
                    root.Add(modulePropField);
                }

                root.Q<Label>("Label_NoModulesText").style.display = 
                    prop_modules.arraySize == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }


            VisualElement gameInstanceModules = root.Q<VisualElement>("GameInstanceModules");
            DropdownField addNewModuleDropdown = root.Q<DropdownField>("Dropdown_AddNewModule");

            //root.Add(propertyField);

            return root;
        }
    }
}
