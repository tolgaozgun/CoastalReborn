using System;
using UnityEngine;
using Zenject;

namespace Core.Installers
{
    /// <summary>
    /// Installer for tablet-related services and UI components.
    /// Follows DI principles and binds implementations to interfaces.
    /// Organized per domain as specified in best practices.
    /// </summary>
    public class TabletInstaller : MonoInstaller
    {
        [Header("Service Configurations")]
        [SerializeField] private CrewServiceConfig crewServiceConfig;
        [SerializeField] private ContractsServiceConfig contractsServiceConfig;
        [SerializeField] private IntelServiceConfig intelServiceConfig;
        [SerializeField] private UpgradeServiceConfig upgradeServiceConfig;

        [Header("UI Prefabs")]
        [SerializeField] private GameObject tabletUIPrefab;
        [SerializeField] private GameObject contractItemPrefab;
        [SerializeField] private GameObject crewItemPrefab;
        [SerializeField] private GameObject intelItemPrefab;
        [SerializeField] private GameObject upgradeItemPrefab;

        public override void InstallBindings()
        {
            InstallServices();
            InstallUIComponents();
            InstallCommands();
            InstallFactories();
        }

        /// <summary>
        /// Binds core services to their interfaces.
        /// Services provide reusable, non-visual functionality without game state dependencies.
        /// </summary>
        private void InstallServices()
        {
            // Crew Service
            Container.BindInterfacesAndSelfTo<CrewService>()
                .AsSingle()
                .WithArguments(crewServiceConfig)
                .NonLazy();

            // Contracts Service
            Container.BindInterfacesAndSelfTo<ContractsService>()
                .AsSingle()
                .WithArguments(contractsServiceConfig)
                .NonLazy();

            // Intel Service
            Container.BindInterfacesAndSelfTo<IntelService>()
                .AsSingle()
                .WithArguments(intelServiceConfig)
                .NonLazy();

            // Upgrade Service
            Container.BindInterfacesAndSelfTo<UpgradeService>()
                .AsSingle()
                .WithArguments(upgradeServiceConfig)
                .NonLazy();

            // Command Invoker
            Container.BindInterfacesAndSelfTo<TabletCommandInvoker>()
                .AsSingle()
                .NonLazy();

            Debug.Log("[TabletInstaller] Services bound to container");
        }

        /// <summary>
        /// Binds UI components and controllers following MVC pattern.
        /// Views contain only visual behavior, never game logic.
        /// </summary>
        private void InstallUIComponents()
        {
            // Tablet UI Controller
            Container.Bind<TabletUIController>()
                .FromComponentInNewPrefab(tabletUIPrefab)
                .AsSingle()
                .UnderTransform(transform)
                .NonLazy();

            // Screen Controllers (bound as interfaces for loose coupling)
            Container.Bind<ContractsScreenController>()
                .FromNewComponentOnRoot()
                .AsSingle()
                .NonLazy();

            Container.Bind<CrewScreenController>()
                .FromNewComponentOnRoot()
                .AsSingle()
                .NonLazy();

            Container.Bind<IntelScreenController>()
                .FromNewComponentOnRoot()
                .AsSingle()
                .NonLazy();

            Container.Bind<UpgradesScreenController>()
                .FromNewComponentOnRoot()
                .AsSingle()
                .NonLazy();

            Debug.Log("[TabletInstaller] UI components bound to container");
        }

        /// <summary>
        /// Binds command factories for complex operation orchestration.
        /// Commands coordinate multiple systems and prevent event spaghetti.
        /// </summary>
        private void InstallCommands()
        {
            Container.Bind<ContractCommandFactory>()
                .AsSingle()
                .NonLazy();

            Container.Bind<CrewCommandFactory>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IntelCommandFactory>()
                .AsSingle()
                .NonLazy();

            Container.Bind<UpgradeCommandFactory>()
                .AsSingle()
                .NonLazy();

            Debug.Log("[TabletInstaller] Command factories bound to container");
        }

        /// <summary>
        /// Binds factories for object creation with DI and pooling support.
        /// Centralizes object creation logic as per best practices.
        /// </summary>
        private void InstallFactories()
        {
            Container.BindFactory<ContractData, ContractItemController, ContractItemController.Factory>()
                .FromComponentInNewPrefab(contractItemPrefab);

            Container.BindFactory<CrewData, CrewItemController, CrewItemController.Factory>()
                .FromComponentInNewPrefab(crewItemPrefab);

            Container.BindFactory<IntelEntry, IntelItemController, IntelItemController.Factory>()
                .FromComponentInNewPrefab(intelItemPrefab);

            Container.BindFactory<UpgradeNode, UpgradeItemController, UpgradeItemController.Factory>()
                .FromComponentInNewPrefab(upgradeItemPrefab);

            Debug.Log("[TabletInstaller] Factories bound to container");
        }
    }

    /// <summary>
    /// Configuration data for Crew Service.
    /// Follows dependency injection best practices.
    /// </summary>
    [Serializable]
    public class CrewServiceConfig
    {
        [Header("Economy Settings")]
        public int startingFunds = 10000;
        public float crewRefreshInterval = 300f; // 5 minutes

        [Header("Generation Settings")]
        public int maxAvailableCrew = 8;
        public int minSkillLevel = 3;
        public int maxSkillLevel = 10;
    }

    /// <summary>
    /// Configuration data for Contracts Service.
    /// </summary>
    [Serializable]
    public class ContractsServiceConfig
    {
        [Header("Generation Settings")]
        public int maxContracts = 10;
        public float refreshChance = 0.3f;
        public float checkInterval = 60f; // 1 minute

        [Header("Contract Parameters")]
        public int minReward = 500;
        public int maxReward = 10000;
    }

    /// <summary>
    /// Configuration data for Intel Service.
    /// </summary>
    [Serializable]
    public class IntelServiceConfig
    {
        [Header("Intel Generation")]
        public int maxIntelEntries = 20;
        public float baseCredibilityDecay = 0.1f;
        public int verificationTime = 30; // seconds
    }

    /// <summary>
    /// Configuration data for Upgrade Service.
    /// </summary>
    [Serializable]
    public class UpgradeServiceConfig
    {
        [Header("Upgrade System")]
        public int startingUpgradePoints = 5;
        public float pointGainRate = 0.1f;
        public int maxTier = 5;
    }
}