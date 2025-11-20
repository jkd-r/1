using UnityEngine;
using System;
using System.IO;

namespace ProtocolEMR.Core.Settings
{
    /// <summary>
    /// Centralized settings management system for Protocol EMR.
    /// Handles graphics, audio, gameplay, and accessibility settings with JSON persistence.
    /// Settings are saved to disk and automatically loaded on game start.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        private GameSettings currentSettings;
        private string settingsPath;

        public event Action OnSettingsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            settingsPath = Path.Combine(Application.persistentDataPath, "game_settings.json");
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    currentSettings = JsonUtility.FromJson<GameSettings>(json);
                    Debug.Log($"Settings loaded from: {settingsPath}");
                }
                else
                {
                    currentSettings = GameSettings.GetDefaults();
                    Debug.Log("Using default settings");
                }

                ApplySettings();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load settings: {e.Message}");
                currentSettings = GameSettings.GetDefaults();
            }
        }

        public void SaveSettings()
        {
            try
            {
                string json = JsonUtility.ToJson(currentSettings, true);
                File.WriteAllText(settingsPath, json);
                Debug.Log($"Settings saved to: {settingsPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save settings: {e.Message}");
            }
        }

        public void ApplySettings()
        {
            ApplyGraphicsSettings();
            ApplyAudioSettings();
            ApplyGameplaySettings();
            ApplyAccessibilitySettings();

            OnSettingsChanged?.Invoke();
        }

        private void ApplyGraphicsSettings()
        {
            QualitySettings.SetQualityLevel((int)currentSettings.graphics.qualityPreset);
            Screen.SetResolution(currentSettings.graphics.resolutionWidth, currentSettings.graphics.resolutionHeight, currentSettings.graphics.fullscreen);
            QualitySettings.vSyncCount = currentSettings.graphics.vsync ? 1 : 0;
            Application.targetFrameRate = currentSettings.graphics.targetFrameRate;
        }

        private void ApplyAudioSettings()
        {
            AudioListener.volume = currentSettings.audio.masterVolume;
        }

        private void ApplyGameplaySettings()
        {
        }

        private void ApplyAccessibilitySettings()
        {
        }

        public GraphicsSettings GetGraphicsSettings() => currentSettings.graphics;
        public void SetGraphicsSettings(GraphicsSettings settings)
        {
            currentSettings.graphics = settings;
            ApplyGraphicsSettings();
            SaveSettings();
        }

        public AudioSettings GetAudioSettings() => currentSettings.audio;
        public void SetAudioSettings(AudioSettings settings)
        {
            currentSettings.audio = settings;
            ApplyAudioSettings();
            SaveSettings();
        }

        public GameplaySettings GetGameplaySettings() => currentSettings.gameplay;
        public void SetGameplaySettings(GameplaySettings settings)
        {
            currentSettings.gameplay = settings;
            ApplyGameplaySettings();
            SaveSettings();
        }

        public AccessibilitySettings GetAccessibilitySettings() => currentSettings.accessibility;
        public void SetAccessibilitySettings(AccessibilitySettings settings)
        {
            currentSettings.accessibility = settings;
            ApplyAccessibilitySettings();
            SaveSettings();
        }

        public float GetMouseSensitivity() => currentSettings.gameplay.mouseSensitivity;
        public void SetMouseSensitivity(float sensitivity)
        {
            currentSettings.gameplay.mouseSensitivity = sensitivity;
            SaveSettings();
        }

        public float GetFieldOfView() => currentSettings.accessibility.fieldOfView;
        public void SetFieldOfView(float fov)
        {
            currentSettings.accessibility.fieldOfView = fov;
            SaveSettings();
        }

        public bool IsCameraBobEnabled() => currentSettings.accessibility.enableCameraBob;
        public void SetCameraBobEnabled(bool enabled)
        {
            currentSettings.accessibility.enableCameraBob = enabled;
            SaveSettings();
        }

        public void ResetToDefaults()
        {
            currentSettings = GameSettings.GetDefaults();
            ApplySettings();
            SaveSettings();
        }
    }

    [Serializable]
    public class GameSettings
    {
        public GraphicsSettings graphics;
        public AudioSettings audio;
        public GameplaySettings gameplay;
        public AccessibilitySettings accessibility;

        public static GameSettings GetDefaults()
        {
            return new GameSettings
            {
                graphics = new GraphicsSettings
                {
                    qualityPreset = QualityPreset.High,
                    resolutionWidth = 1920,
                    resolutionHeight = 1080,
                    fullscreen = true,
                    vsync = true,
                    targetFrameRate = 60
                },
                audio = new AudioSettings
                {
                    masterVolume = 1.0f,
                    musicVolume = 0.7f,
                    sfxVolume = 0.8f,
                    voiceVolume = 1.0f
                },
                gameplay = new GameplaySettings
                {
                    mouseSensitivity = 1.0f,
                    difficulty = Difficulty.Normal,
                    hudOpacity = 1.0f,
                    showObjectiveMarkers = true
                },
                accessibility = new AccessibilitySettings
                {
                    colorblindMode = ColorblindMode.None,
                    enableMotionBlur = true,
                    fieldOfView = 90f,
                    cameraShakeIntensity = 1.0f,
                    enableCameraBob = true,
                    highContrastMode = false,
                    uiScale = 1.0f,
                    enableSubtitles = true,
                    subtitleSize = 1
                }
            };
        }
    }

    [Serializable]
    public class GraphicsSettings
    {
        public QualityPreset qualityPreset;
        public int resolutionWidth;
        public int resolutionHeight;
        public bool fullscreen;
        public bool vsync;
        public int targetFrameRate;
    }

    [Serializable]
    public class AudioSettings
    {
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public float voiceVolume;
    }

    [Serializable]
    public class GameplaySettings
    {
        public float mouseSensitivity;
        public Difficulty difficulty;
        public float hudOpacity;
        public bool showObjectiveMarkers;
    }

    [Serializable]
    public class AccessibilitySettings
    {
        public ColorblindMode colorblindMode;
        public bool enableMotionBlur;
        public float fieldOfView;
        public float cameraShakeIntensity;
        public bool enableCameraBob;
        public bool highContrastMode;
        public float uiScale;
        public bool enableSubtitles;
        public int subtitleSize;
    }

    public enum QualityPreset
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3,
        Custom = 4
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard,
        Extreme
    }

    public enum ColorblindMode
    {
        None,
        Protanopia,
        Deuteranopia,
        Tritanopia,
        Achromatopsia
    }

    public enum SubtitleSize
    {
        Small = 0,
        Medium = 1,
        Large = 2
    }
}
