using System;
using System.Collections.Generic;

namespace Gameplay.Interfaces
{
    /// <summary>
    /// Service for calculating and managing suspicion scores.
    /// </summary>
    public interface ISuspicionSystem
    {
        /// <summary>
        /// Event fired when suspicion score changes.
        /// </summary>
        event Action<string, float> OnSuspicionChanged;

        /// <summary>
        /// Event fired when suspicion threshold is crossed.
        /// </summary>
        event Action<string, SuspicionLevel> OnSuspicionThresholdCrossed;

        /// <summary>
        /// Calculate suspicion score for a ship based on its documents.
        /// </summary>
        /// <param name="shipId">Ship ID</param>
        /// <param name="papers">Ship papers</param>
        /// <returns>Suspicion assessment</returns>
        SuspicionAssessment CalculateSuspicion(string shipId, ShipPapers papers);

        /// <summary>
        /// Get current suspicion for a ship.
        /// </summary>
        /// <param name="shipId">Ship ID</param>
        /// <returns>Current suspicion score</returns>
        float GetSuspicion(string shipId);

        /// <summary>
        /// Get suspicion breakdown for a ship.
        /// </summary>
        /// <param name="shipId">Ship ID</param>
        /// <returns>Detailed suspicion breakdown</returns>
        SuspicionBreakdown GetSuspicionBreakdown(string shipId);

        /// <summary>
        /// Update suspicion for a specific factor.
        /// </summary>
        /// <param name="shipId">Ship ID</param>
        /// <param name="factor">Suspicion factor</param>
        /// <param name="value">New value</param>
        void UpdateSuspicionFactor(string shipId, SuspicionFactor factor, float value);

        /// <summary>
        /// Get the current suspicion level for a ship.
        /// </summary>
        /// <param name="shipId">Ship ID</param>
        /// <returns>Suspicion level</returns>
        SuspicionLevel GetSuspicionLevel(string shipId);

        /// <summary>
        /// Check if suspicion exceeds hold threshold.
        /// </summary>
        /// <param name="shipId">Ship ID</param>
        /// <returns>True if should hold</returns>
        bool ShouldHold(string shipId);

        /// <summary>
        /// Reset suspicion for a ship.
        /// </summary>
        /// <param name="shipId">Ship ID</param>
        void ResetSuspicion(string shipId);

        /// <summary>
        /// Set the weights for suspicion factors.
        /// </summary>
        /// <param name="weights">New weights</param>
        void SetSuspicionWeights(Dictionary<SuspicionFactor, float> weights);

        /// <summary>
        /// Get current suspicion weights.
        /// </summary>
        /// <returns>Current weights</returns>
        Dictionary<SuspicionFactor, float> GetSuspicionWeights();
    }

    /// <summary>
    /// Ship papers containing document information.
    /// </summary>
    [Serializable]
    public class ShipPapers
    {
        public string ShipId;
        public string Origin;
        public string Destination;
        public List<CargoItem> Manifest;
        public List<string> Stamps;
        public string CaptainName;
        public string RegistrationNumber;
        public DateTime IssueDate;
        public DateTime ExpiryDate;
        public List<DocumentAnomaly> Anomalies;

        public ShipPapers()
        {
            Manifest = new List<CargoItem>();
            Stamps = new List<string>();
            Anomalies = new List<DocumentAnomaly>();
        }
    }

    /// <summary>
    /// Cargo item in manifest.
    /// </summary>
    [Serializable]
    public class CargoItem
    {
        public string Description;
        public int Quantity;
        public float Weight;
        public bool IsDeclared;
    }

    /// <summary>
    /// Document anomaly detected.
    /// </summary>
    [Serializable]
    public class DocumentAnomaly
    {
        public string Field;
        public string Description;
        public float SuspicionValue;
        public AnomalyType Type;
    }

    /// <summary>
    /// Types of document anomalies.
    /// </summary>
    public enum AnomalyType
    {
        Mismatch,
        Forgery,
        Missing,
        Expired,
        Inconsistent
    }

    /// <summary>
    /// Factors that contribute to suspicion.
    /// </summary>
    public enum SuspicionFactor
    {
        DocumentMismatch,
        MissingStamps,
        InconsistentData,
        SuspiciousCargo,
        NervousBehavior,
        EvasiveAnswers,
        RegionHeat,
        IntelReports
    }

    /// <summary>
    /// Suspicion assessment result.
    /// </summary>
    [Serializable]
    public class SuspicionAssessment
    {
        public float TotalScore;
        public SuspicionLevel Level;
        public SuspicionBreakdown Breakdown;
        public string Summary;
        public bool ShouldHold;
        public float Confidence;
    }

    /// <summary>
    /// Detailed breakdown of suspicion factors.
    /// </summary>
    [Serializable]
    public class SuspicionBreakdown
    {
        public Dictionary<SuspicionFactor, float> FactorScores;
        public Dictionary<SuspicionFactor, float> FactorWeights;
        public List<DocumentAnomaly> DocumentAnomalies;
        public float WeightedScore;

        public SuspicionBreakdown()
        {
            FactorScores = new Dictionary<SuspicionFactor, float>();
            FactorWeights = new Dictionary<SuspicionFactor, float>();
            DocumentAnomalies = new List<DocumentAnomaly>();
        }
    }

    /// <summary>
    /// Suspicion levels.
    /// </summary>
    public enum SuspicionLevel
    {
        Clear,      // 0-20
        Low,        // 21-40
        Medium,     // 41-60
        High,       // 61-80
        Critical    // 81-100
    }
}