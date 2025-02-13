﻿using System.Collections.Generic;
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

    private int pitySRCounter = 0;  // Counter for SR pity
    private int pitySSRCounter = 0; // Counter for SSR pity

    private int gachaTokens;

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
    }

    private Rarity DetermineRarity(float randomValue)
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

    private GachaItem GetRandomItemFromPool(Rarity rarity)
    {
        List<GachaItem> availableItems = new List<GachaItem>();

        foreach (var item in gachaPool)
        {
            if (item.rarity == rarity)
            {
                availableItems.Add(item);
            }
        }

        int randomIndex = Random.Range(0, availableItems.Count);
        return availableItems[randomIndex];
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
