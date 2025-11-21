using System.Collections;
using UnityEngine;
using ProtocolEMR.Core.Input;
using ProtocolEMR.Core.UI;

namespace ProtocolEMR.UI
{
    /// <summary>
    /// Central coordinator for all runtime UI. Ensures only one overlay is visible
    /// at a time, manages HUD visibility, cursor state, and subscribes to input
    /// events so gameplay systems have a single surface for UI concerns.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Primary Widgets")]
        [SerializeField] private HUDDisplay hudDisplay;
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private UnknownPhoneUI phoneUI;

        [Header("Minimap")]
        [SerializeField] private Transform defaultMinimapTarget;
        [SerializeField] private Camera minimapCamera;

        [Header("Behavior")]
        [SerializeField] private bool manageCursorState = true;
        [SerializeField] private bool hideHudWhenOverlayActive = true;

        private UIState currentState = UIState.Gameplay;
        private bool inputBound;
        private Coroutine inputRetryRoutine;

        public UIState CurrentState => currentState;
        public bool IsOverlayOpen => currentState != UIState.Gameplay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResolveReferences();
        }

        private void Start()
        {
            RegisterCallbacks();
            BindInputEvents();
            InitializeMinimap();
            RefreshState();
            ApplyStateEffects();
        }

        private void OnDestroy()
        {
            UnregisterCallbacks();
            UnbindInputEvents();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        #region Public API
        public void ToggleInventory()
        {
            if (mainMenu != null && (mainMenu.IsMainMenuOpen || mainMenu.IsPauseMenuOpen || mainMenu.IsSettingsOpen))
            {
                return;
            }

            if (phoneUI != null && phoneUI.IsPhoneOpen)
            {
                phoneUI.TogglePhone();
            }

            inventoryUI?.ToggleInventory();
        }

        public void TogglePauseMenu()
        {
            if (inventoryUI != null && inventoryUI.IsOpen)
            {
                inventoryUI.CloseInventory();
            }

            mainMenu?.TogglePauseMenu();
        }

        public void TogglePhoneUI()
        {
            if (mainMenu != null && (mainMenu.IsMainMenuOpen || mainMenu.IsPauseMenuOpen || mainMenu.IsSettingsOpen))
            {
                return;
            }

            phoneUI?.TogglePhone();
        }

        public void RegisterMinimapTarget(Transform target)
        {
            defaultMinimapTarget = target;
            hudDisplay?.SetMinimapTarget(defaultMinimapTarget);
        }

        public void RegisterMinimapCamera(Camera camera)
        {
            minimapCamera = camera;
            hudDisplay?.SetMinimapCamera(minimapCamera);
        }
        #endregion

        #region Initialization
        private void ResolveReferences()
        {
            if (hudDisplay == null)
            {
                hudDisplay = HUDDisplay.Instance ?? FindObjectOfType<HUDDisplay>(true);
            }

            if (mainMenu == null)
            {
                mainMenu = FindObjectOfType<MainMenu>(true);
            }

            if (inventoryUI == null)
            {
                inventoryUI = InventoryUI.Instance ?? FindObjectOfType<InventoryUI>(true);
            }

            if (dialogueUI == null)
            {
                dialogueUI = FindObjectOfType<DialogueUI>(true);
            }

            if (phoneUI == null)
            {
                phoneUI = FindObjectOfType<UnknownPhoneUI>(true);
            }
        }

        private void InitializeMinimap()
        {
            if (hudDisplay == null)
            {
                return;
            }

            if (minimapCamera != null)
            {
                hudDisplay.SetMinimapCamera(minimapCamera);
            }

            if (defaultMinimapTarget != null)
            {
                hudDisplay.SetMinimapTarget(defaultMinimapTarget);
            }
        }
        #endregion

        #region Event Wiring
        private void RegisterCallbacks()
        {
            ResolveReferences();

            if (inventoryUI != null)
            {
                inventoryUI.OnVisibilityChanged += HandleInventoryVisibilityChanged;
            }

            if (mainMenu != null)
            {
                mainMenu.OnPauseMenuStateChanged += HandlePauseMenuChanged;
                mainMenu.OnSettingsMenuStateChanged += HandleSettingsMenuChanged;
                mainMenu.OnMainMenuStateChanged += HandleMainMenuChanged;
            }

            if (dialogueUI != null)
            {
                dialogueUI.OnDialogueVisibilityChanged += HandleDialogueVisibilityChanged;
            }

            if (phoneUI != null)
            {
                phoneUI.OnPhoneVisibilityChanged += HandlePhoneVisibilityChanged;
            }
        }

        private void UnregisterCallbacks()
        {
            if (inventoryUI != null)
            {
                inventoryUI.OnVisibilityChanged -= HandleInventoryVisibilityChanged;
            }

            if (mainMenu != null)
            {
                mainMenu.OnPauseMenuStateChanged -= HandlePauseMenuChanged;
                mainMenu.OnSettingsMenuStateChanged -= HandleSettingsMenuChanged;
                mainMenu.OnMainMenuStateChanged -= HandleMainMenuChanged;
            }

            if (dialogueUI != null)
            {
                dialogueUI.OnDialogueVisibilityChanged -= HandleDialogueVisibilityChanged;
            }

            if (phoneUI != null)
            {
                phoneUI.OnPhoneVisibilityChanged -= HandlePhoneVisibilityChanged;
            }
        }

        private void BindInputEvents()
        {
            if (inputBound)
            {
                return;
            }

            if (InputManager.Instance == null)
            {
                if (inputRetryRoutine == null)
                {
                    inputRetryRoutine = StartCoroutine(BindInputWhenAvailable());
                }
                return;
            }

            InputManager.Instance.OnInventory += ToggleInventory;
            InputManager.Instance.OnPause += TogglePauseMenu;
            InputManager.Instance.OnPhone += TogglePhoneUI;
            inputBound = true;
        }

        private void UnbindInputEvents()
        {
            if (inputRetryRoutine != null)
            {
                StopCoroutine(inputRetryRoutine);
                inputRetryRoutine = null;
            }

            if (!inputBound || InputManager.Instance == null)
            {
                inputBound = false;
                return;
            }

            InputManager.Instance.OnInventory -= ToggleInventory;
            InputManager.Instance.OnPause -= TogglePauseMenu;
            InputManager.Instance.OnPhone -= TogglePhoneUI;
            inputBound = false;
        }

        private IEnumerator BindInputWhenAvailable()
        {
            while (InputManager.Instance == null)
            {
                yield return null;
            }

            inputRetryRoutine = null;
            BindInputEvents();
        }
        #endregion

        #region Event Handlers
        private void HandleInventoryVisibilityChanged(bool visible)
        {
            if (visible)
            {
                mainMenu?.ClosePauseMenu();
                SetState(UIState.Inventory);
            }
            else
            {
                RefreshState();
            }
        }

        private void HandlePauseMenuChanged(bool open)
        {
            if (open)
            {
                inventoryUI?.CloseInventory();
                SetState(UIState.PauseMenu);
            }
            else
            {
                RefreshState();
            }
        }

        private void HandleSettingsMenuChanged(bool open)
        {
            if (open)
            {
                SetState(UIState.Settings);
            }
            else
            {
                RefreshState();
            }
        }

        private void HandleMainMenuChanged(bool open)
        {
            if (open)
            {
                inventoryUI?.CloseInventory();
                SetState(UIState.MainMenu);
            }
            else
            {
                RefreshState();
            }
        }

        private void HandleDialogueVisibilityChanged(bool visible)
        {
            if (visible)
            {
                if (phoneUI != null && phoneUI.IsPhoneOpen)
                {
                    return;
                }

                if (!HasHigherPriorityOverlay())
                {
                    SetState(UIState.Dialogue);
                }
            }
            else
            {
                RefreshState();
            }
        }

        private void HandlePhoneVisibilityChanged(bool visible)
        {
            if (visible)
            {
                inventoryUI?.CloseInventory();
                if (!HasHigherPriorityOverlay())
                {
                    SetState(UIState.Phone);
                }
            }
            else
            {
                RefreshState();
            }
        }
        #endregion

        #region State Management
        private void RefreshState()
        {
            if (mainMenu != null && mainMenu.IsSettingsOpen)
            {
                SetState(UIState.Settings);
                return;
            }

            if (mainMenu != null && mainMenu.IsMainMenuOpen)
            {
                SetState(UIState.MainMenu);
                return;
            }

            if (mainMenu != null && mainMenu.IsPauseMenuOpen)
            {
                SetState(UIState.PauseMenu);
                return;
            }

            if (inventoryUI != null && inventoryUI.IsOpen)
            {
                SetState(UIState.Inventory);
                return;
            }

            if (phoneUI != null && phoneUI.IsPhoneOpen)
            {
                SetState(UIState.Phone);
                return;
            }

            if (dialogueUI != null && dialogueUI.IsVisible)
            {
                SetState(UIState.Dialogue);
                return;
            }

            SetState(UIState.Gameplay);
        }

        private void SetState(UIState newState)
        {
            if (currentState == newState)
            {
                return;
            }

            currentState = newState;
            ApplyStateEffects();
        }

        private bool HasHigherPriorityOverlay()
        {
            if (mainMenu != null && (mainMenu.IsMainMenuOpen || mainMenu.IsPauseMenuOpen || mainMenu.IsSettingsOpen))
            {
                return true;
            }

            if (inventoryUI != null && inventoryUI.IsOpen)
            {
                return true;
            }

            return false;
        }

        private void ApplyStateEffects()
        {
            UpdateCursorState();
            UpdateHudVisibility();
        }

        private void UpdateCursorState()
        {
            if (!manageCursorState)
            {
                return;
            }

            bool shouldUnlock = currentState != UIState.Gameplay && currentState != UIState.Dialogue;
            Cursor.visible = shouldUnlock;
            Cursor.lockState = shouldUnlock ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void UpdateHudVisibility()
        {
            if (!hideHudWhenOverlayActive || hudDisplay == null)
            {
                return;
            }

            bool showHud = currentState == UIState.Gameplay || currentState == UIState.Dialogue || currentState == UIState.Phone;
            hudDisplay.SetHUDVisible(showHud);
        }
        #endregion
    }

    public enum UIState
    {
        Gameplay,
        Inventory,
        PauseMenu,
        Settings,
        Dialogue,
        Phone,
        MainMenu
    }
}
