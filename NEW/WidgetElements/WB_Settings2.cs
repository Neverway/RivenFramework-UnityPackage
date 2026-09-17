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
using UnityEngine.Serialization;
using UnityEngine.UI;
using ReflectionCache = RivenFramework.Utils.Reflection.ReflectionCache;

public class WB_Settings2 : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    // Populated by reflection nonsense, gets all defined setting entries for this project from other classes
    private HashSet<SettingEntry> allSettingDefinitions = new HashSet<SettingEntry>();
    // The data from allSettingDefinitions ordered into the proper Category/Group/Entry format
    private Dictionary<string, Dictionary<string, List<SettingEntry>>> settingsByCategoryAndGroup;
    // Shows the relative list of entries based on what index is selected
    private int selectedCategoryIndex; 
    // Shows the relative image, description header, and description based on what index is selected
    private int selectedEntryIndex;
    // All the created settings entries
    private List<SettingsElementEntry> allEntryElements = new List<SettingsElementEntry>();

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Transform topbarContainer;
    public Transform categoryScrollBoxesContainer;
    public Image referenceImage;
    public TMP_Text descriptionHeaderText;
    public TMP_Text descriptionText;
    public Button applyButton, resetButton, backButton;
    public GameObject topbarCategoryButtonPrefab;
    public GameObject groupPrefab, categoryPrefab;
    public List<GameObject> categoryRootObjects = new List<GameObject>();
    


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        CollectSettingEntriesViaReflection();
        BuildCategoryLookup();
        CreateCategoryButtons();
        CreateCategoryPagesWithGroupsAndEntries();
        selectedCategoryIndex = 0;
        SelectCategory(selectedCategoryIndex);
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void CollectSettingEntriesViaReflection()
    {
        allSettingDefinitions = new HashSet<SettingEntry>();
        
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
                allSettingDefinitions.Add(settingEntry);
        }
    }

    private void BuildCategoryLookup()
    {
        settingsByCategoryAndGroup = new Dictionary<string, Dictionary<string, List<SettingEntry>>>();
        
        foreach (var entry in allSettingDefinitions)
        {
            string category = entry.category.Split("/", StringSplitOptions.None)[0];
            string group = entry.category.Split("/", StringSplitOptions.None)[1];
            string name = entry.category.Split("/", StringSplitOptions.None)[2];
            
            // Create category if it doesn't exist
            if (settingsByCategoryAndGroup.ContainsKey(category) is false) 
                settingsByCategoryAndGroup.Add(category, new Dictionary<string, List<SettingEntry>>());

            // Create group if it doesn't exist
            if (settingsByCategoryAndGroup[category].ContainsKey(group) is false) 
                settingsByCategoryAndGroup[category].Add(group, new List<SettingEntry>());

            // Add entry to group
            settingsByCategoryAndGroup[category][group].Add(entry);
        }
    }

    private void CreateCategoryButtons()
    {
        int categoryId = 0;
        foreach (var category in settingsByCategoryAndGroup.Keys)
        {
            var categoryButton = Instantiate(topbarCategoryButtonPrefab, topbarContainer);
            categoryButton.GetComponentInChildren<TMP_Text>().text = category;
            var categoryButtonButton = categoryButton.GetComponentInChildren<Button>();
            categoryButtonButton.onClick.RemoveAllListeners();
            var cachedID = categoryId;
            categoryButtonButton.onClick.AddListener(() => { SelectCategory(cachedID); });
            categoryId++;
        }
    }
    
    private void CreateCategoryPagesWithGroupsAndEntries()
    {
        allEntryElements.Clear();
        categoryRootObjects = new List<GameObject>();
        foreach (var category in settingsByCategoryAndGroup.Keys)
        {
            var categoryRoot = Instantiate(categoryPrefab, categoryScrollBoxesContainer);
            var categoryContainer = categoryRoot.transform.GetChild(0).GetChild(0).transform;
            categoryRoot.name = category;
            categoryRoot.SetActive(false);
            categoryRootObjects.Add(categoryRoot);
            
            foreach (var group in settingsByCategoryAndGroup[category].Keys)
            {
                // Create the group header
                var groupEntry = Instantiate(groupPrefab, categoryContainer);
                groupEntry.GetComponentInChildren<TMP_Text>().text = group;
                
                // Create each of the entries
                foreach (var setting in settingsByCategoryAndGroup[category][group].ToArray())
                {
                    var settingEntry = setting.Instantiate();
                    settingEntry.transform.parent = categoryContainer;
                    var elementEntry = settingEntry.GetComponent<SettingsElementEntry>();
                    if (elementEntry != null)
                    {
                        allEntryElements.Add(elementEntry);
                        int capturedIndex = allEntryElements.Count - 1;

                        var trigger = elementEntry.GetComponent<EventTrigger>();

                        var entryTrigger = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                        entryTrigger.callback.AddListener((_) => DisplayEntryDetails(capturedIndex));
                        trigger.triggers.Add(entryTrigger);
                    }
                }
            }
        }
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void DisplayEntryDetails(int entryIndex)
    {
        selectedEntryIndex = entryIndex;
        if (selectedEntryIndex < 0)
        {
            referenceImage.gameObject.SetActive(false);
            descriptionHeaderText.text = "";
            descriptionText.text = "";
        }
        else
        {
            var _currentEntry = allEntryElements[selectedEntryIndex];
            referenceImage.sprite = _currentEntry.cachedReferenceImage;
            referenceImage.gameObject.SetActive(referenceImage.sprite != null);
            descriptionHeaderText.text = _currentEntry.text.text;
            descriptionText.text = _currentEntry.cachedDescription;
        }
    }

    public void SelectCategory(int categoryIndex)
    {
        for (int i = 0; i < categoryRootObjects.Count; i++)
        {
            categoryRootObjects[i].SetActive(false);
        }

        Debug.Log(categoryIndex);
        categoryRootObjects[categoryIndex].SetActive(true);
    }


    #endregion
}