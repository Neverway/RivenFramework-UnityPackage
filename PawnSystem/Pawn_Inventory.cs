//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  Gives the pawn the ability to switch between "item" game objects This component works by adding and removing game
//  objects as children to the object it's attached to For first person games, attach this to an "inventory" empty
//  that is parented to the pawns viewpoint
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    public class Pawn_Inventory : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            public int currentIndex;
            public bool allowDuplicateItems;
            public List<GameObject> items;
            public GameObject owner;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void Start()
            {
                UpdateItemList(); 
                UpdateEquippedItem();
            }

            private void Update()
            {

            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
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
                if (_index < 0 || _index >= items.Count) return;
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

            public bool RemoveItem(int _index)
            {
                if (_index < 0 || _index >= items.Count) return false;
                Destroy(items[_index]);
                items.RemoveAt(_index);
                currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, items.Count - 1));
                UpdateEquippedItem();
                return true;
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
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
                var prefabActor = _itemPrefab.GetComponent<Actor>();

                if (prefabActor == null || string.IsNullOrEmpty(prefabActor.id)) return false;

                foreach (var item in items)
                {
                    var actor = item.GetComponent<Actor>();
                    if (actor && actor.id == prefabActor.id) return true;
                }

                return false;
            }
        #endregion
        #endregion
    }
}
