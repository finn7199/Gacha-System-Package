using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Shows the drop rates configured across availableRarities as a single, live table so a
// non-programmer can see how the rarities relate to each other - the confusion the raw
// per-asset slider can't address on its own, since a single GachaRarity has no idea what
// its siblings look like.
[CustomEditor(typeof(GachaSystem))]
public class GachaSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var system = (GachaSystem)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Live Drop Rate Preview", EditorStyles.boldLabel);

        if (system.availableRarities == null || system.availableRarities.Count == 0)
        {
            EditorGUILayout.HelpBox("Add rarities above to see their actual pull chances here.", MessageType.Info);
            return;
        }

        var validRarities = system.availableRarities.Where(r => r != null).ToList();
        if (validRarities.Count == 0)
        {
            EditorGUILayout.HelpBox("Every slot in Available Rarities is empty.", MessageType.Warning);
            return;
        }

        var marginals = GachaRates.ComputeMarginals(validRarities)
            .OrderByDescending(m => m.rarity.rarityTier)
            .ToList();

        EditorGUILayout.HelpBox(
            "Drop rates are rolled rarest-first, one independent check at a time - they do NOT need to add up to 100%. " +
            "\"Actual chance\" below is what really happens per pull once every rarer tier's check has failed.",
            MessageType.None);

        DrawTableHeader();
        foreach (var m in marginals)
        {
            DrawTableRow(m);
        }

        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField($"Total actual chance: {marginals.Sum(m => m.marginalChance) * 100f:0.##}%", EditorStyles.miniLabel);

        DrawWarnings(system, validRarities, marginals);
        DrawTierTools(system, validRarities);
    }

    private static void DrawTableHeader()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tier", EditorStyles.boldLabel, GUILayout.Width(40));
        EditorGUILayout.LabelField("Rarity", EditorStyles.boldLabel, GUILayout.Width(90));
        EditorGUILayout.LabelField("Rate you set", EditorStyles.boldLabel, GUILayout.Width(110));
        EditorGUILayout.LabelField("Actual chance/pull", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
    }

    private static void DrawTableRow(GachaRates.RarityRate m)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(m.rarity.rarityTier.ToString(), GUILayout.Width(40));
        EditorGUILayout.LabelField(m.rarity.rarityName, GUILayout.Width(90));

        string configuredLabel = m.isLeftoverTier
            ? $"(ignored: {m.configuredRate * 100f:0.##}%)"
            : $"{m.configuredRate * 100f:0.##}%";
        EditorGUILayout.LabelField(configuredLabel, GUILayout.Width(110));

        EditorGUILayout.LabelField($"{m.marginalChance * 100f:0.##}%");
        EditorGUILayout.EndHorizontal();
    }

    // The list above is drawn top-to-bottom = most common-to-rarest to match this table's Tier
    // column and the shipped sample project, but list order is only a tie-breaker today - Tier
    // is what the roll logic actually uses. When they disagree, offer to reconcile them, always
    // showing the exact change first since renumbering can rewrite tiers on shared assets.
    private static void DrawTierTools(GachaSystem system, List<GachaRarity> validRarities)
    {
        var changes = GachaTierTools.DescribeRenumber(validRarities);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("List Order vs. Tier Order", EditorStyles.boldLabel);

        if (changes.Count == 0)
        {
            EditorGUILayout.LabelField("List order matches Tier order (top = most common).", EditorStyles.miniLabel);
            return;
        }

        EditorGUILayout.HelpBox(
            "Available Rarities' list order doesn't match Tier order. Tier is what the pull logic actually " +
            "uses - list order only breaks ties between equal tiers.",
            MessageType.Warning);

        foreach (var change in changes)
        {
            EditorGUILayout.LabelField("  " + change, EditorStyles.miniLabel);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Sort List to Match Tiers"))
        {
            Undo.RecordObject(system, "Sort Available Rarities");
            system.availableRarities = validRarities.OrderBy(r => r.rarityTier).ToList();
            EditorUtility.SetDirty(system);
        }

        if (GUILayout.Button("Renumber Tiers from List Order"))
        {
            GachaTierTools.ApplyRenumber(validRarities);
        }
        EditorGUILayout.EndHorizontal();
    }

    private static void DrawWarnings(GachaSystem system, List<GachaRarity> validRarities, List<GachaRates.RarityRate> marginals)
    {
        var leftover = marginals.FirstOrDefault(m => m.isLeftoverTier);
        if (leftover.rarity != null)
        {
            EditorGUILayout.HelpBox(
                $"\"{leftover.rarity.rarityName}\" is your most common rarity (lowest Tier), so its Drop Rate value is ignored - it fills whatever chance is left over ({leftover.marginalChance * 100f:0.##}%).",
                MessageType.Info);
        }

        foreach (var m in marginals)
        {
            if (!m.isLeftoverTier && m.configuredRate >= 1f)
            {
                EditorGUILayout.HelpBox(
                    $"\"{m.rarity.rarityName}\" has a Drop Rate of 100% or more, which makes every less-rare tier below it unreachable except via pity.",
                    MessageType.Warning);
            }
        }

        foreach (var group in GachaRates.FindDuplicateTiers(validRarities))
        {
            EditorGUILayout.HelpBox(
                $"Multiple rarities share Tier {group.Key} ({string.Join(", ", group.Select(r => r.rarityName))}). " +
                "They'll reset each other's pity every time either one is pulled, and it's ambiguous which one is " +
                "the leftover tier. Give each rarity a unique Tier - see the tier tools below.",
                MessageType.Warning);
        }

        if (system.gachaPool != null)
        {
            foreach (var rarity in validRarities)
            {
                bool hasItems = system.gachaPool.Any(item => item != null && item.rarity == rarity);
                if (!hasItems)
                {
                    EditorGUILayout.HelpBox($"No items in Gacha Pool use \"{rarity.rarityName}\" - it will never be pulled.", MessageType.Warning);
                }
            }
        }

        if (validRarities.Any(r => r.useDynamicRate || r.usePitySystem))
        {
            EditorGUILayout.HelpBox(
                "These percentages are base rates only. Rarities using Pity or a Dynamic Rate curve will have higher real long-run chances than shown here.",
                MessageType.None);
        }
    }
}
