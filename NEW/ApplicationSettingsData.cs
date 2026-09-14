using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RivenFramework_Revamp.NEW
{
    [Serializable]
    public class ApplicationSettingsData
    {
        public static ApplicationSettingsData GetThis()
        {
            return GameObject.FindObjectOfType<Dummy>().ApplicationSettings.defaultSettingsData;
        }

    
        [Header("Entry Prefabs")]
        [GimmeDatPrefab("SliderPrefab")]
        public GameObject SliderPrefab;
        [GimmeDatPrefab("CheckboxPrefab")]
        public GameObject CheckboxPrefab;
        [GimmeDatPrefab("DropdownPrefab")]
        public GameObject DropdownPrefab;
        [GimmeDatPrefab("SelectorPrefab")]
        public GameObject SelectorPrefab;
        
        [Header("Default Application Settings")]
        public AppSettingsData_Graphics graphics;
        public AppSettingsData_Audio audio;
        public AppSettingsData_Gameplay gameplay;

    }

    public abstract class AppSettingsData_Category<TSelf>
    {
        public static object GetInstance()
        {
            var parent = ApplicationSettingsData.GetThis();
            var fieldsOfMyType = parent.GetType().GetFields().Where(f => f.FieldType == typeof(TSelf)).ToArray();
            return fieldsOfMyType[0].GetValue(parent);
        }
    }

    [Serializable]
    [ContainsSettingEntries("GetInstance")]
    public class AppSettingsData_Graphics : AppSettingsData_Category<AppSettingsData_Graphics>
    {
        public static object GetInstance()
        {
            return AppSettingsData_Category<AppSettingsData_Graphics>.GetInstance();
        }
        
        [Header("Display")]
        [ErryBox]
        public SettingDropdown targetResolution = new SettingDropdown(
            "Graphics/Display/Resolution", 
            "-", 
            null, false, 0, Enum.GetNames(typeof(TargetResolutionOptions)).ToList());
        public enum TargetResolutionOptions { Native }

        [ErryBox]
        public SettingSelector windowMode = new SettingSelector(
            "Graphics/Display/Window Mode", 
            "-", 
            null, false, 0, Enum.GetNames(typeof(WindowModeOptions)).ToList());
        public enum WindowModeOptions { Windowed, Borderless, FullScreen }
        
        [ErryBox] 
        public SettingCheckbox enableVsync = new SettingCheckbox(
            "Graphics/Display/Enable V-Sync", 
            "-", 
            null, false, false);
        
        [ErryBox] 
        public SettingSlider fpsLimit = new SettingSlider(
            "Graphics/Display/FPS Limit", 
            "-", 
            null, false, 0, 15, 300);
        
        [ErryBox] 
        public SettingSlider brightness = new SettingSlider(
            "Graphics/Display/Brightness", 
            "-", 
            null, false, 100, 0, 200);
        
        [Header("Quality")]
        [ErryBox] 
        public SettingSelector qualityPreset = new SettingSelector(
            "Graphics/Quality/Quality Preset", 
            "-", 
            null, false, 4, Enum.GetNames(typeof(QualityPresetsOptions)).ToList());
        public enum QualityPresetsOptions { Custom, Retro, Low, Medium, High, Divine }
        
        [ErryBox] 
        public SettingSelector resolutionScale = new SettingSelector(
            "Graphics/Quality/Resolution Scale", 
            "How much the rendering is scaled up or down from the target window resolution.", 
            null, false, 4, new List<string>{ "50%", "60%", "75%", "80%", "100%", "200%" });
        public enum ResolutionScaleOptions { Scale50, Scale60, Scale75, Scale80, Scale100, Scale200 }
        
        [ErryBox] 
        public SettingSelector shadowQuality = new SettingSelector(
            "Graphics/Quality/Quality Preset", 
            "-", 
            null, false, 4, Enum.GetNames(typeof(QualityPresetsOptions)).ToList());
        
        [ErryBox] 
        public SettingSlider maxShadowCaster = new SettingSlider(
            "Graphics/Quality/Max Shadow Casters", 
            "Adjusts how many lights can cast real-time shadows at once. " +
            "Which lights are given priority casting shadows varies based on the range and intensity of each light source.", 
            null, false, 6, 0, 300);
        
        [ErryBox] 
        public SettingSelector effectsQuality = new SettingSelector(
            "Graphics/Quality/Effects Quality", 
            "-", 
            null, false, 4, Enum.GetNames(typeof(QualityPresetsOptions)).ToList());
        
        [ErryBox] 
        public SettingSelector textureQuality = new SettingSelector(
            "Graphics/Quality/Texture Quality", 
            "Adjusts the resolution of certain textures. " +
            "Lower values reduce VRAM usage but cause surfaces to appear blurry or pixelated.", 
            null, false, 4, Enum.GetNames(typeof(QualityPresetsOptions)).ToList());
        
        [ErryBox] 
        public SettingSelector postProcessingQuality = new SettingSelector(
            "Graphics/Quality/Post-Processing Quality", 
            "-", 
            null, false, 4, Enum.GetNames(typeof(QualityPresetsOptions)).ToList());
        
        [ErryBox] 
        public SettingSelector dynamicBones = new SettingSelector(
            "Graphics/Quality/Dynamic Bones", 
            "Adjusts the amount of dynamic bones that are used to add fluidity to things like capes, scarfs, tails, fluff, etc.", 
            null, false, 2, Enum.GetNames(typeof(DynamicBonesOptions)).ToList());
        public enum DynamicBonesOptions { Off, Decreased, Enabled }
        
        [Header("Effects")]
        [ErryBox] 
        public SettingSelector antiAliasing = new SettingSelector(
            "Graphics/Effects/Anti-Aliasing", 
            "-", 
            null, false, 0, Enum.GetNames(typeof(AntiAliasingOptions)).ToList());
        public enum AntiAliasingOptions { Off, MSAAx2, MSAAx4, MSAAx8, FXAA, SMAA, TAA }
        
        [ErryBox] 
        public SettingSelector screenSpaceReflections = new SettingSelector(
            "Graphics/Effects/Screen-Space Reflections", 
            "-", 
            null, false, 3, Enum.GetNames(typeof(EffectsOptions)).ToList());
        
        [ErryBox] 
        public SettingSelector depthOfField = new SettingSelector(
            "Graphics/Effects/Depth Of Field", 
            "-", 
            null, false, 3, Enum.GetNames(typeof(EffectsOptions)).ToList());
        
        [ErryBox] 
        public SettingSelector motionBlur = new SettingSelector(
            "Graphics/Effects/Motion Blur", 
            "-", 
            null, false, 3, Enum.GetNames(typeof(EffectsOptions)).ToList());
        
        [ErryBox] 
        public SettingSelector ambientOcclusion = new SettingSelector(
            "Graphics/Effects/Ambient Occlusion", 
            "-", 
            null, false, 3, Enum.GetNames(typeof(EffectsOptions)).ToList());
        
        [ErryBox] 
        public SettingSelector bloom = new SettingSelector(
            "Graphics/Effects/Bloom", 
            "-", 
            null, false, 3, new List<string>{ "Off", "Low", "Medium", "High", "Extreme (90's Sci-Fi Anime)" });
        public enum EffectsOptions { Off, Low, Medium, High, Extreme }
        
    }
    
    [Serializable]
    [ContainsSettingEntries("GetInstance")]
    public class AppSettingsData_Audio : AppSettingsData_Category<AppSettingsData_Audio>
    {
        public static object GetInstance()
        {
            return AppSettingsData_Category<AppSettingsData_Audio>.GetInstance();
        }
        
        [Header("Audio Devices")]
        [ErryBox] 
        public SettingDropdown outputDevice = new SettingDropdown(
            "Audio/Audio Devices/Output Device", 
            "Which device to play the games audio through.", 
            null, false, 0, Enum.GetNames(typeof(AudioDeviceOptions)).ToList());
        public enum AudioDeviceOptions { SystemDefault }
        
        [ErryBox] 
        public SettingDropdown inputDevice = new SettingDropdown(
            "Audio/Audio Devices/Input Device", 
            "Which device to use as your microphone for voice chat.", 
            null, false, 0, Enum.GetNames(typeof(AudioDeviceOptions)).ToList());
        
        [ErryBox] 
        public SettingSlider fpsLimit = new SettingSlider(
            "Audio/Audio Devices/Input Volume", 
            "Controls how loud your microphone sounds to other players.", 
            null, false, 50, 0, 100);
        
        [Header("Audio Mixer")]
        [ErryBox] 
        public SettingSlider masterVolume = new SettingSlider(
            "Audio/Audio Mixer/Master", 
            "Controls the volume for all sounds.", 
            null, false, 50, 0, 100);
        
        [ErryBox] 
        public SettingSlider musicVolume = new SettingSlider(
            "Audio/Audio Mixer/Music", 
            "Controls the volume for music.", 
            null, false, 50, 0, 100);
        
        [ErryBox] 
        public SettingSlider soundEffectsVolume = new SettingSlider(
            "Audio/Audio Mixer/Sound Effects", 
            "Controls the volume for all other sounds that don't fit in the other categories.", 
            null, false, 50, 0, 100);
        
        [ErryBox] 
        public SettingSlider voiceChatVolume = new SettingSlider(
            "Audio/Audio Mixer/Voice Chat", 
            "Controls the volume for how loud other players sound to you.", 
            null, false, 50, 0, 100);
        
        [ErryBox] 
        public SettingSlider characterChatterVolume = new SettingSlider(
            "Audio/Audio Mixer/Character Chatter", 
            "Controls the volume for in-game characters talking.", 
            null, false, 50, 0, 100);
        
        [ErryBox] 
        public SettingSlider ambienceVolume = new SettingSlider(
            "Audio/Audio Mixer/Ambience", 
            "Controls the volume for background environmental noises. (Eg. Crickets, ominous humming, flowing water)", 
            null, false, 50, 0, 100);
        
        [ErryBox] 
        public SettingSlider menusVolume = new SettingSlider(
            "Audio/Audio Mixer/Menus", 
            "Controls the volume for anything in the user interface. (Eg. Button clicks, sliders, notifications)", 
            null, false, 50, 0, 100);
        
        [Header("Accessibility")]
        [ErryBox] 
        public SettingCheckbox visualizeSoundEffects = new SettingCheckbox(
            "Audio/Accessibility/Visualize Sound Effects", 
                    // When enabled, key sound effects will be shown on the in-game HUD (Heads Up Display) with an indicator of the direction they came from.
            "When enabled, key sound effects will be shown as a symbol in a circular ring on the in-game HUD (Heads up display) indicating what direction a certain sound came from.", 
            null, false, false);
        
        [ErryBox] 
        public SettingSelector closedCaptioning = new SettingSelector(
            "Audio/Accessibility/Closed Captioning", 
            "-", 
            null, false, 0, Enum.GetNames(typeof(closedCaptioningOptions)).ToList());
        public enum closedCaptioningOptions { Off, Dialogue, VoiceChat, All }
        
        [ErryBox] 
        public SettingSlider minVolumeDb = new SettingSlider(
            "Audio/Accessibility/Min Volume (dB)", 
            "-", 
            null, false, 0, 0, 70);
        
        [ErryBox] 
        public SettingSlider maxVolumeDb = new SettingSlider(
            "Audio/Accessibility/Max Volume (dB)", 
            "-", 
            null, false, 140, 100, 140);
        
        [ErryBox] 
        public SettingSlider maxFrequencyKhz = new SettingSlider(
            "Audio/Accessibility/Max Frequency (kHz)", 
            "The maximum frequency that any game sounds can play, anything above this limit will be pitch shifted down to fall within this range. " +
            "This is helpful if you can't, or don't want to hear higher pitch sounds.", 
            null, false, 17, 8, 17);
    }
    
    [Serializable]
    [ContainsSettingEntries("GetInstance")]
    public class AppSettingsData_Gameplay : AppSettingsData_Category<AppSettingsData_Gameplay>
    {
        public static object GetInstance()
        {
            return AppSettingsData_Category<AppSettingsData_Gameplay>.GetInstance();
        }
        
        [Header("View")]
        [ErryBox] 
        public SettingCheckbox invertHorizontalLook = new SettingCheckbox(
            "Gameplay/View/Invert Horizontal Look", 
            "When enabled, looking left results in looking right, and looking right results in looking left.", 
            null, false, false);
        
        [ErryBox] 
        public SettingCheckbox invertVerticalLook = new SettingCheckbox(
            "Gameplay/View/Invert Vertical Look", 
            "When enabled, looking up results in looking down, and looking down results in looking up.", 
            null, false, false);
        
        [ErryBox] 
        public SettingSlider joystickLookSensitivity = new SettingSlider(
            "Gameplay/View/Joystick Look Sensitivity", 
            "How much the controller's joystick movement is multiplied by the look speed.", 
            null, false, 6, 1, 50);
        
        [ErryBox] 
        public SettingSlider mouseLookSensitivity = new SettingSlider(
            "Gameplay/View/Mouse Look Sensitivity", 
            "How much the mouse movement is multiplied by the look speed.", 
            null, false, 6, 1, 50);
        
        [ErryBox] 
        public SettingSlider horizontalLookSpeed = new SettingSlider(
            "Gameplay/View/Horizontal Look Speed", 
            "How quickly the player turns when looking left and right.", 
            null, false, 6, 1, 50);
        
        [ErryBox] 
        public SettingSlider verticalLookSpeed = new SettingSlider(
            "Gameplay/View/Vertical Look Speed", 
            "How quickly the player turns when looking up and down.", 
            null, false, 6, 1, 50);
        
        [ErryBox] 
        public SettingSlider fieldOfView = new SettingSlider(
            "Gameplay/View/Field Of View", 
            "", 
            null, false, 80, 20, 120);
        
        [Header("Accessibility")]
        [ErryBox] 
        public SettingSlider colorBlindIntensity = new SettingSlider(
            "Gameplay/Accessibility/Color Blind Filter Intensity", 
            "Adjusts the strength of the color blind filter by shifting the target color values for the active color blind filter further apart.", 
            null, false, 0, 0, 200);
        
        [ErryBox] 
        public SettingSelector colorBlindFilter = new SettingSelector(
            "Gameplay/Accessibility/Color Blind Filter", 
            "Shifts certain color values so they are more distinguishable from one another. " +
            "This is intended to help people with color blindness, but can make parts of the game look distracting and unnatural. " +
            "Protanopia reduces sensitivity to reds, Deuteranopia reduces sensitivity to greens, and Tritanopia reduces sensitivity to blues.", 
            null, false, 0, Enum.GetNames(typeof(ColorBlindFilterOptions)).ToList());
        public enum ColorBlindFilterOptions { Off, Protanopia, Deuteranopia, Tritanopia, Custom }
        
        [ErryBox] 
        public SettingSelector colorBlindSymboling  = new SettingSelector(
            "Gameplay/Accessibility/Color Blind Symboling", 
            "Attempts to add symbols, icons, and patterns to distinguish between gameplay elements that normally relly on color for identification.", 
            null, false, 0, Enum.GetNames(typeof(ColorBlindSymbolingOptions)).ToList());
        public enum ColorBlindSymbolingOptions { Off, Minimal, Decreased, All }
        
        [ErryBox] 
        public SettingCheckbox dyslexicFriendlyFont = new SettingCheckbox(
            "Gameplay/Accessibility/Dyslexic Friendly Font", 
            "When enabled, as many text elements as possible will have their fonts replaced with one that is easier to distinguish the difference between letters. " +
            "Some text, such as on hand painted textures, will not be affected.", 
            null, false, false);
        
        [ErryBox] 
        public SettingSelector reduceStrobingEffects  = new SettingSelector(
            "Gameplay/Accessibility/Reduce Strobing Effects", 
            "Attempts to reduce sudden changes in visuals and light intensities, such as flickering effects or flashing lights. " +
            "The post-processing method renders several frames ahead to try to detect and smoothly fade-in sudden changes in visuals. " +
            "The hand-picked method is done by a developer manually marking certain effects as a 'flicker risk' and either replaces the effect or out right removes it.", 
            null, false, 0, Enum.GetNames(typeof(reduceStrobingEffectsOptions)).ToList());
        public enum reduceStrobingEffectsOptions { Off, PostProcessingOnly, HandPickedOnly, All }
        
        [ErryBox] 
        public SettingCheckbox screenReader = new SettingCheckbox(
            "Gameplay/Accessibility/Screen Reader", 
            "Reads aloud the text in menus and user interfaces when hovering over them via a Text-To-Speech (TTS) voice.", 
            null, false, false);
        
        [ErryBox] 
        public SettingCheckbox textChatReader = new SettingCheckbox(
            "Gameplay/Accessibility/Text Chat Reader", 
            "Reads aloud the text chat in multiplayer games via a Text-To-Speech (TTS) voice.",
            null, false, false);
    }
}