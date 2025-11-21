using System;
using System.Collections.Generic;

namespace Core.Interfaces
{
    /// <summary>
    /// Service for managing intelligence data and sources.
    /// </summary>
    public interface IIntelService
    {
        /// <summary>
        /// Event fired when new intel is received.
        /// </summary>
        event Action<IntelEntry> OnIntelReceived;

        /// <summary>
        /// Event fired when intel credibility changes.
        /// </summary>
        event Action<string, int> OnIntelCredibilityChanged;

        /// <summary>
        /// Get all available intel entries.
        /// </summary>
        /// <returns>List of intel entries</returns>
        IReadOnlyList<IntelEntry> GetIntelEntries();

        /// <summary>
        /// Get intel entries for a specific region.
        /// </summary>
        /// <param name="region">Region name</param>
        /// <returns>List of intel entries for region</returns>
        IReadOnlyList<IntelEntry> GetIntelForRegion(string region);

        /// <summary>
        /// Add a new intel entry.
        /// </summary>
        /// <param name="entry">Intel entry to add</param>
        void AddIntel(IntelEntry entry);

        /// <summary>
        /// Update intel credibility.
        /// </summary>
        /// <param name="intelId">Intel ID</param>
        /// <param name="newCredibility">New credibility score (0-100)</param>
        void UpdateIntelCredibility(string intelId, int newCredibility);

        /// <summary>
        /// Get intel by ID.
        /// </summary>
        /// <param name="intelId">Intel ID</param>
        /// <returns>Intel entry or null</returns>
        IntelEntry GetIntel(string intelId);

        /// <summary>
        /// Cross-reference intel with other sources.
        /// </summary>
        /// <param name="intelId">Intel ID to cross-reference</param>
        /// <returns>Cross-reference results</returns>
        CrossReferenceResult CrossReferenceIntel(string intelId);
    }

    /// <summary>
    /// Represents an intelligence entry.
    /// </summary>
    [Serializable]
    public class IntelEntry
    {
        public string Id;
        public string Title;
        public string Description;
        public string Region;
        public IntelSource Source;
        public int Credibility; // 0-100
        public DateTime Timestamp;
        public List<string> RelatedIntelIds;
        public Dictionary<string, object> Metadata;

        public IntelEntry()
        {
            Id = System.Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
            RelatedIntelIds = new List<string>();
            Metadata = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Intel source types.
    /// </summary>
    public enum IntelSource
    {
        Informant,
        PatrolLog,
        Scan,
        CivilianReport,
        InterceptedCommunication
    }

    /// <summary>
    /// Result of cross-referencing intel.
    /// </summary>
    [Serializable]
    public class CrossReferenceResult
    {
        public bool IsConfirmed;
        public int ConfidenceScore;
        public List<string> ConfirmingSources;
        public List<string> ConflictingSources;
        public string Assessment;
    }
}