using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Interfaces;
using Core.Commands;
using Core.Data;

namespace UI.Controllers
{
    /// <summary>
    /// Controller for the contracts screen in the tablet UI.
    /// Handles contract display, filtering, and interaction.
    /// </summary>
    public class ContractsScreenController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private ScrollRect availableContractsScroll;
        [SerializeField] private ScrollRect activeContractsScroll;
        [SerializeField] private TMP_Text totalEarningsText;
        [SerializeField] private TMP_Text successRateText;
        [SerializeField] private TMP_Text salaryCostText;
        [SerializeField] private Button refreshContractsButton;

        [Header("Filters")]
        [SerializeField] private TMP_Dropdown regionFilter;
        [SerializeField] private TMP_Dropdown difficultyFilter;

        // Prefabs
        [SerializeField] private GameObject contractItemPrefab;

        // Dependencies
        private IContractsService contractsService;
        private ICrewService crewService;
        private TabletCommandInvoker commandInvoker;

        // UI state
        private List<GameObject> availableContractItems = new List<GameObject>();
        private List<GameObject> activeContractItems = new List<GameObject>();

        public void Initialize(
            IContractsService contractsService,
            ICrewService crewService,
            TabletCommandInvoker commandInvoker)
        {
            this.contractsService = contractsService ?? throw new ArgumentNullException(nameof(contractsService));
            this.crewService = crewService ?? throw new ArgumentNullException(nameof(crewService));
            this.commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));

            InitializeFilters();
            InitializeButtons();
            SubscribeToEvents();
        }

        private void InitializeFilters()
        {
            if (regionFilter != null)
            {
                regionFilter.ClearOptions();
                regionFilter.options.Add(new TMP_Dropdown.OptionData("All Regions"));

                // Add unique regions from available contracts
                var regions = contractsService.GetAvailableContracts()
                    .Select(c => c.Region)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();

                foreach (var region in regions)
                {
                    regionFilter.options.Add(new TMP_Dropdown.OptionData(region));
                }

                regionFilter.onValueChanged.AddListener(_ => RefreshScreen());
            }

            if (difficultyFilter != null)
            {
                difficultyFilter.ClearOptions();
                difficultyFilter.options.Add(new TMP_Dropdown.OptionData("All Difficulties"));

                var difficulties = Enum.GetValues(typeof(ContractDifficulty))
                    .Cast<ContractDifficulty>()
                    .Select(d => d.ToString())
                    .ToList();

                foreach (var difficulty in difficulties)
                {
                    difficultyFilter.options.Add(new TMP_Dropdown.OptionData(difficulty));
                }

                difficultyFilter.onValueChanged.AddListener(_ => RefreshScreen());
            }
        }

        private void InitializeButtons()
        {
            if (refreshContractsButton != null)
            {
                refreshContractsButton.onClick.AddListener(() => contractsService.RefreshContracts());
            }
        }

        private void SubscribeToEvents()
        {
            if (contractsService != null)
            {
                contractsService.OnContractAccepted += RefreshScreen;
                contractsService.OnContractStatusChanged += RefreshScreen;
            }

            if (crewService != null)
            {
                crewService.OnCrewHired += RefreshScreen;
                crewService.OnCrewFired += RefreshScreen;
            }
        }

        public void RefreshScreen()
        {
            ClearContractItems();
            PopulateAvailableContracts();
            PopulateActiveContracts();
            UpdateStatistics();
        }

        private void ClearContractItems()
        {
            // Clear available contracts
            foreach (var item in availableContractItems)
            {
                if (item != null)
                    Destroy(item);
            }
            availableContractItems.Clear();

            // Clear active contracts
            foreach (var item in activeContractItems)
            {
                if (item != null)
                    Destroy(item);
            }
            activeContractItems.Clear();
        }

        private void PopulateAvailableContracts()
        {
            var contracts = GetFilteredAvailableContracts();

            foreach (var contract in contracts)
            {
                var item = CreateContractItem(contract, true);
                availableContractItems.Add(item);

                // Set as child of scroll view content
                item.transform.SetParent(availableContractsScroll.content, false);
            }
        }

        private void PopulateActiveContracts()
        {
            var contracts = contractsService.GetActiveContracts();

            foreach (var contract in contracts)
            {
                var item = CreateContractItem(contract, false);
                activeContractItems.Add(item);

                // Set as child of scroll view content
                item.transform.SetParent(activeContractsScroll.content, false);
            }
        }

        private GameObject CreateContractItem(ContractData contract, bool isAvailable)
        {
            if (contractItemPrefab == null)
            {
                Debug.LogError("[ContractsScreenController] Contract item prefab not assigned");
                return null;
            }

            var item = Instantiate(contractItemPrefab);
            var itemController = item.GetComponent<ContractItemController>();

            if (itemController == null)
            {
                itemController = item.AddComponent<ContractItemController>();
            }

            itemController.Setup(contract, isAvailable, this);

            return item;
        }

        private List<ContractData> GetFilteredAvailableContracts()
        {
            var contracts = contractsService.GetAvailableContracts().ToList();

            // Region filter
            if (regionFilter != null && regionFilter.value > 0)
            {
                var selectedRegion = regionFilter.options[regionFilter.value].text;
                contracts = contracts.Where(c => c.Region == selectedRegion).ToList();
            }

            // Difficulty filter
            if (difficultyFilter != null && difficultyFilter.value > 0)
            {
                var selectedDifficulty = difficultyFilter.options[difficultyFilter.value].text;
                if (Enum.TryParse<ContractDifficulty>(selectedDifficulty, out var difficulty))
                {
                    contracts = contracts.Where(c => c.Difficulty == difficulty).ToList();
                }
            }

            return contracts;
        }

        private void UpdateStatistics()
        {
            if (totalEarningsText != null && contractsService != null)
            {
                totalEarningsText.text = $"Total Earnings: ${contractsService.GetTotalEarnings():N0}";
            }

            if (successRateText != null && contractsService != null)
            {
                successRateText.text = $"Success Rate: {contractsService.GetSuccessRate():F1}%";
            }

            if (salaryCostText != null && crewService != null)
            {
                salaryCostText.text = $"Monthly Salaries: ${crewService.GetTotalSalaryCost():N0}";
            }
        }

        /// <summary>
        /// Handles contract acceptance.
        /// </summary>
        public void AcceptContract(ContractData contract)
        {
            if (contract == null) return;

            var command = new AcceptContractCommand(contractsService, crewService, contract.Id);
            commandInvoker.ExecuteCommand(command);
        }

        /// <summary>
        /// Handles contract completion.
        /// </summary>
        public void CompleteContract(ContractData contract)
        {
            if (contract == null) return;

            var command = new CompleteContractCommand(contractsService, crewService, contract.Id);
            commandInvoker.ExecuteCommand(command);
        }

        /// <summary>
        /// Handles contract failure.
        /// </summary>
        public void FailContract(ContractData contract, string reason = "Mission failed")
        {
            if (contract == null) return;

            contractsService.FailContract(contract.Id, reason);
        }

        /// <summary>
        /// Shows detailed contract information.
        /// </summary>
        public void ShowContractDetails(ContractData contract)
        {
            // This could open a modal or detail panel
            Debug.Log($"[ContractsScreenController] Showing details for: {contract.Title}");
        }

        /// <summary>
        /// Gets crew effectiveness for a specific contract.
        /// </summary>
        public float GetCrewEffectivenessForContract(ContractData contract)
        {
            if (contract == null || contract.RequiredSkills.Length == 0) return 0f;

            var totalEffectiveness = 0f;
            var skillCount = 0;

            foreach (var skill in contract.RequiredSkills)
            {
                // Try to find crew with matching specializations
                var crewForSkill = crewService.GetCrewBySpecialization(skill);
                if (crewForSkill.Count > 0)
                {
                    totalEffectiveness += crewForSkill.Average(c => c.GetRoleEffectiveness());
                    skillCount++;
                }
            }

            return skillCount > 0 ? totalEffectiveness / skillCount : 0f;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (contractsService != null)
            {
                contractsService.OnContractAccepted -= RefreshScreen;
                contractsService.OnContractStatusChanged -= RefreshScreen;
            }

            if (crewService != null)
            {
                crewService.OnCrewHired -= RefreshScreen;
                crewService.OnCrewFired -= RefreshScreen;
            }
        }
    }

    /// <summary>
    /// Controller for individual contract UI items.
    /// </summary>
    public class ContractItemController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private TMP_Text regionText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button completeButton;
        [SerializeField] private Button detailsButton;
        [SerializeField] private Image backgroundImage;

        private ContractData contract;
        private ContractsScreenController parentController;
        private bool isAvailable;

        public void Setup(ContractData contract, bool isAvailable, ContractsScreenController parentController)
        {
            this.contract = contract;
            this.isAvailable = isAvailable;
            this.parentController = parentController;

            SetupUI();
        }

        private void SetupUI()
        {
            if (titleText != null)
                titleText.text = contract.Title;

            if (descriptionText != null)
                descriptionText.text = contract.Description;

            if (rewardText != null)
                rewardText.text = $"${contract.Reward:N0}";

            if (difficultyText != null)
                difficultyText.text = contract.Difficulty.ToString();

            if (regionText != null)
                regionText.text = contract.Region;

            if (timeText != null)
            {
                var remaining = contract.GetRemainingTime();
                timeText.text = remaining > TimeSpan.Zero ?
                    $"{remaining.Hours}h {remaining.Minutes}m" : "Expired";
            }

            // Button setup
            if (acceptButton != null)
            {
                acceptButton.gameObject.SetActive(isAvailable);
                acceptButton.onClick.AddListener(() => parentController.AcceptContract(contract));
            }

            if (completeButton != null)
            {
                completeButton.gameObject.SetActive(!isAvailable);
                completeButton.onClick.AddListener(() => parentController.CompleteContract(contract));
            }

            if (detailsButton != null)
            {
                detailsButton.onClick.AddListener(() => parentController.ShowContractDetails(contract));
            }

            // Visual styling based on difficulty
            if (backgroundImage != null)
            {
                backgroundImage.color = GetDifficultyColor(contract.Difficulty);
            }
        }

        private Color GetDifficultyColor(ContractDifficulty difficulty)
        {
            return difficulty switch
            {
                ContractDifficulty.Easy => Color.green * 0.3f,
                ContractDifficulty.Medium => Color.yellow * 0.3f,
                ContractDifficulty.Hard => Color.red * 0.3f,
                ContractDifficulty.Expert => Color.magenta * 0.3f,
                _ => Color.white * 0.3f
            };
        }
    }
}