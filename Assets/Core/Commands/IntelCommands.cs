using System;
using System.Linq;
using UnityEngine;
using Core.Interfaces;

namespace Core.Commands
{
    /// <summary>
    /// Command for verifying intelligence data.
    /// Includes crew assignment and verification logic.
    /// </summary>
    public class VerifyIntelCommand : ITabletCommand
    {
        private readonly IIntelService intelService;
        private readonly ICrewService crewService;
        private readonly string intelId;
        private readonly string crewId;
        private bool executed;

        public string Id => $"verify_intel_{intelId}_by_{crewId}";
        public string Description => $"Verify intel: {GetIntelTitle()} by {GetCrewName()}";

        public VerifyIntelCommand(
            IIntelService intelService,
            ICrewService crewService,
            string intelId,
            string crewId)
        {
            this.intelService = intelService ?? throw new ArgumentNullException(nameof(intelService));
            this.crewService = crewService ?? throw new ArgumentNullException(nameof(crewService));
            this.intelId = intelId ?? throw new ArgumentNullException(nameof(intelId));
            this.crewId = crewId ?? throw new ArgumentNullException(nameof(crewId));
        }

        public bool CanExecute()
        {
            if (executed) return false;

            var intel = intelService.GetIntel(intelId);
            var crew = crewService.GetCrewMember(crewId);

            if (intel == null)
                return false;

            if (crew == null || !crew.IsHired)
                return false;

            // Check if crew has suitable skills for verification
            return crew.AvailableRoles.Contains(Core.Data.CrewRole.Analyst) ||
                   crew.AvailableRoles.Contains(Core.Data.CrewRole.Inspector);
        }

        public void Execute()
        {
            if (!CanExecute())
                throw new InvalidOperationException($"Cannot execute command: {Description}");

            var crew = crewService.GetCrewMember(crewId);
            var verificationSkill = crew.GetStat(Core.Data.CrewStatType.Investigation);

            // Increase credibility based on crew skill
            var intel = intelService.GetIntel(intelId);
            var credibilityBoost = (int)(verificationSkill * 0.1f); // 10% of skill as boost
            var newCredibility = Math.Min(100, intel.Credibility + credibilityBoost);

            intelService.UpdateIntelCredibility(intelId, newCredibility);

            executed = true;
            Debug.Log($"[VerifyIntelCommand] Executed: {Description} (Credibility: {intel.Credibility} → {newCredibility})");
        }

        public void Undo()
        {
            if (!executed)
                throw new InvalidOperationException("Cannot undo a command that hasn't been executed");

            // Note: Undoing verification would be complex
            Debug.LogWarning("[VerifyIntelCommand] Undo not implemented for intel verification");
        }

        private string GetIntelTitle()
        {
            var intel = intelService.GetIntel(intelId);
            return intel?.Title ?? "Unknown Intel";
        }

        private string GetCrewName()
        {
            var crew = crewService.GetCrewMember(crewId);
            return crew?.Name ?? "Unknown";
        }
    }

    /// <summary>
    /// Command for cross-referencing intelligence data.
    /// Includes validation and cross-reference logic.
    /// </summary>
    public class CrossReferenceIntelCommand : ITabletCommand
    {
        private readonly IIntelService intelService;
        private readonly string intelId;
        private CrossReferenceResult result;
        private bool executed;

        public string Id => $"crossref_intel_{intelId}";
        public string Description => $"Cross-reference intel: {GetIntelTitle()}";

        public CrossReferenceIntelCommand(
            IIntelService intelService,
            string intelId)
        {
            this.intelService = intelService ?? throw new ArgumentNullException(nameof(intelService));
            this.intelId = intelId ?? throw new ArgumentNullException(nameof(intelId));
        }

        public bool CanExecute()
        {
            if (executed) return false;

            var intel = intelService.GetIntel(intelId);
            return intel != null;
        }

        public void Execute()
        {
            if (!CanExecute())
                throw new InvalidOperationException($"Cannot execute command: {Description}");

            result = intelService.CrossReferenceIntel(intelId);
            if (result == null)
                throw new InvalidOperationException($"Failed to cross-reference intel: {intelId}");

            executed = true;
            Debug.Log($"[CrossReferenceIntelCommand] Executed: {Description} (Confidence: {result.ConfidenceScore})");
        }

        public void Undo()
        {
            if (!executed)
                throw new InvalidOperationException("Cannot undo a command that hasn't been executed");

            // Cross-referencing is read-only, no undo needed
            executed = false;
            Debug.Log($"[CrossReferenceIntelCommand] Undone: {Description}");
        }

        private string GetIntelTitle()
        {
            var intel = intelService.GetIntel(intelId);
            return intel?.Title ?? "Unknown Intel";
        }
    }
}