//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[GIModuleColor(_color: GIModuleColors.Blue)]
public class GI_ApplicationSettings : GameInstanceModule
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Project Configuration")]
    [Tooltip("If you have changed the application data structure, update this number so that the game knows to make a new config file for the new version")]
    public int configVersion = 1;
    [Tooltip("The default values for the settings (pulled from the constructor in ApplicationSettingsData, overridden here)")]
    [SerializeField] private ApplicationSettingsData defaultSettingsData;
    [Tooltip("A list of which folders contain textures that are affected by the dynamic texture filters")]
    [SerializeField] private List<string> dynamicallyFilteredTexturePaths = new List<string> { "Materials/Textures/DynamicallyFiltered" };

    [Header("Quality Levels")] 
    [Tooltip("Quality setting presets")]
    public ApplicationSettingsData_Quality retroQuality;
    [Tooltip("Quality setting presets")]
    public ApplicationSettingsData_Quality lowQuality, mediumQuality, highQuality, fantasticQuality;
    
    [Header("Debugging")]
    [Tooltip("The current values for the settings")]
    public ApplicationSettingsData currentSettingsData;
    [Tooltip("The unapplied values for the settings, current settings gets set to these values right before applying")]
    public ApplicationSettingsData bufferedSettingsData;
    [Tooltip("If enabled, force the first-time-setup screen to appear when starting the game from the splash screen scene")]
    public bool debugForceEnableFirstTimeSetup;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private string configurationFilePath;
    public Resolution[] resolutions;
    private Texture[] filteredTextures;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_WidgetManager widgetManager;
    [Header("References")]
    public AudioMixer audioMixer;
    public GameObject cameraPrefab;
    public PostProcessProfile postProcessProfile;
    public GameObject framecounterWidget;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
