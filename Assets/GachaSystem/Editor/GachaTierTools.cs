using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Editor-only helpers for working with GachaRarity.rarityTier across the whole project. Kept
// separate from GachaRates.cs because AssetDatabase/Undo can't be referenced from the runtime
// assembly - the actual tier math (SuggestSequentialTiers) still lives there so it stays testable.
public static class GachaTierTools
{
    // Every GachaRarity asset in the project, rarest first. Project-wide, so this works even for
    // an asset that isn't (yet) referenced by any GachaSystem - unlike drop rate, tier is
    // intrinsic to the asset rather than dependent on which list it sits in.
    public static List<GachaRarity> ScanProjectRarities()
    {
        var guids = AssetDatabase.FindAssets("t:GachaRarity");
        var result = new List<GachaRarity>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var rarity = AssetDatabase.LoadAssetAtPath<GachaRarity>(path);
            if (rarity != null)
            {
                result.Add(rarity);
            }
        }

        return result.OrderByDescending(r => r.rarityTier).ToList();
    }

    // The lowest tier value not currently used by any other scanned rarity. A brand-new rarity
    // defaulting to 0 is easy to leave colliding with something else by accident; this gives a
    // one-click way to place it after everything that already exists instead.
    public static int NextFreeTier(IEnumerable<GachaRarity> scanned, GachaRarity excluding)
    {
        var others = scanned.Where(r => r != null && r != excluding).ToList();
        if (others.Count == 0) return 1;
        return others.Max(r => r.rarityTier) + 1;
    }

    // Human-readable preview of what "Renumber Tiers from List Order" would change, so the user
    // can see the effect before committing to it - this can rewrite tiers on shared assets, so it
    // should never run without the user seeing what will change first.
    public static List<string> DescribeRenumber(IList<GachaRarity> rarities)
    {
        var descriptions = new List<string>();
        if (rarities == null) return descriptions;

        var suggested = GachaRates.SuggestSequentialTiers(rarities);
        for (int i = 0; i < rarities.Count; i++)
        {
            var rarity = rarities[i];
            if (rarity == null) continue;
            if (rarity.rarityTier != suggested[i])
            {
                descriptions.Add($"{rarity.rarityName}: {rarity.rarityTier} -> {suggested[i]}");
            }
        }

        return descriptions;
    }

    // Writes sequential tiers (top of list = most common = lowest tier) onto the assets
    // referenced by `rarities`. Only ever called from an explicit button click.
    public static void ApplyRenumber(IList<GachaRarity> rarities)
    {
        if (rarities == null) return;

        var suggested = GachaRates.SuggestSequentialTiers(rarities);
        var validRarities = rarities.Where(r => r != null).ToArray();
        Undo.RecordObjects(validRarities, "Renumber Rarity Tiers");

        for (int i = 0; i < rarities.Count; i++)
        {
            var rarity = rarities[i];
            if (rarity == null) continue;
            rarity.rarityTier = suggested[i];
            EditorUtility.SetDirty(rarity);
        }

        AssetDatabase.SaveAssets();
    }
}
