using UnityEngine;

namespace TicTacJoe.Quicktrade
{
    /// <summary>
    /// Main controller that coordinates the Quicktrade minigame
    /// Handles input events and manages game flow
    /// </summary>
    public class QuicktradeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject sellArea; // Large invisible area for selling

        private QuicktradeGameManager gameManager;
        private QuicktradeUIManager uiManager;

        private void Start()
        {
            gameManager = ManagerHub.Instance.GetManager<QuicktradeGameManager>();
            uiManager = ManagerHub.Instance.GetManager<QuicktradeUIManager>();
            
            SetupEventListeners();
            SetupSellArea();
        }

        private void SetupEventListeners()
        {
            // Listen for UI input events
            ManagerHub.Instance.AddEventListener<QuicktradeStartClickedEventArgs>(OnStartClicked);
            ManagerHub.Instance.AddEventListener<QuicktradeResetClickedEventArgs>(OnResetClicked);
            ManagerHub.Instance.AddEventListener<QuicktradeStockClickedEventArgs>(OnStockClicked);
            ManagerHub.Instance.AddEventListener<QuicktradeSellClickedEventArgs>(OnSellClicked);
        }

        private void SetupSellArea()
        {
            if (sellArea != null)
            {
                // Add click handler to sell area for mouse/touch input
                var button = sellArea.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => {
                        if (gameManager.CurrentGameState == GameState.Playing && 
                            !string.IsNullOrEmpty(gameManager.CurrentHoldingStock))
                        {
                            ManagerHub.Instance.EmitEvent(new QuicktradeSellClickedEventArgs());
                        }
                    });
                }
            }
        }

        private void OnStartClicked(QuicktradeStartClickedEventArgs eventArgs)
        {
            gameManager.StartGame();
        }

        private void OnResetClicked(QuicktradeResetClickedEventArgs eventArgs)
        {
            gameManager.ResetGame();
        }

        private void OnStockClicked(QuicktradeStockClickedEventArgs eventArgs)
        {
            gameManager.BuyStock(eventArgs.StockKey);
        }

        private void OnSellClicked(QuicktradeSellClickedEventArgs eventArgs)
        {
            gameManager.SellStock();
        }

        private void OnDestroy()
        {
            // Remove event listeners
            if (ManagerHub.Instance != null)
            {
                ManagerHub.Instance.RemoveEventListener<QuicktradeStartClickedEventArgs>(OnStartClicked);
                ManagerHub.Instance.RemoveEventListener<QuicktradeResetClickedEventArgs>(OnResetClicked);
                ManagerHub.Instance.RemoveEventListener<QuicktradeStockClickedEventArgs>(OnStockClicked);
                ManagerHub.Instance.RemoveEventListener<QuicktradeSellClickedEventArgs>(OnSellClicked);
            }

            // Clean up sell area button
            if (sellArea != null)
            {
                var button = sellArea.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
        }
    }
}