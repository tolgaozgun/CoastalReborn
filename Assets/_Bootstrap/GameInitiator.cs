using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Shared.DI;
using Core.Interfaces;
using Core.Services;
using Gameplay.Interfaces;
using Gameplay.Services;

namespace _Bootstrap
{
    /// <summary>
    /// Single Entry Point for the game application.
    /// Manages deterministic startup sequence and dependency injection.
    /// </summary>
    public class GameInitiator : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private string initialSceneName = "HarborScene";
        [SerializeField] private string loadingSceneName = "LoadingScene";
        [SerializeField] private bool autoStartOnAwake = true;

        [Header("Startup Configuration")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private float startupTimeout = 30f;

        // Private fields
        private DIContainer container;
        private CancellationTokenSource cancellationTokenSource;
        private bool isInitialized = false;
        private float startupStartTime;
        private bool isDisposed = false;

        // Core service references (for easy access during startup)
        private ISceneLoader sceneLoader;
        private IInputService inputService;
        private IUIService uiService;
        private ITabletService tabletService;

        // Properties
        public DIContainer Container => container;
        public bool IsInitialized => isInitialized;

        // Events
        public static event Action OnGameInitialized;
        public static event Action<float, string> OnStartupProgress;

        private void Awake()
        {
            // Ensure this is the only GameInitiator
            GameInitiator[] initiators = FindObjectsOfType<GameInitiator>();
            if (initiators.Length > 1)
            {
                Debug.LogWarning("Multiple GameInitiators found! Destroying duplicates.");
                for (int i = 1; i < initiators.Length; i++)
                {
                    Destroy(initiators[i].gameObject);
                }
            }

            DontDestroyOnLoad(gameObject);

            if (autoStartOnAwake)
            {
                StartCoroutine(InitializeGame());
            }
        }

        private IEnumerator InitializeGame()
        {
            startupStartTime = Time.time;
            cancellationTokenSource = new CancellationTokenSource();

            // Run startup sequence directly without try-catch
            yield return StartCoroutine(StartupSequence());

            Debug.Log("Game initialization completed successfully.");
        }

        private IEnumerator StartupSequence()
        {
            ReportProgress(0f, "Initializing dependency container...");

            // Phase 1: Create DI Container and bind core services
            container = new DIContainer();
            yield return StartCoroutine(BindCoreServices());

            // Phase 2: Instantiate core services
            ReportProgress(0.2f, "Instantiating core services...");
            yield return StartCoroutine(InstantiateCoreServices());

            // Phase 3: Initialize core services
            ReportProgress(0.4f, "Initializing core services...");
            yield return StartCoroutine(InitializeCoreServices());

            // Phase 4: Show loading screen
            ReportProgress(0.5f, "Preparing loading screen...");
            yield return StartCoroutine(ShowLoadingScreen());

            // Phase 5: Load initial scene
            ReportProgress(0.6f, $"Loading {initialSceneName}...");
            yield return StartCoroutine(LoadInitialScene());

            // Phase 6: Bind gameplay context services
            ReportProgress(0.8f, "Binding gameplay services...");
            yield return StartCoroutine(BindGameplayServices());

            // Phase 7: Initialize gameplay services
            ReportProgress(0.9f, "Initializing gameplay services...");
            yield return StartCoroutine(InitializeGameplayServices());

            // Phase 8: Create game objects and finalize
            ReportProgress(0.95f, "Creating game objects...");
            yield return StartCoroutine(CreateGameObjects());

            // Phase 9: Complete initialization
            ReportProgress(1f, "Startup complete!");
            yield return StartCoroutine(CompleteStartup());
        }

        private IEnumerator BindCoreServices()
        {
            // Input Service
            container.Register<IInputService, InputService>(Shared.DI.DIContainer.Lifetime.Singleton);

            // Scene Loader Service
            container.Register<ISceneLoader, SceneLoaderService>(Shared.DI.DIContainer.Lifetime.Singleton);

            // UI Service
            container.Register<IUIService, UIService>(Shared.DI.DIContainer.Lifetime.Singleton);

            // Tablet Service
            container.Register<ITabletService, TabletService>(Shared.DI.DIContainer.Lifetime.Singleton);

            // Save Service
            container.Register<ISaveService, SaveService>(Shared.DI.DIContainer.Lifetime.Singleton);

            // Intel Service
            container.Register<IIntelService, IntelService>(Shared.DI.DIContainer.Lifetime.Singleton);

            // Upgrade Service
            container.Register<IUpgradeService, UpgradeService>(Shared.DI.DIContainer.Lifetime.Singleton);

            // Audio Service
            container.Register<IAudioService, AudioService>(Shared.DI.DIContainer.Lifetime.Singleton);

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator InstantiateCoreServices()
        {
            Debug.Log("[DEBUG] InstantiateCoreServices: Starting core services instantiation");

            // Create MonoBehaviour services as GameObjects first
            GameObject sceneLoaderObject = new GameObject("SceneLoaderService");
            sceneLoader = sceneLoaderObject.AddComponent<SceneLoaderService>();
            DontDestroyOnLoad(sceneLoaderObject);
            Debug.Log($"[DEBUG] InstantiateCoreServices: SceneLoader created - {sceneLoader != null}");

            GameObject inputServiceObject = new GameObject("InputService");
            inputService = inputServiceObject.AddComponent<InputService>();
            DontDestroyOnLoad(inputServiceObject);
            Debug.Log($"[DEBUG] InstantiateCoreServices: InputService created - {inputService != null}");

            GameObject uiServiceObject = new GameObject("UIService");
            uiService = uiServiceObject.AddComponent<UIService>();
            DontDestroyOnLoad(uiServiceObject);
            Debug.Log($"[DEBUG] InstantiateCoreServices: UIService created - {uiService != null}");

            GameObject tabletServiceObject = new GameObject("TabletService");
            tabletService = tabletServiceObject.AddComponent<TabletService>();
            DontDestroyOnLoad(tabletServiceObject);
            Debug.Log($"[DEBUG] InstantiateCoreServices: TabletService created - {tabletService != null}");

            // Register the created instances in the container
            container.RegisterSingleton<ISceneLoader>(sceneLoader);
            container.RegisterSingleton<IInputService>(inputService);
            container.RegisterSingleton<IUIService>(uiService);
            container.RegisterSingleton<ITabletService>(tabletService);

            Debug.Log("[DEBUG] InstantiateCoreServices: All core services registered in container");

            // Other services are lazy-loaded as needed
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator InitializeCoreServices()
        {
            // Initialize services in the correct order
            if (inputService != null)
            {
                inputService.EnableNavigation();
                yield return new WaitForEndOfFrame();
            }

            if (uiService != null)
            {
                uiService.Initialize();
                yield return new WaitForEndOfFrame();
            }

            if (sceneLoader != null)
            {
                // Scene loader doesn't need explicit initialization
                yield return new WaitForEndOfFrame();
            }

            if (tabletService != null)
            {
                // Tablet service is initialized when first opened
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator ShowLoadingScreen()
        {
            Debug.Log($"[DEBUG] ShowLoadingScreen: Starting loading screen '{loadingSceneName}'");
            Debug.Log($"[DEBUG] ShowLoadingScreen: sceneLoader is null? {sceneLoader == null}");

            // Debug: List all scenes in build settings
            Debug.Log($"[DEBUG] ShowLoadingScreen: Scenes in build settings: {UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings}");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                Debug.Log($"[DEBUG] ShowLoadingScreen: Scene {i}: '{sceneName}' at '{scenePath}'");
            }

            if (!string.IsNullOrEmpty(loadingSceneName))
            {
                Debug.Log($"[DEBUG] ShowLoadingScreen: Attempting to load '{loadingSceneName}'");

                // Load scene directly using Unity's SceneManager
                AsyncOperation loadOperation = null;
                bool loadFailed = false;

                try
                {
                    loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(loadingSceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[DEBUG] ShowLoadingScreen: Exception loading '{loadingSceneName}': {e.Message}");
                    loadFailed = true;
                }

                if (!loadFailed && loadOperation != null)
                {
                    Debug.Log($"[DEBUG] ShowLoadingScreen: Starting to wait for load completion");
                    while (!loadOperation.isDone)
                    {
                        yield return null;
                    }
                    Debug.Log($"[DEBUG] ShowLoadingScreen: Loading screen completed successfully");
                }
                else if (!loadFailed)
                {
                    Debug.LogError($"[DEBUG] ShowLoadingScreen: Unity returned null AsyncOperation for '{loadingSceneName}'");
                }
            }
            else
            {
                Debug.Log($"[DEBUG] ShowLoadingScreen: No loading scene name provided, skipping");
            }

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator LoadInitialScene()
        {
            Debug.Log($"[DEBUG] LoadInitialScene: Starting to load '{initialSceneName}'");
            Debug.Log($"[DEBUG] LoadInitialScene: sceneLoader is null? {sceneLoader == null}");

            if (!string.IsNullOrEmpty(initialSceneName))
            {
                Debug.Log($"[DEBUG] LoadInitialScene: Attempting to load '{initialSceneName}'");

                // Load scene directly using Unity's SceneManager
                AsyncOperation loadOperation = null;
                bool loadFailed = false;

                try
                {
                    loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(initialSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[DEBUG] LoadInitialScene: Exception loading '{initialSceneName}': {e.Message}");
                    loadFailed = true;
                }

                if (!loadFailed && loadOperation != null)
                {
                    Debug.Log($"[DEBUG] LoadInitialScene: Starting to wait for scene load completion");
                    while (!loadOperation.isDone)
                    {
                        ReportProgress(0.6f + (0.2f * loadOperation.progress), $"Loading {initialSceneName}...");
                        yield return null;
                    }
                    Debug.Log($"[DEBUG] LoadInitialScene: Scene '{initialSceneName}' loaded successfully");
                }
                else if (!loadFailed)
                {
                    Debug.LogError($"[DEBUG] LoadInitialScene: Unity returned null AsyncOperation for '{initialSceneName}'");
                }
            }
            else
            {
                Debug.Log($"[DEBUG] LoadInitialScene: No initial scene name provided, skipping");
            }

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator BindGameplayServices()
        {
            // Create MonoBehaviour services as GameObjects
            GameObject shipManagerObject = new GameObject("ShipManager");
            var shipManager = shipManagerObject.AddComponent<ShipManager>();
            container.RegisterSingleton<IShipManager>(shipManager);

            GameObject suspicionSystemObject = new GameObject("SuspicionSystem");
            var suspicionSystem = suspicionSystemObject.AddComponent<SuspicionSystem>();
            container.RegisterSingleton<ISuspicionSystem>(suspicionSystem);

            GameObject patrolDirectorObject = new GameObject("PatrolDirector");
            var patrolDirector = patrolDirectorObject.AddComponent<PatrolDirector>();
            container.RegisterSingleton<IPatrolDirector>(patrolDirector);

            GameObject chaseControllerObject = new GameObject("ChaseController");
            var chaseController = chaseControllerObject.AddComponent<ChaseController>();
            container.RegisterSingleton<IChaseController>(chaseController);

            GameObject eventDirectorObject = new GameObject("EventDirector");
            var eventDirector = eventDirectorObject.AddComponent<EventDirector>();
            container.RegisterSingleton<IEventDirector>(eventDirector);

            GameObject arrestServiceObject = new GameObject("ArrestService");
            var arrestService = arrestServiceObject.AddComponent<ArrestService>();
            container.RegisterSingleton<IArrestService>(arrestService);

            // Make them persistent
            DontDestroyOnLoad(shipManagerObject);
            DontDestroyOnLoad(suspicionSystemObject);
            DontDestroyOnLoad(patrolDirectorObject);
            DontDestroyOnLoad(chaseControllerObject);
            DontDestroyOnLoad(eventDirectorObject);
            DontDestroyOnLoad(arrestServiceObject);

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator InitializeGameplayServices()
        {
            // Initialize gameplay services
            var shipManager = container.TryResolve<IShipManager>();
            if (shipManager != null)
            {
                shipManager.Initialize();
                yield return new WaitForEndOfFrame();
            }

            var suspicionSystem = container.TryResolve<ISuspicionSystem>();
            // Suspicion system doesn't need explicit initialization
            yield return new WaitForEndOfFrame();

            var patrolDirector = container.TryResolve<IPatrolDirector>();
            if (patrolDirector != null)
            {
                patrolDirector.Initialize();
                yield return new WaitForEndOfFrame();
            }

            var chaseController = container.TryResolve<IChaseController>();
            if (chaseController != null)
            {
                chaseController.Initialize();
                yield return new WaitForEndOfFrame();
            }

            var eventDirector = container.TryResolve<IEventDirector>();
            if (eventDirector != null)
            {
                eventDirector.Initialize();
                yield return new WaitForEndOfFrame();
            }

            var arrestService = container.TryResolve<IArrestService>();
            if (arrestService != null)
            {
                arrestService.Initialize();
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator CreateGameObjects()
        {
            // Spawn player boat
            var shipManager = container.TryResolve<IShipManager>();
            if (shipManager != null)
            {
                // Find player spawn point in the loaded scene
                GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
                Vector3 spawnPosition = spawnPoint != null ? spawnPoint.transform.position : Vector3.zero;

                var playerShip = shipManager.SpawnPlayerShip(spawnPosition);
                if (playerShip != null)
                {
                    Debug.Log($"Player boat spawned at position: {spawnPosition}");
                }
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator CompleteStartup()
        {
            isInitialized = true;

            // Hide loading screen if it was shown
            if (!string.IsNullOrEmpty(loadingSceneName))
            {
                sceneLoader.UnloadScene(loadingSceneName);
                yield return new WaitForEndOfFrame();
            }

            // Enable game input
            if (inputService != null)
            {
                inputService.SetCursorState(false, true);
            }

            // Fire game initialized event
            OnGameInitialized?.Invoke();

            if (showDebugInfo)
            {
                float startupTime = Time.time - startupStartTime;
                Debug.Log($"<color=green>Game initialization complete in {startupTime:F2} seconds</color>");
            }

            yield return new WaitForEndOfFrame();
        }

        private void ReportProgress(float progress, string status)
        {
            OnStartupProgress?.Invoke(progress, status);

            if (showDebugInfo)
            {
                Debug.Log($"[Startup] {progress:P0} - {status}");
            }
        }

        private void OnDestroy()
        {
            DisposeResources();
        }

        private void OnApplicationQuit()
        {
            DisposeResources();
        }

        private void DisposeResources()
        {
            if (isDisposed)
                return;

            try
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
            catch (ObjectDisposedException)
            {
                // Already disposed, ignore
            }

            // Cleanup DI container
            container?.ClearSingletons();
            container = null;

            isDisposed = true;
        }

        // Public API for manual startup control
        public void StartInitialization()
        {
            if (!isInitialized)
            {
                StartCoroutine(InitializeGame());
            }
        }

        public void CancelInitialization()
        {
            cancellationTokenSource?.Cancel();
        }

        // Static access for global use
        public static GameInitiator Instance { get; private set; }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}