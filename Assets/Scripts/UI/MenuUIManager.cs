using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using ProtocolEMR.Core;
using ProtocolEMR.Core.Input;
using ProtocolEMR.Core.Settings;

namespace ProtocolEMR.UI
{
    public class MenuUIManager : MonoBehaviour
    {
        public static MenuUIManager Instance { get; private set; }

        [Header("Pause Menu")]
        [SerializeField] private Canvas pauseMenuCanvas;
        [SerializeField] private CanvasGroup pauseMenuGroup;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("Main Menu")]
        [SerializeField] private Canvas mainMenuCanvas;
        [SerializeField] private CanvasGroup mainMenuGroup;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsMainButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button exitButton;

        [Header("Settings Menu")]
        [SerializeField] private Canvas settingsMenuCanvas;
        [SerializeField] private CanvasGroup settingsMenuGroup;
        [SerializeField] private Button[] settingsTabs;
        [SerializeField] private Canvas[] settingsPanels;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;

        [Header("Confirmation Dialog")]
        [SerializeField] private Canvas confirmationCanvas;
        [SerializeField] private Text confirmationText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelConfirmButton;

        private bool isPauseMenuOpen = false;
        private bool isSettingsOpen = false;
        private float animationDuration = 0.3f;

        public event Action OnPauseMenuToggled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            SetupPauseMenu();
            SetupMainMenu();
            SetupSettingsMenu();

            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnPause += TogglePauseMenu;
            }

            if (pauseMenuCanvas != null)
            {
                pauseMenuCanvas.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnPause -= TogglePauseMenu;
            }
        }

        private void SetupPauseMenu()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumGame);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);

            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(OpenInventoryFromPause);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(ConfirmGoToMainMenu);

            if (quitButton != null)
                quitButton.onClick.AddListener(ConfirmQuit);
        }

        private void SetupMainMenu()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(StartNewGame);

            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(LoadGame);

            if (settingsMainButton != null)
                settingsMainButton.onClick.AddListener(OpenSettings);

            if (creditsButton != null)
                creditsButton.onClick.AddListener(ShowCredits);

            if (exitButton != null)
                exitButton.onClick.AddListener(ConfirmQuit);
        }

        private void SetupSettingsMenu()
        {
            if (settingsTabs != null)
            {
                for (int i = 0; i < settingsTabs.Length; i++)
                {
                    int tabIndex = i;
                    settingsTabs[i].onClick.AddListener(() => SwitchSettingsTab(tabIndex));
                }
            }

            if (applyButton != null)
                applyButton.onClick.AddListener(ApplySettings);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(CancelSettings);
        }

        public void TogglePauseMenu()
        {
            if (isPauseMenuOpen)
            {
                ClosePauseMenu();
            }
            else
            {
                OpenPauseMenu();
            }
        }

        public void OpenPauseMenu()
        {
            if (isPauseMenuOpen) return;

            isPauseMenuOpen = true;
            pauseMenuCanvas.enabled = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPaused(true);
            }

            StartCoroutine(AnimateCanvasOpen(pauseMenuGroup));
            OnPauseMenuToggled?.Invoke();
        }

        public void ClosePauseMenu()
        {
            if (!isPauseMenuOpen) return;

            isPauseMenuOpen = false;
            StartCoroutine(AnimateCanvasClose(pauseMenuGroup, () =>
            {
                pauseMenuCanvas.enabled = false;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetPaused(false);
                }
            }));
        }

        public void OpenSettings()
        {
            if (isSettingsOpen) return;

            isSettingsOpen = true;
            settingsMenuCanvas.enabled = true;
            StartCoroutine(AnimateCanvasOpen(settingsMenuGroup));
        }

        public void CloseSettings()
        {
            if (!isSettingsOpen) return;

            isSettingsOpen = false;
            StartCoroutine(AnimateCanvasClose(settingsMenuGroup, () =>
            {
                settingsMenuCanvas.enabled = false;
            }));
        }

        private void SwitchSettingsTab(int tabIndex)
        {
            if (settingsPanels != null && tabIndex < settingsPanels.Length)
            {
                for (int i = 0; i < settingsPanels.Length; i++)
                {
                    settingsPanels[i].enabled = (i == tabIndex);
                }
            }
        }

        private void ApplySettings()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SaveSettings();
            }
            CloseSettings();
        }

        private void CancelSettings()
        {
            CloseSettings();
        }

        private void ResumGame()
        {
            ClosePauseMenu();
        }

        private void OpenInventoryFromPause()
        {
            if (InventoryUIManager.Instance != null)
            {
                InventoryUIManager.Instance.OpenInventory();
            }
        }

        private void StartNewGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Game");
        }

        private void LoadGame()
        {
            Debug.Log("Load game - implement save system");
        }

        private void ShowCredits()
        {
            Debug.Log("Show credits");
        }

        private void ConfirmGoToMainMenu()
        {
            ShowConfirmationDialog("Return to main menu? Unsaved progress will be lost.", () =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            });
        }

        private void ConfirmQuit()
        {
            ShowConfirmationDialog("Quit game? Unsaved progress will be lost.", () =>
            {
                Time.timeScale = 1f;
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        private void ShowConfirmationDialog(string message, Action onConfirm)
        {
            if (confirmationCanvas != null)
            {
                confirmationCanvas.enabled = true;

                if (confirmationText != null)
                {
                    confirmationText.text = message;
                }

                if (confirmButton != null)
                {
                    confirmButton.onClick.RemoveAllListeners();
                    confirmButton.onClick.AddListener(() =>
                    {
                        confirmationCanvas.enabled = false;
                        onConfirm?.Invoke();
                    });
                }

                if (cancelConfirmButton != null)
                {
                    cancelConfirmButton.onClick.RemoveAllListeners();
                    cancelConfirmButton.onClick.AddListener(() =>
                    {
                        confirmationCanvas.enabled = false;
                    });
                }
            }
        }

        private IEnumerator AnimateCanvasOpen(CanvasGroup canvasGroup)
        {
            float elapsed = 0f;
            canvasGroup.alpha = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / animationDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private IEnumerator AnimateCanvasClose(CanvasGroup canvasGroup, Action onComplete = null)
        {
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / animationDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            onComplete?.Invoke();
        }
    }
}
