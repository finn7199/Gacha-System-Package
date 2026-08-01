using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Presents baseDropRate as a 0-100% slider (storage stays 0-1, so no asset or test changes are
// needed) and explains the rarest-first conditional model in place, since a single GachaRarity
// asset can't know whether it's the "leftover" tier - that depends on its siblings in whichever
// GachaSystem list it's placed in. See GachaSystemEditor for the live, cross-rarity view.
[CustomEditor(typeof(GachaRarity))]
public class GachaRarityEditor : Editor
{
    private SerializedProperty rarityNameProp;
    private SerializedProperty rarityTierProp;
    private SerializedProperty baseDropRateProp;
    private SerializedProperty usePitySystemProp;
    private SerializedProperty pityPullCountProp;
    private SerializedProperty useDynamicRateProp;
    private SerializedProperty dropRateCurveProp;

    private List<GachaRarity> projectRarities;

    private void OnEnable()
    {
        rarityNameProp = serializedObject.FindProperty("rarityName");
        rarityTierProp = serializedObject.FindProperty("rarityTier");
        baseDropRateProp = serializedObject.FindProperty("baseDropRate");
        usePitySystemProp = serializedObject.FindProperty("usePitySystem");
        pityPullCountProp = serializedObject.FindProperty("pityPullCount");
        useDynamicRateProp = serializedObject.FindProperty("useDynamicRate");
        dropRateCurveProp = serializedObject.FindProperty("dropRateCurve");

        RefreshProjectRarities();
    }

    private void RefreshProjectRarities()
    {
        projectRarities = GachaTierTools.ScanProjectRarities();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(rarityNameProp);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(rarityTierProp);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            RefreshProjectRarities();
        }

        DrawTierLadder();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Drop Rate Settings", EditorStyles.boldLabel);

        float percent = baseDropRateProp.floatValue * 100f;
        EditorGUI.BeginChangeCheck();
        percent = EditorGUILayout.Slider("Drop Rate (%)", percent, 0f, 100f);
        if (EditorGUI.EndChangeCheck())
        {
            baseDropRateProp.floatValue = Mathf.Clamp01(percent / 100f);
        }

        EditorGUILayout.HelpBox(
            "Chance to get this rarity IF no rarer rarity was hit first this pull (checked rarest-first). " +
            "Rarities do not need to add up to 100%.\n\n" +
            "If this is your most common rarity (the lowest Tier in your Gacha System's list), this value is " +
            "ignored - that rarity automatically fills whatever chance is left over. Open your GachaManager to " +
            "see every rarity's real, actual chance together.",
            MessageType.Info);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pity System", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(usePitySystemProp);
        using (new EditorGUI.DisabledScope(!usePitySystemProp.boolValue))
        {
            EditorGUILayout.PropertyField(pityPullCountProp);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dynamic Rate", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useDynamicRateProp);
        using (new EditorGUI.DisabledScope(!useDynamicRateProp.boolValue))
        {
            EditorGUILayout.PropertyField(dropRateCurveProp);
        }

        serializedObject.ApplyModifiedProperties();
    }

    // A project-wide, read-only view of every GachaRarity's tier, since a single asset has no
    // way to know what values its siblings already use. Tier is intrinsic to the asset (unlike
    // drop rate), so this can scan the whole project instead of depending on a scene being open.
    private void DrawTierLadder()
    {
        var self = (GachaRarity)target;

        EditorGUILayout.Space(2);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tiers used in this project (rarest first)", EditorStyles.miniBoldLabel);
        if (GUILayout.Button("Refresh", GUILayout.Width(60)))
        {
            RefreshProjectRarities();
        }
        EditorGUILayout.EndHorizontal();

        if (projectRarities == null || projectRarities.Count == 0)
        {
            EditorGUILayout.LabelField("No GachaRarity assets found yet.", EditorStyles.miniLabel);
        }
        else
        {
            foreach (var rarity in projectRarities)
            {
                if (rarity == null) continue;

                bool isSelf = rarity == self;
                var sharedWith = projectRarities
                    .Where(r => r != null && r != rarity && r.rarityTier == rarity.rarityTier)
                    .Select(r => r.rarityName)
                    .ToList();
                string sharedNote = sharedWith.Count > 0 ? $"   (shares this tier with {string.Join(", ", sharedWith)})" : "";

                string line = $"{(isSelf ? "> " : "   ")}Tier {rarity.rarityTier}: {rarity.rarityName}{sharedNote}";
                EditorGUILayout.LabelField(line, isSelf ? EditorStyles.boldLabel : EditorStyles.miniLabel);
            }
        }

        int nextFreeTier = GachaTierTools.NextFreeTier(projectRarities, self);
        if (GUILayout.Button($"Use next free tier ({nextFreeTier})"))
        {
            rarityTierProp.intValue = nextFreeTier;
            serializedObject.ApplyModifiedProperties();
            RefreshProjectRarities();
        }
    }
}
