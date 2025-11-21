using UnityEngine;
using Core.Interfaces;
using Core.Services;
using UI.Controllers;

namespace Core
{
    /// <summary>
    /// Bootstrap system for tablet functionality.
    /// Sets up dependency injection and initializes all tablet services.
    /// </summary>
    public class TabletBootstrap : MonoBehaviour
    {
        [Header("Service Prefabs")]
        [SerializeField] private GameObject crewServicePrefab;
        [SerializeField] private GameObject contractsServicePrefab;
        [SerializeField] private GameObject intelServicePrefab;
        [SerializeField] private GameObject upgradeServicePrefab;

        [Header("UI Prefabs")]
        [SerializeField] private GameObject tabletUIPrefab;

        [Header("Instantiation Settings")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool dontDestroyOnLoad = true;

        // Service instances
        private ICrewService crewService;
        private IContractsService contractsService;
        private IIntelService intelService;
        private IUpgradeService upgradeService;
        private TabletUIController tabletUIController;

        // Singleton pattern for easy access
        public static TabletBootstrap Instance { get; private set; }

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (autoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initializes the entire tablet system.
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[TabletBootstrap] Initializing tablet system...");

            // Step 1: Initialize services
            InitializeServices();

            // Step 2: Initialize UI
            InitializeUI();

            // Step 3: Wire up dependencies
            WireDependencies();

            Debug.Log("[TabletBootstrap] Tablet system initialization complete!");
        }

        /// <summary>
        /// Initializes all service instances.
        /// </summary>
        private void InitializeServices()
        {
            // Crew Service
            if (crewServicePrefab != null)
            {
                var crewGO = Instantiate(crewServicePrefab, transform);
                crewService = crewGO.GetComponent<CrewService>();
            }
            else
            {
                // Fallback: look for existing or create new
                crewService = FindObjectOfType<CrewService>();
                if (crewService == null)
                {
                    var crewGO = new GameObject("CrewService");
                    crewService = crewGO.AddComponent<CrewService>();
                    crewGO.transform.SetParent(transform);
                }
            }

            // Contracts Service
            if (contractsServicePrefab != null)
            {
                var contractsGO = Instantiate(contractsServicePrefab, transform);
                contractsService = contractsGO.GetComponent<ContractsService>();
            }
            else
            {
                contractsService = FindObjectOfType<ContractsService>();
                if (contractsService == null)
                {
                    var contractsGO = new GameObject("ContractsService");
                    contractsService = contractsGO.AddComponent<ContractsService>();
                    contractsGO.transform.SetParent(transform);
                }
            }

            // Intel Service
            if (intelServicePrefab != null)
            {
                var intelGO = Instantiate(intelServicePrefab, transform);
                intelService = intelGO.GetComponent<IntelService>();
            }
            else
            {
                intelService = FindObjectOfType<IntelService>();
                if (intelService == null)
                {
                    var intelGO = new GameObject("IntelService");
                    intelService = intelGO.AddComponent<IntelService>();
                    intelGO.transform.SetParent(transform);
                }
            }

            // Upgrade Service
            if (upgradeServicePrefab != null)
            {
                var upgradeGO = Instantiate(upgradeServicePrefab, transform);
                upgradeService = upgradeGO.GetComponent<UpgradeService>();
            }
            else
            {
                upgradeService = FindObjectOfType<UpgradeService>();
                if (upgradeService == null)
                {
                    var upgradeGO = new GameObject("UpgradeService");
                    upgradeService = upgradeGO.AddComponent<UpgradeService>();
                    upgradeGO.transform.SetParent(transform);
                }
            }

            Debug.Log("[TabletBootstrap] Services initialized");
        }

        /// <summary>
        /// Initializes the tablet UI controller.
        /// </summary>
        private void InitializeUI()
        {
            if (tabletUIPrefab != null)
            {
                var tabletGO = Instantiate(tabletUIPrefab, transform);
                tabletUIController = tabletGO.GetComponent<TabletUIController>();
            }
            else
            {
                // Fallback: look for existing or create new
                tabletUIController = FindObjectOfType<TabletUIController>();
                if (tabletUIController == null)
                {
                    Debug.LogWarning("[TabletBootstrap] TabletUIController not found. Please assign Tablet UI prefab or create a TabletUIController in the scene.");
                }
            }

            Debug.Log("[TabletBootstrap] UI initialized");
        }

        /// <summary>
        /// Wires up dependencies between services and UI.
        /// </summary>
        private void WireDependencies()
        {
            if (tabletUIController != null)
            {
                // Use reflection or public methods to set dependencies
                SetTabletDependencies();
            }

            Debug.Log("[TabletBootstrap] Dependencies wired");
        }

        /// <summary>
        /// Sets dependencies on the tablet UI controller.
        /// </summary>
        private void SetTabletDependencies()
        {
            // This assumes TabletUIController has public methods or properties for setting dependencies
            // or we could make them internal and use reflection
            var tabletType = typeof(TabletUIController);

            // Set services via reflection (since they're private fields)
            SetField(tabletType, "crewService", tabletUIController, crewService);
            SetField(tabletType, "contractsService", tabletUIController, contractsService);
            SetField(tabletType, "intelService", tabletUIController, intelService);
            SetField(tabletType, "upgradeService", tabletUIController, upgradeService);

            Debug.Log("[TabletBootstrap] Tablet dependencies set");
        }

        /// <summary>
        /// Sets a private field value via reflection.
        /// </summary>
        private void SetField(System.Type type, string fieldName, object target, object value)
        {
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            field?.SetValue(target, value);
        }

        /// <summary>
        /// Gets service instances for external access.
        /// </summary>
        public ICrewService GetCrewService() => crewService;
        public IContractsService GetContractsService() => contractsService;
        public IIntelService GetIntelService() => intelService;
        public IUpgradeService GetUpgradeService() => upgradeService;
        public TabletUIController GetTabletUIController() => tabletUIController;

        /// <summary>
        /// Opens the tablet UI.
        /// </summary>
        public void OpenTablet()
        {
            tabletUIController?.OpenTablet();
        }

        /// <summary>
        /// Closes the tablet UI.
        /// </summary>
        public void CloseTablet()
        {
            tabletUIController?.CloseTablet();
        }

        /// <summary>
        /// Toggles tablet UI visibility.
        /// </summary>
        public void ToggleTablet()
        {
            tabletUIController?.ToggleTablet();
        }

        private void Update()
        {
            // Handle tablet toggle key
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleTablet();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}