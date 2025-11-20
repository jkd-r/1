using UnityEngine;
using System;
using System.Collections.Generic;
using ProtocolEMR.Core.Settings;

namespace ProtocolEMR.UI
{
    [CreateAssetMenu(fileName = "UITheme", menuName = "ProtocolEMR/UI Theme")]
    public class UITheme : ScriptableObject
    {
        [Serializable]
        public class ColorSet
        {
            public Color primaryAccent;
            public Color secondaryAccent;
            public Color warningColor;
            public Color healthColor;
            public Color staminaColor;
            public Color damageColor;
            public Color textPrimary;
            public Color textSecondary;
            public Color backgroundPrimary;
            public Color backgroundSecondary;
        }

        public ColorSet defaultColors;
        public ColorSet protanopiaColors;
        public ColorSet deuteranopiaColors;
        public ColorSet tritanopiaColors;
        public ColorSet achromatopsiaColors;
    }

    public class UIThemeManager : MonoBehaviour
    {
        public static UIThemeManager Instance { get; private set; }

        [SerializeField] private UITheme currentTheme;

        private UITheme.ColorSet activeColorSet;
        private ColorblindMode activeColorblindMode = ColorblindMode.None;

        public event Action OnThemeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (SettingsManager.Instance != null)
            {
                var accessibility = SettingsManager.Instance.GetAccessibilitySettings();
                SetColorblindMode(accessibility.colorblindMode);
            }
            else
            {
                SetColorblindMode(ColorblindMode.None);
            }
        }

        public void SetColorblindMode(ColorblindMode mode)
        {
            if (activeColorblindMode == mode) return;

            activeColorblindMode = mode;

            if (currentTheme == null) return;

            activeColorSet = mode switch
            {
                ColorblindMode.Protanopia => currentTheme.protanopiaColors,
                ColorblindMode.Deuteranopia => currentTheme.deuteranopiaColors,
                ColorblindMode.Tritanopia => currentTheme.tritanopiaColors,
                _ => currentTheme.defaultColors
            };

            OnThemeChanged?.Invoke();
        }

        public Color GetPrimaryAccent() => activeColorSet.primaryAccent;
        public Color GetSecondaryAccent() => activeColorSet.secondaryAccent;
        public Color GetWarningColor() => activeColorSet.warningColor;
        public Color GetHealthColor() => activeColorSet.healthColor;
        public Color GetStaminaColor() => activeColorSet.staminaColor;
        public Color GetDamageColor() => activeColorSet.damageColor;
        public Color GetTextPrimary() => activeColorSet.textPrimary;
        public Color GetTextSecondary() => activeColorSet.textSecondary;
        public Color GetBackgroundPrimary() => activeColorSet.backgroundPrimary;
        public Color GetBackgroundSecondary() => activeColorSet.backgroundSecondary;

        public UITheme.ColorSet GetCurrentColorSet() => activeColorSet;
    }
}
