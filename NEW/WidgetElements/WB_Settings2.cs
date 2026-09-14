//==========================================( Neverway 2026 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RivenFramework_Revamp.NEW;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionCache = RivenFramework.Utils.Reflection.ReflectionCache;

public class WB_Settings2 : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    public Dictionary<string, Dictionary<string, List<SettingEntry>>> settingEntriesInOrder;
    public int currentCategory; // Shows the relative list of entries based on what index is selected
    public int currentEntry; // Shows the relative image, description header, and description based on what index is selected
    public List<SettingsElementEntry> SettingsElementEntries;
    public HashSet<SettingEntry> allSettingEntries = new HashSet<SettingEntry>();

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Transform topbarContainer;
    public Transform settingsEntryContainer;
    public Image referenceImage;
    public TMP_Text descriptionHeaderText;
    public TMP_Text descriptionText;
    public Button applyButton, resetButton, backButton;

    public GameObject topbarCategoryButtonPrefab;
        
    public GameObject groupPrefab;
    


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        GetAllSettingEntries();
        GetSettingCategories();
        CreateCategoryButtons();
        CreateGroupsAndEntries();
        
        // Step 1 - We need to know what categories should exist based on the root categories of all entries and add them to the top bar
        // Step 3 - We need to create lists of entries for each category and enable only the elements from the list that is currently selected
        //          (which by default should be the first category since the menu was presumably just opened)
        // Step 2 - We need to tie click each category button to showing the respective list of entries
        // Step 4 - We need to tie in hovering over each entry to showing that entry's details
        // Step 5 - We need to tie in each type of entries interactions to modifying the settings buffer
        // Step 6 - We need to add in the controls category and screen on the end of the top bar
        // Pressing Apply needs to dump the buffer into the current settings and save the current settings to the config file
        // Pressing Reset need to prompt the user to confirm if they want to reset all settings, and pressing confirm
        //          needs to restore the buffer, current settings, and config to the project defaults
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    public void ShowEntry(int entryIndex)
    {
        currentEntry = entryIndex;
        if (currentEntry < 0)
        {
            referenceImage.gameObject.SetActive(false);
            descriptionHeaderText.text = "";
            descriptionText.text = "";
        }
        else
        {
            var _currentEntry = SettingsElementEntries[currentEntry];
            referenceImage.sprite = _currentEntry.cachedReferenceImage;
            referenceImage.gameObject.SetActive(referenceImage.sprite != null);
            descriptionHeaderText.text = _currentEntry.text.text;
            descriptionText.text = _currentEntry.cachedDescription;
        }
    }
    
    public void GetAllSettingEntries()
    {
        allSettingEntries = new HashSet<SettingEntry>();
        
        //Loop through all instances of the use of the "ContainsSettingEntries" Attribute, and loop through them
        foreach(var attributeUsage in ReflectionCache.GetAttributeUsages<ContainsSettingEntries>())
        {
            //Get the Attribute and Type off of attributeUsage
            ContainsSettingEntries attribute = attributeUsage.Attribute as ContainsSettingEntries;
            TypeInfo type = attributeUsage.Member as TypeInfo;

            //Get need a reference to the object that actually contains all the settings entries, so to do that:
            //First get the actual method off of the type using the methodname stored on the attribute
            MethodInfo getInstanceMethod = type.GetMethod(attribute.staticGetInstanceMethodName);
            //Then just call that method (which we assume is static) to get the object that actually contains the settings entries
            object settingEntriesOwner = getInstanceMethod.Invoke(null, null);

            //Get all the SettingEntry fields off of that object
            // (we cant use the TypeInfo from the attribute since it might be a base class, and this object might extend it)
            FieldInfo[] settingEntryFields =
                settingEntriesOwner.GetType()
                .GetFields()
                .Where(f => typeof(SettingEntry).IsAssignableFrom(f.FieldType)) //only fields of the SettingEntry type
                .ToArray();

            //Get the value off of each of those fields
            SettingEntry[] settingEntries = 
                settingEntryFields
                    .Select(f => f.GetValue(settingEntriesOwner))
                    .OfType<SettingEntry>()
                    .ToArray();

            //Then add each one to the list of all settingEntries :D
            foreach (SettingEntry settingEntry in settingEntries)
                allSettingEntries.Add(settingEntry);
        }
    }

    public void GetSettingCategories()
    {
        settingEntriesInOrder = new Dictionary<string, Dictionary<string, List<SettingEntry>>>();
        
        foreach (var entry in allSettingEntries)
        {
            string category = entry.category.Split("/", StringSplitOptions.None)[0];
            string group = entry.category.Split("/", StringSplitOptions.None)[1];
            string name = entry.category.Split("/", StringSplitOptions.None)[2];
            
            // Create category if it doesn't exist
            if (settingEntriesInOrder.ContainsKey(category) is false) 
                settingEntriesInOrder.Add(category, new Dictionary<string, List<SettingEntry>>());

            // Create group if it doesn't exist
            if (settingEntriesInOrder[category].ContainsKey(group) is false) 
                settingEntriesInOrder[category].Add(group, new List<SettingEntry>());

            // Add entry to group
            settingEntriesInOrder[category][group].Add(entry);
        }
    }

    public void CreateCategoryButtons()
    {
        foreach (var category in settingEntriesInOrder.Keys)
        {
            var categoryButton = Instantiate(topbarCategoryButtonPrefab, topbarContainer);
            categoryButton.GetComponentInChildren<TMP_Text>().text = category;
        }
    }
    
    public void CreateGroupsAndEntries()
    {
        SettingsElementEntries.Clear();
        foreach (var category in settingEntriesInOrder.Keys)
        {
            foreach (var group in settingEntriesInOrder[category].Keys)
            {
                // Create the group header
                var groupEntry = Instantiate(groupPrefab, settingsEntryContainer);
                groupEntry.GetComponentInChildren<TMP_Text>().text = group;
                
                // Create each of the entries
                foreach (var setting in settingEntriesInOrder[category][group].ToArray())
                {
                    var settingEntry = setting.Instantiate();
                    settingEntry.transform.parent = settingsEntryContainer;
                    var elementEntry = settingEntry.GetComponent<SettingsElementEntry>();
                    if (elementEntry != null)
                    {
                        SettingsElementEntries.Add(elementEntry);
                        int capturedIndex = SettingsElementEntries.Count - 1;

                        var trigger = elementEntry.GetComponent<EventTrigger>();

                        var entryTrigger = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                        entryTrigger.callback.AddListener((_) => ShowEntry(capturedIndex));
                        trigger.triggers.Add(entryTrigger);
                    }
                }
            }
        }
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
