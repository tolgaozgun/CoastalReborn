using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Interfaces;
using Core.Commands;
using Core.Data;

namespace UI.Controllers
{
    /// <summary>
    /// Main controller for the tablet UI system.
    /// Manages all tablet screens and integrates with services.
    /// Follows MVC pattern and clean architecture.
    /// </summary>
    public class TabletUIController : MonoBehaviour
    {
        [Header("Service Dependencies")]
        [SerializeField] private ICrewService crewService;
        [SerializeField] private IContractsService contractsService;
        [SerializeField] private IIntelService intelService;
        [SerializeField] private IUpgradeService upgradeService;

        [Header("UI Components")]
        [SerializeField] private GameObject tabletPanel;
        [SerializeField] private TMP_Text fundsText;
        [SerializeField] private Button contractsButton;
        [SerializeField] private Button crewButton;
        [SerializeField] private Button intelButton;
        [SerializeField] private Button upgradesButton;
        [SerializeField] private Button closeButton;

        [Header("Screen Panels")]
        [SerializeField] private GameObject contractsPanel;
        [SerializeField] private GameObject crewPanel;
        [SerializeField] private GameObject intelPanel;
        [SerializeField] private GameObject upgradesPanel;

        [Header("Command System")]
        [SerializeField] private TabletCommandInvoker commandInvoker;

        // Screen controllers
        private ContractsScreenController contractsController;
        private CrewScreenController crewController;
        private IntelScreenController intelController;
        private UpgradesScreenController upgradesController;

        // Events
        public event Action OnTabletOpened;
        public event Action OnTabletClosed;

        private void Awake()
        {
            InitializeDependencies();
            InitializeUI();
            InitializeScreenControllers();
        }

        private void Start()
        {
            SubscribeToEvents();
            RefreshUI();
        }

        /// <summary>
        /// Initializes service dependencies with fallback to existing implementations.
        /// </summary>
        private void InitializeDependencies()
        {
            // Try to find services in the scene
            if (crewService == null)
                crewService = FindObjectOfType<CrewService>();

            if (contractsService == null)
                contractsService = FindObjectOfType<ContractsService>();

            if (intelService == null)
                intelService = FindObjectOfType<IntelService>();

            if (upgradeService == null)
                upgradeService = FindObjectOfType<UpgradeService>();

            // Create command invoker if not assigned
            if (commandInvoker == null)
                commandInvoker = gameObject.AddComponent<TabletCommandInvoker>();

            Debug.Log("[TabletUIController] Service dependencies initialized");
        }

        /// <summary>
        /// Initializes UI components and button listeners.
        /// </summary>
        private void InitializeUI()
        {
            // Tablet visibility
            if (tabletPanel != null)
                tabletPanel.SetActive(false);

            // Button setup
            if (contractsButton != null)
                contractsButton.onClick.AddListener(() => ShowScreen(TabletScreen.Contracts));

            if (crewButton != null)
                crewButton.onClick.AddListener(() => ShowScreen(TabletScreen.Crew));

            if (intelButton != null)
                intelButton.onClick.AddListener(() => ShowScreen(TabletScreen.Intel));

            if (upgradesButton != null)
                upgradesButton.onClick.AddListener(() => ShowScreen(TabletScreen.Upgrades));

            if (closeButton != null)
                closeButton.onClick.AddListener(CloseTablet);

            Debug.Log("[TabletUIController] UI components initialized");
        }

        /// <summary>
        /// Initializes individual screen controllers.
        /// </summary>
        private void InitializeScreenControllers()
        {
            // Contracts screen
            if (contractsPanel != null)
            {
                contractsController = contractsPanel.GetComponent<ContractsScreenController>();
                if (contractsController == null)
                    contractsController = contractsPanel.AddComponent<ContractsScreenController>();

                contractsController.Initialize(contractsService, crewService, commandInvoker);
            }

            // Crew screen
            if (crewPanel != null)
            {
                crewController = crewPanel.GetComponent<CrewScreenController>();
                if (crewController == null)
                    crewController = crewPanel.AddComponent<CrewScreenController>();

                crewController.Initialize(crewService, commandInvoker);
            }

            // Intel screen
            if (intelPanel != null)
            {
                intelController = intelPanel.GetComponent<IntelScreenController>();
                if (intelController == null)
                    intelController = intelPanel.AddComponent<IntelScreenController>();

                intelController.Initialize(intelService, crewService, commandInvoker);
            }

            // Upgrades screen
            if (upgradesPanel != null)
            {
                upgradesController = upgradesPanel.GetComponent<UpgradesScreenController>();
                if (upgradesController == null)
                    upgradesController = upgradesPanel.AddComponent<UpgradesScreenController>();

                upgradesController.Initialize(upgradeService, crewService, commandInvoker);
            }

            Debug.Log("[TabletUIController] Screen controllers initialized");
        }

        /// <summary>
        /// Subscribes to service events.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (crewService != null)
            {
                crewService.OnCrewHired += RefreshUI;
                crewService.OnCrewFired += RefreshUI;
            }

            if (contractsService != null)
            {
                contractsService.OnContractAccepted += RefreshUI;
                contractsService.OnContractStatusChanged += RefreshUI;
            }

            if (intelService != null)
            {
                intelService.OnIntelReceived += RefreshUI;
            }

            if (commandInvoker != null)
            {
                commandInvoker.OnCommandExecuted += RefreshUI;
                commandInvoker.OnCommandUndone += RefreshUI;
            }
        }

        /// <summary>
        /// Shows the tablet UI.
        /// </summary>
        public void OpenTablet()
        {
            if (tabletPanel == null) return;

            tabletPanel.SetActive(true);
            RefreshUI();
            ShowScreen(TabletScreen.Contracts); // Default to contracts screen

            OnTabletOpened?.Invoke();
            Debug.Log("[TabletUIController] Tablet opened");
        }

        /// <summary>
        /// Hides the tablet UI.
        /// </summary>
        public void CloseTablet()
        {
            if (tabletPanel == null) return;

            tabletPanel.SetActive(false);

            OnTabletClosed?.Invoke();
            Debug.Log("[TabletUIController] Tablet closed");
        }

        /// <summary>
        /// Toggles tablet visibility.
        /// </summary>
        public void ToggleTablet()
        {
            if (tabletPanel.activeSelf)
                CloseTablet();
            else
                OpenTablet();
        }

        /// <summary>
        /// Shows a specific tablet screen.
        /// </summary>
        public void ShowScreen(TabletScreen screen)
        {
            // Hide all panels
            if (contractsPanel != null) contractsPanel.SetActive(false);
            if (crewPanel != null) crewPanel.SetActive(false);
            if (intelPanel != null) intelPanel.SetActive(false);
            if (upgradesPanel != null) upgradesPanel.SetActive(false);

            // Show selected panel
            switch (screen)
            {
                case TabletScreen.Contracts:
                    if (contractsPanel != null)
                    {
                        contractsPanel.SetActive(true);
                        contractsController?.RefreshScreen();
                    }
                    break;

                case TabletScreen.Crew:
                    if (crewPanel != null)
                    {
                        crewPanel.SetActive(true);
                        crewController?.RefreshScreen();
                    }
                    break;

                case TabletScreen.Intel:
                    if (intelPanel != null)
                    {
                        intelPanel.SetActive(true);
                        intelController?.RefreshScreen();
                    }
                    break;

                case TabletScreen.Upgrades:
                    if (upgradesPanel != null)
                    {
                        upgradesPanel.SetActive(true);
                        upgradesController?.RefreshScreen();
                    }
                    break;
            }

            Debug.Log($"[TabletUIController] Showing screen: {screen}");
        }

        /// <summary>
        /// Refreshes the UI data.
        /// </summary>
        public void RefreshUI()
        {
            UpdateFundsDisplay();
            UpdateButtonStates();
        }

        /// <summary>
        /// Updates the funds display.
        /// </summary>
        private void UpdateFundsDisplay()
        {
            if (fundsText != null && crewService != null)
            {
                fundsText.text = $"Funds: ${crewService.GetPlayerFunds():N0}";
            }
        }

        /// <summary>
        /// Updates button states based on available data.
        /// </summary>
        private void UpdateButtonStates()
        {
            if (contractsButton != null && contractsService != null)
            {
                bool hasAvailableContracts = contractsService.GetAvailableContracts().Count > 0;
                contractsButton.interactable = hasAvailableContracts;
            }

            if (crewButton != null && crewService != null)
            {
                bool hasAvailableCrew = crewService.GetAvailableCrew().Count > 0;
                crewButton.interactable = hasAvailableCrew;
            }

            if (intelButton != null && intelService != null)
            {
                bool hasIntel = intelService.GetIntelEntries().Count > 0;
                intelButton.interactable = hasIntel;
            }

            if (upgradesButton != null && upgradeService != null)
            {
                bool hasUpgrades = upgradeService.GetAllUpgradeNodes().Count > 0;
                upgradesButton.interactable = hasUpgrades;
            }
        }

        /// <summary>
        /// Gets the current tablet screen.
        /// </summary>
        public TabletScreen GetCurrentScreen()
        {
            if (contractsPanel != null && contractsPanel.activeSelf) return TabletScreen.Contracts;
            if (crewPanel != null && crewPanel.activeSelf) return TabletScreen.Crew;
            if (intelPanel != null && intelPanel.activeSelf) return TabletScreen.Intel;
            if (upgradesPanel != null && upgradesPanel.activeSelf) return TabletScreen.Upgrades;
            return TabletScreen.None;
        }

        /// <summary>
        /// Public access to command invoker for external systems.
        /// </summary>
        public TabletCommandInvoker GetCommandInvoker() => commandInvoker;

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (crewService != null)
            {
                crewService.OnCrewHired -= RefreshUI;
                crewService.OnCrewFired -= RefreshUI;
            }

            if (contractsService != null)
            {
                contractsService.OnContractAccepted -= RefreshUI;
                contractsService.OnContractStatusChanged -= RefreshUI;
            }

            if (intelService != null)
            {
                intelService.OnIntelReceived -= RefreshUI;
            }

            if (commandInvoker != null)
            {
                commandInvoker.OnCommandExecuted -= RefreshUI;
                commandInvoker.OnCommandUndone -= RefreshUI;
            }
        }
    }

    /// <summary>
    /// Enum representing different tablet screens.
    /// </summary>
    public enum TabletScreen
    {
        None,
        Contracts,
        Crew,
        Intel,
        Upgrades
    }
}