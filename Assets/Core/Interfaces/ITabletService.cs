using System;
using System.Collections.Generic;

namespace Core.Interfaces
{
    /// <summary>
    /// Service for managing tablet state and operations.
    /// </summary>
    public interface ITabletService
    {
        /// <summary>
        /// Check if the tablet is currently active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Currently selected tab.
        /// </summary>
        string CurrentTab { get; }

        /// <summary>
        /// Available tablet tabs.
        /// </summary>
        IReadOnlyList<string> AvailableTabs { get; }

        /// <summary>
        /// Event fired when tablet is activated.
        /// </summary>
        event Action OnTabletActivated;

        /// <summary>
        /// Event fired when tablet is deactivated.
        /// </summary>
        event Action OnTabletDeactivated;

        /// <summary>
        /// Event fired when tab is changed.
        /// </summary>
        event Action<string> OnTabChanged;

        /// <summary>
        /// Event fired when data in a tab is updated.
        /// </summary>
        event Action<string, object> OnTabDataUpdated;

        /// <summary>
        /// Activate the tablet.
        /// </summary>
        void ActivateTablet();

        /// <summary>
        /// Deactivate the tablet.
        /// </summary>
        void DeactivateTablet();

        /// <summary>
        /// Toggle tablet activation state.
        /// </summary>
        void ToggleTablet();

        /// <summary>
        /// Navigate to a specific tab.
        /// </summary>
        /// <param name="tabName">Name of the tab</param>
        void NavigateToTab(string tabName);

        /// <summary>
        /// Navigate to the next tab.
        /// </summary>
        void NavigateToNextTab();

        /// <summary>
        /// Navigate to the previous tab.
        /// </summary>
        void NavigateToPreviousTab();

        /// <summary>
        /// Register a tab with the tablet.
        /// </summary>
        /// <param name="tabName">Name of the tab</param>
        /// <param name="tabData">Initial data for the tab</param>
        void RegisterTab(string tabName, object tabData = null);

        /// <summary>
        /// Unregister a tab from the tablet.
        /// </summary>
        /// <param name="tabName">Name of the tab</param>
        void UnregisterTab(string tabName);

        /// <summary>
        /// Update data for a specific tab.
        /// </summary>
        /// <param name="tabName">Name of the tab</param>
        /// <param name="data">New data for the tab</param>
        void UpdateTabData(string tabName, object data);

        /// <summary>
        /// Get data for a specific tab.
        /// </summary>
        /// <param name="tabName">Name of the tab</param>
        /// <returns>Tab data or null</returns>
        T GetTabData<T>(string tabName);

        /// <summary>
        /// Check if a tab is registered.
        /// </summary>
        /// <param name="tabName">Name of the tab</param>
        /// <returns>True if tab exists</returns>
        bool HasTab(string tabName);

        /// <summary>
        /// Show a notification on the tablet.
        /// </summary>
        /// <param name="message">Notification message</param>
        /// <param name="type">Notification type</param>
        /// <param name="duration">Duration in seconds</param>
        void ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f);

        /// <summary>
        /// Clear all notifications.
        /// </summary>
        void ClearNotifications();
    }

    /// <summary>
    /// Types of tablet notifications.
    /// </summary>
    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success
    }
}