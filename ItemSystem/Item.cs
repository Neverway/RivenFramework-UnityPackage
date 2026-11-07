//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//  Errynei
//  Connorses
//  Soulex
//
// Description
//
//====================================================================================================================//

using UnityEngine;

namespace RivenFramework
{
    /// <summary>
    /// When in a pawn's inventory, the pawn can call the use functions here
    /// </summary>
    public class Item : Actor
    {
        #region====================================( Variables )======================================================//

        #region----------------( Inspector )--------------------------------------------------------------------------//
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
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public virtual void UsePrimary(string _mode = "press")
            {
                switch (_mode)
                {
                    case "press":
                        break;
                    case "release":
                        break;
                }
            }
        
            public virtual void UseSecondary(string _mode = "press")
            {
                switch (_mode)
                {
                    case "press":
                        break;
                    case "release":
                        break;
                }
            }
        
            public virtual void UseTertiary(string _mode = "press")
            {
                switch (_mode)
                {
                    case "press":
                        break;
                    case "release":
                        break;
                }
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion

        #endregion
    }
}