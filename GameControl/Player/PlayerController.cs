using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ham.GameControl.Player
{

    public class PlayerController : MonoBehaviour
    {
        [Header("Player Identity")]
        public string PlayerName = "Player";
        public Color PlayerColor = Color.white;
        public Sprite PlayerSymbol;
        public int PlayerID = 0; // Our own ID system, independent of Unity's playerIndex

        [Header("Debug")]
        public bool enableDebugLogs = true;

        // Events
        public event EventHandler<PlayerActionEventArgs> OnPlayerAction;

        // Properties
        public string Name => PlayerName;
        public Color Color => PlayerColor;
        public Sprite Symbol => PlayerSymbol;
        public int ID => PlayerID;

        // No PlayerInput component needed - InputManager handles all input
        private PlayerManager playerManager;

        private void Start()
        {
            // Find PlayerManager and register with it
            this.playerManager = FindAnyObjectByType<PlayerManager>();
            if (this.playerManager != null)
            {
                this.playerManager.RegisterPlayer(this);
            }

            this.DebugLog($"Player {this.Name} initialized with ID {this.PlayerID}");
        }

        private void OnDestroy()
        {
            // Unregister when destroyed
            if (this.playerManager != null)
            {
                this.playerManager.UnregisterPlayer(this);
            }
        }

        #region Public Methods Called by InputManager

        public void HandleClick(Vector2 screenPosition, GameObject target)
        {
            this.DebugLog($"{this.Name} (ID: {this.PlayerID}) clicked on {target?.name ?? "nothing"}");

            if (target != null)
            {
                // Emit player action event
                var args = new PlayerActionEventArgs
                {
                    Player = this,
                    ActionType = PlayerActionType.Select,
                    Target = target,
                    ScreenPosition = screenPosition
                };

                this.OnPlayerAction?.Invoke(this, args);
            }
        }

        public void HandleNavigation(Vector2 direction)
        {
            this.DebugLog($"{this.Name} navigated: {direction}");

            var args = new PlayerActionEventArgs
            {
                Player = this,
                ActionType = PlayerActionType.Navigate,
                Direction = direction
            };

            this.OnPlayerAction?.Invoke(this, args);
        }

        public void HandleSelect()
        {
            this.DebugLog($"{this.Name} selected");

            var args = new PlayerActionEventArgs
            {
                Player = this,
                ActionType = PlayerActionType.Interact
            };

            this.OnPlayerAction?.Invoke(this, args);
        }

        public void HandleCancel()
        {
            this.DebugLog($"{this.Name} cancelled");

            var args = new PlayerActionEventArgs
            {
                Player = this,
                ActionType = PlayerActionType.Cancel
            };

            this.OnPlayerAction?.Invoke(this, args);
        }

        #endregion

        #region Setup Methods

        public void SetPlayerData(string name, Color color, Sprite symbol, int id)
        {
            this.PlayerName = name;
            this.PlayerColor = color;
            this.PlayerSymbol = symbol;
            this.PlayerID = id;

            // Update GameObject name for easier debugging
            this.gameObject.name = $"Player_{id}_{name}";

            this.DebugLog($"Player data set: {name}, ID: {id}");
        }

        public void SetActive(bool active)
        {
            this.gameObject.SetActive(active);
        }

        #endregion

        #region Helper Methods

        private void DebugLog(string message)
        {
            if (this.enableDebugLogs)
            {
                Debug.Log($"[PlayerController] {message}");
            }
        }

        #endregion
    }

    // Event Args
    public enum PlayerActionType
    {
        Select,
        Navigate,
        Interact,
        Cancel
    }

    public class PlayerActionEventArgs : EventArgs
    {
        public PlayerController Player { get; set; }
        public PlayerActionType ActionType { get; set; }
        public GameObject Target { get; set; }
        public Vector2 ScreenPosition { get; set; }
        public Vector2 Direction { get; set; }
    }
}