using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    public static GachaSystem Instance { get; private set; }

    [Header("Rarity Configuration")]
    [Tooltip("List of all available rarities")]
    public List<GachaRarity> availableRarities;

    [Header("Gacha Pool")]
    public List<GachaItem> gachaPool;

    private Dictionary<GachaRarity, int> pityCounters = new Dictionary<GachaRarity, int>();
    public int gachaTokens = 200;

    [Header("Debug Options")]
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        InitializePityCounters();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        ValidateRarityConfiguration();
    }

    private void InitializePityCounters()
    {
        pityCounters.Clear();
        EnsurePityCountersInitialized();
    }

    // Adds a counter for any rarity that doesn't have one yet, without disturbing existing
    // progress. Called before every pull so availableRarities can be assigned or changed after
    // Awake (e.g. in tests, or tooling that builds the roster at runtime) without losing pity.
    private void EnsurePityCountersInitialized()
    {
        if (availableRarities == null) return;

        foreach (var rarity in availableRarities)
        {
            if (rarity == null) continue;
            if (!pityCounters.ContainsKey(rarity))
            {
                pityCounters[rarity] = 0;
            }
        }
    }

    private void ValidateRarityConfiguration()
    {
        if (availableRarities == null || availableRarities.Count == 0)
        {
            Debug.LogWarning("[GachaSystem] No rarities configured!");
            return;
        }

        var validRarities = availableRarities.Where(r => r != null).ToList();
        if (validRarities.Count != availableRarities.Count)
        {
            Debug.LogWarning("[GachaSystem] availableRarities contains empty slots. Fill or remove them.");
        }

        // Each tier is rolled independently, rarest first (see SelectHierarchicalRarity), so
        // baseDropRate is a conditional "chance given no rarer tier already hit" rather than a
        // marginal probability. These don't need to sum to 1, but a non-lowest tier sitting at
        // or above 1.0 will make every tier below it unreachable except via its own pity.
        var marginals = GachaRates.ComputeMarginals(validRarities);
        var lowestTier = marginals.FirstOrDefault(m => m.isLeftoverTier).rarity;
        foreach (var rarity in validRarities)
        {
            if (rarity != lowestTier && rarity.baseDropRate >= 1f)
            {
                Debug.LogWarning($"[GachaSystem] {rarity.rarityName} has baseDropRate >= 1.0, which makes every less-rare tier below it unreachable outside of pity.");
            }
        }

        var duplicateNames = validRarities.GroupBy(r => r.rarityName).Where(g => g.Count() > 1);
        foreach (var group in duplicateNames)
        {
            Debug.LogWarning($"[GachaSystem] Multiple rarities share the name '{group.Key}'. GetPityStatus() will merge them.");
        }

        foreach (var group in GachaRates.FindDuplicateTiers(validRarities))
        {
            Debug.LogWarning($"[GachaSystem] Multiple rarities share Tier {group.Key} ({string.Join(", ", group.Select(r => r.rarityName))}). They will reset each other's pity every time either one is pulled, and it's ambiguous which one is treated as the most common (leftover) tier. Give each rarity a unique Tier value.");
        }

        if (showDebugLogs)
        {
            foreach (var rarity in validRarities)
            {
                float currentDynamic = 0f;
                float maxDynamic = 0f;

                if (rarity.useDynamicRate && rarity.dropRateCurve != null)
                {
                    int currentPity = pityCounters.TryGetValue(rarity, out int pity) ? pity : 0;
                    int pityGoal = Mathf.Max(1, rarity.pityPullCount);
                    float normalizedPity = Mathf.Clamp01((float)currentPity / pityGoal);

                    currentDynamic = rarity.dropRateCurve.Evaluate(normalizedPity);
                    maxDynamic = rarity.dropRateCurve.Evaluate(1f);
                }

                float currentTotal = Mathf.Clamp01(rarity.baseDropRate + currentDynamic);
                float maxTotal = Mathf.Clamp01(rarity.baseDropRate + maxDynamic);

                string dynamicInfo = rarity.useDynamicRate ?
                    $", Dynamic: +{currentDynamic:F4} (current), +{maxDynamic:F4} (max), Total: {currentTotal:F4} (current), {maxTotal:F4} (max)" :
                    "";

                Debug.Log($"[GachaSystem] {rarity.rarityName}: Base={rarity.baseDropRate:F4}{dynamicInfo}");
            }
        }

        if (gachaPool != null)
        {
            foreach (var rarity in validRarities)
            {
                int itemCount = gachaPool.Count(item => item != null && item.rarity == rarity);
                if (itemCount == 0)
                {
                    Debug.LogWarning($"[GachaSystem] No items found for rarity: {rarity.rarityName}. This rarity will never be pulled!");
                }
            }
        }
    }

    public List<GachaItem> PerformPull(int pullCount)
    {
        if (pullCount <= 0)
        {
            if (showDebugLogs) Debug.LogWarning($"[GachaSystem] PerformPull called with non-positive pullCount ({pullCount}); ignoring.");
            return new List<GachaItem>();
        }

        if (gachaTokens < pullCount)
        {
            if (showDebugLogs) Debug.Log("Not enough tokens!");
            return new List<GachaItem>();
        }

        gachaTokens -= pullCount;
        List<GachaItem> pulledItems = new List<GachaItem>();

        for (int i = 0; i < pullCount; i++)
        {
            GachaItem item = Pull();
            if (item != null)
            {
                pulledItems.Add(item);
            }
        }

        return pulledItems;
    }

    public GachaItem Pull()
    {
        if (availableRarities == null || !availableRarities.Any(r => r != null))
        {
            Debug.LogError("[GachaSystem] Cannot pull: no rarities configured.");
            return null;
        }

        EnsurePityCountersInitialized();

        // 1. Increment all pity counters first
        foreach (var rarity in availableRarities)
        {
            if (rarity != null && rarity.usePitySystem && pityCounters.ContainsKey(rarity))
            {
                pityCounters[rarity]++;
            }
        }

        // 2. Check for pity guarantees (Highest tier takes priority)
        GachaRarity guaranteedRarity = CheckPityGuarantees();

        if (guaranteedRarity != null)
        {
            if (showDebugLogs) Debug.Log($"Pity activated for {guaranteedRarity.rarityName}!");
            ResetPityCountersAtOrBelow(guaranteedRarity);
            return GetRandomItemFromPool(guaranteedRarity);
        }

        // 3. Roll each tier independently, rarest first
        GachaRarity selectedRarity = SelectHierarchicalRarity();
        if (selectedRarity == null)
        {
            Debug.LogError("[GachaSystem] Cannot pull: no valid rarity resolved.");
            return null;
        }

        ResetPityCountersAtOrBelow(selectedRarity);

        if (showDebugLogs) Debug.Log($"Pulled {selectedRarity.rarityName}");

        return GetRandomItemFromPool(selectedRarity);
    }

    // Rolls tiers rarest-first, each as an independent check (mirrors Genshin-style banners:
    // the 5-star check happens first, then the 4-star check, etc.), rather than building one
    // cumulative distribution. This keeps every tier's chance mathematically valid ([0,1]) on
    // its own, regardless of what pity-driven bonuses other tiers are carrying that pull - no
    // normalization step is needed, and the most common tier is simply whatever's left over
    // once every rarer tier has failed its roll.
    private GachaRarity SelectHierarchicalRarity()
    {
        var sortedRarities = availableRarities.Where(r => r != null).OrderByDescending(r => r.rarityTier).ToList();
        if (sortedRarities.Count == 0) return null;

        GachaRarity lowestTier = sortedRarities[sortedRarities.Count - 1];

        for (int i = 0; i < sortedRarities.Count - 1; i++)
        {
            GachaRarity rarity = sortedRarities[i];
            float rate = rarity.baseDropRate;

            if (rarity.useDynamicRate && rarity.dropRateCurve != null && pityCounters.TryGetValue(rarity, out int currentPity))
            {
                int pityGoal = Mathf.Max(1, rarity.pityPullCount);
                float normalizedPity = Mathf.Clamp01((float)currentPity / pityGoal);
                rate += rarity.dropRateCurve.Evaluate(normalizedPity);
            }

            rate = Mathf.Clamp01(rate);

            if (Random.value < rate)
            {
                return rarity;
            }
        }

        return lowestTier;
    }

    private GachaRarity CheckPityGuarantees()
    {
        foreach (var rarity in availableRarities.Where(r => r != null).OrderByDescending(r => r.rarityTier))
        {
            if (rarity.usePitySystem &&
                pityCounters.TryGetValue(rarity, out int currentPity) &&
                currentPity >= Mathf.Max(1, rarity.pityPullCount))
            {
                return rarity;
            }
        }
        return null;
    }

    // Obtaining a rarity satisfies the pity of every rarity at least as common (e.g. a 5-star
    // pull also resets 4-star pity), matching how real gacha pity guarantees are scoped to
    // "this tier or better" rather than an exact tier match.
    private void ResetPityCountersAtOrBelow(GachaRarity obtainedRarity)
    {
        if (obtainedRarity == null) return;

        foreach (var rarity in availableRarities)
        {
            if (rarity != null && rarity.rarityTier <= obtainedRarity.rarityTier && pityCounters.ContainsKey(rarity))
            {
                pityCounters[rarity] = 0;
            }
        }
    }

    private GachaItem GetRandomItemFromPool(GachaRarity rarity)
    {
        List<GachaItem> availableItems = gachaPool.Where(item => item != null && item.rarity == rarity).ToList();

        if (availableItems.Count == 0)
        {
            Debug.LogError($"[GachaSystem] No items found for rarity: {rarity.rarityName}");
            return null;
        }

        return availableItems[Random.Range(0, availableItems.Count)];
    }

    public void AddGachaTokens(int addAmount)
    {
        gachaTokens += addAmount;
    }

    public void SetGachaTokens(int setAmount)
    {
        gachaTokens = setAmount;
    }

    public Dictionary<string, int> GetPityStatus()
    {
        Dictionary<string, int> status = new Dictionary<string, int>();
        foreach (var pair in pityCounters)
        {
            status[pair.Key.rarityName] = pair.Value;
        }
        return status;
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            InitializePityCounters();
        }
        ValidateRarityConfiguration();
    }
}