using UnityEngine;

[CreateAssetMenu(fileName = "TradeableAsset", menuName = "Scriptable Objects/TradeableAsset")]
public class TradeableAsset : ScriptableObject
{
    public int volatility = 1;
    public int initialPrice = 7;

    [SerializeField]
    private int currentprice;
    
}
