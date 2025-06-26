using UnityEngine;
using TMPro;

namespace TicTacJoe.Quicktrade
{
    public class QuicktradeResultItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI stockNameText;
        [SerializeField] private TextMeshProUGUI finalPriceText;
        [SerializeField] private TextMeshProUGUI changeText;

        [Header("Colors")]
        [SerializeField] private Color positiveColor = Color.green;
        [SerializeField] private Color negativeColor = Color.red;

        public void Setup(string stockKey, float finalPrice, float change)
        {
            stockNameText.text = $"Stock {stockKey}";
            finalPriceText.text = $"{finalPrice:F2} kr";
            
            changeText.text = $"{(change >= 0 ? "+" : "")}{change:F2} kr";
            changeText.color = change >= 0 ? positiveColor : negativeColor;
        }
    }
}