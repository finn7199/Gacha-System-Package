using UnityEngine;
using System.Collections.Generic;

public class GachaSystem : MonoBehaviour
{
    public enum Rarity
    {
        R,   // Common
        SR,  // Rare
        SSR  // Super Rare
    }

    [Header("Gacha Settings")]
    public float baseDropRateR = 0.9f;  // Base drop rate for Rarity "R"
    public float baseDropRateSR = 0.09f; // Base drop rate for Rarity "SR"
    public float baseDropRateSSR = 0.01f; // Base drop rate for Rarity "SSR"

    [Header("Pity System")]
    public int pitySRThreshold = 10;   // Every 10th pull guarantees SR
    public int pitySSRThreshold = 100; // Every 100th pull guarantees SSR

    [Header("SSR Drop Rate Curve")]
    [Tooltip("The curve defines how SSR drop rate increases with pity.")]
    public AnimationCurve ssrDropRateCurve;  // Curve for SSR drop rate based on pity

    [Header("Items")]
    public List<GachaItem> gachaItems; // List of GachaItems to be used in the gacha pull (ScriptableObjects)

    public int pitySRCounter = 0;  // Counter for SR pity
    public int pitySSRCounter = 0; // Counter for SSR pity

    public List<GachaItem> PerformPull(int pullCount)
    {
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
        if (pitySSRCounter >= pitySSRThreshold)
        {
            rarity = Rarity.SSR; 
            pitySSRCounter = 0;  
        }
        else if (pitySRCounter >= pitySRThreshold)
        {
            rarity = Rarity.SR; 
            pitySRCounter = 0; 
        }

        // Get item based on determined rarity
        return GetRandomItemByRarity(rarity);
    }

    private Rarity DetermineRarity(float randomValue)
    {
        // Adjust SSR drop rate using the curve
        float adjustedSSRRate = baseDropRateSSR + ssrDropRateCurve.Evaluate(pitySSRCounter);

        Debug.LogError("SSR Rate: "+adjustedSSRRate);
        Debug.LogError("Current Rate Value:" +randomValue);

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

    private GachaItem GetRandomItemByRarity(Rarity rarity)
    {
        List<GachaItem> availableItems = new List<GachaItem>();

        foreach (var item in gachaItems)
        {
            if (item.rarity == rarity)
            {
                availableItems.Add(item);
            }
        }

        int randomIndex = Random.Range(0, availableItems.Count);
        return availableItems[randomIndex];
    }
}
