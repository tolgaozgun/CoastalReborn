using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Interfaces
{
    /// <summary>
    /// Service for managing game input and action maps.
    /// </summary>
    public interface IInputService
    {
        /// <summary>
        /// Get the current input action asset.
        /// </summary>
        InputActionAsset InputActions { get; }

        /// <summary>
        /// Enable a specific action map.
        /// </summary>
        void EnableActionMap(string actionMapName);

        /// <summary>
        /// Disable a specific action map.
        /// </summary>
        void DisableActionMap(string actionMapName);

        /// <summary>
        /// Enable all navigation actions.
        /// </summary>
        void EnableNavigation();

        /// <summary>
        /// Disable all navigation actions.
        /// </summary>
        void DisableNavigation();

        /// <summary>
        /// Enable all tablet actions.
        /// </summary>
        void EnableTablet();

        /// <summary>
        /// Disable all tablet actions.
        /// </summary>
        void DisableTablet();

        /// <summary>
        /// Enable pause actions.
        /// </summary>
        void EnablePause();

        /// <summary>
        /// Disable pause actions.
        /// </summary>
        void DisablePause();

        /// <summary>
        /// Set cursor lock state.
        /// </summary>
        void SetCursorState(bool locked, bool visible);

        /// <summary>
        /// Get value from a specific action.
        /// </summary>
        T GetActionValue<T>(string actionName) where T : struct;

        /// <summary>
        /// Check if an action was pressed this frame.
        /// </summary>
        bool WasPressedThisFrame(string actionName);

        /// <summary>
        /// Check if an action is currently being held.
        /// </summary>
        bool IsHeld(string actionName);

        /// <summary>
        /// Check if an action was released this frame.
        /// </summary>
        bool WasReleasedThisFrame(string actionName);

        /// <summary>
        /// Event fired when an action is triggered.
        /// </summary>
        event System.Action<string> OnActionTriggered;
    }
}