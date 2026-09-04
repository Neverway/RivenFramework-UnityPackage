using JetBrains.Annotations;
using RivenFramework.Utils.Reflection;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace RivenFramework
{
    /*
    [CustomEditor(typeof(GameInstance))]
    public class GameInstanceEditor : Editor
    {
        [SerializeField]
        public VisualTreeAsset treeAsset;

        protected VisualElement root;

        public override VisualElement CreateInspectorGUI()
        {
            root = treeAsset.Instantiate();

            UpdateInspector();

            root.TrackSerializedObjectValue(serializedObject, so => UpdateInspector());
            return root;
        }

        public void UpdateInspector()
        {
            List<Type> unusedGameInstanceModuleTypes =
                TypeCache.GetTypesDerivedFrom<GameInstanceModule>()
                .Where((t) => !t.IsAbstract && t.IsSerializable)
                .ToList();


            VisualElement gameInstanceModules = root.Q<VisualElement>("GameInstanceModules");
            SerializedProperty prop_modules = serializedObject.FindProperty("modules");
            if (prop_modules == null)
            {
                Debug.LogError("Could not find serialized property: modules");
                return;
            }
            if (prop_modules.isArray)
            {
                for (int i = 0; i < prop_modules.arraySize; i++)
                {
                    SerializedProperty prop_module = prop_modules.GetArrayElementAtIndex(i);
                    object boxedValue = prop_module.boxedValue;
                    if (boxedValue != null)
                        unusedGameInstanceModuleTypes.Remove(boxedValue.GetType());

                    PropertyField modulePropField = new PropertyField();
                    modulePropField.BindProperty(prop_module);
                    gameInstanceModules.Add(modulePropField);
                }

                root.Q<Label>("Label_NoModulesText").style.display =
                    prop_modules.arraySize == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }



            DropdownField modulesDropdown = root.Q<DropdownField>("Dropdown_AddNewModule");

            if (unusedGameInstanceModuleTypes.Count == 0)
            {
                modulesDropdown.SetValueWithoutNotify("-- No new Modules to add --");
                modulesDropdown.choices = new List<string> { "" };
            }
            else
            {
                modulesDropdown.choices = unusedGameInstanceModuleTypes.Select((t) => GameInstanceModulePropertyDrawer.GetModuleNameFromType(t)).ToList();
                modulesDropdown.SetValueWithoutNotify("-- Select Module to add --");
                modulesDropdown.RegisterCallback<ChangeEvent<string>>(evt =>
                {
                    int index = modulesDropdown.choices.IndexOf(evt.newValue);
                    Type newModuleType = unusedGameInstanceModuleTypes[index];

                    // Grow the array by one
                    int newIndex = prop_modules.arraySize;
                    prop_modules.arraySize++;

                    // Get the newly created element
                    SerializedProperty newElement = prop_modules.GetArrayElementAtIndex(newIndex);

                    // Assign the new instance via managedReferenceValue (not objectReferenceValue)
                    newElement.managedReferenceValue = Activator.CreateInstance(newModuleType);
                    serializedObject.ApplyModifiedProperties();
                    this.Repaint(); //Trying to repaint here

                    modulesDropdown.SetValueWithoutNotify("-- Select Module to add --");
                });
            }
        }
    } */
    [CustomEditor(typeof(GameInstance))]
    public class GameInstanceEditor : Editor
    {
        [SerializeField]
        public VisualTreeAsset treeAsset;

        VisualElement gameInstanceModules;
        DropdownField modulesDropdown;
        Label noModulesLabel;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = treeAsset.Instantiate();

            gameInstanceModules = root.Q<VisualElement>("GameInstanceModules");
            modulesDropdown = root.Q<DropdownField>("Dropdown_AddNewModule");
            noModulesLabel = root.Q<Label>("Label_NoModulesText");

            RebuildModulesUI();

            // Rebuild whenever the SerializedObject changes for any reason
            root.TrackSerializedObjectValue(serializedObject, so => RebuildModulesUI());

            return root;
        }

        void RebuildModulesUI()
        {
            SerializedProperty prop_modules = serializedObject.FindProperty("modules");
            if (prop_modules == null)
            {
                Debug.LogError("Could not find serialized property: modules");
                return;
            }

            List<Type> unusedGameInstanceModuleTypes =
                TypeCache.GetTypesDerivedFrom<GameInstanceModule>()
                .Where((t) => !t.IsAbstract && t.IsSerializable)
                .ToList();

            gameInstanceModules.Clear();

            if (prop_modules.isArray)
            {
                for (int i = 0; i < prop_modules.arraySize; i++)
                {
                    SerializedProperty prop_module = prop_modules.GetArrayElementAtIndex(i);
                    object boxedValue = prop_module.boxedValue;
                    if (boxedValue != null)
                        unusedGameInstanceModuleTypes.Remove(boxedValue.GetType());

                    PropertyField modulePropField = new PropertyField();
                    modulePropField.BindProperty(prop_module);
                    gameInstanceModules.Add(modulePropField);
                }

                noModulesLabel.style.display =
                    prop_modules.arraySize == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (unusedGameInstanceModuleTypes.Count == 0)
            {
                modulesDropdown.SetValueWithoutNotify("-- No new Modules to add --");
                modulesDropdown.choices = new List<string> { "" };
            }
            else
            {
                modulesDropdown.choices = unusedGameInstanceModuleTypes
                    .Select((t) => GameInstanceModulePropertyDrawer.GetModuleNameFromType(t))
                    .ToList();
                modulesDropdown.SetValueWithoutNotify("-- Select Module to add --");

                // clear old handler to avoid stacking duplicates across rebuilds
                modulesDropdown.UnregisterCallback<ChangeEvent<string>>(OnModuleSelected);
                modulesDropdown.RegisterCallback<ChangeEvent<string>>(OnModuleSelected);
            }
        }

        void OnModuleSelected(ChangeEvent<string> evt)
        {
            SerializedProperty prop_modules = serializedObject.FindProperty("modules");

            List<Type> unusedGameInstanceModuleTypes =
                TypeCache.GetTypesDerivedFrom<GameInstanceModule>()
                .Where((t) => !t.IsAbstract && t.IsSerializable)
                .ToList();
            for (int i = 0; i < prop_modules.arraySize; i++)
            {
                object boxedValue = prop_modules.GetArrayElementAtIndex(i).boxedValue;
                if (boxedValue != null)
                    unusedGameInstanceModuleTypes.Remove(boxedValue.GetType());
            }

            int index = modulesDropdown.choices.IndexOf(evt.newValue);
            if (index < 0) return;
            Type newModuleType = unusedGameInstanceModuleTypes[index];

            int newIndex = prop_modules.arraySize;
            prop_modules.arraySize++;
            SerializedProperty newElement = prop_modules.GetArrayElementAtIndex(newIndex);
            newElement.managedReferenceValue = Activator.CreateInstance(newModuleType);
            serializedObject.ApplyModifiedProperties(); // this triggers TrackSerializedObjectValue → RebuildModulesUI

            modulesDropdown.SetValueWithoutNotify("-- Select Module to add --");
        }
    }
}
