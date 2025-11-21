using System;
using System.Collections.Generic;
using Core.Data;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for managing crew members and their assignments.
    /// Follows single responsibility principle - only handles crew management.
    /// </summary>
    public interface ICrewService
    {
        /// <summary>
        /// Event fired when a crew member is hired.
        /// </summary>
        event Action<CrewData> OnCrewHired;

        /// <summary>
        /// Event fired when a crew member is fired.
        /// </summary>
        event Action<CrewData> OnCrewFired;

        /// <summary>
        /// Event fired when a crew member's role changes.
        /// </summary>
        event Action<CrewData, CrewRole, CrewRole> OnCrewRoleChanged;

        /// <summary>
        /// Gets all available crew members for hire.
        /// </summary>
        IReadOnlyList<CrewData> GetAvailableCrew();

        /// <summary>
        /// Gets all hired crew members.
        /// </summary>
        IReadOnlyList<CrewData> GetHiredCrew();

        /// <summary>
        /// Gets crew members assigned to a specific role.
        /// </summary>
        IReadOnlyList<CrewData> GetCrewByRole(CrewRole role);

        /// <summary>
        /// Gets a specific crew member by ID.
        /// </summary>
        CrewData GetCrewMember(string crewId);

        /// <summary>
        /// Attempts to hire a crew member.
        /// </summary>
        bool TryHireCrew(string crewId, CrewRole role, int availableFunds);

        /// <summary>
        /// Fires a crew member.
        /// </summary>
        bool FireCrew(string crewId);

        /// <summary>
        /// Changes a crew member's role.
        /// </summary>
        bool TryChangeCrewRole(string crewId, CrewRole newRole);

        /// <summary>
        /// Gets the total salary cost per month.
        /// </summary>
        int GetTotalSalaryCost();

        /// <summary>
        /// Gets crew effectiveness in a specific role.
        /// </summary>
        float GetRoleEffectiveness(CrewRole role);

        /// <summary>
        /// Checks if a crew member can be assigned to a role.
        /// </summary>
        bool CanAssignToRole(string crewId, CrewRole role);

        /// <summary>
        /// Gets crew with specific specialization.
        /// </summary>
        IReadOnlyList<CrewData> GetCrewBySpecialization(string specialization);

        /// <summary>
        /// Refreshes available crew pool.
        /// </summary>
        void RefreshAvailableCrew();
    }
}