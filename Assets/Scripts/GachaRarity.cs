using UnityEngine;

[CreateAssetMenu(fileName = "Rarity", menuName = "Gacha System/Rarity")]
public class GachaRarity : ScriptableObject
{
    public string rarityName;
    public float baseDropRate; //0.0 to 1.0
    private int pityCount;
    public bool usePitySystem;
    public int pityThreshold; 
    public bool useDropRateCurve; 
    public AnimationCurve dropRateCurve; // use curve if bool is on
}
