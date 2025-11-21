using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Interfaces;
using Core.Data;

namespace Core.Services
{
    /// <summary>
    /// Implementation of the crew service.
    /// Follows single responsibility principle and clean code practices.
    /// </summary>
    public class CrewService : MonoBehaviour, ICrewService
    {
        private readonly Dictionary<string, CrewData> availableCrew = new Dictionary<string, CrewData>();
        private readonly Dictionary<string, CrewData> hiredCrew = new Dictionary<string, CrewData>();
        private int playerFunds = 10000; // Starting funds

        // Events for UI controllers to subscribe to
        public event Action<CrewData> OnCrewHired;
        public event Action<CrewData> OnCrewFired;
        public event Action<CrewData, CrewRole, CrewRole> OnCrewRoleChanged;

        private void Awake()
        {
            GenerateMockCrew();
        }

        private void Start()
        {
            // Refresh available crew periodically
            InvokeRepeating(nameof(RefreshAvailableCrew), 300f, 300f); // Every 5 minutes
        }

        /// <summary>
        /// Generates mock crew members for demonstration.
        /// </summary>
        private void GenerateMockCrew()
        {
            var mockCrew = new[]
            {
                new CrewData(
                    "crew_001",
                    "Sarah Chen",
                    "Experienced harbor inspector with sharp eye for detail. Former customs agent with 15 years of experience.",
                    new[] { CrewRole.Inspector, CrewRole.Scanner, CrewRole.Analyst },
                    8,
                    2500,
                    new[]
                    {
                        new CrewStat(CrewStatType.AttentionToDetail, 9f),
                        new CrewStat(CrewStatType.Investigation, 8f),
                        new CrewStat(CrewStatType.StressTolerance, 7f)
                    },
                    new[] { "Inspector", "Document Analysis" }
                ),
                new CrewData(
                    "crew_002",
                    "Marcus Rodriguez",
                    "Former navy pilot with exceptional navigation skills. Expert in coastal waters and tactical maneuvering.",
                    new[] { CrewRole.Driver, CrewRole.Navigator, CrewRole.Commander },
                    9,
                    3200,
                    new[]
                    {
                        new CrewStat(CrewStatType.Navigation, 10f),
                        new CrewStat(CrewStatType.Leadership, 8f),
                        new CrewStat(CrewStatType.ReactionTime, 9f)
                    },
                    new[] { "Pilot", "Navigation" }
                ),
                new CrewData(
                    "crew_003",
                    "James Mitchell",
                    "Veteran marine with extensive boarding experience. Expert in close-quarters operations and arrest procedures.",
                    new[] { CrewRole.Boarder, CrewRole.Gunner, CrewRole.Driver },
                    7,
                    2800,
                    new[]
                    {
                        new CrewStat(CrewStatType.ReactionTime, 8f),
                        new CrewStat(CrewStatType.TechnicalSkill, 6f),
                        new CrewStat(CrewStatType.StressTolerance, 9f)
                    },
                    new[] { "Boarding", "Combat" }
                ),
                new CrewData(
                    "crew_004",
                    "Dr. Elena Volkov",
                    "Technical specialist in advanced scanning systems. PhD in engineering with military background.",
                    new[] { CrewRole.Scanner, CrewRole.Engineer, CrewRole.Analyst },
                    9,
                    3500,
                    new[]
                    {
                        new CrewStat(CrewStatType.TechnicalSkill, 10f),
                        new CrewStat(CrewStatType.AttentionToDetail, 8f),
                        new CrewStat(CrewStatType.Investigation, 7f)
                    },
                    new[] { "Scanner", "Engineering", "Analysis" }
                ),
                new CrewData(
                    "crew_005",
                    "Thomas Andersson",
                    "Former intelligence analyst with exceptional pattern recognition skills. Expert in data analysis.",
                    new[] { CrewRole.Analyst, CrewRole.Commander, CrewRole.Inspector },
                    6,
                    2200,
                    new[]
                    {
                        new CrewStat(CrewStatType.Investigation, 9f),
                        new CrewStat(CrewStatType.Communication, 8f),
                        new CrewStat(CrewStatType.Leadership, 6f)
                    },
                    new[] { "Intelligence", "Analysis" }
                ),
                new CrewData(
                    "crew_006",
                    "Maria Garcia",
                    "Skilled mechanic and engineer with experience maintaining high-speed patrol vessels.",
                    new[] { CrewRole.Engineer, CrewRole.Driver, CrewRole.Scanner },
                    7,
                    2400,
                    new[]
                    {
                        new CrewStat(CrewStatType.TechnicalSkill, 8f),
                        new CrewStat(CrewStatType.Navigation, 7f),
                        new CrewStat(CrewStatType.ReactionTime, 6f)
                    },
                    new[] { "Engineering", "Maintenance" }
                )
            };

            foreach (var crew in mockCrew)
            {
                availableCrew[crew.Id] = crew;
            }
        }

        public IReadOnlyList<CrewData> GetAvailableCrew()
        {
            return availableCrew.Values.ToList().AsReadOnly();
        }

        public IReadOnlyList<CrewData> GetHiredCrew()
        {
            return hiredCrew.Values.ToList().AsReadOnly();
        }

        public IReadOnlyList<CrewData> GetCrewByRole(CrewRole role)
        {
            return hiredCrew.Values
                .Where(c => c.CurrentRole == role)
                .ToList()
                .AsReadOnly();
        }

        public CrewData GetCrewMember(string crewId)
        {
            return hiredCrew.TryGetValue(crewId, out var crew) ? crew :
                   availableCrew.TryGetValue(crewId, out crew) ? crew : null;
        }

        public bool TryHireCrew(string crewId, CrewRole role, int availableFunds)
        {
            if (!availableCrew.TryGetValue(crewId, out var crew))
                return false;

            // Update player funds from parameter
            playerFunds = availableFunds;

            if (!crew.TryHire(role, playerFunds))
                return false;

            playerFunds -= crew.Salary;
            availableCrew.Remove(crewId);
            hiredCrew[crewId] = crew;

            OnCrewHired?.Invoke(crew);
            Debug.Log($"[CrewService] Hired crew member: {crew.Name} as {role}");

            return true;
        }

        public bool FireCrew(string crewId)
        {
            if (!hiredCrew.TryGetValue(crewId, out var crew))
                return false;

            hiredCrew.Remove(crewId);
            crew.Fire();
            availableCrew[crewId] = crew;

            OnCrewFired?.Invoke(crew);
            Debug.Log($"[CrewService] Fired crew member: {crew.Name}");

            return true;
        }

        public bool TryChangeCrewRole(string crewId, CrewRole newRole)
        {
            if (!hiredCrew.TryGetValue(crewId, out var crew))
                return false;

            var oldRole = crew.CurrentRole;
            if (!crew.TryChangeRole(newRole))
                return false;

            OnCrewRoleChanged?.Invoke(crew, oldRole, newRole);
            Debug.Log($"[CrewService] Changed {crew.Name} role from {oldRole} to {newRole}");

            return true;
        }

        public int GetTotalSalaryCost()
        {
            return hiredCrew.Values.Sum(c => c.Salary);
        }

        public float GetRoleEffectiveness(CrewRole role)
        {
            var roleCrew = GetCrewByRole(role);
            if (roleCrew.Count == 0)
                return 0f;

            return roleCrew.Average(c => c.GetRoleEffectiveness());
        }

        public bool CanAssignToRole(string crewId, CrewRole role)
        {
            var crew = GetCrewMember(crewId);
            return crew?.AvailableRoles.Contains(role) ?? false;
        }

        public IReadOnlyList<CrewData> GetCrewBySpecialization(string specialization)
        {
            return hiredCrew.Values
                .Where(c => c.Specializations.Any(spec =>
                    spec.Equals(specialization, StringComparison.OrdinalIgnoreCase)))
                .ToList()
                .AsReadOnly();
        }

        public void RefreshAvailableCrew()
        {
            // Remove some old crew members to simulate turnover
            if (availableCrew.Count > 8)
            {
                var toRemove = availableCrew.Keys.Take(2).ToArray();
                foreach (var id in toRemove)
                {
                    availableCrew.Remove(id);
                }
            }

            // Occasionally add new crew members
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
            {
                GenerateMockCrew();
            }

            Debug.Log($"[CrewService] Available crew refreshed. Count: {availableCrew.Count}");
        }

        /// <summary>
        /// Gets current player funds (for UI display).
        /// </summary>
        public int GetPlayerFunds()
        {
            return playerFunds;
        }

        /// <summary>
        /// Adds funds to player account (e.g., from contract rewards).
        /// </summary>
        public void AddFunds(int amount)
        {
            if (amount > 0)
            {
                playerFunds += amount;
                Debug.Log($"[CrewService] Added {amount} funds. Total: {playerFunds}");
            }
        }
    }
}