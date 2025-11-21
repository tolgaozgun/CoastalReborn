using System;
using UnityEngine;
using Core.Interfaces;

namespace Core.Commands
{
    /// <summary>
    /// Command for purchasing upgrade nodes.
    /// Includes cost validation and prerequisite checking.
    /// </summary>
    public class PurchaseUpgradeCommand : ITabletCommand
    {
        private readonly ICrewService crewService;
        private readonly IUpgradeService upgradeService;
        private readonly string nodeId;
        private bool executed;

        public string Id => $"purchase_upgrade_{nodeId}";
        public string Description => $"Purchase upgrade: {GetUpgradeName()}";

        public PurchaseUpgradeCommand(
            ICrewService crewService,
            IUpgradeService upgradeService,
            string nodeId)
        {
            this.crewService = crewService ?? throw new ArgumentNullException(nameof(crewService));
            this.upgradeService = upgradeService ?? throw new ArgumentNullException(nameof(upgradeService));
            this.nodeId = nodeId ?? throw new ArgumentNullException(nameof(nodeId));
        }

        public bool CanExecute()
        {
            if (executed) return false;

            var upgrade = upgradeService.GetUpgradeNode(nodeId);
            if (upgrade == null || upgrade.IsPurchased)
                return false;

            return upgrade.GetCurrentCost() <= crewService.GetPlayerFunds();
        }

        public void Execute()
        {
            if (!CanExecute())
                throw new InvalidOperationException($"Cannot execute command: {Description}");

            var upgrade = upgradeService.GetUpgradeNode(nodeId);
            var availableFunds = crewService.GetPlayerFunds();

            if (!upgradeService.CanPurchaseNode(nodeId))
                throw new InvalidOperationException($"Prerequisites not met for upgrade: {nodeId}");

            var success = upgradeService.PurchaseNode(nodeId);
            if (!success)
                throw new InvalidOperationException($"Failed to purchase upgrade: {nodeId}");

            // Deduct cost from funds (using points system would be better)
            crewService.AddFunds(-upgrade.GetCurrentCost());

            executed = true;
            Debug.Log($"[PurchaseUpgradeCommand] Executed: {Description}");
        }

        public void Undo()
        {
            if (!executed)
                throw new InvalidOperationException("Cannot undo a command that hasn't been executed");

            // Note: Undoing upgrade purchase would be complex and potentially unbalanced
            Debug.LogWarning("[PurchaseUpgradeCommand] Undo not implemented for upgrade purchases");
        }

        private string GetUpgradeName()
        {
            var upgrade = upgradeService.GetUpgradeNode(nodeId);
            return upgrade?.Name ?? "Unknown Upgrade";
        }
    }
}