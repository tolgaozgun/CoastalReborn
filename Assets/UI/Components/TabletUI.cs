using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Interfaces;
using Core.Services;

namespace UI.Components
{
    /// <summary>
    /// Tablet UI controller component.
    /// </summary>
    public class TabletUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas tabletCanvas;
        [SerializeField] private GameObject tabletPanel;
        [SerializeField] private Button contractsTab;
        [SerializeField] private Button intelTab;
        [SerializeField] private Button upgradesTab;
        [SerializeField] private Button crewTab;
        [SerializeField] private Button settingsTab;
        [SerializeField] private Button closeButton;

        [Header("Tab Panels")]
        [SerializeField] private GameObject contractsPanel;
        [SerializeField] private GameObject intelPanel;
        [SerializeField] private GameObject upgradesPanel;
        [SerializeField] private GameObject crewPanel;
        [SerializeField] private GameObject settingsPanel;

        private ITabletService tabletService;
        private IUIService uiService;
        private Dictionary<string, GameObject> tabPanels = new Dictionary<string, GameObject>();
        private string currentTab = "Contracts";

        private void Awake()
        {
            // Set up tab panel dictionary
            tabPanels["Contracts"] = contractsPanel;
            tabPanels["Intel"] = intelPanel;
            tabPanels["Upgrades"] = upgradesPanel;
            tabPanels["Crew"] = crewPanel;
            tabPanels["Settings"] = settingsPanel;

            // Hide all panels initially
            foreach (var panel in tabPanels.Values)
            {
                if (panel != null)
                    panel.SetActive(false);
            }

            if (tabletPanel != null)
                tabletPanel.SetActive(false);
        }

        private void Start()
        {
            // Get services (fallback to FindObjectOfType if DI isn't ready)
            tabletService = FindObjectOfType<TabletService>();
            uiService = FindObjectOfType<UIService>();

            if (tabletService != null)
            {
                tabletService.OnTabletActivated += OnTabletActivated;
                tabletService.OnTabletDeactivated += OnTabletDeactivated;
                tabletService.OnTabChanged += OnTabChanged;
            }

            SetupUIButtons();

            // Set initial state
            if (tabletPanel != null)
                tabletPanel.SetActive(tabletService?.IsActive ?? false);

            if (tabletService?.IsActive == true)
            {
                ShowTab(currentTab);
            }
        }

        private void SetupUIButtons()
        {
            if (contractsTab != null)
            {
                contractsTab.onClick.RemoveAllListeners();
                contractsTab.onClick.AddListener(() => SwitchTab("Contracts"));
            }

            if (intelTab != null)
            {
                intelTab.onClick.RemoveAllListeners();
                intelTab.onClick.AddListener(() => SwitchTab("Intel"));
            }

            if (upgradesTab != null)
            {
                upgradesTab.onClick.RemoveAllListeners();
                upgradesTab.onClick.AddListener(() => SwitchTab("Upgrades"));
            }

            if (crewTab != null)
            {
                crewTab.onClick.RemoveAllListeners();
                crewTab.onClick.AddListener(() => SwitchTab("Crew"));
            }

            if (settingsTab != null)
            {
                settingsTab.onClick.RemoveAllListeners();
                settingsTab.onClick.AddListener(() => SwitchTab("Settings"));
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() =>
                {
                    tabletService?.DeactivateTablet();
                    uiService?.CloseTablet();
                });
            }
        }

        private void SwitchTab(string tabName)
        {
            if (tabletService != null)
            {
                tabletService.NavigateToTab(tabName);
            }
            else
            {
                ShowTab(tabName);
            }
        }

        private void ShowTab(string tabName)
        {
            // Hide all panels
            foreach (var kvp in tabPanels)
            {
                if (kvp.Value != null)
                    kvp.Value.SetActive(false);
            }

            // Show selected panel
            if (tabPanels.TryGetValue(tabName, out GameObject panel) && panel != null)
            {
                panel.SetActive(true);
                currentTab = tabName;
            }

            // Update button states
            UpdateTabButtonStates();
        }

        private void UpdateTabButtonStates()
        {
            UpdateButtonState(contractsTab, currentTab == "Contracts");
            UpdateButtonState(intelTab, currentTab == "Intel");
            UpdateButtonState(upgradesTab, currentTab == "Upgrades");
            UpdateButtonState(crewTab, currentTab == "Crew");
            UpdateButtonState(settingsTab, currentTab == "Settings");
        }

        private void UpdateButtonState(Button button, bool isSelected)
        {
            if (button == null) return;

            var colors = button.colors;
            colors.normalColor = isSelected ? Color.yellow : Color.white;
            button.colors = colors;
        }

        // Service event handlers
        private void OnTabletActivated()
        {
            if (tabletPanel != null)
            {
                tabletPanel.SetActive(true);
            }
            ShowTab(currentTab);
        }

        private void OnTabletDeactivated()
        {
            if (tabletPanel != null)
            {
                tabletPanel.SetActive(false);
            }
        }

        private void OnTabChanged(string newTab)
        {
            ShowTab(newTab);
        }

        // Public methods
        public void Initialize()
        {
            // Ensure tablet starts closed
            if (tabletPanel != null)
                tabletPanel.SetActive(false);
        }

        public void ShowTablet()
        {
            tabletService?.ActivateTablet();
            uiService?.OpenTablet();
        }

        public void HideTablet()
        {
            tabletService?.DeactivateTablet();
            uiService?.CloseTablet();
        }

        public void ToggleTablet()
        {
            if (tabletService != null)
            {
                tabletService.ToggleTablet();
            }
            else
            {
                if (tabletPanel != null && tabletPanel.activeSelf)
                {
                    HideTablet();
                }
                else
                {
                    ShowTablet();
                }
            }
        }

        private void OnDestroy()
        {
            if (tabletService != null)
            {
                tabletService.OnTabletActivated -= OnTabletActivated;
                tabletService.OnTabletDeactivated -= OnTabletDeactivated;
                tabletService.OnTabChanged -= OnTabChanged;
            }
        }
    }
}