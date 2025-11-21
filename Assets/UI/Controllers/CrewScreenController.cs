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
    /// Controller for the crew management screen in the tablet UI.
    /// Handles crew display, hiring, firing, and role assignment.
    /// </summary>
    public class CrewScreenController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private ScrollRect availableCrewScroll;
        [SerializeField] private ScrollRect hiredCrewScroll;
        [SerializeField] private TMP_Text totalCrewText;
        [SerializeField] private TMP_Text totalSalaryText;
        [SerializeField] private TMP_Text fundsText;
        [SerializeField] private Button refreshCrewButton;

        [Header("Role Filters")]
        [SerializeField] private TMP_Dropdown roleFilter;

        // Prefabs
        [SerializeField] private GameObject crewItemPrefab;

        // Dependencies
        private ICrewService crewService;
        private TabletCommandInvoker commandInvoker;

        // UI state
        private List<GameObject> availableCrewItems = new List<GameObject>();
        private List<GameObject> hiredCrewItems = new List<GameObject>();

        public void Initialize(ICrewService crewService, TabletCommandInvoker commandInvoker)
        {
            this.crewService = crewService ?? throw new ArgumentNullException(nameof(crewService));
            this.commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));

            InitializeFilters();
            InitializeButtons();
            SubscribeToEvents();
        }

        private void InitializeFilters()
        {
            if (roleFilter != null)
            {
                roleFilter.ClearOptions();
                roleFilter.options.Add(new TMP_Dropdown.OptionData("All Roles"));

                var roles = Enum.GetValues(typeof(CrewRole))
                    .Cast<CrewRole>()
                    .Where(r => r != CrewRole.Unassigned)
                    .Select(r => r.ToString())
                    .ToList();

                foreach (var role in roles)
                {
                    roleFilter.options.Add(new TMP_Dropdown.OptionData(role));
                }

                roleFilter.onValueChanged.AddListener(_ => RefreshScreen());
            }
        }

        private void InitializeButtons()
        {
            if (refreshCrewButton != null)
            {
                refreshCrewButton.onClick.AddListener(() => crewService.RefreshAvailableCrew());
            }
        }

        private void SubscribeToEvents()
        {
            if (crewService != null)
            {
                crewService.OnCrewHired += RefreshScreen;
                crewService.OnCrewFired += RefreshScreen;
                crewService.OnCrewRoleChanged += RefreshScreen;
            }
        }

        public void RefreshScreen()
        {
            ClearCrewItems();
            PopulateAvailableCrew();
            PopulateHiredCrew();
            UpdateStatistics();
        }

        private void ClearCrewItems()
        {
            // Clear available crew
            foreach (var item in availableCrewItems)
            {
                if (item != null)
                    Destroy(item);
            }
            availableCrewItems.Clear();

            // Clear hired crew
            foreach (var item in hiredCrewItems)
            {
                if (item != null)
                    Destroy(item);
            }
            hiredCrewItems.Clear();
        }

        private void PopulateAvailableCrew()
        {
            var crew = GetFilteredAvailableCrew();

            foreach (var crewMember in crew)
            {
                var item = CreateCrewItem(crewMember, true);
                availableCrewItems.Add(item);

                // Set as child of scroll view content
                item.transform.SetParent(availableCrewScroll.content, false);
            }
        }

        private void PopulateHiredCrew()
        {
            var crew = crewService.GetHiredCrew();

            foreach (var crewMember in crew)
            {
                var item = CreateCrewItem(crewMember, false);
                hiredCrewItems.Add(item);

                // Set as child of scroll view content
                item.transform.SetParent(hiredCrewScroll.content, false);
            }
        }

        private GameObject CreateCrewItem(CrewData crewMember, bool isAvailable)
        {
            if (crewItemPrefab == null)
            {
                Debug.LogError("[CrewScreenController] Crew item prefab not assigned");
                return null;
            }

            var item = Instantiate(crewItemPrefab);
            var itemController = item.GetComponent<CrewItemController>();

            if (itemController == null)
            {
                itemController = item.AddComponent<CrewItemController>();
            }

            itemController.Setup(crewMember, isAvailable, this);

            return item;
        }

        private List<CrewData> GetFilteredAvailableCrew()
        {
            var crew = crewService.GetAvailableCrew().ToList();

            // Role filter
            if (roleFilter != null && roleFilter.value > 0)
            {
                var selectedRole = roleFilter.options[roleFilter.value].text;
                if (Enum.TryParse<CrewRole>(selectedRole, out var role))
                {
                    crew = crew.Where(c => c.AvailableRoles.Contains(role)).ToList();
                }
            }

            return crew;
        }

        private void UpdateStatistics()
        {
            var hiredCrew = crewService.GetHiredCrew();

            if (totalCrewText != null)
            {
                totalCrewText.text = $"Total Crew: {hiredCrew.Count}";
            }

            if (totalSalaryText != null)
            {
                totalSalaryText.text = $"Monthly Cost: ${crewService.GetTotalSalaryCost():N0}";
            }

            if (fundsText != null)
            {
                fundsText.text = $"Available Funds: ${crewService.GetPlayerFunds():N0}";
            }
        }

        /// <summary>
        /// Handles crew member hiring.
        /// </summary>
        public void HireCrewMember(CrewData crewMember, CrewRole role)
        {
            if (crewMember == null) return;

            var command = new HireCrewCommand(crewService, crewMember.Id, role);
            commandInvoker.ExecuteCommand(command);
        }

        /// <summary>
        /// Handles crew member firing.
        /// </summary>
        public void FireCrewMember(CrewData crewMember)
        {
            if (crewMember == null) return;

            var command = new FireCrewCommand(crewService, crewMember.Id);
            commandInvoker.ExecuteCommand(command);
        }

        /// <summary>
        /// Handles crew member role change.
        /// </summary>
        public void ChangeCrewRole(CrewData crewMember, CrewRole newRole)
        {
            if (crewMember == null) return;

            var command = new ChangeCrewRoleCommand(crewService, crewMember.Id, newRole);
            commandInvoker.ExecuteCommand(command);
        }

        /// <summary>
        /// Shows detailed crew member information.
        /// </summary>
        public void ShowCrewDetails(CrewData crewMember)
        {
            // This could open a modal or detail panel
            Debug.Log($"[CrewScreenController] Showing details for: {crewMember.Name}");
        }

        /// <summary>
        /// Gets crew effectiveness by role.
        /// </summary>
        public float GetRoleEffectiveness(CrewRole role)
        {
            return crewService.GetRoleEffectiveness(role);
        }

        /// <summary>
        /// Checks if a crew member can be assigned to a role.
        /// </summary>
        public bool CanAssignToRole(CrewData crewMember, CrewRole role)
        {
            return crewService.CanAssignToRole(crewMember.Id, role);
        }

        /// <summary>
        /// Gets crew members by specialization.
        /// </summary>
        public IReadOnlyList<CrewData> GetCrewBySpecialization(string specialization)
        {
            return crewService.GetCrewBySpecialization(specialization);
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (crewService != null)
            {
                crewService.OnCrewHired -= RefreshScreen;
                crewService.OnCrewFired -= RefreshScreen;
                crewService.OnCrewRoleChanged -= RefreshScreen;
            }
        }
    }

    /// <summary>
    /// Controller for individual crew member UI items.
    /// </summary>
    public class CrewItemController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text biographyText;
        [SerializeField] private TMP_Text skillLevelText;
        [SerializeField] private TMP_Text salaryText;
        [SerializeField] private TMP_Text roleText;
        [SerializeField] private TMP_Text specializationsText;
        [SerializeField] private Button hireButton;
        [SerializeField] private Button fireButton;
        [SerializeField] private Button changeRoleButton;
        [SerializeField] private Button detailsButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Dropdown roleDropdown;

        private CrewData crewMember;
        private CrewScreenController parentController;
        private bool isAvailable;

        public void Setup(CrewData crewMember, bool isAvailable, CrewScreenController parentController)
        {
            this.crewMember = crewMember;
            this.isAvailable = isAvailable;
            this.parentController = parentController;

            SetupUI();
        }

        private void SetupUI()
        {
            if (nameText != null)
                nameText.text = crewMember.Name;

            if (biographyText != null)
                biographyText.text = crewMember.Biography;

            if (skillLevelText != null)
                skillLevelText.text = $"Skill: {crewMember.SkillLevel}/10";

            if (salaryText != null)
                salaryText.text = $"${crewMember.Salary:N0}/mo";

            if (roleText != null)
                roleText.text = crewMember.CurrentRole != CrewRole.Unassigned ?
                    crewMember.CurrentRole.ToString() : "None";

            if (specializationsText != null)
            {
                var specs = string.Join(", ", crewMember.Specializations);
                specializationsText.text = specs.Length > 0 ? specs : "None";
            }

            // Button setup
            if (hireButton != null)
            {
                hireButton.gameObject.SetActive(isAvailable);
                hireButton.onClick.AddListener(() => ShowHireDialog());
            }

            if (fireButton != null)
            {
                fireButton.gameObject.SetActive(!isAvailable);
                fireButton.onClick.AddListener(() => parentController.FireCrewMember(crewMember));
            }

            if (changeRoleButton != null)
            {
                changeRoleButton.gameObject.SetActive(!isAvailable);
                changeRoleButton.onClick.AddListener(() => ShowRoleChangeDialog());
            }

            if (detailsButton != null)
            {
                detailsButton.onClick.AddListener(() => parentController.ShowCrewDetails(crewMember));
            }

            // Role dropdown setup
            if (roleDropdown != null)
            {
                roleDropdown.gameObject.SetActive(!isAvailable);
                SetupRoleDropdown();
            }

            // Visual styling based on skill level
            if (backgroundImage != null)
            {
                backgroundImage.color = GetSkillLevelColor(crewMember.SkillLevel);
            }
        }

        private void SetupRoleDropdown()
        {
            if (roleDropdown == null) return;

            roleDropdown.ClearOptions();

            var availableRoles = crewMember.AvailableRoles
                .Where(r => r != CrewRole.Unassigned)
                .Select(r => r.ToString())
                .ToList();

            foreach (var role in availableRoles)
            {
                roleDropdown.options.Add(new TMP_Dropdown.OptionData(role));
            }

            // Set current role
            if (crewMember.CurrentRole != CrewRole.Unassigned)
            {
                var currentRoleIndex = roleDropdown.options.FindIndex(o => o.text == crewMember.CurrentRole.ToString());
                if (currentRoleIndex >= 0)
                {
                    roleDropdown.value = currentRoleIndex;
                }
            }

            roleDropdown.onValueChanged.AddListener(OnRoleChanged);
        }

        private void OnRoleChanged(int index)
        {
            if (index >= 0 && index < crewMember.AvailableRoles.Length)
            {
                var newRole = crewMember.AvailableRoles[index];
                if (newRole != crewMember.CurrentRole)
                {
                    parentController.ChangeCrewRole(crewMember, newRole);
                }
            }
        }

        private void ShowHireDialog()
        {
            // Show role selection dialog for hiring
            if (crewMember.AvailableRoles.Length > 1)
            {
                // Would normally open a modal to select role
                // For now, hire with first available role
                var firstRole = crewMember.AvailableRoles.FirstOrDefault(r => r != CrewRole.Unassigned);
                if (firstRole != CrewRole.Unassigned)
                {
                    parentController.HireCrewMember(crewMember, firstRole);
                }
            }
            else
            {
                // Only one role available
                var role = crewMember.AvailableRoles.FirstOrDefault(r => r != CrewRole.Unassigned);
                if (role != CrewRole.Unassigned)
                {
                    parentController.HireCrewMember(crewMember, role);
                }
            }
        }

        private void ShowRoleChangeDialog()
        {
            // This could open a more sophisticated dialog
            // For now, the dropdown handles it
        }

        private Color GetSkillLevelColor(int skillLevel)
        {
            if (skillLevel <= 3) return Color.red * 0.3f;
            if (skillLevel <= 6) return Color.yellow * 0.3f;
            if (skillLevel <= 8) return Color.green * 0.3f;
            return Color.cyan * 0.3f;
        }
    }
}