using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Stub implementation of the intel service.
    /// </summary>
    public class IntelService : MonoBehaviour, IIntelService
    {
        private List<IntelEntry> intelEntries = new List<IntelEntry>();

        public event Action<IntelEntry> OnIntelReceived;
        public event Action<string, int> OnIntelCredibilityChanged;

        public IReadOnlyList<IntelEntry> GetIntelEntries()
        {
            return intelEntries.AsReadOnly();
        }

        public IReadOnlyList<IntelEntry> GetIntelForRegion(string region)
        {
            return intelEntries.FindAll(i => i.Region == region);
        }

        public void AddIntel(IntelEntry entry)
        {
            intelEntries.Add(entry);
            OnIntelReceived?.Invoke(entry);
        }

        public void UpdateIntelCredibility(string intelId, int newCredibility)
        {
            var entry = GetIntel(intelId);
            if (entry != null)
            {
                entry.Credibility = Mathf.Clamp(newCredibility, 0, 100);
                OnIntelCredibilityChanged?.Invoke(intelId, entry.Credibility);
            }
        }

        public IntelEntry GetIntel(string intelId)
        {
            return intelEntries.Find(i => i.Id == intelId);
        }

        public CrossReferenceResult CrossReferenceIntel(string intelId)
        {
            return new CrossReferenceResult
            {
                IsConfirmed = false,
                ConfidenceScore = 0,
                ConfirmingSources = new List<string>(),
                ConflictingSources = new List<string>(),
                Assessment = "Cross-reference not implemented yet"
            };
        }
    }
}