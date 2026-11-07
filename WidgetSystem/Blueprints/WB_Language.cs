//==========================================( Neverway 2024 )=========================================================//
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
            //=-----------------=
            // Public Variables
            //=-----------------=


            //=-----------------=
            // Private Variables
            //=-----------------=


            //=-----------------=
            // Reference Variables
            //=-----------------=
            private ApplicationSettings applicationSettings;
            [SerializeField] private Button buttonBack;


            //=-----------------=
            // Mono Functions
            //=-----------------=
            private void Start()
            {
                applicationSettings = FindObjectOfType<ApplicationSettings>();
                if (buttonBack) buttonBack.onClick.AddListener(() => { Destroy(gameObject); });
            }


            //=-----------------=
            // Internal Functions
            //=-----------------=


            //=-----------------=
            // External Functions
            //=-----------------=
            public void SetLocaleID(int _localeID)
            {
                applicationSettings.bufferedSettingsData.localeID = _localeID;
                applicationSettings.currentSettingsData.localeID = _localeID;
                applicationSettings.ApplySettings();
            }
        }
}