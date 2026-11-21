//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//
//====================================================================================================================//

using UnityEngine;
using UnityEngine.UI;

namespace RivenFramework
{
    public class WB_Language : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [SerializeField] private Button buttonBack;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private ApplicationSettings applicationSettings;
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void Start()
            {
                applicationSettings = FindObjectOfType<ApplicationSettings>();
                if (buttonBack) buttonBack.onClick.AddListener(() => { Destroy(gameObject); });
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public void SetLocaleID(int _localeID)
            {
                applicationSettings.bufferedSettingsData.localeID = _localeID;
                applicationSettings.currentSettingsData.localeID = _localeID;
                applicationSettings.ApplySettings();
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
