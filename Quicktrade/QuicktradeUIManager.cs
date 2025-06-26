using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TicTacJoe.Quicktrade
{
    public class QuicktradeUIManager : Manager
    {
        [Header("Game Status UI")]
        [SerializeField] private TextMeshProUGUI timeLeftText;
        [SerializeField] private TextMeshProUGUI profitText;
        [SerializeField] private TextMeshProUGUI holdingText;
        [SerializeField] private GameObject gameStatusPanel;

        [Header("Stock Tracker UI")]
        [SerializeField] private Transform stockTrackersParent;
        [SerializeField] private GameObject stockTrackerPrefab;
        [SerializeField] private GameObject sellInstructionsPanel;

        [Header("Game Controls")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private GameObject controlsPanel;

        [Header("Results UI")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private TextMeshProUGUI finalProfitText;
        [SerializeField] private Transform resultsStocksParent;
        [SerializeField] private GameObject resultStockPrefab;

        [Header("Instructions")]
        [SerializeField] private GameObject instructionsPanel;

        private Dictionary<string, QuicktradeStockTracker> stockTrackers;
        private QuicktradeGameManager gameManager;

        public override void Initialize()
        {
            base.Initialize();
            gameManager = ManagerHub.Instance.GetManager<QuicktradeGameManager>();
            
            SetupUI();
            SetupEventListeners();
        }

        private void SetupUI()
        {
            stockTrackers = new Dictionary<string, QuicktradeStockTracker>();
            
            // Create stock trackers
            string[] stockKeys = { "A", "B", "C", "D" };
            foreach (string stockKey in stockKeys)
            {
                GameObject trackerGO = Instantiate(stockTrackerPrefab, stockTrackersParent);
                QuicktradeStockTracker tracker = trackerGO.GetComponent<QuicktradeStockTracker>();
                tracker.Initialize(stockKey, gameManager.Stocks[stockKey]);
                stockTrackers[stockKey] = tracker;
            }

            // Setup buttons
            startButton.onClick.AddListener(() => EmitEvent(new QuicktradeStartClickedEventArgs()));
            resetButton.onClick.AddListener(() => EmitEvent(new QuicktradeResetClickedEventArgs()));

            // Initial state
            UpdateGameStateUI(GameState.Ready);
        }

        private void SetupEventListeners()
        {
            // Game state events
            AddEventListener<QuicktradeGameStartedEventArgs>(OnGameStarted);
            AddEventListener<QuicktradeGameEndedEventArgs>(OnGameEnded);
            AddEventListener<QuicktradeGameResetEventArgs>(OnGameReset);
            AddEventListener<QuicktradeTimeUpdateEventArgs>(OnTimeUpdate);

            // Trading events
            AddEventListener<QuicktradeStockPurchasedEventArgs>(OnStockPurchased);
            AddEventListener<QuicktradeStockSoldEventArgs>(OnStockSold);
            AddEventListener<QuicktradePricesUpdatedEventArgs>(OnPricesUpdated);
            AddEventListener<QuicktradeMoonEventArgs>(OnMoonEvent);

            // Input events from stock trackers
            AddEventListener<QuicktradeStockClickedEventArgs>(OnStockClicked);
            AddEventListener<QuicktradeSellClickedEventArgs>(OnSellClicked);
        }

        private void OnGameStarted(QuicktradeGameStartedEventArgs eventArgs)
        {
            UpdateGameStateUI(GameState.Playing);
            Debug.Log($"Moon stock for this game: {eventArgs.MoonStock}");
        }

        private void OnGameEnded(QuicktradeGameEndedEventArgs eventArgs)
        {
            UpdateGameStateUI(GameState.Finished);
            ShowResults(eventArgs.FinalProfit, eventArgs.FinalStocks);
        }

        private void OnGameReset(QuicktradeGameResetEventArgs eventArgs)
        {
            UpdateGameStateUI(GameState.Ready);
            HideResults();
            
            // Reset all stock trackers
            foreach (var tracker in stockTrackers.Values)
            {
                tracker.ResetTracker();
            }
        }

        private void OnTimeUpdate(QuicktradeTimeUpdateEventArgs eventArgs)
        {
            timeLeftText.text = $"{eventArgs.TimeLeft:F0}s";
        }

        private void OnStockPurchased(QuicktradeStockPurchasedEventArgs eventArgs)
        {
            holdingText.text = $"Stock {eventArgs.StockKey}";
            profitText.text = $"{(eventArgs.CurrentProfit >= 0 ? "+" : "")}{eventArgs.CurrentProfit:F2} kr";
            profitText.color = eventArgs.CurrentProfit >= 0 ? Color.green : Color.red;

            // Update stock tracker selection
            UpdateStockTrackerSelection(eventArgs.StockKey);
            
            // Show sell instructions
            sellInstructionsPanel.SetActive(true);
        }

        private void OnStockSold(QuicktradeStockSoldEventArgs eventArgs)
        {
            holdingText.text = "None";
            profitText.text = $"{(eventArgs.TotalProfit >= 0 ? "+" : "")}{eventArgs.TotalProfit:F2} kr";
            profitText.color = eventArgs.TotalProfit >= 0 ? Color.green : Color.red;

            // Clear stock tracker selection
            UpdateStockTrackerSelection(null);
            
            // Hide sell instructions
            sellInstructionsPanel.SetActive(false);
        }

        private void OnPricesUpdated(QuicktradePricesUpdatedEventArgs eventArgs)
        {
            // Update all stock trackers
            foreach (var kvp in eventArgs.Stocks)
            {
                if (stockTrackers.ContainsKey(kvp.Key))
                {
                    stockTrackers[kvp.Key].UpdateDisplay(kvp.Value);
                }
            }

            // Update current profit display
            profitText.text = $"{(eventArgs.CurrentProfit >= 0 ? "+" : "")}{eventArgs.CurrentProfit:F2} kr";
            profitText.color = eventArgs.CurrentProfit >= 0 ? Color.green : Color.red;
        }

        private void OnMoonEvent(QuicktradeMoonEventArgs eventArgs)
        {
            if (stockTrackers.ContainsKey(eventArgs.MoonStock))
            {
                stockTrackers[eventArgs.MoonStock].SetMoonState(true);
            }
        }

        private void OnStockClicked(QuicktradeStockClickedEventArgs eventArgs)
        {
            gameManager.BuyStock(eventArgs.StockKey);
        }

        private void OnSellClicked(QuicktradeSellClickedEventArgs eventArgs)
        {
            gameManager.SellStock();
        }

        private void UpdateGameStateUI(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.Ready:
                    timeLeftText.text = "Ready";
                    profitText.text = "0.00 kr";
                    profitText.color = Color.black;
                    holdingText.text = "None";
                    
                    startButton.gameObject.SetActive(true);
                    resetButton.gameObject.SetActive(false);
                    sellInstructionsPanel.SetActive(false);
                    
                    SetStockTrackersInteractable(false);
                    break;

                case GameState.Playing:
                    startButton.gameObject.SetActive(false);
                    resetButton.gameObject.SetActive(true);
                    
                    SetStockTrackersInteractable(true);
                    break;

                case GameState.Finished:
                    timeLeftText.text = "Finished";
                    startButton.gameObject.SetActive(false);
                    resetButton.gameObject.SetActive(true);
                    sellInstructionsPanel.SetActive(false);
                    
                    SetStockTrackersInteractable(false);
                    break;
            }
        }

        private void SetStockTrackersInteractable(bool interactable)
        {
            foreach (var tracker in stockTrackers.Values)
            {
                tracker.SetInteractable(interactable);
            }
        }

        private void UpdateStockTrackerSelection(string selectedStock)
        {
            foreach (var kvp in stockTrackers)
            {
                kvp.Value.SetSelected(kvp.Key == selectedStock);
            }
        }

        private void ShowResults(float finalProfit, Dictionary<string, StockData> finalStocks)
        {
            resultsPanel.SetActive(true);
            
            finalProfitText.text = $"{(finalProfit >= 0 ? "+" : "")}{finalProfit:F2} kr";
            finalProfitText.color = finalProfit >= 0 ? Color.green : Color.red;

            // Clear existing result items
            foreach (Transform child in resultsStocksParent)
            {
                Destroy(child.gameObject);
            }

            // Create result items for each stock
            foreach (var kvp in finalStocks)
            {
                GameObject resultGO = Instantiate(resultStockPrefab, resultsStocksParent);
                QuicktradeResultItem resultItem = resultGO.GetComponent<QuicktradeResultItem>();
                
                float change = kvp.Value.CurrentPrice - 7f; // Starting price was 7
                resultItem.Setup(kvp.Key, kvp.Value.CurrentPrice, change);
            }
        }

        private void HideResults()
        {
            resultsPanel.SetActive(false);
        }

        protected override void OnDestroy()
        {
            if (startButton != null) startButton.onClick.RemoveAllListeners();
            if (resetButton != null) resetButton.onClick.RemoveAllListeners();
            
            base.OnDestroy();
        }
    }
}