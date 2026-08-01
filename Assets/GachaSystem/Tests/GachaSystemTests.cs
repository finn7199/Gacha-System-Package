using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GachaSystemTests
{
    private GameObject _go;
    private GachaSystem _system;
    private readonly List<Object> _createdAssets = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("GachaSystemUnderTest");
        _system = _go.AddComponent<GachaSystem>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_go);
        foreach (var asset in _createdAssets)
        {
            Object.DestroyImmediate(asset);
        }
        _createdAssets.Clear();
    }

    private GachaRarity CreateRarity(string name, int tier, float baseRate, bool usePity = false, int pityCount = 10, bool useDynamic = false, AnimationCurve curve = null)
    {
        var rarity = ScriptableObject.CreateInstance<GachaRarity>();
        rarity.rarityName = name;
        rarity.rarityTier = tier;
        rarity.baseDropRate = baseRate;
        rarity.usePitySystem = usePity;
        rarity.pityPullCount = pityCount;
        rarity.useDynamicRate = useDynamic;
        rarity.dropRateCurve = curve;
        _createdAssets.Add(rarity);
        return rarity;
    }

    private GachaItem CreateItem(string name, GachaRarity rarity)
    {
        var item = ScriptableObject.CreateInstance<GachaItem>();
        item.itemName = name;
        item.rarity = rarity;
        _createdAssets.Add(item);
        return item;
    }

    [Test]
    public void Pull_WithSingleRarity_AlwaysReturnsThatRarity()
    {
        var common = CreateRarity("Common", tier: 1, baseRate: 1f);
        var item = CreateItem("Rock", common);

        _system.availableRarities = new List<GachaRarity> { common };
        _system.gachaPool = new List<GachaItem> { item };

        for (int i = 0; i < 20; i++)
        {
            Assert.AreSame(item, _system.Pull());
        }
    }

    [Test]
    public void Pull_DistributionMatchesConfiguredRates_WithinTolerance()
    {
        var r = CreateRarity("R", tier: 1, baseRate: 0.9f);
        var sr = CreateRarity("SR", tier: 2, baseRate: 0.09f);
        var ssr = CreateRarity("SSR", tier: 3, baseRate: 0.01f);

        _system.availableRarities = new List<GachaRarity> { r, sr, ssr };
        _system.gachaPool = new List<GachaItem>
        {
            CreateItem("r-item", r),
            CreateItem("sr-item", sr),
            CreateItem("ssr-item", ssr),
        };

        Random.InitState(12345);

        const int sampleSize = 20000;
        int rCount = 0, srCount = 0, ssrCount = 0;

        for (int i = 0; i < sampleSize; i++)
        {
            var pulled = _system.Pull();
            if (pulled.rarity == r) rCount++;
            else if (pulled.rarity == sr) srCount++;
            else if (pulled.rarity == ssr) ssrCount++;
        }

        // Expected marginal probabilities under the rarest-first independent-roll model:
        // P(SSR) = 0.01, P(SR) = (1-0.01)*0.09, P(R) = (1-0.01)*(1-0.09)
        Assert.AreEqual(0.01f, ssrCount / (float)sampleSize, 0.01f);
        Assert.AreEqual(0.0891f, srCount / (float)sampleSize, 0.02f);
        Assert.AreEqual(0.9009f, rCount / (float)sampleSize, 0.02f);
    }

    [Test]
    public void PityGuarantee_TriggersOnTheExactConfiguredPull()
    {
        var common = CreateRarity("Common", tier: 1, baseRate: 1f);
        var rare = CreateRarity("Rare", tier: 2, baseRate: 0f, usePity: true, pityCount: 5);

        _system.availableRarities = new List<GachaRarity> { common, rare };
        _system.gachaPool = new List<GachaItem>
        {
            CreateItem("common-item", common),
            CreateItem("rare-item", rare),
        };

        for (int i = 0; i < 4; i++)
        {
            Assert.AreEqual(common, _system.Pull().rarity, $"Pull {i + 1} should not be Rare before pity is reached");
        }

        Assert.AreEqual(rare, _system.Pull().rarity, "5th pull should be pity-guaranteed Rare");
    }

    [Test]
    public void ObtainingHigherTier_ResetsPityOfLowerTiers()
    {
        var common = CreateRarity("Common", tier: 1, baseRate: 1f);
        var mid = CreateRarity("Mid", tier: 2, baseRate: 0f, usePity: true, pityCount: 3);
        var high = CreateRarity("High", tier: 3, baseRate: 0f, usePity: true, pityCount: 3);

        _system.availableRarities = new List<GachaRarity> { common, mid, high };
        _system.gachaPool = new List<GachaItem>
        {
            CreateItem("common-item", common),
            CreateItem("mid-item", mid),
            CreateItem("high-item", high),
        };

        GachaItem last = null;
        for (int i = 0; i < 3; i++)
        {
            last = _system.Pull();
        }

        Assert.AreEqual(high, last.rarity, "Mid and High pity are due on the same pull; the rarer tier should win");

        var status = _system.GetPityStatus();
        Assert.AreEqual(0, status["High"], "High's own pity should reset");
        Assert.AreEqual(0, status["Mid"], "Getting a High should also satisfy Mid's pity, since High counts as Mid-or-better");
    }

    [Test]
    public void PerformPull_NonPositiveCount_ReturnsEmptyAndLeavesTokensUnchanged()
    {
        var common = CreateRarity("Common", tier: 1, baseRate: 1f);
        _system.availableRarities = new List<GachaRarity> { common };
        _system.gachaPool = new List<GachaItem> { CreateItem("item", common) };
        _system.gachaTokens = 200;

        Assert.IsEmpty(_system.PerformPull(0));
        Assert.IsEmpty(_system.PerformPull(-5));
        Assert.AreEqual(200, _system.gachaTokens, "A non-positive pull count must never change the token balance");
    }

    [Test]
    public void PerformPull_InsufficientTokens_ReturnsEmptyAndLeavesTokensUnchanged()
    {
        var common = CreateRarity("Common", tier: 1, baseRate: 1f);
        _system.availableRarities = new List<GachaRarity> { common };
        _system.gachaPool = new List<GachaItem> { CreateItem("item", common) };
        _system.gachaTokens = 3;

        Assert.IsEmpty(_system.PerformPull(5));
        Assert.AreEqual(3, _system.gachaTokens);
    }

    [Test]
    public void AddAndSetGachaTokens_UpdateBalanceCorrectly()
    {
        _system.gachaTokens = 10;
        _system.AddGachaTokens(5);
        Assert.AreEqual(15, _system.gachaTokens);

        _system.SetGachaTokens(42);
        Assert.AreEqual(42, _system.gachaTokens);
    }

    [Test]
    public void Pull_WithNoRaritiesConfigured_ReturnsNullWithoutThrowing()
    {
        _system.availableRarities = new List<GachaRarity>();
        _system.gachaPool = new List<GachaItem>();

        LogAssert.Expect(LogType.Error, "[GachaSystem] Cannot pull: no rarities configured.");

        GachaItem result = null;
        Assert.DoesNotThrow(() => result = _system.Pull());
        Assert.IsNull(result);
    }
}
