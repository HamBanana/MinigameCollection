using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Ham.GameControl;

namespace TicTacJoe.Quicktrade
{
    public enum GameState
    {
        Ready,
        Playing,
        Finished
    }

    public class QuicktradeGameManager : Manager
    {
        [Header("Game Settings")]
        [SerializeField] private float gameDuration = 10f;
        [SerializeField] private float priceUpdateInterval = 0.0167f; // ~60fps
        [SerializeField] private int moonTriggerFrame = 180; // 3 seconds at 60fps

        private GameState currentGameState = GameState.Ready;
        private float timeLeft;
        private int updateCount;
        private string moonStock;
        private bool moonTriggered;
        private string currentHoldingStock;
        private float purchasePrice;
        private float totalProfit;

        // Stock data
        private Dictionary<string, StockData> stocks;

        // Coroutines
        private Coroutine gameTimerCoroutine;
        private Coroutine priceUpdateCoroutine;

        public override void Initialize()
        {
            base.Initialize();
            InitializeStocks();
        }

        private void InitializeStocks()
        {
            stocks = new Dictionary<string, StockData>
            {
                ["A"] = new StockData("Stock A", 7f, 0.1f, 10f),
                ["B"] = new StockData("Stock B", 7f, 0.5f, 30f),
                ["C"] = new StockData("Stock C", 7f, 0.3f, 70f),
                ["D"] = new StockData("Stock D", 7f, 1f, 5f)
            };
        }

        public void StartGame()
        {
            if (currentGameState != GameState.Ready) return;

            currentGameState = GameState.Playing;
            timeLeft = gameDuration;
            updateCount = 0;
            moonTriggered = false;
            currentHoldingStock = null;
            purchasePrice = 0f;
            totalProfit = 0f;

            // Select random moon stock
            string[] stockKeys = new string[] { "A", "B", "C", "D" };
            moonStock = stockKeys[UnityEngine.Random.Range(0, stockKeys.Length)];

            // Reset all stock data
            foreach (var stock in stocks.Values)
            {
                stock.Reset();
            }

            // Start coroutines
            gameTimerCoroutine = StartCoroutine(GameTimer());
            priceUpdateCoroutine = StartCoroutine(PriceUpdateLoop());

            // Emit game started event
            EmitEvent(new QuicktradeGameStartedEventArgs(moonStock));
        }

        public void BuyStock(string stockKey)
        {
            if (currentGameState != GameState.Playing || !stocks.ContainsKey(stockKey)) return;

            // Sell current stock if holding any
            if (!string.IsNullOrEmpty(currentHoldingStock))
            {
                float sellPrice = stocks[currentHoldingStock].CurrentPrice;
                totalProfit += sellPrice - purchasePrice;
            }

            // Buy new stock
            currentHoldingStock = stockKey;
            purchasePrice = stocks[stockKey].CurrentPrice;

            EmitEvent(new QuicktradeStockPurchasedEventArgs(stockKey, purchasePrice, GetCurrentProfit()));
        }

        public void SellStock()
        {
            if (currentGameState != GameState.Playing || string.IsNullOrEmpty(currentHoldingStock)) return;

            float sellPrice = stocks[currentHoldingStock].CurrentPrice;
            totalProfit += sellPrice - purchasePrice;

            string soldStock = currentHoldingStock;
            currentHoldingStock = null;
            purchasePrice = 0f;

            EmitEvent(new QuicktradeStockSoldEventArgs(soldStock, sellPrice, totalProfit));
        }

        public void ResetGame()
        {
            StopAllCoroutines();

            currentGameState = GameState.Ready;
            timeLeft = gameDuration;
            updateCount = 0;
            moonTriggered = false;
            moonStock = null;
            currentHoldingStock = null;
            purchasePrice = 0f;
            totalProfit = 0f;

            InitializeStocks();
            EmitEvent(new QuicktradeGameResetEventArgs());
        }

        private void EndGame()
        {
            if (gameTimerCoroutine != null) StopCoroutine(gameTimerCoroutine);
            if (priceUpdateCoroutine != null) StopCoroutine(priceUpdateCoroutine);

            // Calculate final profit if holding stock
            if (!string.IsNullOrEmpty(currentHoldingStock))
            {
                float finalPrice = stocks[currentHoldingStock].CurrentPrice;
                totalProfit += finalPrice - purchasePrice;
            }

            currentGameState = GameState.Finished;
            EmitEvent(new QuicktradeGameEndedEventArgs(totalProfit, stocks));
        }

        private IEnumerator GameTimer()
        {
            while (timeLeft > 0)
            {
                yield return new WaitForSeconds(1f);
                timeLeft -= 1f;
                EmitEvent(new QuicktradeTimeUpdateEventArgs(timeLeft));
            }
            EndGame();
        }

        private IEnumerator PriceUpdateLoop()
        {
            while (currentGameState == GameState.Playing)
            {
                yield return new WaitForSeconds(priceUpdateInterval);
                UpdateStockPrices();
                updateCount++;
            }
        }

        private void UpdateStockPrices()
        {
            // Trigger moon event at specific frame
            if (!moonTriggered && updateCount >= moonTriggerFrame)
            {
                moonTriggered = true;
                EmitEvent(new QuicktradeMoonEventArgs(moonStock));
            }

            foreach (var kvp in stocks)
            {
                string stockKey = kvp.Key;
                StockData stock = kvp.Value;

                float priceChange;

                if (stockKey == moonStock && moonTriggered)
                {
                    // Moon stock behavior - massive continuous growth
                    priceChange = UnityEngine.Random.Range(0.5f, 1.3f);
                }
                else
                {
                    // Normal price behavior
                    float baseChange = (UnityEngine.Random.value - 0.5f) * stock.Volatility * 0.3f;
                    float shortEffect = 0f;

                    if (UnityEngine.Random.value < 0.01f) // 1% chance per frame
                    {
                        if (UnityEngine.Random.value < stock.ShortInterest / 100f)
                        {
                            shortEffect = -UnityEngine.Random.value * 0.2f;
                        }
                        else if (stock.ShortInterest > 50f && UnityEngine.Random.value < 0.3f)
                        {
                            shortEffect = UnityEngine.Random.value * 0.5f;
                        }
                    }

                    priceChange = baseChange + shortEffect;
                }

                stock.UpdatePrice(priceChange);
            }

            EmitEvent(new QuicktradePricesUpdatedEventArgs(stocks, GetCurrentProfit()));
        }

        public float GetCurrentProfit()
        {
            if (string.IsNullOrEmpty(currentHoldingStock))
                return totalProfit;

            return totalProfit + (stocks[currentHoldingStock].CurrentPrice - purchasePrice);
        }

        // Public getters for UI
        public GameState CurrentGameState => currentGameState;
        public float TimeLeft => timeLeft;
        public string CurrentHoldingStock => currentHoldingStock;
        public float PurchasePrice => purchasePrice;
        public Dictionary<string, StockData> Stocks => stocks;
        public string MoonStock => moonStock;
        public bool IsMoonTriggered => moonTriggered;
    }

    [System.Serializable]
    public class StockData
    {
        public string Name { get; private set; }
        public float CurrentPrice { get; private set; }
        public float Volatility { get; private set; }
        public float ShortInterest { get; private set; }
        public List<float> PriceHistory { get; private set; }

        private const float STARTING_PRICE = 7f;

        public StockData(string name, float startingPrice, float volatility, float shortInterest)
        {
            Name = name;
            CurrentPrice = startingPrice;
            Volatility = volatility;
            ShortInterest = shortInterest;
            PriceHistory = new List<float> { startingPrice };
        }

        public void UpdatePrice(float change)
        {
            CurrentPrice = Mathf.Max(0f, CurrentPrice + change);
            CurrentPrice = Mathf.Round(CurrentPrice * 100f) / 100f; // Round to 2 decimal places

            PriceHistory.Add(CurrentPrice);

            // Keep history manageable
            if (PriceHistory.Count > 200)
            {
                PriceHistory.RemoveAt(0);
            }
        }

        public void Reset()
        {
            CurrentPrice = STARTING_PRICE;
            PriceHistory.Clear();
            PriceHistory.Add(STARTING_PRICE);
        }

        public float GetPriceChange()
        {
            if (PriceHistory.Count < 2) return 0f;
            return CurrentPrice - PriceHistory[PriceHistory.Count - 2];
        }
    }
}