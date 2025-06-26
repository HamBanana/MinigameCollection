using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace TicTacJoe.Quicktrade
{
    public class QuicktradeStockTracker : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI stockNameText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI volatilityText;
        [SerializeField] private TextMeshProUGUI shortInterestText;
        [SerializeField] private TextMeshProUGUI purchasePriceText;
        [SerializeField] private Button stockButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject moonIcon;
        [SerializeField] private LineRenderer priceGraph;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.cyan;
        [SerializeField] private Color moonColor = Color.yellow;
        [SerializeField] private Color positiveColor = Color.green;
        [SerializeField] private Color negativeColor = Color.red;
        [SerializeField] private Color neutralColor = Color.black;

        [Header("Graph Settings")]
        [SerializeField] private int maxGraphPoints = 200;
        [SerializeField] private float graphWidth = 2f;
        [SerializeField] private float graphHeight = 1f;

        private string stockKey;
        private StockData stockData;
        private bool isSelected;
        private bool isMooning;
        private bool isInteractable;
        private List<Vector3> graphPoints;

        public void Initialize(string key, StockData data)
        {
            stockKey = key;
            stockData = data;
            isSelected = false;
            isMooning = false;
            isInteractable = false;
            graphPoints = new List<Vector3>();

            SetupUI();
            SetupGraph();
            UpdateDisplay(stockData);
        }

        private void SetupUI()
        {
            stockNameText.text = $"Stock {stockKey}";
            volatilityText.text = $"Volatility: {(stockData.Volatility * 100):F0}%";
            shortInterestText.text = $"Short Interest: {stockData.ShortInterest:F0}%";
            
            stockButton.onClick.AddListener(() => {
                if (isInteractable)
                {
                    ManagerHub.Instance.EmitEvent(new QuicktradeStockClickedEventArgs(stockKey));
                }
            });

            moonIcon.SetActive(false);
            purchasePriceText.gameObject.SetActive(false);
            
            UpdateBackgroundColor();
        }

        private void SetupGraph()
        {
            if (priceGraph == null) return;

            priceGraph.positionCount = 0;
            priceGraph.startWidth = 0.02f;
            priceGraph.endWidth = 0.02f;
            priceGraph.material = new Material(Shader.Find("Sprites/Default"));
            priceGraph.color = Color.blue;
        }

        public void UpdateDisplay(StockData data)
        {
            stockData = data;
            
            // Update price text with color based on recent change
            priceText.text = $"{data.CurrentPrice:F2} kr";
            
            float priceChange = data.GetPriceChange();
            if (priceChange > 0)
                priceText.color = positiveColor;
            else if (priceChange < 0)
                priceText.color = negativeColor;
            else
                priceText.color = neutralColor;

            UpdateGraph();
        }

        private void UpdateGraph()
        {
            if (priceGraph == null || stockData.PriceHistory.Count < 2) return;

            // Update graph points
            graphPoints.Clear();
            
            float minPrice = float.MaxValue;
            float maxPrice = float.MinValue;
            
            // Find min and max for scaling
            foreach (float price in stockData.PriceHistory)
            {
                minPrice = Mathf.Min(minPrice, price);
                maxPrice = Mathf.Max(maxPrice, price);
            }
            
            float priceRange = maxPrice - minPrice;
            if (priceRange == 0) priceRange = 1f; // Avoid division by zero

            // Create graph points
            for (int i = 0; i < stockData.PriceHistory.Count; i++)
            {
                float x = (i / (float)(stockData.PriceHistory.Count - 1)) * graphWidth - graphWidth * 0.5f;
                float y = ((stockData.PriceHistory[i] - minPrice) / priceRange) * graphHeight - graphHeight * 0.5f;
                
                graphPoints.Add(new Vector3(x, y, 0));
            }

            // Update LineRenderer
            priceGraph.positionCount = graphPoints.Count;
            priceGraph.SetPositions(graphPoints.ToArray());
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateBackgroundColor();
            
            if (selected && stockData != null)
            {
                purchasePriceText.text = $"Bought at: {stockData.CurrentPrice:F2} kr";
                purchasePriceText.gameObject.SetActive(true);
            }
            else
            {
                purchasePriceText.gameObject.SetActive(false);
            }
        }

        public void SetMoonState(bool mooning)
        {
            isMooning = mooning;
            moonIcon.SetActive(mooning);
            UpdateBackgroundColor();
            
            if (mooning)
            {
                stockNameText.text = $"Stock {stockKey} 🚀";
            }
            else
            {
                stockNameText.text = $"Stock {stockKey}";
            }
        }

        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            stockButton.interactable = interactable;
            
            // Visual feedback for interactability
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            canvasGroup.alpha = interactable ? 1f : 0.6f;
        }

        public void ResetTracker()
        {
            isSelected = false;
            isMooning = false;
            SetSelected(false);
            SetMoonState(false);
            
            if (priceGraph != null)
                priceGraph.positionCount = 0;
            
            graphPoints.Clear();
        }

        private void UpdateBackgroundColor()
        {
            if (backgroundImage == null) return;

            Color targetColor = normalColor;
            
            if (isMooning)
                targetColor = moonColor;
            else if (isSelected)
                targetColor = selectedColor;
            
            backgroundImage.color = targetColor;
        }

        private void OnDestroy()
        {
            if (stockButton != null)
                stockButton.onClick.RemoveAllListeners();
        }
    }
}