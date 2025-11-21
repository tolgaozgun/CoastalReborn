using System;

namespace Core.Data
{
    /// <summary>
    /// Represents intelligence information that players can gather and use.
    /// Data model with immutable getters and clear business rules.
    /// </summary>
    [Serializable]
    public class IntelData
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public IntelType Type { get; }
        public IntelSource Source { get; }
        public int Credibility { get; } // 0-100
        public string Region { get; }
        public DateTime Timestamp { get; }
        public DateTime ExpiresAt { get; }
        public bool IsVerified { get; private set; }
        public string[] RelatedContracts { get; }
        public string[] RelatedFactions { get; }

        public IntelData(
            string id,
            string title,
            string description,
            IntelType type,
            IntelSource source,
            int credibility,
            string region,
            TimeSpan validDuration,
            string[] relatedContracts = null,
            string[] relatedFactions = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Type = type;
            Source = source;
            Credibility = Math.Clamp(credibility, 0, 100);
            Region = region ?? throw new ArgumentNullException(nameof(region));
            Timestamp = DateTime.UtcNow;
            ExpiresAt = Timestamp + validDuration;
            RelatedContracts = relatedContracts ?? Array.Empty<string>();
            RelatedFactions = relatedFactions ?? Array.Empty<string>();
        }

        /// <summary>
        /// Checks if this intel is still valid (not expired).
        /// </summary>
        public bool IsValid => DateTime.UtcNow <= ExpiresAt;

        /// <summary>
        /// Gets the time remaining before this intel expires.
        /// </summary>
        public TimeSpan GetTimeRemaining()
        {
            var remaining = ExpiresAt - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        /// <summary>
        /// Verifies this intel as accurate.
        /// </summary>
        public void Verify()
        {
            IsVerified = true;
        }

        /// <summary>
        /// Gets the effective credibility based on source and time.
        /// </summary>
        public int GetEffectiveCredibility()
        {
            if (!IsValid)
                return 0;

            var sourceMultiplier = GetSourceCredibilityMultiplier(Source);
            var timeDecay = GetTimeDecayMultiplier();

            return (int)(Credibility * sourceMultiplier * timeDecay);
        }

        private static float GetSourceCredibilityMultiplier(IntelSource source)
        {
            return source switch
            {
                IntelSource.Informant => 0.8f,
                IntelSource.Scanner => 1.2f,
                IntelSource.PatrolLog => 1.0f,
                IntelSource.Civilian => 0.4f,
                IntelSource.Satellite => 1.1f,
                _ => 1.0f
            };
        }

        private float GetTimeDecayMultiplier()
        {
            var age = DateTime.UtcNow - Timestamp;
            var totalValidDuration = ExpiresAt - Timestamp;

            if (totalValidDuration <= TimeSpan.Zero)
                return 0f;

            var ageRatio = (float)(age.TotalHours / totalValidDuration.TotalHours);
            return Math.Max(0.5f, 1f - (ageRatio * 0.5f));
        }
    }

    public enum IntelType
    {
        SuspiciousActivity,
        ContrabandLocation,
        SmugglerRoute,
        FactionMovement,
        PirateActivity,
        WeatherReport,
        PortStatus
    }

    public enum IntelSource
    {
        Informant,
        Scanner,
        PatrolLog,
        Civilian,
        Satellite,
        Anonymous
    }
}