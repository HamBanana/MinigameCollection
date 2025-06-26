using System;
using System.Collections.Generic;

namespace TicTacJoe.Quicktrade
{
    // Game state events
    public class QuicktradeGameStartedEventArgs : EventArgs
    {
        public string MoonStock { get; }
        
        public QuicktradeGameStartedEventArgs(string moonStock)
        {
            MoonStock = moonStock;
        }
    }

    public class QuicktradeGameEndedEventArgs : EventArgs
    {
        public float FinalProfit { get; }
        public Dictionary<string, StockData> FinalStocks { get; }
        
        public QuicktradeGameEndedEventArgs(float finalProfit, Dictionary<string, StockData> finalStocks)
        {
            FinalProfit = finalProfit;
            FinalStocks = new Dictionary<string, StockData>(finalStocks);
        }
    }

    public class QuicktradeGameResetEventArgs : EventArgs
    {
    }

    public class QuicktradeTimeUpdateEventArgs : EventArgs
    {
        public float TimeLeft { get; }
        
        public QuicktradeTimeUpdateEventArgs(float timeLeft)
        {
            TimeLeft = timeLeft;
        }
    }

    // Stock trading events
    public class QuicktradeStockPurchasedEventArgs : EventArgs
    {
        public string StockKey { get; }
        public float PurchasePrice { get; }
        public float CurrentProfit { get; }
        
        public QuicktradeStockPurchasedEventArgs(string stockKey, float purchasePrice, float currentProfit)
        {
            StockKey = stockKey;
            PurchasePrice = purchasePrice;
            CurrentProfit = currentProfit;
        }
    }

    public class QuicktradeStockSoldEventArgs : EventArgs
    {
        public string StockKey { get; }
        public float SellPrice { get; }
        public float TotalProfit { get; }
        
        public QuicktradeStockSoldEventArgs(string stockKey, float sellPrice, float totalProfit)
        {
            StockKey = stockKey;
            SellPrice = sellPrice;
            TotalProfit = totalProfit;
        }
    }

    // Price update events
    public class QuicktradePricesUpdatedEventArgs : EventArgs
    {
        public Dictionary<string, StockData> Stocks { get; }
        public float CurrentProfit { get; }
        
        public QuicktradePricesUpdatedEventArgs(Dictionary<string, StockData> stocks, float currentProfit)
        {
            Stocks = stocks;
            CurrentProfit = currentProfit;
        }
    }

    public class QuicktradeMoonEventArgs : EventArgs
    {
        public string MoonStock { get; }
        
        public QuicktradeMoonEventArgs(string moonStock)
        {
            MoonStock = moonStock;
        }
    }

    // Input events
    public class QuicktradeStockClickedEventArgs : EventArgs
    {
        public string StockKey { get; }
        
        public QuicktradeStockClickedEventArgs(string stockKey)
        {
            StockKey = stockKey;
        }
    }

    public class QuicktradeSellClickedEventArgs : EventArgs
    {
    }

    public class QuicktradeStartClickedEventArgs : EventArgs
    {
    }

    public class QuicktradeResetClickedEventArgs : EventArgs
    {
    }
}