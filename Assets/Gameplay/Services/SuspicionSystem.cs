using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;
using Gameplay.Interfaces;

namespace Gameplay.Services
{
    /// <summary>
    /// Stub implementation of the suspicion system.
    /// </summary>
    public class SuspicionSystem : MonoBehaviour, ISuspicionSystem
    {
        private Dictionary<string, float> shipSuspicion = new Dictionary<string, float>();
        private Dictionary<string, SuspicionBreakdown> suspicionBreakdowns = new Dictionary<string, SuspicionBreakdown>();
        private Dictionary<SuspicionFactor, float> suspicionWeights = new Dictionary<SuspicionFactor, float>();

        public event Action<string, float> OnSuspicionChanged;
        public event Action<string, SuspicionLevel> OnSuspicionThresholdCrossed;

        private void Awake()
        {
            InitializeDefaultWeights();
        }

        private void InitializeDefaultWeights()
        {
            suspicionWeights[SuspicionFactor.DocumentMismatch] = 25f;
            suspicionWeights[SuspicionFactor.MissingStamps] = 15f;
            suspicionWeights[SuspicionFactor.InconsistentData] = 20f;
            suspicionWeights[SuspicionFactor.SuspiciousCargo] = 30f;
            suspicionWeights[SuspicionFactor.NervousBehavior] = 10f;
            suspicionWeights[SuspicionFactor.EvasiveAnswers] = 15f;
            suspicionWeights[SuspicionFactor.RegionHeat] = 10f;
            suspicionWeights[SuspicionFactor.IntelReports] = 20f;
        }

        public SuspicionAssessment CalculateSuspicion(string shipId, ShipPapers papers)
        {
            SuspicionBreakdown breakdown = new SuspicionBreakdown();

            // Calculate suspicion based on documents
            float totalScore = 0f;

            // Check for document anomalies
            if (papers.Anomalies != null)
            {
                foreach (var anomaly in papers.Anomalies)
                {
                    SuspicionFactor factor = ConvertAnomalyToFactor(anomaly.Type);
                    float weight = suspicionWeights.TryGetValue(factor, out float w) ? w : 10f;
                    float score = anomaly.SuspicionValue * weight / 100f;

                    breakdown.FactorScores[factor] = score;
                    breakdown.FactorWeights[factor] = weight;
                    breakdown.DocumentAnomalies.Add(anomaly);
                    totalScore += score;
                }
            }

            // Apply some random civilian error noise
            float civilianError = UnityEngine.Random.Range(0f, 15f);
            totalScore += civilianError;

            // Clamp to 0-100 range
            totalScore = Mathf.Clamp(totalScore, 0f, 100f);
            breakdown.WeightedScore = totalScore;

            SuspicionLevel level = GetSuspicionLevelFromScore(totalScore);

            // Store results
            shipSuspicion[shipId] = totalScore;
            suspicionBreakdowns[shipId] = breakdown;

            return new SuspicionAssessment
            {
                TotalScore = totalScore,
                Level = level,
                Breakdown = breakdown,
                Summary = GenerateSuspicionSummary(level, breakdown),
                ShouldHold = totalScore > 60f,
                Confidence = 0.8f
            };
        }

        public float GetSuspicion(string shipId)
        {
            return shipSuspicion.TryGetValue(shipId, out float suspicion) ? suspicion : 0f;
        }

        public SuspicionBreakdown GetSuspicionBreakdown(string shipId)
        {
            return suspicionBreakdowns.TryGetValue(shipId, out SuspicionBreakdown breakdown) ? breakdown : new SuspicionBreakdown();
        }

        public void UpdateSuspicionFactor(string shipId, SuspicionFactor factor, float value)
        {
            if (!suspicionBreakdowns.ContainsKey(shipId))
            {
                suspicionBreakdowns[shipId] = new SuspicionBreakdown();
            }

            var breakdown = suspicionBreakdowns[shipId];
            breakdown.FactorScores[factor] = value;

            // Recalculate total score
            float totalScore = 0f;
            foreach (var kvp in breakdown.FactorScores)
            {
                float weight = suspicionWeights.TryGetValue(kvp.Key, out float w) ? w : 1f;
                totalScore += kvp.Value * weight;
            }

            totalScore = Mathf.Clamp(totalScore, 0f, 100f);
            shipSuspicion[shipId] = totalScore;

            OnSuspicionChanged?.Invoke(shipId, totalScore);

            // Check for threshold crossing
            SuspicionLevel newLevel = GetSuspicionLevelFromScore(totalScore);
            SuspicionLevel oldLevel = GetSuspicionLevelFromScore(totalScore - (value * suspicionWeights[factor]));
            if (newLevel != oldLevel)
            {
                OnSuspicionThresholdCrossed?.Invoke(shipId, newLevel);
            }
        }

        public SuspicionLevel GetSuspicionLevel(string shipId)
        {
            float suspicion = GetSuspicion(shipId);
            return GetSuspicionLevelFromScore(suspicion);
        }

        public bool ShouldHold(string shipId)
        {
            return GetSuspicion(shipId) > 60f;
        }

        public void ResetSuspicion(string shipId)
        {
            shipSuspicion.Remove(shipId);
            suspicionBreakdowns.Remove(shipId);
            OnSuspicionChanged?.Invoke(shipId, 0f);
        }

        public void SetSuspicionWeights(Dictionary<SuspicionFactor, float> weights)
        {
            suspicionWeights = new Dictionary<SuspicionFactor, float>(weights);
        }

        public Dictionary<SuspicionFactor, float> GetSuspicionWeights()
        {
            return new Dictionary<SuspicionFactor, float>(suspicionWeights);
        }

        private SuspicionFactor ConvertAnomalyToFactor(AnomalyType anomalyType)
        {
            switch (anomalyType)
            {
                case AnomalyType.Mismatch:
                    return SuspicionFactor.DocumentMismatch;
                case AnomalyType.Forgery:
                    return SuspicionFactor.InconsistentData;
                case AnomalyType.Missing:
                    return SuspicionFactor.MissingStamps;
                case AnomalyType.Expired:
                    return SuspicionFactor.DocumentMismatch;
                case AnomalyType.Inconsistent:
                    return SuspicionFactor.InconsistentData;
                default:
                    return SuspicionFactor.DocumentMismatch;
            }
        }

        private SuspicionLevel GetSuspicionLevelFromScore(float score)
        {
            if (score <= 20f) return SuspicionLevel.Clear;
            if (score <= 40f) return SuspicionLevel.Low;
            if (score <= 60f) return SuspicionLevel.Medium;
            if (score <= 80f) return SuspicionLevel.High;
            return SuspicionLevel.Critical;
        }

        private string GenerateSuspicionSummary(SuspicionLevel level, SuspicionBreakdown breakdown)
        {
            switch (level)
            {
                case SuspicionLevel.Clear:
                    return "Documents appear to be in order.";
                case SuspicionLevel.Low:
                    return "Minor discrepancies found, but likely legitimate.";
                case SuspicionLevel.Medium:
                    return "Several issues detected. Further investigation recommended.";
                case SuspicionLevel.High:
                    return "Multiple serious concerns. High probability of smuggling.";
                case SuspicionLevel.Critical:
                    return "Clear evidence of illegal activity. Recommend immediate action.";
                default:
                    return "Unable to determine suspicion level.";
            }
        }
    }
}