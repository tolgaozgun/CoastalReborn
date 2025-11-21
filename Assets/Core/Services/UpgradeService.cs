using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Stub implementation of the upgrade service.
    /// </summary>
    public class UpgradeService : MonoBehaviour, IUpgradeService
    {
        [SerializeField] private int startingPoints = 10;

        private int availablePoints;
        private List<UpgradeNode> allNodes = new List<UpgradeNode>();

        public int AvailablePoints => availablePoints;

        public event Action<UpgradeNode> OnUpgradePurchased;
        public event Action<int> OnUpgradePointsEarned;

        private void Awake()
        {
            availablePoints = startingPoints;
            InitializeUpgradeTree();
        }

        private void InitializeUpgradeTree()
        {
            // TODO: Load upgrade tree from data
            // For now, create some basic nodes
            allNodes.Add(new UpgradeNode
            {
                Id = "scanner_range_1",
                Name = "Scanner Range I",
                Description = "Increases scanner detection range by 20%",
                Category = UpgradeCategory.Scanner,
                Cost = 5,
                Effects = new List<UpgradeEffect>
                {
                    new UpgradeEffect { Type = UpgradeEffectType.ScannerRange, Value = 1.2f, Operation = EffectOperation.Multiply }
                }
            });
        }

        public IReadOnlyList<UpgradeNode> GetAllUpgradeNodes()
        {
            return allNodes.AsReadOnly();
        }

        public IReadOnlyList<UpgradeNode> GetNodesInCategory(UpgradeCategory category)
        {
            return allNodes.FindAll(n => n.Category == category);
        }

        public UpgradeNode GetUpgradeNode(string nodeId)
        {
            return allNodes.Find(n => n.Id == nodeId);
        }

        public bool CanPurchaseNode(string nodeId)
        {
            var node = GetUpgradeNode(nodeId);
            if (node == null) return false;

            return !node.IsMaxLevel() && availablePoints >= node.GetCurrentCost();
        }

        public bool PurchaseNode(string nodeId)
        {
            var node = GetUpgradeNode(nodeId);
            if (node == null || !CanPurchaseNode(nodeId)) return false;

            availablePoints -= node.GetCurrentCost();
            node.CurrentLevel++;
            node.IsPurchased = true;

            OnUpgradePurchased?.Invoke(node);
            return true;
        }

        public bool IsNodePurchased(string nodeId)
        {
            var node = GetUpgradeNode(nodeId);
            return node?.IsPurchased ?? false;
        }

        public int GetNodeLevel(string nodeId)
        {
            var node = GetUpgradeNode(nodeId);
            return node?.CurrentLevel ?? 0;
        }

        public float GetTotalEffectValue(UpgradeEffectType effectType)
        {
            float totalValue = 0f;
            foreach (var node in allNodes)
            {
                if (node.IsPurchased)
                {
                    foreach (var effect in node.Effects)
                    {
                        if (effect.Type == effectType)
                        {
                            totalValue += effect.Value;
                        }
                    }
                }
            }
            return totalValue;
        }

        public void AddUpgradePoints(int points)
        {
            availablePoints += points;
            OnUpgradePointsEarned?.Invoke(points);
        }

        public void ResetUpgrades()
        {
            // TODO: Return points spent and reset all upgrades
        }
    }
}