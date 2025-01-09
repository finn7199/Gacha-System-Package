using UnityEngine;

[CreateAssetMenu(fileName = "GachaItem", menuName = "Gacha System/GachaItem")]
public class GachaItem : ScriptableObject
{
    public string itemName;
    public GachaSystem.Rarity rarity;  
    public Sprite itemImage;  
}
