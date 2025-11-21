using System;
using System.Collections.Generic;
using Core.Data;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for managing contracts that players can accept and complete.
    /// Follows dependency inversion principle - depends on abstractions, not concretions.
    /// </summary>
    public interface IContractsService
    {
        /// <summary>
        /// Event fired when a contract is accepted.
        /// </summary>
        event Action<ContractData> OnContractAccepted;

        /// <summary>
        /// Event fired when a contract status changes.
        /// </summary>
        event Action<ContractData, ContractStatus, ContractStatus> OnContractStatusChanged;

        /// <summary>
        /// Gets all available contracts.
        /// </summary>
        IReadOnlyList<ContractData> GetAvailableContracts();

        /// <summary>
        /// Gets all active contracts.
        /// </summary>
        IReadOnlyList<ContractData> GetActiveContracts();

        /// <summary>
        /// Gets a specific contract by ID.
        /// </summary>
        ContractData GetContract(string contractId);

        /// <summary>
        /// Gets contracts filtered by region.
        /// </summary>
        IReadOnlyList<ContractData> GetContractsByRegion(string region);

        /// <summary>
        /// Gets contracts filtered by difficulty.
        /// </summary>
        IReadOnlyList<ContractData> GetContractsByDifficulty(ContractDifficulty difficulty);

        /// <summary>
        /// Attempts to accept a contract.
        /// </summary>
        bool TryAcceptContract(string contractId, string[] playerQualifications);

        /// <summary>
        /// Completes a contract with success.
        /// </summary>
        bool CompleteContract(string contractId, int performanceScore = 100);

        /// <summary>
        /// Marks a contract as failed.
        /// </summary>
        bool FailContract(string contractId, string reason = "Mission failed");

        /// <summary>
        /// Checks if a contract can be accepted.
        /// </summary>
        bool CanAcceptContract(string contractId, string[] playerQualifications);

        /// <summary>
        /// Gets the total reward from all completed contracts.
        /// </summary>
        int GetTotalEarnings();

        /// <summary>
        /// Gets the success rate percentage.
        /// </summary>
        float GetSuccessRate();

        /// <summary>
        /// Refreshes available contracts (call periodically).
        /// </summary>
        void RefreshContracts();
    }
}