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

    // Dictionary to track pity counters for each rarity
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
        DontDestroyOnLoad(gameObject);
        InitializePityCounters();
    }

    private void Start()
    {
        ValidateRarityConfiguration();
    }

    private void InitializePityCounters()
    {
        pityCounters.Clear();
        foreach (var rarity in availableRarities)
        {
            pityCounters[rarity] = 0;
        }
    }

    private void ValidateRarityConfiguration()
    {
        float totalBaseRate = availableRarities.Sum(r => r.baseDropRate);
        if (Mathf.Abs(totalBaseRate - 1f) > 0.01f)
        {
            Debug.LogError($"Total base drop rates ({totalBaseRate}) do not sum to 1! This will cause inconsistent pull results.");

            // Normalize rates if they don't add up to 1
            if (totalBaseRate > 0)
            {
                foreach (var rarity in availableRarities)
                {
                    rarity.baseDropRate /= totalBaseRate;
                }
                Debug.Log("Drop rates have been automatically normalized.");
            }
        }

        // Validate that we have items for each rarity
        foreach (var rarity in availableRarities)
        {
            int itemCount = gachaPool.Count(item => item.rarity == rarity);
            if (itemCount == 0)
            {
                Debug.LogWarning($"No items found for rarity: {rarity.rarityName}. This rarity will never be pulled!");
            }
        }
    }

    public List<GachaItem> PerformPull(int pullCount)
    {
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
        // Check pity guarantees first
        GachaRarity guaranteedRarity = CheckPityGuarantees();
        if (guaranteedRarity != null)
        {
            if (showDebugLogs) Debug.Log($"Pity activated for {guaranteedRarity.rarityName}!");
            ResetPityCounter(guaranteedRarity);

            // Increment pity for other rarities
            IncrementPityCountersExcept(guaranteedRarity);

            return GetRandomItemFromPool(guaranteedRarity);
        }

        // Calculate adjusted drop rates based on dynamic rates
        Dictionary<GachaRarity, float> adjustedRates = CalculateAdjustedRates();

        // Normalize rates to ensure they sum to 1
        NormalizeRates(adjustedRates);

        // Select rarity based on adjusted rates
        GachaRarity selectedRarity = SelectRarityFromAdjustedRates(adjustedRates);

        // Update pity counters - reset for selected rarity, increment for others
        ResetPityCounter(selectedRarity);
        IncrementPityCountersExcept(selectedRarity);

        if (showDebugLogs) Debug.Log($"Pulled {selectedRarity.rarityName}");

        return GetRandomItemFromPool(selectedRarity);
    }

    private Dictionary<GachaRarity, float> CalculateAdjustedRates()
    {
        Dictionary<GachaRarity, float> adjustedRates = new Dictionary<GachaRarity, float>();

        foreach (var rarity in availableRarities)
        {
            float rate = rarity.baseDropRate;

            // Apply dynamic rate adjustment if enabled
            if (rarity.useDynamicRate && pityCounters.ContainsKey(rarity))
            {
                int currentPity = pityCounters[rarity];
                float dynamicBonus = rarity.dropRateCurve.Evaluate((float)currentPity / rarity.pityPullCount);

                // The dynamic rate should be a bonus on top of the base rate
                rate += dynamicBonus;

                if (showDebugLogs) Debug.Log($"{rarity.rarityName}: Base={rarity.baseDropRate:F4}, Dynamic={dynamicBonus:F4}, Total={rate:F4} (Pity: {currentPity}/{rarity.pityPullCount})");
            }
            else
            {
                if (showDebugLogs && pityCounters.ContainsKey(rarity))
                    Debug.Log($"{rarity.rarityName}: {rate:F4} (Pity: {pityCounters[rarity]})");
            }

            adjustedRates[rarity] = Mathf.Clamp01(rate); // Ensure rate is between 0 and 1
        }

        return adjustedRates;
    }

    private void NormalizeRates(Dictionary<GachaRarity, float> rates)
    {
        float sum = rates.Values.Sum();
        if (Mathf.Abs(sum - 1f) > 0.001f && sum > 0)
        {
            foreach (var rarity in rates.Keys.ToList())
            {
                rates[rarity] = rates[rarity] / sum;
            }
        }
    }

    private GachaRarity SelectRarityFromAdjustedRates(Dictionary<GachaRarity, float> adjustedRates)
    {
        float randomValue = Random.value;
        float currentProbability = 0f;

        // Sort rarities by tier to ensure consistent probability distribution
        var sortedRarities = availableRarities.OrderBy(r => r.rarityTier).ToList();

        foreach (var rarity in sortedRarities)
        {
            if (!adjustedRates.ContainsKey(rarity)) continue;

            float rate = adjustedRates[rarity];
            currentProbability += rate;

            if (randomValue <= currentProbability)
            {
                return rarity;
            }
        }

        // Fallback to highest tier rarity if we somehow miss all probabilities
        var fallbackRarity = sortedRarities.LastOrDefault();
        if (showDebugLogs) Debug.LogWarning($"Fallback to: {fallbackRarity?.rarityName ?? "null"}");
        return fallbackRarity;
    }

    private GachaRarity CheckPityGuarantees()
    {
        // Check rarities in descending order (highest tier first)
        foreach (var rarity in availableRarities.OrderByDescending(r => r.rarityTier))
        {
            if (rarity.usePitySystem && pityCounters.ContainsKey(rarity) &&
                pityCounters[rarity] >= rarity.pityPullCount)
            {
                return rarity;
            }
        }
        return null;
    }

    private void ResetPityCounter(GachaRarity rarity)
    {
        if (pityCounters.ContainsKey(rarity))
        {
            pityCounters[rarity] = 0;
        }
    }

    private void IncrementPityCountersExcept(GachaRarity excludedRarity)
    {
        foreach (var rarity in availableRarities)
        {
            if (rarity != excludedRarity && rarity.usePitySystem && pityCounters.ContainsKey(rarity))
            {
                pityCounters[rarity]++;
            }
        }
    }

    private GachaItem GetRandomItemFromPool(GachaRarity rarity)
    {
        List<GachaItem> availableItems = gachaPool.Where(item => item.rarity == rarity).ToList();

        if (availableItems.Count == 0)
        {
            Debug.LogError($"No items found for rarity: {rarity.rarityName}");
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
        ValidateRarityConfiguration();
    }
}