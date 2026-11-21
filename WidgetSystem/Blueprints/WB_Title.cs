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

using RivenFramework;
using UnityEngine;
using UnityEngine.UI;

namespace RivenFramework
{
    public class WB_Title : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            public string targetLevelID;
            //private LevelManager levelLoader; // Added for loading the overworld levels from Cartographer
            [SerializeField] private Button buttonMainGame,
                buttonExtras,
                buttonRanking,
                buttonSettings,
                buttonQuit,
                buttonCredits,
                buttonLanguage;
            [SerializeField] private GameObject extrasWidget, rankingWidget, settingsWidget, creditsWidget, languageWidget;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            private GI_WidgetManager widgetManager;
            private GI_WorldLoader worldLoader;
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
                widgetManager = GameInstance.Get<GI_WidgetManager>();
                worldLoader = GameInstance.Get<GI_WorldLoader>();
                //levelLoader = FindObjectOfType<LevelManager>();
                buttonMainGame.onClick.AddListener(delegate { OnClick("buttonMainGame"); });
                buttonExtras.onClick.AddListener(delegate { OnClick("buttonExtras"); });
                buttonRanking.onClick.AddListener(delegate { OnClick("buttonRanking"); });
                buttonSettings.onClick.AddListener(delegate { OnClick("buttonSettings"); });
                buttonQuit.onClick.AddListener(delegate { OnClick("buttonQuit"); });
                buttonCredits.onClick.AddListener(delegate { OnClick("buttonCredits"); });
                buttonLanguage.onClick.AddListener(delegate { OnClick("buttonLanguage"); });
            }

            private void Update()
            {

            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private void OnClick(string button)
            {
                switch (button)
                {
                    case "buttonMainGame":
                        if (!worldLoader) worldLoader = GameInstance.Get<GI_WorldLoader>();
                        //if (!levelLoader) levelLoader = FindObjectOfType<LevelManager>();
                        worldLoader.LoadWorld(targetLevelID);
                        // levelLoader.Load("", true); // Replace with function to load save file level
                        break;
                    case "buttonExtras":
                        if (!widgetManager) widgetManager = GameInstance.Get<GI_WidgetManager>();
                        widgetManager.AddWidget(extrasWidget);
                        break;
                    case "buttonRanking":
                        if (!widgetManager) widgetManager = GameInstance.Get<GI_WidgetManager>();
                        widgetManager.AddWidget(rankingWidget);
                        break;
                    case "buttonSettings":
                        if (!widgetManager) widgetManager = GameInstance.Get<GI_WidgetManager>();
                        widgetManager.AddWidget(settingsWidget); // Create the settings widget
                        //Destroy(gameObject); // Remove the current widget
                        //GameInstance.GetWidget("WB_Settings").GetComponent<WB_Settings>().Init();
                        break;
                    case "buttonQuit":
                        Application.Quit();
                        break;
                    case "buttonCredits":
                        if (!widgetManager) widgetManager = GameInstance.Get<GI_WidgetManager>();
                        widgetManager.AddWidget(creditsWidget);
                        break;
                    case "buttonLanguage":
                        if (!widgetManager) widgetManager = GameInstance.Get<GI_WidgetManager>();
                        widgetManager.AddWidget(languageWidget);
                        break;
                }
            }
        #endregion
        #endregion
    }
}
