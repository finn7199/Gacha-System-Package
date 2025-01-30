using UnityEngine;

[CreateAssetMenu(fileName = "Gacha Item", menuName = "Gacha System/GachaItem")]
public class GachaItem : ScriptableObject
{
    public string itemName;
    public GachaRarity rarity;  
    public Sprite itemImage;  
}
