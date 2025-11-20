using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.IO;

namespace ProtocolEMR.Core.Input
{
    /// <summary>
    /// Centralized input handling system for Protocol EMR.
    /// Manages input actions, rebinding, and persistence of custom key bindings.
    /// Supports both keyboard/mouse and gamepad input with remappable controls.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Input Settings")]
        [SerializeField] private bool enableHoldToSprint = true;
        [SerializeField] private bool enableHoldToCrouch = true;

        private PlayerInputActions inputActions;
        private string bindingsPath;

        public PlayerInputActions InputActions => inputActions;
        public bool EnableHoldToSprint { get => enableHoldToSprint; set => enableHoldToSprint = value; }
        public bool EnableHoldToCrouch { get => enableHoldToCrouch; set => enableHoldToCrouch = value; }

        public event Action<Vector2> OnMove;
        public event Action OnSprintPressed;
        public event Action OnSprintReleased;
        public event Action OnCrouchPressed;
        public event Action OnCrouchReleased;
        public event Action OnJump;
        public event Action<Vector2> OnLook;
        public event Action OnInteract;
        public event Action OnInventory;
        public event Action OnPhone;
        public event Action OnPause;
        public event Action OnFire;
        public event Action OnFireReleased;
        public event Action OnAimPressed;
        public event Action OnAimReleased;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            inputActions = new PlayerInputActions();
            bindingsPath = Path.Combine(Application.persistentDataPath, "input_bindings.json");

            LoadBindings();
            SetupInputCallbacks();
        }

        private void OnEnable()
        {
            inputActions?.Enable();
        }

        private void OnDisable()
        {
            inputActions?.Disable();
        }

        private void SetupInputCallbacks()
        {
            inputActions.Movement.Move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
            inputActions.Movement.Move.canceled += ctx => OnMove?.Invoke(Vector2.zero);

            inputActions.Movement.Sprint.performed += ctx => OnSprintPressed?.Invoke();
            inputActions.Movement.Sprint.canceled += ctx => OnSprintReleased?.Invoke();

            inputActions.Movement.Crouch.performed += ctx => OnCrouchPressed?.Invoke();
            inputActions.Movement.Crouch.canceled += ctx => OnCrouchReleased?.Invoke();

            inputActions.Movement.Jump.performed += ctx => OnJump?.Invoke();

            inputActions.Look.MouseLook.performed += ctx => OnLook?.Invoke(ctx.ReadValue<Vector2>());

            inputActions.Interact.Use.performed += ctx => OnInteract?.Invoke();

            inputActions.UI.Inventory.performed += ctx => OnInventory?.Invoke();
            inputActions.UI.Phone.performed += ctx => OnPhone?.Invoke();
            inputActions.UI.Pause.performed += ctx => OnPause?.Invoke();

            inputActions.Combat.Fire.performed += ctx => OnFire?.Invoke();
            inputActions.Combat.Fire.canceled += ctx => OnFireReleased?.Invoke();

            inputActions.Combat.AimDownSights.performed += ctx => OnAimPressed?.Invoke();
            inputActions.Combat.AimDownSights.canceled += ctx => OnAimReleased?.Invoke();
        }

        public void SaveBindings()
        {
            try
            {
                string json = inputActions.SaveBindingOverridesAsJson();
                File.WriteAllText(bindingsPath, json);
                Debug.Log($"Input bindings saved to: {bindingsPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save input bindings: {e.Message}");
            }
        }

        public void LoadBindings()
        {
            try
            {
                if (File.Exists(bindingsPath))
                {
                    string json = File.ReadAllText(bindingsPath);
                    inputActions.LoadBindingOverridesFromJson(json);
                    Debug.Log($"Input bindings loaded from: {bindingsPath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load input bindings: {e.Message}");
            }
        }

        public void ResetBindings()
        {
            inputActions.RemoveAllBindingOverrides();
            if (File.Exists(bindingsPath))
            {
                File.Delete(bindingsPath);
            }
            Debug.Log("Input bindings reset to defaults");
        }

        public void StartRebinding(string actionName, int bindingIndex, Action<bool> onComplete)
        {
            InputAction action = inputActions.FindAction(actionName);
            if (action == null)
            {
                Debug.LogError($"Action '{actionName}' not found");
                onComplete?.Invoke(false);
                return;
            }

            action.Disable();

            var rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .OnComplete(operation =>
                {
                    action.Enable();
                    operation.Dispose();
                    SaveBindings();
                    onComplete?.Invoke(true);
                })
                .OnCancel(operation =>
                {
                    action.Enable();
                    operation.Dispose();
                    onComplete?.Invoke(false);
                });

            rebindOperation.Start();
        }

        public void EnableMovementInput(bool enable)
        {
            if (enable)
                inputActions.Movement.Enable();
            else
                inputActions.Movement.Disable();
        }

        public void EnableLookInput(bool enable)
        {
            if (enable)
                inputActions.Look.Enable();
            else
                inputActions.Look.Disable();
        }

        public void EnableCombatInput(bool enable)
        {
            if (enable)
                inputActions.Combat.Enable();
            else
                inputActions.Combat.Disable();
        }

        public void EnableUIInput(bool enable)
        {
            if (enable)
                inputActions.UI.Enable();
            else
                inputActions.UI.Disable();
        }

        public Vector2 GetMovementInput()
        {
            return inputActions.Movement.Move.ReadValue<Vector2>();
        }

        public Vector2 GetLookInput()
        {
            return inputActions.Look.MouseLook.ReadValue<Vector2>();
        }

        public bool IsSprintPressed()
        {
            return inputActions.Movement.Sprint.IsPressed();
        }

        public bool IsCrouchPressed()
        {
            return inputActions.Movement.Crouch.IsPressed();
        }

        public bool IsFirePressed()
        {
            return inputActions.Combat.Fire.IsPressed();
        }

        public bool IsAimPressed()
        {
            return inputActions.Combat.AimDownSights.IsPressed();
        }
    }
}
