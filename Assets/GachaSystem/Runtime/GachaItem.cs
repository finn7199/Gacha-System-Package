using UnityEngine;

[CreateAssetMenu(fileName = "GachaItem", menuName = "Gacha System/GachaItem")]
public class GachaItem : ScriptableObject
{
    public string itemName;

    [Tooltip("Reference to the rarity ScriptableObject")]
    public GachaRarity rarity;
    public Sprite itemImage;
}