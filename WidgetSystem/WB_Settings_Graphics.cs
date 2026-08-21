//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Settings sub-widget for changing graphics options
// Notes:
//
//=============================================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

    public class WB_Settings_Graphics : MonoBehaviour
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

        // Display
        [SerializeField] private TMP_Dropdown targetResolution;
        [SerializeField] private Button_Selector windowMode;
        [SerializeField] private Toggle enableVsync;
        [SerializeField] private Slider fpslimit;
        [SerializeField] private Toggle showFramecounter;

        // Quality
        [SerializeField] public Button_Selector qualityPreset;
        [SerializeField] public Button_Selector resolutionScale;
        [SerializeField] public Button_Selector shadowQuality;
        [SerializeField] public Slider maxShadowCasters;
        [SerializeField] public Button_Selector effectsQuality;
        [SerializeField] public Button_Selector textureQuality;
        [SerializeField] public Button_Selector postprocessingQuality;
        [SerializeField] public Button_Selector dynamicBones;

        // Effects
        [SerializeField] public Button_Selector antialiasing;
        [SerializeField] public Button_Selector motionBlur;
        [SerializeField] public Button_Selector ambientOcclusion;
        [SerializeField] public Button_Selector bloom;


        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Start()
        {
            applicationSettings = FindObjectOfType<ApplicationSettings>();
            InitButtonValues();
            InitEventListeners(); // This is where the values of the applicationSettings are updated via the buttons and stuff :3
        }

        private void Update()
        {
            applicationSettings.bufferedSettingsData.windowMode = windowMode.currentIndex;

            applicationSettings.bufferedSettingsData.qualityPreset = qualityPreset.currentIndex;
            applicationSettings.bufferedSettingsData.quality.resolutionScale = resolutionScale.currentIndex;
            applicationSettings.bufferedSettingsData.quality.shadowQuality = shadowQuality.currentIndex;
            applicationSettings.bufferedSettingsData.quality.effectsQuality = effectsQuality.currentIndex;
            applicationSettings.bufferedSettingsData.quality.textureQuality = textureQuality.currentIndex;
            applicationSettings.bufferedSettingsData.quality.postprocessingQuality = postprocessingQuality.currentIndex;
            applicationSettings.bufferedSettingsData.quality.dynamicBones = dynamicBones.currentIndex;

            applicationSettings.bufferedSettingsData.antialiasing = antialiasing.currentIndex;
            applicationSettings.bufferedSettingsData.motionBlur = motionBlur.currentIndex;
            applicationSettings.bufferedSettingsData.ambientOcclusion = ambientOcclusion.currentIndex;
            applicationSettings.bufferedSettingsData.bloom = bloom.currentIndex;
        }

        //=-----------------=
        // Internal Functions
        //=-----------------=
        public void InitButtonValues()
        {
            applicationSettings.bufferedSettingsData = new ApplicationSettingsData(applicationSettings.currentSettingsData);
            PopulateTargetResolutionDropdown();
            targetResolution.value = applicationSettings.bufferedSettingsData.targetResolution;
            windowMode.currentIndex = applicationSettings.bufferedSettingsData.windowMode;
            enableVsync.isOn = applicationSettings.bufferedSettingsData.enableVysnc;
            fpslimit.value = applicationSettings.bufferedSettingsData.fpsLimit;
            showFramecounter.isOn = applicationSettings.bufferedSettingsData.showFramecounter;

            qualityPreset.currentIndex = applicationSettings.bufferedSettingsData.qualityPreset;
            resolutionScale.currentIndex = applicationSettings.bufferedSettingsData.quality.resolutionScale;
            shadowQuality.currentIndex = applicationSettings.bufferedSettingsData.quality.shadowQuality;
            maxShadowCasters.value = applicationSettings.bufferedSettingsData.quality.maxShadowCasters;
            effectsQuality.currentIndex = applicationSettings.bufferedSettingsData.quality.effectsQuality;
            textureQuality.currentIndex = applicationSettings.bufferedSettingsData.quality.textureQuality;
            postprocessingQuality.currentIndex = applicationSettings.bufferedSettingsData.quality.postprocessingQuality;
            dynamicBones.currentIndex = applicationSettings.bufferedSettingsData.quality.dynamicBones;

            antialiasing.currentIndex = applicationSettings.bufferedSettingsData.antialiasing;
            motionBlur.currentIndex = applicationSettings.bufferedSettingsData.motionBlur;
            ambientOcclusion.currentIndex = applicationSettings.bufferedSettingsData.ambientOcclusion;
            bloom.currentIndex = applicationSettings.bufferedSettingsData.bloom;
        }

        private void InitEventListeners()
        {
            targetResolution.onValueChanged.AddListener(delegate
            {
                applicationSettings.bufferedSettingsData.targetResolution = targetResolution.value;
            });
            enableVsync.onValueChanged.AddListener(delegate
            {
                applicationSettings.bufferedSettingsData.enableVysnc = enableVsync.isOn;
            });
            fpslimit.onValueChanged.AddListener(delegate
            {
                applicationSettings.bufferedSettingsData.fpsLimit = Mathf.RoundToInt(fpslimit.value);
            });
            maxShadowCasters.onValueChanged.AddListener(delegate
            {
                applicationSettings.bufferedSettingsData.quality.maxShadowCasters = Mathf.RoundToInt(maxShadowCasters.value);
            });
            showFramecounter.onValueChanged.AddListener(delegate
            {
                applicationSettings.bufferedSettingsData.showFramecounter = showFramecounter.isOn;
            });
            qualityPreset.onValueChanged.AddListener(() => 
            {
                switch (qualityPreset.currentIndex)
                {
                    case 0:
                        resolutionScale.currentIndex = applicationSettings.retroQuality.resolutionScale;
                        shadowQuality.currentIndex = applicationSettings.retroQuality.shadowQuality;
                        maxShadowCasters.value = applicationSettings.retroQuality.maxShadowCasters;
                        effectsQuality.currentIndex = applicationSettings.retroQuality.effectsQuality;
                        textureQuality.currentIndex = applicationSettings.retroQuality.textureQuality;
                        postprocessingQuality.currentIndex = applicationSettings.retroQuality.postprocessingQuality;
                        dynamicBones.currentIndex = applicationSettings.retroQuality.dynamicBones;
                        break;
                    case 1:
                        resolutionScale.currentIndex = applicationSettings.lowQuality.resolutionScale;
                        shadowQuality.currentIndex = applicationSettings.lowQuality.shadowQuality;
                        maxShadowCasters.value = applicationSettings.lowQuality.maxShadowCasters;
                        effectsQuality.currentIndex = applicationSettings.lowQuality.effectsQuality;
                        textureQuality.currentIndex = applicationSettings.lowQuality.textureQuality;
                        postprocessingQuality.currentIndex = applicationSettings.lowQuality.postprocessingQuality;
                        dynamicBones.currentIndex = applicationSettings.lowQuality.dynamicBones;
                        break;
                    case 2:
                        resolutionScale.currentIndex = applicationSettings.mediumQuality.resolutionScale;
                        shadowQuality.currentIndex = applicationSettings.mediumQuality.shadowQuality;
                        maxShadowCasters.value = applicationSettings.mediumQuality.maxShadowCasters;
                        effectsQuality.currentIndex = applicationSettings.mediumQuality.effectsQuality;
                        textureQuality.currentIndex = applicationSettings.mediumQuality.textureQuality;
                        postprocessingQuality.currentIndex = applicationSettings.mediumQuality.postprocessingQuality;
                        dynamicBones.currentIndex = applicationSettings.mediumQuality.dynamicBones;
                        break;
                    case 3:
                        resolutionScale.currentIndex = applicationSettings.highQuality.resolutionScale;
                        shadowQuality.currentIndex = applicationSettings.highQuality.shadowQuality;
                        maxShadowCasters.value = applicationSettings.highQuality.maxShadowCasters;
                        effectsQuality.currentIndex = applicationSettings.highQuality.effectsQuality;
                        textureQuality.currentIndex = applicationSettings.highQuality.textureQuality;
                        postprocessingQuality.currentIndex = applicationSettings.highQuality.postprocessingQuality;
                        dynamicBones.currentIndex = applicationSettings.highQuality.dynamicBones;
                        break;
                    case 4:
                        resolutionScale.currentIndex = applicationSettings.fantasticQuality.resolutionScale;
                        shadowQuality.currentIndex = applicationSettings.fantasticQuality.shadowQuality;
                        maxShadowCasters.value = applicationSettings.fantasticQuality.maxShadowCasters;
                        effectsQuality.currentIndex = applicationSettings.fantasticQuality.effectsQuality;
                        textureQuality.currentIndex = applicationSettings.fantasticQuality.textureQuality;
                        postprocessingQuality.currentIndex = applicationSettings.fantasticQuality.postprocessingQuality;
                        dynamicBones.currentIndex = applicationSettings.fantasticQuality.dynamicBones;
                        break;
                }
            });
        }

        void PopulateTargetResolutionDropdown()
        {
            targetResolution.ClearOptions();
            //int currentResolutionIndex = 0;

            // Create a list to hold the resolution strings
            var options = new List<TMP_Dropdown.OptionData>();

            for (int i = 0; i < applicationSettings.resolutions.Length; i++)
            {
                var aspect = GetAspectRatio(applicationSettings.resolutions[i].width,
                    applicationSettings.resolutions[i].height);
                // Don't allow not perfect aspect ratios
                /*if (aspect == "")
                {
                    continue;
                }*/
                string resolution =
                    $"{applicationSettings.resolutions[i].width}x{applicationSettings.resolutions[i].height} [{aspect}]";
                options.Add(new TMP_Dropdown.OptionData(resolution));

                // Check if this resolution is the current one
                if (applicationSettings.resolutions[i].width == Screen.width &&
                    applicationSettings.resolutions[i].height == Screen.height)
                {
                    //currentResolutionIndex = i;
                }
            }

            // Set the dropdown options
            targetResolution.AddOptions(options);
            //targetResolution.value = currentResolutionIndex;
            targetResolution.RefreshShownValue();
        }

        string GetAspectRatio(int width, int height)
        {
            float aspectRatio = (float)width / height;

            if (Mathf.Approximately(aspectRatio, 16f / 9f))
            {
                return "16:9";
            }

            if (Mathf.Approximately(aspectRatio, 4f / 3f))
            {
                return "4:3";
            }

            if (Mathf.Approximately(aspectRatio, 21f / 9f))
            {
                return "21:9";
            }

            if (Mathf.Approximately(aspectRatio, 16f / 10f))
            {
                return "16:10";
            }

            if (Mathf.Approximately(aspectRatio, 3f / 2f))
            {
                return "3:2";
            }

            // Return an empty string for non-perfect aspect ratios
            return "";
        }


        //=-----------------=
        // External Functions
        //=-----------------=
    }