//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Gives the pawn the ability to switch between "item" game objects
// Notes: This component works by adding and removing game objects as children to the object it's attached to
// For first person games, attach this to an "inventory" empty that is parented to the pawns viewpoint
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn_Inventory : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public int currentIndex;
    public bool allowDuplicateItems;
    public List<GameObject> items;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    public GameObject owner;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        UpdateItemList();
    }

    private void Update()
    {
    
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void UpdateEquippedItem()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i == currentIndex) items[i].SetActive(true);
            else items[i].SetActive(false);
        }
    }

    private void UpdateItemList()
    {
        items.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            items.Add(transform.GetChild(i).gameObject);
        }
    }

    private bool HasItem(GameObject _itemPrefab)
    {
        if (_itemPrefab.GetComponent<Actor>() is null) return false;
        
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].GetComponent<Actor>())
            {
                if (items[i].GetComponent<Actor>().id == _itemPrefab.GetComponent<Actor>().id) return true;
            }
        }

        return false;
    }
    

    //=-----------------=
    // External Functions
    //=-----------------=
    public void SwitchPreviouse()
    {
        // Exit if no items
        if (items.Count <= 0 ) return;
        
        // Wrap around when at last
        if (currentIndex - 1 < 0) currentIndex = items.Count - 1;
        // Switch down one
        else currentIndex--;
        
        // Enable item that was switched to
        UpdateEquippedItem();
    }
    public void SwitchNext()
    {
        // Exit if no items
        if (items.Count <= 0 ) return;
        
        // Wrap around when at last
        if (currentIndex + 1 >= items.Count) currentIndex = 0;
        // Switch down one
        else currentIndex++;
        
        // Enable item that was switched to
        UpdateEquippedItem();
    }

    public void SwitchTo(int _index)
    {
        if (items.Count < _index) return;
        currentIndex = _index;
        UpdateEquippedItem();
    }

    public bool AddItem(GameObject _itemPrefab)
    {
        if (HasItem(_itemPrefab) && !allowDuplicateItems)
        {
            return false;
        }
        
        var newItem = Instantiate(_itemPrefab, transform);
        newItem.SetActive(false);
        UpdateItemList();
        SwitchTo(items.Count-1);
        return true;
    }

    public bool RemoveItem()
    {
        return false;
    }
}
