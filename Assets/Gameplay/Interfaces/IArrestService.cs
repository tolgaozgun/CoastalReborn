using System;
using System.Collections.Generic;

namespace Gameplay.Interfaces
{
    /// <summary>
    /// Service for managing arrest operations and outcomes.
    /// </summary>
    public interface IArrestService
    {
        /// <summary>
        /// Event fired when an arrest is initiated.
        /// </summary>
        event Action<string> OnArrestInitiated;

        /// <summary>
        /// Event fired when an arrest is completed.
        /// </summary>
        event Action<ArrestResult> OnArrestCompleted;

        /// <summary>
        /// Get all completed arrests for the current session.
        /// </summary>
        IReadOnlyList<ArrestResult> SessionArrests { get; }

        /// <summary>
        /// Initiate an arrest of a target ship.
        /// </summary>
        /// <param name="targetShipId">Target ship ID</param>
        /// <param name="suspicionLevel">Suspicion level at time of arrest</param>
        /// <param name="evidence">Evidence supporting the arrest</param>
        /// <returns>Arrest operation ID</returns>
        string InitiateArrest(string targetShipId, float suspicionLevel, List<Evidence> evidence);

        /// <summary>
        /// Complete an arrest operation.
        /// </summary>
        /// <param name="arrestId">Arrest operation ID</param>
        /// <param name="outcome">Arrest outcome</param>
        void CompleteArrest(string arrestId, ArrestOutcome outcome);

        /// <summary>
        /// Get an arrest operation by ID.
        /// </summary>
        /// <param name="arrestId">Arrest ID</param>
        /// <returns>Arrest operation or null</returns>
        ArrestOperation GetArrestOperation(string arrestId);

        /// <summary>
        /// Check if an arrest is legally justified.
        /// </summary>
        /// <param name="suspicionLevel">Suspicion level</param>
        /// <param name="evidence">Supporting evidence</param>
        /// <returns>Legal assessment</returns>
        LegalAssessment AssessArrestLegality(float suspicionLevel, List<Evidence> evidence);

        /// <summary>
        /// Calculate reputation impact of an arrest.
        /// </summary>
        /// <param name="outcome">Arrest outcome</param>
        /// <param name="legality">Legal assessment</param>
        /// <returns>Reputation impact</returns>
        float CalculateReputationImpact(ArrestOutcome outcome, LegalAssessment legality);

        /// <summary>
        /// Seize contraband from a ship.
        /// </summary>
        /// <param name="targetShipId">Target ship ID</param>
        /// <returns>List of seized items</returns>
        List<ContrabandItem> SeizeContraband(string targetShipId);

        /// <summary>
        /// Initialize arrest service.
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// Arrest operation data.
    /// </summary>
    [Serializable]
    public class ArrestOperation
    {
        public string Id;
        public string TargetShipId;
        public DateTime StartTime;
        public float SuspicionLevel;
        public List<Evidence> Evidence;
        public ArrestOutcome Outcome;
        public bool IsCompleted;

        public ArrestOperation()
        {
            Id = System.Guid.NewGuid().ToString();
            StartTime = DateTime.Now;
            Evidence = new List<Evidence>();
            IsCompleted = false;
        }
    }

    /// <summary>
    /// Arrest result summary.
    /// </summary>
    [Serializable]
    public class ArrestResult
    {
        public string ArrestId;
        public string TargetShipId;
        public ArrestOutcome Outcome;
        public LegalAssessment Legality;
        public float ReputationImpact;
        public List<ContrabandItem> SeizedItems;
        public DateTime CompletionTime;

        public ArrestResult()
        {
            SeizedItems = new List<ContrabandItem>();
            CompletionTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Possible arrest outcomes.
    /// </summary>
    public enum ArrestOutcome
    {
        SuccessfulArrest,
        FalsePositive,
        Resistance,
        Escape,
        ShipDestroyed
    }

    /// <summary>
    /// Legal assessment of arrest justification.
    /// </summary>
    public enum LegalAssessment
    {
        FullyJustified,
        PartiallyJustified,
        Unjustified,
        Borderline
    }

    /// <summary>
    /// Evidence supporting an arrest.
    /// </summary>
    [Serializable]
    public class Evidence
    {
        public string Id;
        public string Type;
        public string Description;
        public float Credibility;
        public DateTime Timestamp;
    }

    /// <summary>
    /// Seized contraband item.
    /// </summary>
    [Serializable]
    public class ContrabandItem
    {
        public string Type;
        public int Quantity;
        public float EstimatedValue;
        public string Description;
    }
}