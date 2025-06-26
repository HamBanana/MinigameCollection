using UnityEngine;
using UnityEngine.InputSystem;

namespace TicTacJoe.Quicktrade
{
    public class QuicktradeInputController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference sellAction;
        [SerializeField] private InputActionReference startGameAction;
        [SerializeField] private InputActionReference resetGameAction;

        private QuicktradeGameManager gameManager;

        private void Start()
        {
            gameManager = ManagerHub.Instance.GetManager<QuicktradeGameManager>();
            SetupInputActions();
        }

        private void SetupInputActions()
        {
            // Sell stock action (Space key or gamepad button)
            if (sellAction != null)
            {
                sellAction.action.performed += OnSellAction;
                sellAction.action.Enable();
            }

            // Start game action (Enter key)
            if (startGameAction != null)
            {
                startGameAction.action.performed += OnStartGameAction;
                startGameAction.action.Enable();
            }

            // Reset game action (R key)
            if (resetGameAction != null)
            {
                resetGameAction.action.performed += OnResetGameAction;
                resetGameAction.action.Enable();
            }
        }

        private void OnSellAction(InputAction.CallbackContext context)
        {
            if (gameManager.CurrentGameState == GameState.Playing && !string.IsNullOrEmpty(gameManager.CurrentHoldingStock))
            {
                ManagerHub.Instance.EmitEvent(new QuicktradeSellClickedEventArgs());
            }
        }

        private void OnStartGameAction(InputAction.CallbackContext context)
        {
            if (gameManager.CurrentGameState == GameState.Ready)
            {
                ManagerHub.Instance.EmitEvent(new QuicktradeStartClickedEventArgs());
            }
        }

        private void OnResetGameAction(InputAction.CallbackContext context)
        {
            if (gameManager.CurrentGameState == GameState.Finished || gameManager.CurrentGameState == GameState.Playing)
            {
                ManagerHub.Instance.EmitEvent(new QuicktradeResetClickedEventArgs());
            }
        }

        private void OnDestroy()
        {
            // Disable input actions
            if (sellAction != null)
            {
                sellAction.action.performed -= OnSellAction;
                sellAction.action.Disable();
            }

            if (startGameAction != null)
            {
                startGameAction.action.performed -= OnStartGameAction;
                startGameAction.action.Disable();
            }

            if (resetGameAction != null)
            {
                resetGameAction.action.performed -= OnResetGameAction;
                resetGameAction.action.Disable();
            }
        }
    }
}