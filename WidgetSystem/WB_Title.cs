//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using UnityEngine;
using UnityEngine.UI;

namespace Neverway.Framework.ApplicationManagement
{
    public class WB_Title : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        public string targetLevelID;


        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=
        private GI_WidgetManager widgetManager;
        //private WorldLoader worldLoader;
        //private LevelManager levelLoader; // Added for loading the overworld levels from Cartographer

        [SerializeField] private Button buttonMainGame,
            buttonExtras,
            buttonRanking,
            buttonSettings,
            buttonQuit,
            buttonCredits,
            buttonLanguage;

        [SerializeField] private GameObject extrasWidget, rankingWidget, settingsWidget, creditsWidget, languageWidget;


        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Start()
        {
            widgetManager = FindObjectOfType<GI_WidgetManager>();
            //worldLoader = FindObjectOfType<WorldLoader>();
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

        //=-----------------=
        // Internal Functions
        //=-----------------=
        private void OnClick(string button)
        {
            switch (button)
            {
                case "buttonMainGame":
                    //if (!worldLoader) worldLoader = FindObjectOfType<WorldLoader>();
                    //if (!levelLoader) levelLoader = FindObjectOfType<LevelManager>();
                    //worldLoader.LoadWorld(targetLevelID);
                    // levelLoader.Load("", true); // Replace with function to load save file level
                    break;
                case "buttonExtras":
                    if (!widgetManager) widgetManager = FindObjectOfType<GI_WidgetManager>();
                    widgetManager.AddWidget(extrasWidget);
                    break;
                case "buttonRanking":
                    if (!widgetManager) widgetManager = FindObjectOfType<GI_WidgetManager>();
                    widgetManager.AddWidget(rankingWidget);
                    break;
                case "buttonSettings":
                    if (!widgetManager) widgetManager = FindObjectOfType<GI_WidgetManager>();
                    widgetManager.AddWidget(settingsWidget); // Create the settings widget
                    //Destroy(gameObject); // Remove the current widget
                    //GameInstance.GetWidget("WB_Settings").GetComponent<WB_Settings>().Init();
                    break;
                case "buttonQuit":
                    Application.Quit();
                    break;
                case "buttonCredits":
                    if (!widgetManager) widgetManager = FindObjectOfType<GI_WidgetManager>();
                    widgetManager.AddWidget(creditsWidget);
                    break;
                case "buttonLanguage":
                    if (!widgetManager) widgetManager = FindObjectOfType<GI_WidgetManager>();
                    widgetManager.AddWidget(languageWidget);
                    break;
            }
        }


        //=-----------------=
        // External Functions
        //=-----------------=
    }
}