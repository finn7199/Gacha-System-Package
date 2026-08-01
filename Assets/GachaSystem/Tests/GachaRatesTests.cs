using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class GachaRatesTests
{
    private readonly List<Object> _createdAssets = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        foreach (var asset in _createdAssets)
        {
            Object.DestroyImmediate(asset);
        }
        _createdAssets.Clear();
    }

    private GachaRarity CreateRarity(string name, int tier, float baseRate)
    {
        var rarity = ScriptableObject.CreateInstance<GachaRarity>();
        rarity.rarityName = name;
        rarity.rarityTier = tier;
        rarity.baseDropRate = baseRate;
        _createdAssets.Add(rarity);
        return rarity;
    }

    [Test]
    public void ComputeMarginals_MatchesCanonicalDistribution()
    {
        // Same configuration as GachaSystemTests.Pull_DistributionMatchesConfiguredRates_WithinTolerance -
        // this asserts the closed-form math agrees with that test's empirical sampling.
        var r = CreateRarity("R", tier: 1, baseRate: 0.9f);
        var sr = CreateRarity("SR", tier: 2, baseRate: 0.09f);
        var ssr = CreateRarity("SSR", tier: 3, baseRate: 0.01f);

        var marginals = GachaRates.ComputeMarginals(new List<GachaRarity> { r, sr, ssr });

        var ssrRate = marginals.Single(m => m.rarity == ssr);
        var srRate = marginals.Single(m => m.rarity == sr);
        var rRate = marginals.Single(m => m.rarity == r);

        Assert.AreEqual(0.01f, ssrRate.marginalChance, 0.0001f);
        Assert.AreEqual(0.0891f, srRate.marginalChance, 0.0001f);
        Assert.AreEqual(0.9009f, rRate.marginalChance, 0.0001f);

        Assert.IsFalse(ssrRate.isLeftoverTier);
        Assert.IsFalse(srRate.isLeftoverTier);
        Assert.IsTrue(rRate.isLeftoverTier, "R has the lowest Tier, so it should be flagged as the leftover tier");
    }

    [Test]
    public void ComputeMarginals_LeftoverTierIgnoresItsOwnConfiguredRate()
    {
        var common = CreateRarity("Common", tier: 1, baseRate: 0.5f);
        var rare = CreateRarity("Rare", tier: 2, baseRate: 0.2f);

        var marginals = GachaRates.ComputeMarginals(new List<GachaRarity> { common, rare });
        var commonRate = marginals.Single(m => m.rarity == common);

        Assert.IsTrue(commonRate.isLeftoverTier);
        Assert.AreEqual(0.5f, commonRate.configuredRate, "configuredRate should still report the raw authored value");
        Assert.AreEqual(0.8f, commonRate.marginalChance, 0.0001f, "marginalChance should be 1 - Rare's rate, not Common's own configured rate");
    }

    [Test]
    public void FindDuplicateTiers_ReturnsGroupsWithMoreThanOneRarity()
    {
        var a = CreateRarity("A", tier: 1, baseRate: 0.5f);
        var b = CreateRarity("B", tier: 1, baseRate: 0.3f);
        var c = CreateRarity("C", tier: 2, baseRate: 0.1f);

        var duplicates = GachaRates.FindDuplicateTiers(new List<GachaRarity> { a, b, c });

        Assert.AreEqual(1, duplicates.Count);
        Assert.AreEqual(1, duplicates[0].Key);
        CollectionAssert.AreEquivalent(new[] { a, b }, duplicates[0].ToList());
    }

    [Test]
    public void SuggestSequentialTiers_AssignsTierByListPosition_TopIsMostCommon()
    {
        var a = CreateRarity("A", tier: 5, baseRate: 0.5f);
        var b = CreateRarity("B", tier: 5, baseRate: 0.3f);
        var c = CreateRarity("C", tier: 1, baseRate: 0.1f);

        var suggested = GachaRates.SuggestSequentialTiers(new List<GachaRarity> { a, b, c });

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, suggested);
    }
}
