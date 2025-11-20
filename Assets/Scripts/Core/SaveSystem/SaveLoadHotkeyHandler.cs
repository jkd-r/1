using UnityEngine;
using UnityEngine.InputSystem;

namespace ProtocolEMR.Core.SaveSystem
{
    /// <summary>
    /// Handles keyboard hotkeys for quick save (F5) and quick load (F9).
    /// Provides instant save/load functionality without opening menus.
    /// </summary>
    public class SaveLoadHotkeyHandler : MonoBehaviour
    {
        public static SaveLoadHotkeyHandler Instance { get; private set; }

        [Header("Hotkey Settings")]
        [SerializeField] private bool enableQuickSave = true;
        [SerializeField] private bool enableQuickLoad = true;
        [SerializeField] private bool requireConfirmationForQuickLoad = true;

        private bool quickLoadConfirmationPending;
        private float confirmationTimeout = 5f;
        private float confirmationTimer;

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

        private void Update()
        {
            HandleHotkeys();
            HandleConfirmationTimeout();
        }

        private void HandleHotkeys()
        {
            // F5 - Quick Save
            if (enableQuickSave && Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
            {
                PerformQuickSave();
            }

            // F9 - Quick Load
            if (enableQuickLoad && Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
            {
                PerformQuickLoad();
            }

            // F6 - Create Checkpoint
            if (Keyboard.current != null && Keyboard.current.f6Key.wasPressedThisFrame)
            {
                CreateCheckpoint();
            }
        }

        private void HandleConfirmationTimeout()
        {
            if (quickLoadConfirmationPending)
            {
                confirmationTimer += Time.unscaledDeltaTime;
                
                if (confirmationTimer >= confirmationTimeout)
                {
                    quickLoadConfirmationPending = false;
                    Debug.Log("Quick load confirmation expired");
                }
            }
        }

        /// <summary>
        /// Performs a quick save to the dedicated quick-save slot.
        /// </summary>
        private void PerformQuickSave()
        {
            if (SaveGameManager.Instance == null)
            {
                Debug.LogWarning("SaveGameManager not found");
                return;
            }

            if (SaveGameManager.Instance.IsSaving)
            {
                Debug.LogWarning("Save already in progress");
                return;
            }

            if (Core.ProfileManager.Instance?.CurrentProfile == null)
            {
                Debug.LogWarning("No active profile. Cannot quick save.");
                ShowNotification("No active profile. Please create a profile first.");
                return;
            }

            SaveGameManager.Instance.QuickSave();
            Debug.Log("Quick save performed (F5)");
        }

        /// <summary>
        /// Performs a quick load from the most recent save.
        /// </summary>
        private void PerformQuickLoad()
        {
            if (SaveGameManager.Instance == null)
            {
                Debug.LogWarning("SaveGameManager not found");
                return;
            }

            if (SaveGameManager.Instance.IsLoading)
            {
                Debug.LogWarning("Load already in progress");
                return;
            }

            if (Core.ProfileManager.Instance?.CurrentProfile == null)
            {
                Debug.LogWarning("No active profile. Cannot quick load.");
                ShowNotification("No active profile. Please select a profile first.");
                return;
            }

            if (requireConfirmationForQuickLoad && !quickLoadConfirmationPending)
            {
                quickLoadConfirmationPending = true;
                confirmationTimer = 0f;
                Debug.Log("Press F9 again within 5 seconds to confirm quick load");
                ShowNotification("Press F9 again to confirm load");
                return;
            }

            quickLoadConfirmationPending = false;
            SaveGameManager.Instance.QuickLoad();
            Debug.Log("Quick load performed (F9)");
        }

        /// <summary>
        /// Creates a checkpoint save.
        /// </summary>
        private void CreateCheckpoint()
        {
            if (SaveTriggerSystem.Instance != null)
            {
                SaveTriggerSystem.Instance.CreateCheckpoint("Manual Checkpoint");
                Debug.Log("Checkpoint created (F6)");
            }
        }

        /// <summary>
        /// Enables or disables quick save functionality.
        /// </summary>
        public void SetQuickSaveEnabled(bool enabled)
        {
            enableQuickSave = enabled;
        }

        /// <summary>
        /// Enables or disables quick load functionality.
        /// </summary>
        public void SetQuickLoadEnabled(bool enabled)
        {
            enableQuickLoad = enabled;
        }

        /// <summary>
        /// Sets whether quick load requires confirmation.
        /// </summary>
        public void SetQuickLoadConfirmation(bool required)
        {
            requireConfirmationForQuickLoad = required;
        }

        private void ShowNotification(string message)
        {
            if (UI.NotificationManager.Instance != null)
            {
                UI.NotificationManager.Instance.ShowNotification(message);
            }
        }
    }
}
