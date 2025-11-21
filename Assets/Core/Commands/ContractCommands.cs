using System;
using UnityEngine;
using Core.Interfaces;
using Core.Data;

namespace Core.Commands
{
    /// <summary>
    /// Command for accepting a contract.
    /// Encapsulates contract acceptance logic with proper validation.
    /// </summary>
    public class AcceptContractCommand : ITabletCommand
    {
        private readonly IContractsService contractsService;
        private readonly ICrewService crewService;
        private readonly string contractId;
        private readonly string[] playerQualifications;
        private bool executed;

        public string Id => $"accept_contract_{contractId}";
        public string Description => $"Accept contract: {GetContractTitle()}";

        public AcceptContractCommand(
            IContractsService contractsService,
            ICrewService crewService,
            string contractId,
            string[] playerQualifications = null)
        {
            this.contractsService = contractsService ?? throw new ArgumentNullException(nameof(contractsService));
            this.crewService = crewService ?? throw new ArgumentNullException(nameof(crewService));
            this.contractId = contractId ?? throw new ArgumentNullException(nameof(contractId));
            this.playerQualifications = playerQualifications ?? GetDefaultQualifications();
        }

        public bool CanExecute()
        {
            if (executed) return false;

            var contract = contractsService.GetContract(contractId);
            if (contract == null || contract.Status != ContractStatus.Available)
                return false;

            return contractsService.CanAcceptContract(contractId, playerQualifications);
        }

        public void Execute()
        {
            if (!CanExecute())
                throw new InvalidOperationException($"Cannot execute command: {Description}");

            var success = contractsService.TryAcceptContract(contractId, playerQualifications);
            if (!success)
                throw new InvalidOperationException($"Failed to accept contract: {contractId}");

            executed = true;
            Debug.Log($"[AcceptContractCommand] Executed: {Description}");
        }

        public void Undo()
        {
            if (!executed)
                throw new InvalidOperationException("Cannot undo a command that hasn't been executed");

            // Note: In a real implementation, we might not want to allow undoing contract acceptance
            // or we might need additional logic to handle refunds, etc.
            Debug.LogWarning("[AcceptContractCommand] Undo not implemented for contract acceptance");
        }

        private string GetContractTitle()
        {
            var contract = contractsService.GetContract(contractId);
            return contract?.Title ?? "Unknown Contract";
        }

        private string[] GetDefaultQualifications()
        {
            var qualifications = new System.Collections.Generic.List<string>();

            // Add qualifications based on hired crew
            var hiredCrew = crewService.GetHiredCrew();
            foreach (var crew in hiredCrew)
            {
                if (crew.CurrentRole != CrewRole.Unassigned)
                {
                    qualifications.Add(crew.CurrentRole.ToString());
                }
            }

            // Add specializations as qualifications
            foreach (var crew in hiredCrew)
            {
                foreach (var spec in crew.Specializations)
                {
                    if (!qualifications.Contains(spec))
                        qualifications.Add(spec);
                }
            }

            return qualifications.Count > 0 ? qualifications.ToArray() : new[] { "Basic" };
        }
    }

    /// <summary>
    /// Command for completing a contract with performance evaluation.
    /// Includes performance-based reward calculations.
    /// </summary>
    public class CompleteContractCommand : ITabletCommand
    {
        private readonly IContractsService contractsService;
        private readonly ICrewService crewService;
        private readonly string contractId;
        private readonly int performanceScore;
        private bool executed;

        public string Id => $"complete_contract_{contractId}";
        public string Description => $"Complete contract: {GetContractTitle()}";

        public CompleteContractCommand(
            IContractsService contractsService,
            ICrewService crewService,
            string contractId,
            int performanceScore = 100)
        {
            this.contractsService = contractsService ?? throw new ArgumentNullException(nameof(contractsService));
            this.crewService = crewService ?? throw new ArgumentNullException(nameof(crewService));
            this.contractId = contractId ?? throw new ArgumentNullException(nameof(contractId));
            this.performanceScore = Math.Clamp(performanceScore, 0, 200);
        }

        public bool CanExecute()
        {
            if (executed) return false;

            var contract = contractsService.GetContract(contractId);
            return contract != null && contract.Status == ContractStatus.Active;
        }

        public void Execute()
        {
            if (!CanExecute())
                throw new InvalidOperationException($"Cannot execute command: {Description}");

            var success = contractsService.CompleteContract(contractId, performanceScore);
            if (!success)
                throw new InvalidOperationException($"Failed to complete contract: {contractId}");

            executed = true;
            Debug.Log($"[CompleteContractCommand] Executed: {Description} (Score: {performanceScore})");

            // Add rewards to crew service funds
            var contract = contractsService.GetContract(contractId);
            if (contract != null)
            {
                var finalReward = (int)(contract.Reward * (performanceScore / 100f));
                crewService.AddFunds(finalReward);
            }
        }

        public void Undo()
        {
            if (!executed)
                throw new InvalidOperationException("Cannot undo a command that hasn't been executed");

            // Note: Undoing contract completion would be complex
            Debug.LogWarning("[CompleteContractCommand] Undo not implemented for contract completion");
        }

        private string GetContractTitle()
        {
            var contract = contractsService.GetContract(contractId);
            return contract?.Title ?? "Unknown Contract";
        }
    }
}