using System;
using UnityEngine;
using Core.Interfaces;
using Core.Data;

namespace Core.Commands
{
    /// <summary>
    /// Command for hiring a crew member with role assignment.
    /// Includes fund validation and role assignment logic.
    /// </summary>
    public class HireCrewCommand : ITabletCommand
    {
        private readonly ICrewService crewService;
        private readonly string crewId;
        private readonly CrewRole role;
        private bool executed;

        public string Id => $"hire_crew_{crewId}_{role}";
        public string Description => $"Hire crew member: {GetCrewName()} as {role}";

        public HireCrewCommand(
            ICrewService crewService,
            string crewId,
            CrewRole role)
        {
            this.crewService = crewService ?? throw new ArgumentNullException(nameof(crewService));
            this.crewId = crewId ?? throw new ArgumentNullException(nameof(crewId));
            this.role = role;
        }

        public bool CanExecute()
        {
            if (executed) return false;

            var crew = crewService.GetCrewMember(crewId);
            if (crew == null || crew.IsHired)
                return false;

            if (!crewService.CanAssignToRole(crewId, role))
                return false;

            return crew.Salary <= crewService.GetPlayerFunds();
        }

        public void Execute()
        {
            if (!CanExecute())
                throw new InvalidOperationException($"Cannot execute command: {Description}");

            var availableFunds = crewService.GetPlayerFunds();
            var success = crewService.TryHireCrew(crewId, role, availableFunds);

            if (!success)
                throw new InvalidOperationException($"Failed to hire crew member: {crewId}");

            executed = true;
            Debug.Log($"[HireCrewCommand] Executed: {Description}");
        }

        public void Undo()
        {
            if (!executed)
                throw new InvalidOperationException("Cannot undo a command that hasn't been executed");

            var success = crewService.FireCrew(crewId);
            if (success)
            {
                executed = false;
                Debug.Log($"[HireCrewCommand] Undone: {Description}");
            }
            else
            {
                throw new InvalidOperationException($"Failed to undo hire command: {crewId}");
            }
        }

        private string GetCrewName()
        {
            var crew = crewService.GetCrewMember(crewId);
            return crew?.Name ?? "Unknown";
        }
    }

    /// <summary>
    /// Command for firing a crew member with proper cleanup.
    /// Includes role deassignment and resource management.
    /// </summary>
    public class FireCrewCommand : ITabletCommand
    {
        private readonly ICrewService crewService;
        private readonly string crewId;
        private CrewRole previousRole;
        private bool executed;

        public string Id => $"fire_crew_{crewId}";
        public string Description => $"Fire crew member: {GetCrewName()}";

        public FireCrewCommand(ICrewService crewService, string crewId)
        {
            this.crewService = crewService ?? throw new ArgumentNullException(nameof(crewService));
            this.crewId = crewId ?? throw new ArgumentNullException(nameof(crewId));
        }

        public bool CanExecute()
        {
            if (executed) return false;

            var crew = crewService.GetCrewMember(crewId);
            return crew != null && crew.IsHired;
        }

        public void Execute()
        {
            if (!CanExecute())
                throw new InvalidOperationException($"Cannot execute command: {Description}");

            // Store previous role for undo
            var crew = crewService.GetCrewMember(crewId);
            previousRole = crew.CurrentRole;

            var success = crewService.FireCrew(crewId);
            if (!success)
                throw new InvalidOperationException($"Failed to fire crew member: {crewId}");

            executed = true;
            Debug.Log($"[FireCrewCommand] Executed: {Description}");
        }

        public void Undo()
        {
            if (!executed)
                throw new InvalidOperationException("Cannot undo a command that hasn't been executed");

            var availableFunds = crewService.GetPlayerFunds();
            var success = crewService.TryHireCrew(crewId, previousRole, availableFunds);

            if (success)
            {
                executed = false;
                Debug.Log($"[FireCrewCommand] Undone: {Description}");
            }
            else
            {
                throw new InvalidOperationException($"Failed to undo fire command: {crewId}");
            }
        }

        private string GetCrewName()
        {
            var crew = crewService.GetCrewMember(crewId);
            return crew?.Name ?? "Unknown";
        }
    }

    /// <summary>
    /// Command for changing crew member roles.
    /// Includes validation and role transition logic.
    /// </summary>
    public class ChangeCrewRoleCommand : ITabletCommand
    {
        private readonly ICrewService crewService;
        private readonly string crewId;
        private readonly CrewRole newRole;
        private CrewRole oldRole;
        private bool executed;

        public string Id => $"change_role_{crewId}_{newRole}";
        public string Description => $"Change role for {GetCrewName()}: {oldRole} → {newRole}";

        public ChangeCrewRoleCommand(
            ICrewService crewService,
            string crewId,
            CrewRole newRole)
        {
            this.crewService = crewService ?? throw new ArgumentNullException(nameof(crewService));
            this.crewId = crewId ?? throw new ArgumentNullException(nameof(crewId));
            this.newRole = newRole;
        }

        public bool CanExecute()
        {
            if (executed) return false;

            var crew = crewService.GetCrewMember(crewId);
            if (crew == null || !crew.IsHired)
                return false;

            return crewService.CanAssignToRole(crewId, newRole);
        }

        public void Execute()
        {
            if (!CanExecute())
                throw new InvalidOperationException($"Cannot execute command: {Description}");

            var crew = crewService.GetCrewMember(crewId);
            oldRole = crew.CurrentRole;

            var success = crewService.TryChangeCrewRole(crewId, newRole);
            if (!success)
                throw new InvalidOperationException($"Failed to change crew role: {crewId}");

            executed = true;
            Debug.Log($"[ChangeCrewRoleCommand] Executed: {Description}");
        }

        public void Undo()
        {
            if (!executed)
                throw new InvalidOperationException("Cannot undo a command that hasn't been executed");

            var success = crewService.TryChangeCrewRole(crewId, oldRole);
            if (success)
            {
                executed = false;
                Debug.Log($"[ChangeCrewRoleCommand] Undone: {Description}");
            }
            else
            {
                throw new InvalidOperationException($"Failed to undo role change: {crewId}");
            }
        }

        private string GetCrewName()
        {
            var crew = crewService.GetCrewMember(crewId);
            return crew?.Name ?? "Unknown";
        }
    }
}