using System;

namespace Core.Data
{
    /// <summary>
    /// Represents a crew member that can be assigned to different roles.
    /// Immutable data model with clear business rules for role assignments.
    /// </summary>
    [Serializable]
    public class CrewData
    {
        public string Id { get; }
        public string Name { get; }
        public string Biography { get; }
        public CrewRole CurrentRole { get; private set; }
        public CrewRole[] AvailableRoles { get; }
        public int SkillLevel { get; }
        public int Salary { get; }
        public DateTime HiredAt { get; private set; }
        public bool IsHired { get; private set; }
        public string[] Specializations { get; }
        public CrewStat[] Stats { get; }

        public CrewData(
            string id,
            string name,
            string biography,
            CrewRole[] availableRoles,
            int skillLevel,
            int salary,
            CrewStat[] stats,
            string[] specializations = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Biography = biography ?? string.Empty;
            AvailableRoles = availableRoles ?? throw new ArgumentNullException(nameof(availableRoles));
            SkillLevel = Math.Clamp(skillLevel, 1, 10);
            Salary = Math.Max(0, salary);
            Stats = stats ?? Array.Empty<CrewStat>();
            Specializations = specializations ?? Array.Empty<string>();
            CurrentRole = CrewRole.Unassigned;
        }

        /// <summary>
        /// Hires this crew member for a specific role.
        /// </summary>
        public bool TryHire(CrewRole role, int availableFunds)
        {
            if (IsHired || Salary > availableFunds)
                return false;

            if (!Array.Exists(AvailableRoles, availableRole => availableRole == role))
                return false;

            IsHired = true;
            CurrentRole = role;
            HiredAt = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Changes the crew member's role (only if hired and role is available).
        /// </summary>
        public bool TryChangeRole(CrewRole newRole)
        {
            if (!IsHired || newRole == CurrentRole)
                return false;

            if (!Array.Exists(AvailableRoles, availableRole => availableRole == newRole))
                return false;

            CurrentRole = newRole;
            return true;
        }

        /// <summary>
        /// Fires this crew member.
        /// </summary>
        public void Fire()
        {
            IsHired = false;
            CurrentRole = CrewRole.Unassigned;
        }

        /// <summary>
        /// Gets the effectiveness of this crew member in their current role.
        /// </summary>
        public float GetRoleEffectiveness()
        {
            if (!IsHired || CurrentRole == CrewRole.Unassigned)
                return 0f;

            var baseEffectiveness = SkillLevel / 10f;
            var specializationBonus = GetSpecializationBonus();

            return Math.Min(1f, baseEffectiveness + specializationBonus);
        }

        /// <summary>
        /// Gets a specific stat value.
        /// </summary>
        public float GetStat(CrewStatType statType)
        {
            var stat = Array.Find(Stats, s => s.Type == statType);
            return stat?.Value ?? 0f;
        }

        private float GetSpecializationBonus()
        {
            if (Specializations.Length == 0)
                return 0f;

            // Check if current role matches any specialization
            var roleSpecialization = CurrentRole.ToString();
            var hasSpecialization = Array.Exists(Specializations,
                spec => spec.Equals(roleSpecialization, StringComparison.OrdinalIgnoreCase));

            return hasSpecialization ? 0.1f : 0f; // 10% bonus for specialization
        }
    }

    /// <summary>
    /// Represents a specific crew member stat.
    /// </summary>
    [Serializable]
    public class CrewStat
    {
        public CrewStatType Type { get; }
        public float Value { get; }

        public CrewStat(CrewStatType type, float value)
        {
            Type = type;
            Value = Math.Max(0f, value);
        }
    }

    public enum CrewRole
    {
        Unassigned,
        Driver,        // Boat navigation
        Inspector,     // Document inspection
        Scanner,       // X-ray scanner operation
        Boarder,       // Boarding and arrest
        Gunner,        // Weapons systems
        Engineer,      // Ship maintenance
        Analyst,       // Intel analysis
        Commander      // Overall coordination
    }

    public enum CrewStatType
    {
        AttentionToDetail,  // For inspection work
        ReactionTime,       // For boarding/combat
        TechnicalSkill,     // For scanner/engine work
        Leadership,         // For coordination
        StressTolerance,    // Resistance to pressure
        Navigation,         // Boat handling
        Communication,      // Team coordination
        Investigation       // Intel gathering
    }
}