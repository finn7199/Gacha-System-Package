using UnityEngine;

[CreateAssetMenu(fileName = "NewGachaRarity", menuName = "Gacha System/Gacha Rarity")]
public class GachaRarity : ScriptableObject
{
    [Tooltip("Name of the rarity (e.g., R, SR, SSR)")]
    public string rarityName;

    [Tooltip("Order of this rarity (lower number = more common)")]
    public int rarityTier;

    [Header("Drop Rate Settings")]
    [Tooltip("Base drop rate for this rarity (0-1)")]
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