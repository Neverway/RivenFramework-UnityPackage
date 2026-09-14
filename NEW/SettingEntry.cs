using System;
using System.Collections.Generic;
using System.Reflection;
using ErryLib.Reflection;
using TMPro;
using UnityEngine;

namespace RivenFramework_Revamp.NEW
{
    public class GimmeDatPrefab : Attribute
    {
        public string name;

        public GimmeDatPrefab(string name)
        {
            this.name = name;
        }

        public static GameObject GetPrefabByName(string name, object target)
        {
            var attributes = ReflectionCache.GetAttributeUsageInfos<GimmeDatPrefab>();
            foreach (var attribute in attributes)
            {
                if (attribute.As<GimmeDatPrefab>().name == name)
                {
                    FieldInfo field = attribute.Member as FieldInfo;
                    return field.GetValue(target) as GameObject;
                }
            }

            return null;
        }
    }

    public abstract class SettingEntry
    {
        [Tooltip("The settings tab, category, and display name of this settings entry (eg Audio/Volume Channel Mixers/Master)")]
        [HideInInspector] public string category;
        [Tooltip("The description of this settings entry")]
        [HideInInspector] public string description;
        [Tooltip("The index of the reference image that accompanies the description of this settings entry")]
        public Sprite referenceImage;
        [Tooltip("If enabled, this setting can still be changed, but it's not exposed to the player in the settings menu")]
        public bool hidden;
        public abstract GameObject Instantiate();
    }
    
    public abstract class SettingEntry<T> : SettingEntry
    {
        [Tooltip("The default value of this settings entry")]
        public T defaultValue;
        [Tooltip("The current value of this settings entry?")]
        [HideInInspector] public T value;

        public SettingEntry(string category, string description, Sprite referenceImage, bool hidden, T defaultValue)
        {
            this.category = category;
            this.description = description;
            this.referenceImage = referenceImage;
            this.hidden = hidden;
            this.defaultValue = defaultValue;
        }

        public virtual void Set(T val) => value = val;
        public virtual T Get() => value;

        public override GameObject Instantiate()
        {
            GameObject obj = GetInstantiatedFromPrefab();

            return obj;
        }
        protected abstract GameObject GetInstantiatedFromPrefab();
    }

    [Serializable]
    public class SettingSlider : SettingEntry<int>
    {
        protected int min, max;
        public SettingSlider(string category, string description, Sprite referenceImage, bool hidden, int defaultValue, int min, int max)
            : base(category, description, referenceImage, hidden, defaultValue) 
        { 
            this.min = min;
            this.max = max;
        }
        protected override GameObject GetInstantiatedFromPrefab()
        {
            var entryObject = GameObject.Instantiate(GimmeDatPrefab.GetPrefabByName("SliderPrefab", ApplicationSettingsData.GetThis()));
            if (entryObject)
            {
                var entryElement = entryObject.GetComponent<SettingsElementEntry>();
                entryElement.SetInfo(category.Split("/", StringSplitOptions.None)[2], description, referenceImage, hidden, _sliderValue:defaultValue, min, max);
            }
            return entryObject;
        }
    }

    [Serializable]
    public class SettingCheckbox : SettingEntry<bool>
    {
        public SettingCheckbox(string category, string description, Sprite referenceImage, bool hidden, bool defaultValue)
            : base(category, description, referenceImage, hidden, defaultValue) 
        { 
        }
        protected override GameObject GetInstantiatedFromPrefab()
        {
            var entryObject = GameObject.Instantiate(GimmeDatPrefab.GetPrefabByName("CheckboxPrefab", ApplicationSettingsData.GetThis()));
            if (entryObject)
            {
                var entryElement = entryObject.GetComponent<SettingsElementEntry>();
                entryElement.SetInfo(category.Split("/", StringSplitOptions.None)[2], description, referenceImage, hidden, _checkboxValue:defaultValue);
            }
            return entryObject;
        }
    }

    [Serializable]
    public class SettingDropdown: SettingEntry<int>
    {
        protected List<string> optionsData;
        public SettingDropdown(string category, string description, Sprite referenceImage, bool hidden, int defaultValue, List<string> optionsData)
            : base(category, description, referenceImage, hidden, defaultValue)
        {
            this.optionsData = optionsData;
        }
        protected override GameObject GetInstantiatedFromPrefab()
        {
            var entryObject = GameObject.Instantiate(GimmeDatPrefab.GetPrefabByName("DropdownPrefab", ApplicationSettingsData.GetThis()));
            if (entryObject)
            {
                var entryElement = entryObject.GetComponent<SettingsElementEntry>();
                
                List<TMP_Dropdown.OptionData> newDropdownOptions = new();

                for (int i = 0; i < optionsData.Count; i++)
                {
                    newDropdownOptions.Add(new TMP_Dropdown.OptionData(optionsData[i], null));
                }
                
                entryElement.SetInfo(category.Split("/", StringSplitOptions.None)[2], description, referenceImage, hidden, defaultValue, _dropdownOptions:newDropdownOptions);
            }
            return entryObject;
        }
    }

    [Serializable]
    public class SettingSelector: SettingEntry<int>
    {
        protected List<string> selectorOptions;
        public SettingSelector(string category, string description, Sprite referenceImage, bool hidden, int defaultValue, List<string> selectorOptions)
            : base(category, description, referenceImage, hidden, defaultValue)
        {
            this.selectorOptions = selectorOptions;
        }
        protected override GameObject GetInstantiatedFromPrefab()
        {
            var entryObject = GameObject.Instantiate(GimmeDatPrefab.GetPrefabByName("SelectorPrefab", ApplicationSettingsData.GetThis()));
            if (entryObject)
            {
                var entryElement = entryObject.GetComponent<SettingsElementEntry>();
                entryElement.SetInfo(category.Split("/", StringSplitOptions.None)[2], description, referenceImage, hidden, defaultValue, selectorOptions);
            }
            return entryObject;
        }
    }
    
    
    
    
    
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ContainsSettingEntries : PropertyAttribute
    {
        public string staticGetInstanceMethodName;
        public ContainsSettingEntries(string staticGetInstanceMethodName)
        {
            this.staticGetInstanceMethodName = staticGetInstanceMethodName;
        }
    }
    
    /*
    [ContainsSettingEntries("GetWhereTheSetupStuffHappens")]
    public class WhereTheSetupStuffHappens
    {
        
        public static object GetWhereTheSetupStuffHappens()
        {
            return new WhereTheSetupStuffHappens();
        }
        
        [ErryBox]
        public SettingCheckbox testThingy = new SettingCheckbox(
            "Balls?/YIPPE!/The fog errynei, the fog is coming", 
            "-", 
            null, false, false);
    }*/
}