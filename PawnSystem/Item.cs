//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using UnityEngine;

/// <summary>
/// When in a pawn's inventory, the pawn can call the use functions here
/// </summary>
public class Item : Actor
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
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
}
