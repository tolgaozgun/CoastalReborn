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
    /// Controller for the intelligence screen in the tablet UI.
    /// Handles intel display, verification, and cross-referencing.
    /// </summary>
    public class IntelScreenController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private ScrollRect intelScroll;
        [SerializeField] private TMP_Text totalIntelText;
        [SerializeField] private TMP_Text verifiedIntelText;
        [SerializeField] private Button refreshIntelButton;

        [Header("Filters")]
        [SerializeField] private TMP_Dropdown regionFilter;
        [SerializeField] private TMP_Dropdown sourceFilter;
        [SerializeField] private TMP_Dropdown credibilityFilter;

        // Prefabs
        [SerializeField] private GameObject intelItemPrefab;

        // Dependencies
        private IIntelService intelService;
        private ICrewService crewService;
        private TabletCommandInvoker commandInvoker;

        // UI state
        private List<GameObject> intelItems = new List<GameObject>();

        public void Initialize(
            IIntelService intelService,
            ICrewService crewService,
            TabletCommandInvoker commandInvoker)
        {
            this.intelService = intelService ?? throw new ArgumentNullException(nameof(intelService));
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

                var regions = intelService.GetIntelEntries()
                    .Select(i => i.Region)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();

                foreach (var region in regions)
                {
                    regionFilter.options.Add(new TMP_Dropdown.OptionData(region));
                }

                regionFilter.onValueChanged.AddListener(_ => RefreshScreen());
            }

            if (sourceFilter != null)
            {
                sourceFilter.ClearOptions();
                sourceFilter.options.Add(new TMP_Dropdown.OptionData("All Sources"));

                var sources = Enum.GetValues(typeof(IntelSource))
                    .Cast<IntelSource>()
                    .Select(s => s.ToString())
                    .ToList();

                foreach (var source in sources)
                {
                    sourceFilter.options.Add(new TMP_Dropdown.OptionData(source));
                }

                sourceFilter.onValueChanged.AddListener(_ => RefreshScreen());
            }

            if (credibilityFilter != null)
            {
                credibilityFilter.ClearOptions();
                credibilityFilter.options.Add(new TMP_Dropdown.OptionData("All Levels"));
                credibilityFilter.options.Add(new TMP_Dropdown.OptionData("High (80-100)"));
                credibilityFilter.options.Add(new TMP_Dropdown.OptionData("Medium (50-79)"));
                credibilityFilter.options.Add(new TMP_Dropdown.OptionData("Low (0-49)"));

                credibilityFilter.onValueChanged.AddListener(_ => RefreshScreen());
            }
        }

        private void InitializeButtons()
        {
            if (refreshIntelButton != null)
            {
                refreshIntelButton.onClick.AddListener(RefreshScreen);
            }
        }

        private void SubscribeToEvents()
        {
            if (intelService != null)
            {
                intelService.OnIntelReceived += RefreshScreen;
                intelService.OnIntelCredibilityChanged += RefreshScreen;
            }
        }

        public void RefreshScreen()
        {
            ClearIntelItems();
            PopulateIntel();
            UpdateStatistics();
        }

        private void ClearIntelItems()
        {
            foreach (var item in intelItems)
            {
                if (item != null)
                    Destroy(item);
            }
            intelItems.Clear();
        }

        private void PopulateIntel()
        {
            var intel = GetFilteredIntel();

            foreach (var intelEntry in intel)
            {
                var item = CreateIntelItem(intelEntry);
                intelItems.Add(item);

                // Set as child of scroll view content
                item.transform.SetParent(intelScroll.content, false);
            }
        }

        private GameObject CreateIntelItem(IntelEntry intelEntry)
        {
            if (intelItemPrefab == null)
            {
                Debug.LogError("[IntelScreenController] Intel item prefab not assigned");
                return null;
            }

            var item = Instantiate(intelItemPrefab);
            var itemController = item.GetComponent<IntelItemController>();

            if (itemController == null)
            {
                itemController = item.AddComponent<IntelItemController>();
            }

            itemController.Setup(intelEntry, this);

            return item;
        }

        private List<IntelEntry> GetFilteredIntel()
        {
            var intel = intelService.GetIntelEntries().ToList();

            // Region filter
            if (regionFilter != null && regionFilter.value > 0)
            {
                var selectedRegion = regionFilter.options[regionFilter.value].text;
                intel = intel.Where(i => i.Region == selectedRegion).ToList();
            }

            // Source filter
            if (sourceFilter != null && sourceFilter.value > 0)
            {
                var selectedSource = sourceFilter.options[sourceFilter.value].text;
                if (Enum.TryParse<IntelSource>(selectedSource, out var source))
                {
                    intel = intel.Where(i => i.Source == source).ToList();
                }
            }

            // Credibility filter
            if (credibilityFilter != null && credibilityFilter.value > 0)
            {
                switch (credibilityFilter.value)
                {
                    case 1: // High (80-100)
                        intel = intel.Where(i => i.Credibility >= 80).ToList();
                        break;
                    case 2: // Medium (50-79)
                        intel = intel.Where(i => i.Credibility >= 50 && i.Credibility < 80).ToList();
                        break;
                    case 3: // Low (0-49)
                        intel = intel.Where(i => i.Credibility < 50).ToList();
                        break;
                }
            }

            return intel.OrderByDescending(i => i.Timestamp).ToList();
        }

        private void UpdateStatistics()
        {
            var intel = intelService.GetIntelEntries();

            if (totalIntelText != null)
            {
                totalIntelText.text = $"Total Intel: {intel.Count}";
            }

            if (verifiedIntelText != null)
            {
                var verifiedCount = intel.Count(i => i.Credibility >= 75);
                verifiedIntelText.text = $"Verified: {verifiedCount}";
            }
        }

        /// <summary>
        /// Handles intel verification with crew member.
        /// </summary>
        public void VerifyIntel(IntelEntry intelEntry, string crewId)
        {
            if (intelEntry == null) return;

            var command = new VerifyIntelCommand(intelService, crewService, intelEntry.Id, crewId);
            commandInvoker.ExecuteCommand(command);
        }

        /// <summary>
        /// Handles intel cross-referencing.
        /// </summary>
        public void CrossReferenceIntel(IntelEntry intelEntry)
        {
            if (intelEntry == null) return;

            var command = new CrossReferenceIntelCommand(intelService, intelEntry.Id);
            commandInvoker.ExecuteCommand(command);
        }

        /// <summary>
        /// Shows detailed intel information.
        /// </summary>
        public void ShowIntelDetails(IntelEntry intelEntry)
        {
            // This could open a modal or detail panel
            Debug.Log($"[IntelScreenController] Showing details for: {intelEntry.Title}");
        }

        /// <summary>
        /// Gets crew members suitable for intel verification.
        /// </summary>
        public IReadOnlyList<Core.Data.CrewData> GetSuitableVerifiers()
        {
            var crew = crewService.GetHiredCrew();
            return crew.Where(c =>
                c.AvailableRoles.Contains(Core.Data.CrewRole.Analyst) ||
                c.AvailableRoles.Contains(Core.Data.CrewRole.Inspector)).ToList();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (intelService != null)
            {
                intelService.OnIntelReceived -= RefreshScreen;
                intelService.OnIntelCredibilityChanged -= RefreshScreen;
            }
        }
    }

    /// <summary>
    /// Controller for individual intel UI items.
    /// </summary>
    public class IntelItemController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text regionText;
        [SerializeField] private TMP_Text sourceText;
        [SerializeField] private TMP_Text credibilityText;
        [SerializeField] private TMP_Text timestampText;
        [SerializeField] private Button verifyButton;
        [SerializeField] private Button crossReferenceButton;
        [SerializeField] private Button detailsButton;
        [SerializeField] private Image credibilityBar;
        [SerializeField] private Image backgroundImage;

        private IntelEntry intelEntry;
        private IntelScreenController parentController;

        public void Setup(IntelEntry intelEntry, IntelScreenController parentController)
        {
            this.intelEntry = intelEntry;
            this.parentController = parentController;

            SetupUI();
        }

        private void SetupUI()
        {
            if (titleText != null)
                titleText.text = intelEntry.Title;

            if (descriptionText != null)
                descriptionText.text = intelEntry.Description;

            if (regionText != null)
                regionText.text = intelEntry.Region;

            if (sourceText != null)
                sourceText.text = intelEntry.Source.ToString();

            if (credibilityText != null)
                credibilityText.text = $"Credibility: {intelEntry.Credibility}%";

            if (timestampText != null)
                timestampText.text = intelEntry.Timestamp.ToString("MM/dd HH:mm");

            // Button setup
            if (verifyButton != null)
            {
                verifyButton.onClick.AddListener(() => ShowVerificationDialog());
            }

            if (crossReferenceButton != null)
            {
                crossReferenceButton.onClick.AddListener(() => parentController.CrossReferenceIntel(intelEntry));
            }

            if (detailsButton != null)
            {
                detailsButton.onClick.AddListener(() => parentController.ShowIntelDetails(intelEntry));
            }

            // Visual elements
            UpdateCredibilityBar();
            UpdateBackground();
        }

        private void UpdateCredibilityBar()
        {
            if (credibilityBar != null)
            {
                credibilityBar.fillAmount = intelEntry.Credibility / 100f;
                credibilityBar.color = GetCredibilityColor(intelEntry.Credibility);
            }
        }

        private void UpdateBackground()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = GetSourceColor(intelEntry.Source);
            }
        }

        private Color GetCredibilityColor(int credibility)
        {
            if (credibility >= 75) return Color.green;
            if (credibility >= 50) return Color.yellow;
            return Color.red;
        }

        private Color GetSourceColor(IntelSource source)
        {
            return source switch
            {
                IntelSource.Informant => Color.magenta * 0.3f,
                IntelSource.PatrolLog => Color.blue * 0.3f,
                IntelSource.Scan => Color.cyan * 0.3f,
                IntelSource.CivilianReport => Color.gray * 0.3f,
                IntelSource.InterceptedCommunication => Color.red * 0.3f,
                _ => Color.white * 0.3f
            };
        }

        private void ShowVerificationDialog()
        {
            var verifiers = parentController.GetSuitableVerifiers();
            if (verifiers.Count > 0)
            {
                // For now, use the first available verifier
                var verifier = verifiers.First();
                parentController.VerifyIntel(intelEntry, verifier.Id);
            }
            else
            {
                Debug.LogWarning("[IntelItemController] No suitable crew members available for intel verification");
            }
        }
    }
}