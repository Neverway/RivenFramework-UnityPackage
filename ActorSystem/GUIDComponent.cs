//==========================================( Neverway 2027 )=========================================================//
// Author
//  Errynei
//
// Contributors
//  Liz M.
//  Connorses
//  Soulex
//
// Description
//
//====================================================================================================================//

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ErryLib
{
    public class GUIDComponent : MonoBehaviour
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
            [StringAsGUID, SerializeField] 
            [Tooltip("The globaly unique identifier for this object")]
            private string guid;
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
            private void OnValidate()
            {
                #if UNITY_EDITOR
                if (Event.current != null)
                {
                    if (Event.current.type == EventType.ExecuteCommand)
                    {
                        if (Event.current.commandName == "Duplicate" || Event.current.commandName == "Paste")
                            NewGUID();
                    }
                    if (Event.current.type == EventType.DragUpdated)
                    {
                        NewGUID();
                    }

                }
                #endif
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public string GetGUID() => guid;
            public string SetGUID(string newGuid) => guid = newGuid;

            public string NewGUID()
            {
                guid = Guid.NewGuid().ToString();
                #if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                #endif
                return guid;
            }

        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private void Reset() => NewGUID();
        #endregion

        #endregion
    }
}