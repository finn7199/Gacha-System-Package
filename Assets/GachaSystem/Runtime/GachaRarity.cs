using UnityEngine;

[CreateAssetMenu(fileName = "NewGachaRarity", menuName = "Gacha System/Gacha Rarity")]
public class GachaRarity : ScriptableObject
{
    [Tooltip("Name of the rarity (e.g., R, SR, SSR)")]
    public string rarityName;

    [Tooltip("Only the relative order matters, not the exact number (e.g. 1/2/3 works exactly like 10/20/30). Lower = more common. The lowest tier's Drop Rate is ignored (it fills whatever is left over), and pity resets cascade to every tier at or below whichever tier is obtained - so if two rarities share a tier, they reset each other's pity, and if everything is left at the same tier, pity never fires at all. See GachaRarityEditor's tier list below and GachaSystemEditor's rate table for a live view across all your rarities.")]
    public int rarityTier;

    [Header("Drop Rate Settings")]
    [Tooltip("Chance to get this rarity IF no rarer rarity was hit first this pull (rarest-first, does not need to sum to 1 across rarities). Ignored for your most common rarity (lowest Rarity Tier) - it fills whatever is left over. See GachaSystemEditor's rate table for the real per-pull chance.")]
    [Range(0, 1)]
    public float baseDropRate;

    [Header("Pity System")]
    [Tooltip("Enable pity system for this rarity")]
    public bool usePitySystem;

    [Tooltip("Number of pulls required to guarantee this rarity")]
    [Min(1)]
    public int pityPullCount = 10;

    [Header("Dynamic Rate")]
    [Tooltip("Enable dynamic drop rate that increases with pulls")]
    public bool useDynamicRate;

    [Tooltip("Curve defining how drop rate increases with pity")]
    public AnimationCurve dropRateCurve;
}