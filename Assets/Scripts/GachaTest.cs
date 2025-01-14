using System.Collections.Generic;
using UnityEngine;

public class GachaTest : MonoBehaviour
{
    public void PerformTestPull(int noOfPulls)
    {
        List<GachaItem> pulledItems = GachaSystem.Instance.PerformPull(noOfPulls);

        foreach (var item in pulledItems)
        {
            Debug.Log("Pulled Item: " + item.itemName + " - Rarity: " + item.rarity);
        }
    }
}
