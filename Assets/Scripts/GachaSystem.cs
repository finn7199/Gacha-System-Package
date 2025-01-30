using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    public static GachaSystem Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public enum Rarity //Rarities enum
    {
        R,   // Rare
        SR,  // Super Rare
        SSR  // Super+ Rare
    }

    [Header("Base Gacha Drop Rates")]
    [Tooltip("レアリティ「R」のドロップ率")]
    public float baseDropRateR = 0.9f;  // Base drop rate for Rarity "R"
    [Tooltip("レアリティ「SR」のドロップ率")]
    public float baseDropRateSR = 0.09f; // Base drop rate for Rarity "SR"
    [Tooltip("レアリティ「SSR」のドロップ率")]
    public float baseDropRateSSR = 0.01f; // Base drop rate for Rarity "SSR"

    [Header("Pity System")]
    [Tooltip("SR保証に必要な最大ガチャ数")]
    public int pitySR = 10;   // Every 10th pull guarantees SR
    [Tooltip("SSR保証に必要な最大ガチャ数")]
    public int pitySSR = 100; // Every 100th pull guarantees SSR

    [Header("SSR Drop Rate Curve")]
    [Tooltip("The curve defines how SSR drop rate increases with pity (この曲線は、ガチャをすることでSSRのドロップ率がどのように上昇するかを定義している。)")]
    public AnimationCurve ssrDropRateCurve;  // Curve for SSR drop rate based on pity

    [Header("Gacha Pool")]
    [Tooltip("GachaItemのリスト")]
    public List<GachaItem> gachaPool; // List of GachaItems to be used in the gacha pull (ScriptableObjects)

    [Header("Gacha Rarities")]
    public List<GachaRarity> raritiesList;
    private Dictionary<string, GachaRarity> raritiesDict; // Dictionary for easy lookup

    private int pitySRCounter = 0;  // Counter for SR pity
    private int pitySSRCounter = 0; // Counter for SSR pity
    private Dictionary<string, int> pityCounters = new Dictionary<string, int>(); // Tracks pity for each rarity

    public int gachaTokens = 200;

    private void Start()
    {
        // Convert rarity list into dictionary for quick lookup
        raritiesDict = new Dictionary<string, GachaRarity>();
        foreach (var rarity in raritiesList)
        {
            raritiesDict[rarity.rarityName] = rarity;
            pityCounters[rarity.rarityName] = 0; // Initialize pity counters
        }
    }
    public List<GachaItem> PerformPull(int pullCount)
    {
        if (gachaTokens < pullCount)
        {
            //Implement not enough tokens functionality here. Redirect to buy more tokens.
            Debug.Log("Not enough tokens!");
            return null;
        }

        gachaTokens -= pullCount;
        Debug.Log("Remaining tokens: "+gachaTokens);

        List<GachaItem> pulledItems = new List<GachaItem>();

        for (int i = 0; i < pullCount; i++)
        {
            GachaItem pulledItem = Pull();
            pulledItems.Add(pulledItem);
        }

        return pulledItems;
    }

    public GachaItem Pull()
    {
        float randomValue = Random.value; // Random value for drop chance
        GachaRarity selectedRarity = DetermineRarity(randomValue);

        // Reset pity counter if we get the selected rarity
        pityCounters[selectedRarity.rarityName] = 0;

        return GetRandomItemFromPool(selectedRarity);
        /*
        Rarity rarity = DetermineRarity(randomValue);

        // Handle pity counters
        if (rarity == Rarity.SR)
        {
            pitySRCounter = 0;
        }
        else
        {
            pitySRCounter++;
        }

        if (rarity == Rarity.SSR)
        {
            pitySSRCounter = 0;
        }
        else
        {
            pitySSRCounter++;
        }

        // Apply pity guarantees
        if (pitySSRCounter >= pitySSR)
        {
            rarity = Rarity.SSR;
            pitySSRCounter = 0;
        }
        else if (pitySRCounter >= pitySR)
        {
            rarity = Rarity.SR;
            pitySRCounter = 0;
        }

        // Get item based on determined rarity
        return GetRandomItemFromPool(rarity);
        */
    }

    private GachaRarity DetermineRarity(float randomValue)
    {
        foreach (var rarity in raritiesList)
        {
            float adjustedRate = rarity.baseDropRate;

            // Apply pity curve if enabled
            if (rarity.useDropRateCurve)
            {
                adjustedRate += rarity.dropRateCurve.Evaluate(pityCounters[rarity.rarityName]);
            }

            if (randomValue < adjustedRate || pityCounters[rarity.rarityName] >= rarity.pityThreshold)
            {
                pityCounters[rarity.rarityName] = 0; // Reset pity if we get it
                return rarity;
            }

            // If rarity not hit, increase pity
            pityCounters[rarity.rarityName]++;
        }

        return raritiesList[0]; // Default to first rarity in case of issue
    }

    private Rarity DetermineRarityR(float randomValue)
    {
        // Adjust SSR drop rate using the curve
        float adjustedSSRRate = baseDropRateSSR + ssrDropRateCurve.Evaluate(pitySSRCounter);

        if (randomValue < adjustedSSRRate)
        {
            return Rarity.SSR;
        }
        else if (randomValue < adjustedSSRRate + baseDropRateSR)
        {
            return Rarity.SR;
        }
        else
        {
            return Rarity.R;
        }
    }

    private GachaItem GetRandomItemFromPool(GachaRarity rarity)
    {
        List<GachaItem> availableItems = new List<GachaItem>();

        foreach (var item in gachaPool)
        {
            if (item.rarity == rarity)
            {
                availableItems.Add(item);
            }
        }
        return availableItems.Count > 0 ? availableItems[Random.Range(0, availableItems.Count)] : null;

        //int randomIndex = Random.Range(0, availableItems.Count);
        //return availableItems[randomIndex];
    }
    public void AddGachaTokens(int addAmount)
    {
        gachaTokens += addAmount;
    }

    public void SetGachaTokens(int setAmount)
    {
        gachaTokens = setAmount;
    }
}
