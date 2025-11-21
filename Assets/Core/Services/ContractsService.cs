using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Interfaces;
using Core.Data;

namespace Core.Services
{
    /// <summary>
    /// Implementation of the contracts service.
    /// Follows SOLID principles - single responsibility, depends on abstractions.
    /// </summary>
    public class ContractsService : MonoBehaviour, IContractsService
    {
        private readonly Dictionary<string, ContractData> contracts = new Dictionary<string, ContractData>();
        private readonly Dictionary<string, ContractData> activeContracts = new Dictionary<string, ContractData>();
        private int totalEarnings = 0;
        private int completedContracts = 0;
        private int failedContracts = 0;

        // Events for UI controllers to subscribe to
        public event Action<ContractData> OnContractAccepted;
        public event Action<ContractData, ContractStatus, ContractStatus> OnContractStatusChanged;

        private void Awake()
        {
            GenerateMockContracts();
        }

        private void Start()
        {
            // Check for expired contracts periodically
            InvokeRepeating(nameof(CheckExpiredContracts), 60f, 60f);
        }

        /// <summary>
        /// Generates mock contracts for demonstration.
        /// </summary>
        private void GenerateMockContracts()
        {
            var mockContracts = new[]
            {
                new ContractData(
                    "contract_001",
                    "Dock Inspection Duty",
                    "Inspect incoming vessels at Harbor North dock for contraband and forged documents.",
                    "Harbor North",
                    ContractDifficulty.Easy,
                    500,
                    TimeSpan.FromHours(2),
                    new[] { "Basic Inspection" },
                    new[] { "Smuggler cartel" }
                ),
                new ContractData(
                    "contract_002",
                    "Coastal Patrol",
                    "Patrol the eastern coastline for suspicious activity and unauthorized vessels.",
                    "Eastern Coast",
                    ContractDifficulty.Medium,
                    1200,
                    TimeSpan.FromHours(4),
                    new[] { "Navigation", "Combat" },
                    new[] { "Pirate gang", "Smuggler network" }
                ),
                new ContractData(
                    "contract_003",
                    "High-Value Target",
                    "Intercept and apprehend a known smuggler leader traveling through the region.",
                    "Southern Passage",
                    ContractDifficulty.Hard,
                    3000,
                    TimeSpan.FromHours(6),
                    new[] { "Advanced Combat", "Tactical Planning" },
                    new[] { "Smuggler cartel" }
                ),
                new ContractData(
                    "contract_004",
                    "Port Security Detail",
                    "Provide security for a diplomatic convoy arriving at Harbor West.",
                    "Harbor West",
                    ContractDifficulty.Easy,
                    800,
                    TimeSpan.FromHours(3),
                    new[] { "Basic Security" },
                    new string[0]
                ),
                new ContractData(
                    "contract_005",
                    "Deep Sea Surveillance",
                    "Monitor a deep water area for illegal submarine activity using advanced scanners.",
                    "Deep Waters",
                    ContractDifficulty.Expert,
                    5000,
                    TimeSpan.FromHours(8),
                    new[] { "Advanced Scanner Operation", "Sonar Analysis" },
                    new[] { "Advanced smuggling ring" }
                )
            };

            foreach (var contract in mockContracts)
            {
                contracts[contract.Id] = contract;
            }
        }

        public IReadOnlyList<ContractData> GetAvailableContracts()
        {
            return contracts.Values
                .Where(c => c.Status == ContractStatus.Available)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<ContractData> GetActiveContracts()
        {
            return activeContracts.Values.ToList().AsReadOnly();
        }

        public ContractData GetContract(string contractId)
        {
            return contracts.TryGetValue(contractId, out var contract) ? contract : null;
        }

        public IReadOnlyList<ContractData> GetContractsByRegion(string region)
        {
            return contracts.Values
                .Where(c => c.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<ContractData> GetContractsByDifficulty(ContractDifficulty difficulty)
        {
            return contracts.Values
                .Where(c => c.Difficulty == difficulty)
                .ToList()
                .AsReadOnly();
        }

        public bool TryAcceptContract(string contractId, string[] playerQualifications)
        {
            if (!contracts.TryGetValue(contractId, out var contract))
                return false;

            if (!contract.TryChangeStatus(ContractStatus.Active))
                return false;

            if (!contract.CanAccept(playerQualifications))
            {
                // Revert status if qualifications don't match
                contract.TryChangeStatus(ContractStatus.Available);
                return false;
            }

            activeContracts[contractId] = contract;
            OnContractAccepted?.Invoke(contract);
            OnContractStatusChanged?.Invoke(contract, ContractStatus.Available, ContractStatus.Active);

            Debug.Log($"[ContractsService] Contract accepted: {contract.Title}");
            return true;
        }

        public bool CompleteContract(string contractId, int performanceScore = 100)
        {
            if (!activeContracts.TryGetValue(contractId, out var contract))
                return false;

            var oldStatus = contract.Status;
            if (!contract.TryChangeStatus(ContractStatus.Completed))
                return false;

            activeContracts.Remove(contractId);
            totalEarnings += contract.Reward;
            completedContracts++;

            // Apply performance modifier to reward
            var finalReward = (int)(contract.Reward * (performanceScore / 100f));
            totalEarnings += (finalReward - contract.Reward);

            OnContractStatusChanged?.Invoke(contract, oldStatus, ContractStatus.Completed);
            Debug.Log($"[ContractsService] Contract completed: {contract.Title} (Reward: {finalReward})");

            return true;
        }

        public bool FailContract(string contractId, string reason = "Mission failed")
        {
            if (!activeContracts.TryGetValue(contractId, out var contract))
                return false;

            var oldStatus = contract.Status;
            if (!contract.TryChangeStatus(ContractStatus.Failed))
                return false;

            activeContracts.Remove(contractId);
            failedContracts++;

            OnContractStatusChanged?.Invoke(contract, oldStatus, ContractStatus.Failed);
            Debug.Log($"[ContractsService] Contract failed: {contract.Title} - {reason}");

            return true;
        }

        public bool CanAcceptContract(string contractId, string[] playerQualifications)
        {
            var contract = GetContract(contractId);
            return contract?.CanAccept(playerQualifications) ?? false;
        }

        public int GetTotalEarnings()
        {
            return totalEarnings;
        }

        public float GetSuccessRate()
        {
            var totalAttempts = completedContracts + failedContracts;
            return totalAttempts > 0 ? (float)completedContracts / totalAttempts * 100f : 0f;
        }

        public void RefreshContracts()
        {
            // Remove old expired contracts
            var expiredContracts = contracts.Values
                .Where(c => c.Status == ContractStatus.Expired)
                .ToList();

            foreach (var expired in expiredContracts)
            {
                contracts.Remove(expired.Id);
                activeContracts.Remove(expired.Id);
            }

            // Generate new contracts occasionally
            if (contracts.Count < 10 && UnityEngine.Random.Range(0f, 1f) < 0.3f)
            {
                GenerateMockContracts();
            }

            Debug.Log($"[ContractsService] Contracts refreshed. Available: {GetAvailableContracts().Count}");
        }

        private void CheckExpiredContracts()
        {
            var now = DateTime.UtcNow;
            var expiredContracts = activeContracts.Values
                .Where(c => c.Status == ContractStatus.Active && c.GetRemainingTime() <= TimeSpan.Zero)
                .ToList();

            foreach (var expired in expiredContracts)
            {
                FailContract(expired.Id, "Contract expired");
            }
        }
    }
}