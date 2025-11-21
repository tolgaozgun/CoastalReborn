using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Implementation of the tablet service.
    /// </summary>
    public class TabletService : MonoBehaviour, ITabletService
    {
        [Header("Tablet Configuration")]
        [SerializeField] private bool startActive = false;

        private bool isActive = false;
        private string currentTab = "Contracts";
        private Dictionary<string, object> tabs = new Dictionary<string, object>();
        private List<string> availableTabs = new List<string>();

        public bool IsActive => isActive;
        public string CurrentTab => currentTab;
        public IReadOnlyList<string> AvailableTabs => availableTabs;

        public event Action OnTabletActivated;
        public event Action OnTabletDeactivated;
        public event Action<string> OnTabChanged;
        public event Action<string, object> OnTabDataUpdated;

        private void Awake()
        {
            // Initialize default tabs
            RegisterTab("Contracts");
            RegisterTab("Intel");
            RegisterTab("Upgrades");
            RegisterTab("Crew");
            RegisterTab("Settings");

            if (startActive)
            {
                ActivateTablet();
            }
        }

        public void ActivateTablet()
        {
            if (isActive) return;

            isActive = true;
            OnTabletActivated?.Invoke();
        }

        public void DeactivateTablet()
        {
            if (!isActive) return;

            isActive = false;
            OnTabletDeactivated?.Invoke();
        }

        public void ToggleTablet()
        {
            if (isActive)
            {
                DeactivateTablet();
            }
            else
            {
                ActivateTablet();
            }
        }

        public void NavigateToTab(string tabName)
        {
            if (!tabs.ContainsKey(tabName))
            {
                Debug.LogWarning($"Tab '{tabName}' not found!");
                return;
            }

            currentTab = tabName;
            OnTabChanged?.Invoke(currentTab);
        }

        public void NavigateToNextTab()
        {
            int currentIndex = availableTabs.IndexOf(currentTab);
            if (currentIndex >= 0 && currentIndex < availableTabs.Count - 1)
            {
                NavigateToTab(availableTabs[currentIndex + 1]);
            }
        }

        public void NavigateToPreviousTab()
        {
            int currentIndex = availableTabs.IndexOf(currentTab);
            if (currentIndex > 0)
            {
                NavigateToTab(availableTabs[currentIndex - 1]);
            }
        }

        public void RegisterTab(string tabName, object tabData = null)
        {
            if (!tabs.ContainsKey(tabName))
            {
                tabs[tabName] = tabData;
                availableTabs.Add(tabName);

                // Sort tabs alphabetically
                availableTabs.Sort();
            }
        }

        public void UnregisterTab(string tabName)
        {
            if (tabs.ContainsKey(tabName))
            {
                tabs.Remove(tabName);
                availableTabs.Remove(tabName);

                // Switch to first available tab if current tab was removed
                if (currentTab == tabName && availableTabs.Count > 0)
                {
                    NavigateToTab(availableTabs[0]);
                }
            }
        }

        public void UpdateTabData(string tabName, object data)
        {
            if (tabs.ContainsKey(tabName))
            {
                tabs[tabName] = data;
                OnTabDataUpdated?.Invoke(tabName, data);
            }
        }

        public T GetTabData<T>(string tabName)
        {
            if (tabs.ContainsKey(tabName) && tabs[tabName] is T)
            {
                return (T)tabs[tabName];
            }
            return default(T);
        }

        public bool HasTab(string tabName)
        {
            return tabs.ContainsKey(tabName);
        }

        public void ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            // For now, just log the notification
            Debug.Log($"[Tablet Notification] {type}: {message}");
            // TODO: Implement actual notification UI
        }

        public void ClearNotifications()
        {
            // TODO: Implement notification clearing
        }
    }
}