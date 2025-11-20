using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using ProtocolEMR.Core.Settings;
using ProtocolEMR.Core.Input;

namespace ProtocolEMR.UI
{
    public class SettingsPanelUI : MonoBehaviour
    {
        [Header("Graphics Settings")]
        [SerializeField] private Slider resolutionSlider;
        [SerializeField] private Slider fpsCapSlider;
        [SerializeField] private Slider textureQualitySlider;
        [SerializeField] private Slider shadowQualitySlider;
        [SerializeField] private Slider effectsQualitySlider;
        [SerializeField] private Toggle rayTracingToggle;
        [SerializeField] private Slider fovSlider;
        [SerializeField] private Toggle motionBlurToggle;
        [SerializeField] private Toggle depthOfFieldToggle;

        [Header("Audio Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider voiceVolumeSlider;
        [SerializeField] private Toggle spatialAudioToggle;
        [SerializeField] private Toggle subtitlesToggle;

        [Header("Gameplay Settings")]
        [SerializeField] private Dropdown difficultyDropdown;
        [SerializeField] private Slider hudOpacitySlider;
        [SerializeField] private Toggle objectiveMarkersToggle;
        [SerializeField] private Dropdown crosshairStyleDropdown;
        [SerializeField] private Slider cameraSensitivitySlider;
        [SerializeField] private Toggle sprintToggleToggle;
        [SerializeField] private Toggle invertYAxisToggle;

        [Header("Accessibility Settings")]
        [SerializeField] private Dropdown colorblindModeDropdown;
        [SerializeField] private Toggle highContrastToggle;
        [SerializeField] private Slider uiScaleSlider;

        [Header("Keybinding Panel")]
        [SerializeField] private Transform keybindingContainer;
        [SerializeField] private GameObject keybindingItemPrefab;
        [SerializeField] private Button rebindAllButton;
        [SerializeField] private Button resetAllButton;

        [Header("Text Display")]
        [SerializeField] private Text currentResolutionText;
        [SerializeField] private Text currentFpsText;
        [SerializeField] private Text masterVolumeText;
        [SerializeField] private Text hudOpacityText;
        [SerializeField] private Text cameraSensitivityText;

        private Dictionary<string, KeybindingItemUI> keybindingItems;
        private GameSettings currentSettings;

        private void Awake()
        {
            keybindingItems = new Dictionary<string, KeybindingItemUI>();
        }

        private void Start()
        {
            LoadCurrentSettings();
            SetupUICallbacks();
            CreateKeybindingItems();
        }

        private void LoadCurrentSettings()
        {
            if (SettingsManager.Instance == null) return;

            var graphics = SettingsManager.Instance.GetGraphicsSettings();
            var audio = SettingsManager.Instance.GetAudioSettings();
            var gameplay = SettingsManager.Instance.GetGameplaySettings();
            var accessibility = SettingsManager.Instance.GetAccessibilitySettings();

            // Load graphics settings
            if (fovSlider != null)
                fovSlider.value = accessibility.fieldOfView;

            if (motionBlurToggle != null)
                motionBlurToggle.isOn = accessibility.enableMotionBlur;

            // Load audio settings
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = audio.masterVolume * 100;

            if (musicVolumeSlider != null)
                musicVolumeSlider.value = audio.musicVolume * 100;

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = audio.sfxVolume * 100;

            if (voiceVolumeSlider != null)
                voiceVolumeSlider.value = audio.voiceVolume * 100;

            // Load gameplay settings
            if (hudOpacitySlider != null)
                hudOpacitySlider.value = gameplay.hudOpacity * 100;

            if (objectiveMarkersToggle != null)
                objectiveMarkersToggle.isOn = gameplay.showObjectiveMarkers;

            if (cameraSensitivitySlider != null)
                cameraSensitivitySlider.value = gameplay.mouseSensitivity;

            // Load accessibility settings
            if (colorblindModeDropdown != null)
                colorblindModeDropdown.value = (int)accessibility.colorblindMode;
        }

        private void SetupUICallbacks()
        {
            // Graphics callbacks
            if (fovSlider != null)
                fovSlider.onValueChanged.AddListener(OnFOVChanged);

            if (motionBlurToggle != null)
                motionBlurToggle.onValueChanged.AddListener(OnMotionBlurChanged);

            // Audio callbacks
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            if (voiceVolumeSlider != null)
                voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);

            // Gameplay callbacks
            if (hudOpacitySlider != null)
                hudOpacitySlider.onValueChanged.AddListener(OnHUDOpacityChanged);

            if (cameraSensitivitySlider != null)
                cameraSensitivitySlider.onValueChanged.AddListener(OnCameraSensitivityChanged);

            if (objectiveMarkersToggle != null)
                objectiveMarkersToggle.onValueChanged.AddListener(OnObjectiveMarkersChanged);

            // Accessibility callbacks
            if (colorblindModeDropdown != null)
                colorblindModeDropdown.onValueChanged.AddListener(OnColorblindModeChanged);

            if (rebindAllButton != null)
                rebindAllButton.onClick.AddListener(OnRebindAll);

            if (resetAllButton != null)
                resetAllButton.onClick.AddListener(OnResetAll);
        }

        private void CreateKeybindingItems()
        {
            if (InputManager.Instance == null || keybindingContainer == null)
                return;

            var actions = new string[]
            {
                "Move Forward",
                "Move Left",
                "Move Backward",
                "Move Right",
                "Sprint",
                "Crouch",
                "Jump",
                "Interact",
                "Inventory",
                "Pause",
                "Fire",
                "Aim"
            };

            foreach (string actionName in actions)
            {
                GameObject itemObj = Instantiate(keybindingItemPrefab, keybindingContainer);
                KeybindingItemUI item = itemObj.GetComponent<KeybindingItemUI>();
                if (item != null)
                {
                    item.Initialize(actionName);
                    keybindingItems[actionName] = item;
                }
            }
        }

        private void OnFOVChanged(float value)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetFieldOfView(value);
            }
        }

        private void OnMotionBlurChanged(bool value)
        {
            var accessibility = SettingsManager.Instance?.GetAccessibilitySettings();
            if (accessibility != null)
            {
                accessibility.enableMotionBlur = value;
                SettingsManager.Instance?.SetAccessibilitySettings(accessibility);
            }
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (masterVolumeText != null)
                masterVolumeText.text = $"{value:F0}%";

            AudioListener.volume = value / 100f;
        }

        private void OnMusicVolumeChanged(float value) { }
        private void OnSFXVolumeChanged(float value) { }
        private void OnVoiceVolumeChanged(float value) { }

        private void OnHUDOpacityChanged(float value)
        {
            if (hudOpacityText != null)
                hudOpacityText.text = $"{value:F0}%";

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.SetHUDOpacity(value / 100f);
            }
        }

        private void OnCameraSensitivityChanged(float value)
        {
            if (cameraSensitivityText != null)
                cameraSensitivityText.text = value.ToString("F2");

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetMouseSensitivity(value);
            }
        }

        private void OnObjectiveMarkersChanged(bool value) { }

        private void OnColorblindModeChanged(int value)
        {
            ColorblindMode mode = (ColorblindMode)value;
            ApplyColorblindMode(mode);
        }

        private void OnRebindAll()
        {
            Debug.Log("Rebind all keybindings");
        }

        private void OnResetAll()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.ResetBindings();
                Debug.Log("Reset all keybindings to defaults");
            }
        }

        private void ApplyColorblindMode(ColorblindMode mode)
        {
            // This would apply colorblind filters to the UI
            // Implementation depends on your UI color scheme system
            Debug.Log($"Applied colorblind mode: {mode}");
        }
    }
}
