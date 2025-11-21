using UnityEngine;
using UnityEngine.InputSystem;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Implementation of the input service.
    /// </summary>
    public class InputService : MonoBehaviour, IInputService
    {
        [Header("Input Configuration")]
        [SerializeField] private InputActionAsset inputActions;

        private InputActionMap navigationActionMap;
        private InputActionMap tabletActionMap;
        private InputActionMap pauseActionMap;

        public InputActionAsset InputActions => inputActions;

        public event System.Action<string> OnActionTriggered;

        private void Awake()
        {
            Debug.Log("[InputService] Awake - Initializing input system...");

            if (inputActions == null)
            {
                Debug.LogWarning("[InputService] InputActions asset not assigned! Attempting to find it...");
                inputActions = Resources.Load<InputActionAsset>("InputActions");
                Debug.Log($"[InputService] InputActions asset loaded from Resources: {inputActions != null}");
            }
            else
            {
                Debug.Log("[InputService] InputActions asset was assigned in Inspector");
            }

            if (inputActions != null)
            {
                Debug.Log($"[InputService] Available action maps in asset:");
                foreach (var actionMap in inputActions.actionMaps)
                {
                    Debug.Log($"[InputService] - {actionMap.name}");
                }

                navigationActionMap = inputActions.FindActionMap("Navigation");
                tabletActionMap = inputActions.FindActionMap("Tablet");
                pauseActionMap = inputActions.FindActionMap("Pause");

                Debug.Log($"[InputService] Navigation action map found: {navigationActionMap != null}");
                Debug.Log($"[InputService] Tablet action map found: {tabletActionMap != null}");
                Debug.Log($"[InputService] Pause action map found: {pauseActionMap != null}");
            }
            else
            {
                Debug.LogError("[InputService] InputActions asset not found! Input will not work.");
            }
        }

        private void OnEnable()
        {
            EnableNavigation();
        }

        private void OnDisable()
        {
            DisableAllActionMaps();
        }

        public void EnableActionMap(string actionMapName)
        {
            var actionMap = inputActions?.FindActionMap(actionMapName);
            if (actionMap != null)
            {
                actionMap.Enable();
            }
        }

        public void DisableActionMap(string actionMapName)
        {
            var actionMap = inputActions?.FindActionMap(actionMapName);
            if (actionMap != null)
            {
                actionMap.Disable();
            }
        }

        public void EnableNavigation()
        {
            Debug.Log("[InputService] EnableNavigation called");
            Debug.Log($"[InputService] Navigation action map is null: {navigationActionMap == null}");

            navigationActionMap?.Enable();
            tabletActionMap?.Disable();
            pauseActionMap?.Enable();

            Debug.Log("[InputService] Navigation action map enabled");

            // List available actions in Navigation map
            if (navigationActionMap != null)
            {
                Debug.Log("[InputService] Available actions in Navigation map:");
                foreach (var action in navigationActionMap.actions)
                {
                    Debug.Log($"[InputService] - {action.name}");
                }
            }
        }

        public void DisableNavigation()
        {
            navigationActionMap?.Disable();
        }

        public void EnableTablet()
        {
            navigationActionMap?.Disable();
            tabletActionMap?.Enable();
            pauseActionMap?.Enable();
        }

        public void DisableTablet()
        {
            tabletActionMap?.Disable();
        }

        public void EnablePause()
        {
            pauseActionMap?.Enable();
        }

        public void DisablePause()
        {
            pauseActionMap?.Disable();
        }

        public void SetCursorState(bool locked, bool visible)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = visible;
        }

        public T GetActionValue<T>(string actionName) where T : struct
        {
            var action = inputActions?.FindAction(actionName);
            if (action != null)
            {
                return action.ReadValue<T>();
            }
            return default(T);
        }

        public bool WasPressedThisFrame(string actionName)
        {
            var action = inputActions?.FindAction(actionName);
            return action != null && action.WasPressedThisFrame();
        }

        public bool IsHeld(string actionName)
        {
            var action = inputActions?.FindAction(actionName);
            return action != null && action.IsPressed();
        }

        public bool WasReleasedThisFrame(string actionName)
        {
            var action = inputActions?.FindAction(actionName);
            return action != null && action.WasReleasedThisFrame();
        }

        private void DisableAllActionMaps()
        {
            navigationActionMap?.Disable();
            tabletActionMap?.Disable();
            pauseActionMap?.Disable();
        }
    }
}