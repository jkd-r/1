using UnityEngine;
using UnityEngine.UI;
using System;
using ProtocolEMR.Core.Input;

namespace ProtocolEMR.UI
{
    public class KeybindingItemUI : MonoBehaviour
    {
        [SerializeField] private Text actionNameText;
        [SerializeField] private Text bindingText;
        [SerializeField] private Button rebindButton;
        [SerializeField] private Button resetButton;

        private string actionName;
        private bool isRebinding = false;

        private void Awake()
        {
            if (rebindButton != null)
                rebindButton.onClick.AddListener(OnRebindPressed);

            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetPressed);
        }

        private void OnDestroy()
        {
            if (rebindButton != null)
                rebindButton.onClick.RemoveListener(OnRebindPressed);

            if (resetButton != null)
                resetButton.onClick.RemoveListener(OnResetPressed);
        }

        public void Initialize(string action)
        {
            actionName = action;

            if (actionNameText != null)
                actionNameText.text = action;

            UpdateBindingDisplay();
        }

        private void UpdateBindingDisplay()
        {
            if (bindingText != null)
            {
                // This would get the actual binding from InputManager
                // For now, we'll just show a placeholder
                bindingText.text = "[Click to rebind]";
            }
        }

        private void OnRebindPressed()
        {
            if (InputManager.Instance == null) return;

            isRebinding = true;

            if (bindingText != null)
                bindingText.text = "Press any key...";

            if (rebindButton != null)
                rebindButton.interactable = false;

            // Start rebinding process
            InputManager.Instance.StartRebinding(actionName, 0, OnRebindComplete);
        }

        private void OnRebindComplete(bool success)
        {
            isRebinding = false;

            if (rebindButton != null)
                rebindButton.interactable = true;

            UpdateBindingDisplay();

            if (success)
            {
                Debug.Log($"Rebinded {actionName} successfully");
            }
        }

        private void OnResetPressed()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.ResetBindings();
                UpdateBindingDisplay();
            }
        }
    }
}
