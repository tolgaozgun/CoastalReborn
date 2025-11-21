using System;
using System.Collections.Generic;

namespace Core.Interfaces
{
    /// <summary>
    /// Service for managing player upgrades and progression.
    /// </summary>
    public interface IUpgradeService
    {
        /// <summary>
        /// Event fired when an upgrade is purchased.
        /// </summary>
        event Action<UpgradeNode> OnUpgradePurchased;

        /// <summary>
        /// Event fired when upgrade points are earned.
        /// </summary>
        event Action<int> OnUpgradePointsEarned;

        /// <summary>
        /// Current upgrade points available.
        /// </summary>
        int AvailablePoints { get; }

        /// <summary>
        /// Get all available upgrade nodes.
        /// </summary>
        /// <returns>All upgrade nodes in the tree</returns>
        IReadOnlyList<UpgradeNode> GetAllUpgradeNodes();

        /// <summary>
        /// Get upgrade nodes for a specific category.
        /// </summary>
        /// <param name="category">Upgrade category</param>
        /// <returns>Upgrade nodes in category</returns>
        IReadOnlyList<UpgradeNode> GetNodesInCategory(UpgradeCategory category);

        /// <summary>
        /// Get an upgrade node by ID.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns>Upgrade node or null</returns>
        UpgradeNode GetUpgradeNode(string nodeId);

        /// <summary>
        /// Check if a node can be purchased.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns>True if can be purchased</returns>
        bool CanPurchaseNode(string nodeId);

        /// <summary>
        /// Purchase an upgrade node.
        /// </summary>
        /// <param name="nodeId">Node ID to purchase</param>
        /// <returns>True if purchase successful</returns>
        bool PurchaseNode(string nodeId);

        /// <summary>
        /// Check if a node is purchased.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns>True if purchased</returns>
        bool IsNodePurchased(string nodeId);

        /// <summary>
        /// Get the current level of a node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns>Current level (0 if not purchased)</returns>
        int GetNodeLevel(string nodeId);

        /// <summary>
        /// Calculate the total effect value for an upgrade type.
        /// </summary>
        /// <param name="effectType">Effect type to calculate</param>
        /// <returns>Total effect value from all purchased upgrades</returns>
        float GetTotalEffectValue(UpgradeEffectType effectType);

        /// <summary>
        /// Add upgrade points.
        /// </summary>
        /// <param name="points">Points to add</param>
        void AddUpgradePoints(int points);

        /// <summary>
        /// Reset all upgrades.
        /// </summary>
        void ResetUpgrades();
    }

    /// <summary>
    /// Represents an upgrade node in the upgrade tree.
    /// </summary>
    [Serializable]
    public class UpgradeNode
    {
        public string Id;
        public string Name;
        public string Description;
        public UpgradeCategory Category;
        public int Cost;
        public int MaxLevel;
        public List<string> PrerequisiteIds;
        public List<UpgradeEffect> Effects;
        public string IconPath;
        public bool IsPurchased;
        public int CurrentLevel;

        public UpgradeNode()
        {
            Id = System.Guid.NewGuid().ToString();
            MaxLevel = 1;
            PrerequisiteIds = new List<string>();
            Effects = new List<UpgradeEffect>();
            CurrentLevel = 0;
            IsPurchased = false;
        }

        public int GetCurrentCost()
        {
            return Cost * (CurrentLevel + 1);
        }

        public bool IsMaxLevel()
        {
            return CurrentLevel >= MaxLevel;
        }
    }

    /// <summary>
    /// Upgrade categories.
    /// </summary>
    public enum UpgradeCategory
    {
        Scanner,
        Engine,
        Boarding,
        Intel,
        Armor
    }

    /// <summary>
    /// Individual upgrade effect.
    /// </summary>
    [Serializable]
    public class UpgradeEffect
    {
        public UpgradeEffectType Type;
        public float Value;
        public EffectOperation Operation;
    }

    /// <summary>
    /// Types of upgrade effects.
    /// </summary>
    public enum UpgradeEffectType
    {
        ScannerRange,
        ScannerPower,
        ScannerHeatReduction,
        EngineSpeed,
        EngineAcceleration,
        BoardingSpeed,
        BoardingSuccessRate,
        IntelAccuracy,
        IntelRange,
        ArmorDamage,
        ArmorDurability
    }

    /// <summary>
    /// How the effect value is applied.
    /// </summary>
    public enum EffectOperation
    {
        Add,
        Multiply,
        Set
    }
}