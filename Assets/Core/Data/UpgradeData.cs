using System;
using System.Collections.Generic;

namespace Core.Data
{
    /// <summary>
    /// Represents an upgrade that players can purchase to improve their capabilities.
    /// Follows immutable data model principles with clear business logic.
    /// </summary>
    [Serializable]
    public class UpgradeData
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public UpgradeCategory Category { get; }
        public int Cost { get; }
        public UpgradeTier Tier { get; }
        public bool IsPurchased { get; private set; }
        public bool IsUnlocked { get; private set; }
        public string[] Prerequisites { get; }
        public UpgradeEffect[] Effects { get; }
        public string[] Counters { get; } // What faction tactics this counters

        public UpgradeData(
            string id,
            string name,
            string description,
            UpgradeCategory category,
            int cost,
            UpgradeTier tier,
            UpgradeEffect[] effects,
            string[] prerequisites = null,
            string[] counters = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Category = category;
            Cost = Math.Max(0, cost);
            Tier = tier;
            Effects = effects ?? Array.Empty<UpgradeEffect>();
            Prerequisites = prerequisites ?? Array.Empty<string>();
            Counters = counters ?? Array.Empty<string>();
        }

        /// <summary>
        /// Unlocks this upgrade for purchase (when prerequisites are met).
        /// </summary>
        public void Unlock()
        {
            if (IsPurchased)
                return;

            IsUnlocked = true;
        }

        /// <summary>
        /// Purchases this upgrade if unlocked and affordable.
        /// </summary>
        public bool TryPurchase(int availableFunds)
        {
            if (!IsUnlocked || IsPurchased || Cost > availableFunds)
                return false;

            IsPurchased = true;
            return true;
        }

        /// <summary>
        /// Gets whether this upgrade counters a specific faction tactic.
        /// </summary>
        public bool CountersTactic(string tacticId)
        {
            return Array.Exists(Counters, counter =>
                counter.Equals(tacticId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all effects of this upgrade.
        /// </summary>
        public IReadOnlyList<UpgradeEffect> GetEffects()
        {
            return IsPurchased ? Effects : Array.Empty<UpgradeEffect>();
        }
    }

    /// <summary>
    /// Represents a specific effect that an upgrade provides.
    /// </summary>
    [Serializable]
    public class UpgradeEffect
    {
        public string StatName { get; }
        public float Modifier { get; }
        public EffectType Type { get; }
        public string Description { get; }

        public UpgradeEffect(string statName, float modifier, EffectType type, string description = "")
        {
            StatName = statName ?? throw new ArgumentNullException(nameof(statName));
            Modifier = modifier;
            Type = type;
            Description = description ?? string.Empty;
        }

        /// <summary>
        /// Applies this effect to a base value.
        /// </summary>
        public float Apply(float baseValue)
        {
            return Type switch
            {
                EffectType.Additive => baseValue + Modifier,
                EffectType.Multiplicative => baseValue * Modifier,
                EffectType.Override => Modifier,
                _ => baseValue
            };
        }
    }

    public enum UpgradeCategory
    {
        Scanner,
        Engine,
        BoardingGear,
        Intel,
        Defensive,
        Offensive
    }

    public enum UpgradeTier
    {
        Basic,
        Advanced,
        Elite,
        Prototype
    }

    public enum EffectType
    {
        Additive,
        Multiplicative,
        Override
    }
}