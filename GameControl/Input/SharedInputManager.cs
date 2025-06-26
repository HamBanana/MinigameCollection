using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Ham.GameControl.Player;

namespace Ham.GameControl.Input
{

    public class SharedInputManager : Manager
    {
        [Header("Input Settings")]
        public InputActionAsset inputActions;

        [Header("Player Assignment")]
        [Tooltip("How to determine which player performed an action")]
        public PlayerDetectionMode playerDetectionMode = PlayerDetectionMode.ClickLocation;

        [Header("Keyboard Player Assignment")]
        [Tooltip("For keyboard users, which player should receive the input")]
        public int keyboardPlayerID = 0;

        [Header("Debug")]
        public bool enableDebugLogs = true;

        // Input Actions
        private InputAction clickAction;
        private InputAction navigateAction;
        private InputAction selectAction;
        private InputAction cancelAction;

        // Player Management
        private PlayerManager playerManager;

        // For keyboard/gamepad navigation
        private int currentNavigatingPlayerID = 0;

        protected override void Start()
        {
            base.Start();

            this.playerManager = Manager.Get<PlayerManager>();
            this.SetupInputActions();

            this.DebugLog("SharedInputManager initialized");
        }

        private void SetupInputActions()
        {
            if (this.inputActions == null)
            {
                this.DebugLog("No InputActionAsset assigned");
                return;
            }

            // Get actions
            this.clickAction = this.inputActions.FindAction("Click");
            this.navigateAction = this.inputActions.FindAction("Navigate");
            this.selectAction = this.inputActions.FindAction("Select");
            this.cancelAction = this.inputActions.FindAction("Cancel");

            // Subscribe to actions
            if (this.clickAction != null)
            {
                this.clickAction.performed += this.OnClick;
                this.clickAction.Enable();
            }

            if (this.navigateAction != null)
            {
                this.navigateAction.performed += this.OnNavigate;
                this.navigateAction.Enable();
            }

            if (this.selectAction != null)
            {
                this.selectAction.performed += this.OnSelect;
                this.selectAction.Enable();
            }

            if (this.cancelAction != null)
            {
                this.cancelAction.performed += this.OnCancel;
                this.cancelAction.Enable();
            }

            this.DebugLog("Input actions set up and enabled");
        }

        #region Input Callbacks

        private void OnClick(InputAction.CallbackContext ctx)
        {
            Vector2 screenPos = this.GetPointerPosition();
            GameObject target = this.GetClickTarget(screenPos);

            PlayerController player = this.DeterminePlayerFromClick(screenPos, target);

            if (player != null)
            {
                this.DebugLog($"Click routed to {player.Name} at {screenPos}");
                player.HandleClick(screenPos, target);
            }
            else
            {
                this.DebugLog($"Click at {screenPos} - no player determined");
            }
        }

        private void OnNavigate(InputAction.CallbackContext ctx)
        {
            Vector2 direction = ctx.ReadValue<Vector2>();

            // For navigation, use the currently navigating player or keyboard assigned player
            PlayerController player = this.playerManager.GetPlayer(this.currentNavigatingPlayerID)
                                     ?? this.playerManager.GetPlayer(this.keyboardPlayerID);

            if (player != null)
            {
                this.DebugLog($"Navigation routed to {player.Name}: {direction}");
                player.HandleNavigation(direction);
            }
        }

        private void OnSelect(InputAction.CallbackContext ctx)
        {
            // Use current navigating player or keyboard assigned player
            PlayerController player = this.playerManager.GetPlayer(this.currentNavigatingPlayerID)
                                     ?? this.playerManager.GetPlayer(this.keyboardPlayerID);

            if (player != null)
            {
                this.DebugLog($"Select routed to {player.Name}");
                player.HandleSelect();
            }
        }

        private void OnCancel(InputAction.CallbackContext ctx)
        {
            // Use current navigating player or keyboard assigned player
            PlayerController player = this.playerManager.GetPlayer(this.currentNavigatingPlayerID)
                                     ?? this.playerManager.GetPlayer(this.keyboardPlayerID);

            if (player != null)
            {
                this.DebugLog($"Cancel routed to {player.Name}");
                player.HandleCancel();
            }
        }

        #endregion

        #region Player Detection Logic

        private PlayerController DeterminePlayerFromClick(Vector2 screenPos, GameObject target)
        {
            switch (this.playerDetectionMode)
            {
                case PlayerDetectionMode.CurrentPlayer:
                    return this.playerManager.CurrentPlayer;

                case PlayerDetectionMode.ClickLocation:
                    return this.DeterminePlayerFromLocation(screenPos, target);

                case PlayerDetectionMode.AlwaysFirstPlayer:
                    return this.playerManager.GetPlayer(0);

                case PlayerDetectionMode.CycleOnClick:
                    this.playerManager.NextPlayer();
                    return this.playerManager.CurrentPlayer;

                default:
                    return this.playerManager.CurrentPlayer;
            }
        }

        private PlayerController DeterminePlayerFromLocation(Vector2 screenPos, GameObject target)
        {
            // Option 1: If clicking on a game object that belongs to a specific player
            if (target != null)
            {
                // Check if the target has player ownership info
                var ownable = target.GetComponent<Ownable>();
                if (ownable != null && ownable.Owner != null)
                {
                    return ownable.Owner;
                }

                // Check if it's a UI element with player info
                var playerUI = target.GetComponent<PlayerUIElement>();
                if (playerUI != null)
                {
                    return this.playerManager.GetPlayer(playerUI.PlayerID);
                }
            }

            // Option 2: Determine by screen region (split-screen style)
            return this.DeterminePlayerFromScreenRegion(screenPos);
        }

        private PlayerController DeterminePlayerFromScreenRegion(Vector2 screenPos)
        {
            // Example: Split screen horizontally for 2 players
            if (this.playerManager.PlayerCount == 2)
            {
                float screenMidpoint = Screen.width * 0.5f;
                return screenPos.x < screenMidpoint ? this.playerManager.GetPlayer(0) : this.playerManager.GetPlayer(1);
            }

            // Example: Quadrants for 4 players  
            if (this.playerManager.PlayerCount == 4)
            {
                bool leftSide = screenPos.x < Screen.width * 0.5f;
                bool topSide = screenPos.y > Screen.height * 0.5f;

                if (leftSide && topSide) return this.playerManager.GetPlayer(0);
                if (!leftSide && topSide) return this.playerManager.GetPlayer(1);
                if (leftSide && !topSide) return this.playerManager.GetPlayer(2);
                if (!leftSide && !topSide) return this.playerManager.GetPlayer(3);
            }

            // Default: current player
            return this.playerManager.CurrentPlayer;
        }

        #endregion

        #region Helper Methods

        private Vector2 GetPointerPosition()
        {
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }

            if (Pointer.current != null)
            {
                return Pointer.current.position.ReadValue();
            }

            return Vector2.zero;
        }

        private GameObject GetClickTarget(Vector2 screenPos)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.collider.gameObject;
            }

            return null;
        }

        private void DebugLog(string message)
        {
            if (this.enableDebugLogs)
            {
                Debug.Log($"[SharedInputManager] {message}");
            }
        }

        #endregion

        #region Public Methods

        public void SetNavigatingPlayer(int playerID)
        {
            this.currentNavigatingPlayerID = playerID;
            this.DebugLog($"Navigating player set to ID: {playerID}");
        }

        public void SetKeyboardPlayer(int playerID)
        {
            this.keyboardPlayerID = playerID;
            this.DebugLog($"Keyboard player set to ID: {playerID}");
        }

        #endregion

        private void OnDestroy()
        {
            // Clean up input actions
            if (this.clickAction != null) this.clickAction.performed -= this.OnClick;
            if (this.navigateAction != null) this.navigateAction.performed -= this.OnNavigate;
            if (this.selectAction != null) this.selectAction.performed -= this.OnSelect;
            if (this.cancelAction != null) this.cancelAction.performed -= this.OnCancel;
        }
    }

    // Helper component for UI elements that belong to specific players
    public class PlayerUIElement : MonoBehaviour
    {
        public int PlayerID;
    }

    public enum PlayerDetectionMode
    {
        CurrentPlayer,      // Always route to current player (turn-based)
        ClickLocation,      // Determine player based on where they clicked
        AlwaysFirstPlayer,  // Always route to player 0 (single player on shared device)
        CycleOnClick       // Switch to next player on each click (hot-seat style)
    }
}