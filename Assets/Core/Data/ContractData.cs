using System;

namespace Core.Data
{
    /// <summary>
    /// Represents a contract that players can accept and complete.
    /// Data model following immutability principles - getters only trigger no side effects.
    /// </summary>
    [Serializable]
    public class ContractData
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public string Region { get; }
        public ContractDifficulty Difficulty { get; }
        public int Reward { get; }
        public TimeSpan Duration { get; }
        public ContractStatus Status { get; private set; }
        public DateTime AcceptedAt { get; private set; }
        public string[] RequiredQualifications { get; }
        public string[] TargetFactions { get; }

        public ContractData(
            string id,
            string title,
            string description,
            string region,
            ContractDifficulty difficulty,
            int reward,
            TimeSpan duration,
            string[] requiredQualifications = null,
            string[] targetFactions = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Region = region ?? throw new ArgumentNullException(nameof(region));
            Difficulty = difficulty;
            Reward = reward;
            Duration = duration;
            Status = ContractStatus.Available;
            RequiredQualifications = requiredQualifications ?? Array.Empty<string>();
            TargetFactions = targetFactions ?? Array.Empty<string>();
        }

        /// <summary>
        /// Changes the contract status - only allowed state transitions.
        /// </summary>
        public bool TryChangeStatus(ContractStatus newStatus)
        {
            if (!IsValidStatusTransition(Status, newStatus))
                return false;

            Status = newStatus;

            if (newStatus == ContractStatus.Active)
                AcceptedAt = DateTime.UtcNow;

            return true;
        }

        /// <summary>
        /// Checks if the player can accept this contract.
        /// </summary>
        public bool CanAccept(string[] playerQualifications)
        {
            if (Status != ContractStatus.Available)
                return false;

            // Check if player has required qualifications
            foreach (string required in RequiredQualifications)
            {
                bool hasQualification = Array.Exists(playerQualifications,
                    qual => qual.Equals(required, StringComparison.OrdinalIgnoreCase));

                if (!hasQualification)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets remaining time for active contracts.
        /// </summary>
        public TimeSpan? GetRemainingTime()
        {
            if (Status != ContractStatus.Active)
                return null;

            var elapsed = DateTime.UtcNow - AcceptedAt;
            var remaining = Duration - elapsed;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        private static bool IsValidStatusTransition(ContractStatus from, ContractStatus to)
        {
            return (from, to) switch
            {
                (ContractStatus.Available, ContractStatus.Active) => true,
                (ContractStatus.Active, ContractStatus.Completed) => true,
                (ContractStatus.Active, ContractStatus.Failed) => true,
                (ContractStatus.Active, ContractStatus.Expired) => true,
                _ => false
            };
        }
    }

    public enum ContractDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert
    }

    public enum ContractStatus
    {
        Available,
        Active,
        Completed,
        Failed,
        Expired
    }
}