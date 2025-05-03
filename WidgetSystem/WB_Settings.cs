//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using UnityEngine;
using UnityEngine.UI;

    public class WB_Settings : MonoBehaviour
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
        private GI_WidgetManager widgetManager;
        private ApplicationSettings applicationSettings;

        [SerializeField] private Button buttonBack,
            buttonApply,
            buttonReset,
            buttonGraphics,
            buttonAudio,
            buttonControls,
            buttonGameplay;

        [SerializeField] private GameObject graphicsWidget, audioWidget, controlsWidget, gameplayWidget;


        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Start()
        {
            widgetManager = FindObjectOfType<GI_WidgetManager>();
            applicationSettings = FindObjectOfType<ApplicationSettings>();
            //applicationSettings.LoadSettings();
            buttonBack.onClick.AddListener(delegate { OnClick("buttonBack"); });
            buttonApply.onClick.AddListener(delegate { OnClick("buttonApply"); });
            buttonReset.onClick.AddListener(delegate { OnClick("buttonReset"); });
            buttonGraphics.onClick.AddListener(delegate { OnClick("buttonGraphics"); });
            buttonAudio.onClick.AddListener(delegate { OnClick("buttonAudio"); });
            buttonControls.onClick.AddListener(delegate { OnClick("buttonControls"); });
            buttonGameplay.onClick.AddListener(delegate { OnClick("buttonGameplay"); });
            Init();
        }

        private void OnDestroy()
        {
            RemoveSubwidgets();
        }

        //=-----------------=
        // Internal Functions
        //=-----------------=
        private void OnClick(string button)
        {
            switch (button)
            {
                case "buttonBack":
                    //if (!gameInstance) gameInstance = FindObjectOfType<GameInstance>();
                    //gameInstance.UI_ShowTitle();
                    RemoveSubwidgets();
                    Destroy(gameObject);
                    break;
                case "buttonApply":
                    applicationSettings.ApplySettings();
                    break;
                case "buttonReset":
                    applicationSettings.ResetSettings();
                    break;
                case "buttonGraphics":
                    if (!widgetManager) widgetManager = FindObjectOfType<GI_WidgetManager>();
                    RemoveSubwidgets();
                    widgetManager.AddWidget(graphicsWidget);
                    break;
                case "buttonAudio":
                    if (!widgetManager) widgetManager = FindObjectOfType<GI_WidgetManager>();
                    RemoveSubwidgets();
                    widgetManager.AddWidget(audioWidget);
                    break;
                case "buttonControls":
                    if (!widgetManager) widgetManager = FindObjectOfType<GI_WidgetManager>();
                    RemoveSubwidgets();
                    widgetManager.AddWidget(controlsWidget);
                    break;
                case "buttonGameplay":
                    if (!widgetManager) widgetManager = FindObjectOfType<GI_WidgetManager>();
                    RemoveSubwidgets();
                    widgetManager.AddWidget(gameplayWidget);
                    break;
            }
        }


        //=-----------------=
        // External Functions
        //=-----------------=
        [Tooltip("Call this to add the first setting sub-widget")]
        public void Init()
        {
            RemoveSubwidgets();
            widgetManager.AddWidget(graphicsWidget);
        }

        public void RemoveSubwidgets()
        {
            Destroy(widgetManager.GetExistingWidget("WB_Settings_Graphics"));
            Destroy(widgetManager.GetExistingWidget("WB_Settings_Audio"));
            Destroy(widgetManager.GetExistingWidget("WB_Settings_Controls"));
            Destroy(widgetManager.GetExistingWidget("WB_Settings_Gameplay"));
        }
    }