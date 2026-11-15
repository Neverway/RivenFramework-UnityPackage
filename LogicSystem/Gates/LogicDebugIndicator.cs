//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  The images to set the sprite renderer to when the indicator is on or off
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    /// <summary>
    /// A visual indicator that reflects wether a logic bool is true or false using a sprite renderer
    /// </summary>
    public class LogicDebugIndicator : Logic
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Tooltip("The images to set the sprite renderer to when the indicator is on or off")]
            public Sprite indicatorOn, indicatorOff;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            [Tooltip("The input for if this indicator is on")]
            public LogicInput<bool> input = new(false);
            [Tooltip("This is just an output that reflects the same value as the input, used for making this indicator be in-line with a logic link")]
            public LogicOutput<bool> output = new(false);
            [Tooltip("The sprite renderer that reflects this indicators on and off state")]
            private SpriteRenderer spriteRenderer;
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
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            private void Update()
            {
                if (input)
                {
                    output.Set(true);
                    spriteRenderer.sprite = indicatorOn;
                }
                else
                {
                    output.Set(false);
                    spriteRenderer.sprite = indicatorOff;
                }
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
