using System.Collections.Generic;
using UnityEngine;

public class GachaTest : MonoBehaviour
{
    public GachaSystem gachaSystem;

    public void PerformTestPull(int noOfPulls)
    {
        List<GachaItem> pulledItems = gachaSystem.PerformPull(noOfPulls);

        foreach (var item in pulledItems)
        {
            Debug.Log("Pulled Item: " + item.itemName + " - Rarity: " + item.rarity);
        }
    }
}
