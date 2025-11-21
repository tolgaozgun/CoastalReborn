using System;
using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// Service for managing UI canvases, modals, and navigation.
    /// </summary>
    public interface IUIService
    {
        /// <summary>
        /// Check if the tablet UI is currently open.
        /// </summary>
        bool IsTabletOpen { get; }

        /// <summary>
        /// Check if any modal is currently open.
        /// </summary>
        bool IsModalOpen { get; }

        /// <summary>
        /// Current input context (Navigation, Tablet, etc.).
        /// </summary>
        string CurrentInputContext { get; }

        /// <summary>
        /// Event fired when tablet is opened.
        /// </summary>
        event Action OnTabletOpened;

        /// <summary>
        /// Event fired when tablet is closed.
        /// </summary>
        event Action OnTabletClosed;

        /// <summary>
        /// Event fired when input context changes.
        /// </summary>
        event Action<string> OnInputContextChanged;

        /// <summary>
        /// Event fired when modal is opened.
        /// </summary>
        event Action<string> OnModalOpened;

        /// <summary>
        /// Event fired when modal is closed.
        /// </summary>
        event Action<string> OnModalClosed;

        /// <summary>
        /// Register a UI canvas with the service.
        /// </summary>
        /// <param name="canvasName">Name of the canvas</param>
        /// <param name="canvas">Canvas component</param>
        void RegisterCanvas(string canvasName, Canvas canvas);

        /// <summary>
        /// Unregister a UI canvas.
        /// </summary>
        /// <param name="canvasName">Name of the canvas</param>
        void UnregisterCanvas(string canvasName);

        /// <summary>
        /// Show or hide a specific canvas.
        /// </summary>
        /// <param name="canvasName">Name of the canvas</param>
        /// <param name="show">Whether to show or hide</param>
        void SetCanvasVisibility(string canvasName, bool show);

        /// <summary>
        /// Open the tablet UI.
        /// </summary>
        void OpenTablet();

        /// <summary>
        /// Close the tablet UI.
        /// </summary>
        void CloseTablet();

        /// <summary>
        /// Toggle tablet open/closed state.
        /// </summary>
        void ToggleTablet();

        /// <summary>
        /// Open a modal dialog.
        /// </summary>
        /// <param name="modalName">Name of the modal</param>
        /// <param name="data">Optional data to pass to modal</param>
        void OpenModal(string modalName, object data = null);

        /// <summary>
        /// Close the current modal.
        /// </summary>
        /// <param name="modalName">Name of the modal to close</param>
        void CloseModal(string modalName);

        /// <summary>
        /// Close all open modals.
        /// </summary>
        void CloseAllModals();

        /// <summary>
        /// Set the current input context.
        /// </summary>
        /// <param name="context">New input context</param>
        void SetInputContext(string context);

        /// <summary>
        /// Get a canvas by name.
        /// </summary>
        /// <param name="canvasName">Name of the canvas</param>
        /// <returns>Canvas component or null</returns>
        Canvas GetCanvas(string canvasName);

        /// <summary>
        /// Initialize the UI service.
        /// </summary>
        void Initialize();
    }
}