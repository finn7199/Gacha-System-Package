using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Pure, side-effect-free rate math shared by GachaSystem's validation and the Editor tooling
// in Assets/Scripts/Editor/, so the "what will actually happen" math lives in exactly one
// place and can't drift from GachaSystem.SelectHierarchicalRarity().
public static class GachaRates
{
    public struct RarityRate
    {
        public GachaRarity rarity;

        // The raw baseDropRate value as authored on the asset.
        public float configuredRate;

        // The real per-pull probability of landing this rarity, accounting for every rarer
        // tier's roll having to fail first. Ignores pity/dynamic-curve bonuses - this reflects
        // only the authored base rates.
        public float marginalChance;

        // True if this is the most common rarity (lowest rarityTier), whose configuredRate is
        // never actually rolled - it just receives whatever probability is left over.
        public bool isLeftoverTier;
    }

    // Computes each rarity's actual marginal probability under the rarest-first independent-roll
    // model. Tie-breaking for duplicate rarityTier values mirrors SelectHierarchicalRarity's
    // OrderByDescending (stable sort, so ties keep list order) so this always agrees with what
    // GachaSystem will actually roll.
    public static List<RarityRate> ComputeMarginals(IList<GachaRarity> rarities)
    {
        var result = new List<RarityRate>();
        if (rarities == null) return result;

        var sorted = rarities.Where(r => r != null).OrderByDescending(r => r.rarityTier).ToList();
        if (sorted.Count == 0) return result;

        GachaRarity leftoverTier = sorted[sorted.Count - 1];
        float remaining = 1f;

        foreach (var rarity in sorted)
        {
            bool isLeftover = rarity == leftoverTier;
            float clampedRate = Mathf.Clamp01(rarity.baseDropRate);

            result.Add(new RarityRate
            {
                rarity = rarity,
                configuredRate = rarity.baseDropRate,
                marginalChance = isLeftover ? remaining : remaining * clampedRate,
                isLeftoverTier = isLeftover
            });

            if (!isLeftover)
            {
                remaining *= (1f - clampedRate);
            }
        }

        return result;
    }

    // Rarities sharing the same rarityTier are ambiguous: it's unclear which one is meant to be
    // the "leftover" tier, and validation vs. the actual roll could disagree about it. Worse,
    // since ResetPityCountersAtOrBelow compares tiers with <=, any two rarities sharing a tier
    // will reset each other's pity every time either one is pulled.
    public static List<IGrouping<int, GachaRarity>> FindDuplicateTiers(IList<GachaRarity> rarities)
    {
        if (rarities == null) return new List<IGrouping<int, GachaRarity>>();

        return rarities.Where(r => r != null)
            .GroupBy(r => r.rarityTier)
            .Where(g => g.Count() > 1)
            .ToList();
    }

    // The tier each entry in `rarities` would get if renumbered to match its position in the
    // list, top-to-bottom = most common-to-rarest (matching how the sample project's list and
    // GachaSystemEditor's list-vs-tier preview are laid out). Only relative order is meaningful
    // to the roll logic, so this is a safe, lossless renumbering as long as the list is already
    // in the order the user wants - it does not read or depend on any rarity's current tier.
    public static int[] SuggestSequentialTiers(IList<GachaRarity> rarities)
    {
        if (rarities == null) return new int[0];

        var result = new int[rarities.Count];
        for (int i = 0; i < rarities.Count; i++)
        {
            result[i] = i + 1;
        }
        return result;
    }
}
