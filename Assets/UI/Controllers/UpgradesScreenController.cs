using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMP.Text;
using Core.Interfaces;
using Core.Commands;

namespace UI.Controllers
{
    /// <summary>
    /// Controller for the upgrades screen in the tablet UI.
    /// Handles upgrade display, purchasing, and application.
    /// </summary>
    public class UpgradesScreenController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private ScrollRect upgradesScroll;
        [SerializeField] private TMP_Text upgradePointsText;
        [SerializeField] private TMP_Text purchasedUpgradesText;
        [SerializeField] private Button resetUpgradesButton;

        [Header("Filters")]
        [SerializeField] private TMP_Dropdown categoryFilter;
        [SerializeField] private Toggle purchasedToggle;
        [SerializeField] private Toggle availableToggle;

        // Prefabs
        [SerializeField] private GameObject upgradeItemPrefab;

        // Dependencies
        private IUpgradeService upgradeService;
        private ICrewService crewService;
        private TabletCommandInvoker commandInvoker;

        // UI state
        private List<GameObject> upgradeItems = new List<GameObject>();

        public void Initialize(
            IUpgradeService upgradeService,
            ICrewService crewService,
            TabletCommandInvoker commandInvoker)
        {
            this.upgradeService = upgradeService ?? throw new ArgumentNullException(nameof(upgradeService));
            this.crewService = crewService ?? throw new ArgumentNullException(nameof(crewService));
            this.commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));

            InitializeFilters();
            InitializeButtons();
            SubscribeToEvents();
        }

        private void InitializeFilters()
        {
            if (categoryFilter != null)
            {
                categoryFilter.ClearOptions();
                categoryFilter.options.Add(new TMP_Dropdown.OptionData("All Categories"));

                var categories = Enum.GetValues(typeof(UpgradeCategory))
                    .Cast<UpgradeCategory>()
                    .Select(c => c.ToString())
                    .ToList();

                foreach (var category in categories)
                {
                    categoryFilter.options.Add(new TMP_Dropdown.OptionData(category));
                }

                categoryFilter.onValueChanged.AddListener(_ => RefreshScreen());
            }

            if (purchasedToggle != null)
            {
                purchasedToggle.isOn = true;
                purchasedToggle.onValueChanged.AddListener(_ => RefreshScreen());
            }

            if (availableToggle != null)
            {
                availableToggle.isOn = true;
                availableToggle.onValueChanged.AddListener(_ => RefreshScreen());
            }
        }

        private void InitializeButtons()
        {
            if (resetUpgradesButton != null)
            {
                resetUpgradesButton.onClick.AddListener(() => {
                    if (ConfirmResetUpgrades())
                    {
                        upgradeService.ResetUpgrades();
                    }
                });
            }
        }

        private void SubscribeToEvents()
        {
            if (upgradeService != null)
            {
                upgradeService.OnUpgradePurchased += RefreshScreen;
                upgradeService.OnUpgradePointsEarned += RefreshScreen;
            }
        }

        public void RefreshScreen()
        {
            ClearUpgradeItems();
            PopulateUpgrades();
            UpdateStatistics();
        }

        private void ClearUpgradeItems()
        {
            foreach (var item in upgradeItems)
            {
                if (item != null)
                    Destroy(item);
            }
            upgradeItems.Clear();
        }

        private void PopulateUpgrades()
        {
            var upgrades = GetFilteredUpgrades();

            foreach (var upgrade in upgrades)
            {
                var item = CreateUpgradeItem(upgrade);
                upgradeItems.Add(item);

                // Set as child of scroll view content
                item.transform.SetParent(upgradesScroll.content, false);
            }
        }

        private GameObject CreateUpgradeItem(UpgradeNode upgrade)
        {
            if (upgradeItemPrefab == null)
            {
                Debug.LogError("[UpgradesScreenController] Upgrade item prefab not assigned");
                return null;
            }

            var item = Instantiate(upgradeItemPrefab);
            var itemController = item.GetComponent<UpgradeItemController>();

            if (itemController == null)
            {
                itemController = item.AddComponent<UpgradeItemController>();
            }

            itemController.Setup(upgrade, this);

            return item;
        }

        private List<UpgradeNode> GetFilteredUpgrades()
        {
            var upgrades = upgradeService.GetAllUpgradeNodes().ToList();

            // Category filter
            if (categoryFilter != null && categoryFilter.value > 0)
            {
                var selectedCategory = categoryFilter.options[categoryFilter.value].text;
                if (Enum.TryParse<UpgradeCategory>(selectedCategory, out var category))
                {
                    upgrades = upgrades.Where(u => u.Category == category).ToList();
                }
            }

            // Purchased filter
            if (purchasedToggle != null && !purchasedToggle.isOn)
            {
                upgrades = upgrades.Where(u => !u.IsPurchased).ToList();
            }

            // Available filter
            if (availableToggle != null && !availableToggle.isOn)
            {
                upgrades = upgrades.Where(u => u.IsPurchased).ToList();
            }

            return upgrades.OrderBy(u => u.Category).ThenBy(u => u.Cost).ToList();
        }

        private void UpdateStatistics()
        {
            if (upgradePointsText != null && upgradeService != null)
            {
                upgradePointsText.text = $"Upgrade Points: {upgradeService.AvailablePoints}";
            }

            if (purchasedUpgradesText != null && upgradeService != null)
            {
                var purchasedCount = upgradeService.GetAllUpgradeNodes().Count(u => u.IsPurchased);
                var totalCount = upgradeService.GetAllUpgradeNodes().Count;
                purchasedUpgradesText.text = $"Purchased: {purchasedCount}/{totalCount}";
            }
        }

        /// <summary>
        /// Handles upgrade purchase.
        /// </summary>
        public void PurchaseUpgrade(UpgradeNode upgrade)
        {
            if (upgrade == null) return;

            var command = new PurchaseUpgradeCommand(crewService, upgradeService, upgrade.Id);
            commandInvoker.ExecuteCommand(command);
        }

        /// <summary>
        /// Shows detailed upgrade information.
        /// </summary>
        public void ShowUpgradeDetails(UpgradeNode upgrade)
        {
            // This could open a modal or detail panel
            Debug.Log($"[UpgradesScreenController] Showing details for: {upgrade.Name}");
        }

        /// <summary>
        /// Gets total effect value for an effect type.
        /// </summary>
        public float GetTotalEffectValue(UpgradeEffectType effectType)
        {
            return upgradeService.GetTotalEffectValue(effectType);
        }

        /// <summary>
        /// Checks if an upgrade can be purchased.
        /// </summary>
        public bool CanPurchaseUpgrade(UpgradeNode upgrade)
        {
            return upgradeService.CanPurchaseNode(upgrade.Id);
        }

        /// <summary>
        /// Gets upgrade prerequisites status.
        /// </summary>
        public (bool Met, List<string> Missing) GetPrerequisiteStatus(UpgradeNode upgrade)
        {
            var missingPrereqs = new List<string>();
            var allMet = true;

            foreach (var prereqId in upgrade.PrerequisiteIds)
            {
                if (!upgradeService.IsNodePurchased(prereqId))
                {
                    var prereqNode = upgradeService.GetUpgradeNode(prereqId);
                    if (prereqNode != null)
                    {
                        missingPrereqs.Add(prereqNode.Name);
                        allMet = false;
                    }
                }
            }

            return (allMet, missingPrereqs);
        }

        private bool ConfirmResetUpgrades()
        {
            // In a real implementation, this would show a confirmation dialog
            Debug.LogWarning("[UpgradesScreenController] Reset upgrades confirmation not implemented");
            return false;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (upgradeService != null)
            {
                upgradeService.OnUpgradePurchased -= RefreshScreen;
                upgradeService.OnUpgradePointsEarned -= RefreshScreen;
            }
        }
    }

    /// <summary>
    /// Controller for individual upgrade UI items.
    /// </summary>
    public class UpgradeItemController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text categoryText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button detailsButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject purchasedIndicator;
        [SerializeField] private GameObject lockedIndicator;

        private UpgradeNode upgrade;
        private UpgradesScreenController parentController;

        public void Setup(UpgradeNode upgrade, UpgradesScreenController parentController)
        {
            this.upgrade = upgrade;
            this.parentController = parentController;

            SetupUI();
        }

        private void SetupUI()
        {
            if (nameText != null)
                nameText.text = upgrade.Name;

            if (descriptionText != null)
                descriptionText.text = upgrade.Description;

            if (categoryText != null)
                categoryText.text = upgrade.Category.ToString();

            if (costText != null)
                costText.text = $"Cost: {upgrade.GetCurrentCost()}";

            if (levelText != null)
            {
                if (upgrade.IsPurchased)
                {
                    levelText.text = $"Level: {upgrade.CurrentLevel}/{upgrade.MaxLevel}";
                }
                else
                {
                    levelText.text = "Not Purchased";
                }
            }

            // Button setup
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(() => parentController.PurchaseUpgrade(upgrade));
                UpdatePurchaseButton();
            }

            if (detailsButton != null)
            {
                detailsButton.onClick.AddListener(() => parentController.ShowUpgradeDetails(upgrade));
            }

            // Indicators
            if (purchasedIndicator != null)
                purchasedIndicator.SetActive(upgrade.IsPurchased);

            if (lockedIndicator != null)
                UpdateLockIndicator();

            // Visual styling
            if (backgroundImage != null)
            {
                backgroundImage.color = GetCategoryColor(upgrade.Category);
            }
        }

        private void UpdatePurchaseButton()
        {
            if (purchaseButton == null) return;

            var canPurchase = parentController.CanPurchaseUpgrade(upgrade);
            purchaseButton.interactable = canPurchase && !upgrade.IsPurchased && !upgrade.IsMaxLevel();

            if (upgrade.IsPurchased && upgrade.IsMaxLevel())
            {
                purchaseButton.gameObject.SetActive(false);
            }
            else
            {
                purchaseButton.gameObject.SetActive(true);
                purchaseButton.GetComponentInChildren<TMP_Text>()?.SetText(
                    upgrade.IsPurchased ? "Upgrade" : "Purchase"
                );
            }
        }

        private void UpdateLockIndicator()
        {
            if (lockedIndicator == null) return;

            var (met, missing) = parentController.GetPrerequisiteStatus(upgrade);
            lockedIndicator.SetActive(!met);
        }

        private Color GetCategoryColor(UpgradeCategory category)
        {
            return category switch
            {
                UpgradeCategory.Scanner => Color.cyan * 0.3f,
                UpgradeCategory.Engine => Color.yellow * 0.3f,
                UpgradeCategory.Boarding => Color.red * 0.3f,
                UpgradeCategory.Intel => Color.magenta * 0.3f,
                UpgradeCategory.Armor => Color.gray * 0.3f,
                _ => Color.white * 0.3f
            };
        }
    }
}