using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ham.GameControl.Player
{
    public class PlayerManager : Manager
    {
        [Header("Player Settings")]
        public GameObject playerPrefab;
        public Color[] playerColors = { Color.red, Color.blue, Color.green, Color.yellow };
        public Sprite[] playerSymbols;
        public string[] playerNames = { "Player 1", "Player 2", "Player 3", "Player 4" };

        [Header("Game Settings")]
        public int maxPlayers = 4;
        public bool autoStartWithMinPlayers = true;
        public int minPlayersToStart = 2;

        [Header("Turn-Based Settings")]
        public bool isTurnBased = true; // Toggle for turn-based vs simultaneous

        [Header("Debug")]
        public bool enableDebugLogs = true;

        // Events
        public event EventHandler<PlayerJoinedEventArgs> OnPlayerJoined;
        public event EventHandler<PlayerLeftEventArgs> OnPlayerLeft;
        public event EventHandler<PlayerActionEventArgs> OnPlayerAction;
        public event EventHandler<PlayerTurnEventArgs> OnTurnChanged;

        // Player Management (Our own ID system)
        private Dictionary<int, PlayerController> playersById = new Dictionary<int, PlayerController>();
        public List<PlayerController> Players => this.playersById.Values.OrderBy(p => p.ID).ToList();
        public PlayerController CurrentPlayer { get; private set; }
        public int CurrentPlayerID { get; private set; } = 0;
        public int PlayerCount => this.playersById.Count;

        // State
        public bool IsGameActive { get; private set; } = false;
        private int nextPlayerID = 0;

        protected override void Start()
        {
            base.Start();

            // Create initial players (you can adjust this)
            this.CreateInitialPlayers(2);

            this.DebugLog("PlayerManager initialized with shared input system");
        }

        #region Player Creation & Management

        private void CreateInitialPlayers(int count)
        {
            for (int i = 0; i < count && i < this.maxPlayers; i++)
            {
                this.CreatePlayer();
            }

            if (this.PlayerCount > 0)
            {
                this.SetCurrentPlayer(0); // Set first player as current

                if (this.autoStartWithMinPlayers && this.PlayerCount >= this.minPlayersToStart)
                {
                    this.StartGame();
                }
            }
        }

        public PlayerController CreatePlayer()
        {
            if (this.PlayerCount >= this.maxPlayers)
            {
                this.DebugLog($"Cannot create player - max players ({this.maxPlayers}) reached");
                return null;
            }

            // Create player GameObject
            GameObject playerObj;
            if (this.playerPrefab != null)
            {
                playerObj = Instantiate(this.playerPrefab);
            }
            else
            {
                playerObj = new GameObject("Player");
                playerObj.AddComponent<PlayerController>();
            }

            PlayerController player = playerObj.GetComponent<PlayerController>();
            if (player == null)
            {
                this.DebugLog("Player prefab missing PlayerController component");
                Destroy(playerObj);
                return null;
            }

            // Set up player data
            int playerID = this.nextPlayerID++;
            string playerName = playerID < this.playerNames.Length ? this.playerNames[playerID] : $"Player {playerID + 1}";
            Color playerColor = playerID < this.playerColors.Length ? this.playerColors[playerID] : Color.white;
            Sprite playerSymbol = playerID < this.playerSymbols.Length ? this.playerSymbols[playerID] : null;

            player.SetPlayerData(playerName, playerColor, playerSymbol, playerID);

            this.DebugLog($"Created player: {player.Name} (ID: {playerID})");
            return player;
        }

        public void RemovePlayer(int playerID)
        {
            if (this.playersById.TryGetValue(playerID, out PlayerController player))
            {
                this.UnregisterPlayer(player);
                Destroy(player.gameObject);
            }
        }

        public void RegisterPlayer(PlayerController player)
        {
            int playerID = player.ID;

            if (this.playersById.ContainsKey(playerID))
            {
                this.DebugLog($"Warning: Player with ID {playerID} already registered");
                return;
            }

            // Add to our tracking
            this.playersById[playerID] = player;

            // Subscribe to player actions
            player.OnPlayerAction += this.HandlePlayerAction;

            // Apply customization if available
            var customizationSystem = Manager.Get<PlayerCustomizationSystem>();
            if (customizationSystem != null)
            {
                customizationSystem.ApplyCustomizationToPlayer(player);
            }

            this.DebugLog($"Player registered: {player.Name} (ID: {playerID}, Total: {this.PlayerCount})");

            // Emit event
            this.OnPlayerJoined?.Invoke(this, new PlayerJoinedEventArgs { player = player });
        }

        public void UnregisterPlayer(PlayerController player)
        {
            int playerID = player.ID;

            if (!this.playersById.ContainsKey(playerID))
            {
                this.DebugLog($"Warning: Player with ID {playerID} not found for unregistration");
                return;
            }

            // Unsubscribe from events
            player.OnPlayerAction -= this.HandlePlayerAction;

            // Remove from tracking
            this.playersById.Remove(playerID);

            this.DebugLog($"Player unregistered: {player.Name} (Remaining: {this.PlayerCount})");

            // Emit event
            this.OnPlayerLeft?.Invoke(this, new PlayerLeftEventArgs { player = player });

            // Handle current player adjustment
            if (player == this.CurrentPlayer)
            {
                if (this.PlayerCount > 0)
                {
                    // Set next available player as current
                    var nextPlayer = this.Players.FirstOrDefault();
                    this.SetCurrentPlayer(nextPlayer.ID);
                }
                else
                {
                    this.CurrentPlayer = null;
                    this.IsGameActive = false;
                }
            }
        }

        #endregion

        #region Turn Management

        public void SetCurrentPlayer(int playerID)
        {
            if (!this.playersById.TryGetValue(playerID, out PlayerController player))
            {
                this.DebugLog($"Invalid player ID for SetCurrentPlayer: {playerID}");
                return;
            }

            PlayerController previousPlayer = this.CurrentPlayer;
            this.CurrentPlayer = player;
            this.CurrentPlayerID = playerID;

            this.DebugLog($"Current player set to: {this.CurrentPlayer.Name} (ID: {this.CurrentPlayerID})");

            // Emit turn changed event
            this.OnTurnChanged?.Invoke(this, new PlayerTurnEventArgs
            {
                PreviousPlayer = previousPlayer,
                CurrentPlayer = this.CurrentPlayer,
                PlayerIndex = this.CurrentPlayerID
            });
        }

        public void NextPlayer()
        {
            if (this.PlayerCount == 0) return;

            var orderedPlayers = this.Players; // Already ordered by ID
            int currentIndex = orderedPlayers.IndexOf(this.CurrentPlayer);

            if (currentIndex == -1)
            {
                // Current player not found, set to first
                this.SetCurrentPlayer(orderedPlayers[0].ID);
                return;
            }

            // Get next player in order
            int nextIndex = (currentIndex + 1) % orderedPlayers.Count;
            this.SetCurrentPlayer(orderedPlayers[nextIndex].ID);
        }

        public bool IsCurrentPlayer(PlayerController player)
        {
            return this.CurrentPlayer == player;
        }

        public bool IsPlayersTurn(PlayerController player)
        {
            if (!this.isTurnBased) return true; // In non-turn-based games, it's always everyone's turn
            return this.IsCurrentPlayer(player);
        }

        #endregion

        #region Input Handling

        private void HandlePlayerAction(object sender, PlayerActionEventArgs e)
        {
            this.DebugLog($"Player action from {e.Player.Name} (ID: {e.Player.ID}): {e.ActionType}");

            // In turn-based games, only process actions from current player
            // In simultaneous games, process all player actions
            if (this.IsGameActive && this.isTurnBased && !this.IsCurrentPlayer(e.Player))
            {
                this.DebugLog($"Not {e.Player.Name}'s turn (Current: {this.CurrentPlayer?.Name})");
                return;
            }

            // Forward the action
            this.OnPlayerAction?.Invoke(this, e);
        }

        #endregion

        #region Game State Management

        public void StartGame()
        {
            if (this.PlayerCount < this.minPlayersToStart)
            {
                this.DebugLog($"Not enough players to start ({this.PlayerCount}/{this.minPlayersToStart})");
                return;
            }

            this.IsGameActive = true;

            // Set first player as current (even for non-turn-based games, for consistency)
            if (this.CurrentPlayer == null && this.PlayerCount > 0)
            {
                this.SetCurrentPlayer(this.Players[0].ID);
            }

            string gameType = this.isTurnBased ? "turn-based" : "simultaneous";
            this.DebugLog($"Game started with {this.PlayerCount} players ({gameType})");
        }

        public void EndGame()
        {
            this.IsGameActive = false;
            this.DebugLog("Game ended");
        }

        public void RestartGame()
        {
            this.EndGame();
            this.StartGame();
        }

        public void SetGameMode(bool turnBased)
        {
            this.isTurnBased = turnBased;
            string mode = turnBased ? "turn-based" : "simultaneous";
            this.DebugLog($"Game mode set to: {mode}");
        }

        #endregion

        #region Public Utility Methods

        public PlayerController GetPlayer(int playerID)
        {
            this.playersById.TryGetValue(playerID, out PlayerController player);
            return player;
        }

        public PlayerController GetPlayerByName(string name)
        {
            return this.Players.FirstOrDefault(p => p.Name == name);
        }

        public void DebugPlayerStatus()
        {
            this.DebugLog("=== Player Status ===");
            var orderedPlayers = this.Players;
            for (int i = 0; i < orderedPlayers.Count; i++)
            {
                var player = orderedPlayers[i];
                bool isCurrent = player == this.CurrentPlayer;
                this.DebugLog($"[ID: {player.ID}] {player.Name} {(isCurrent ? "(CURRENT)" : "")}");
            }
            this.DebugLog($"Game Active: {this.IsGameActive}");
            this.DebugLog($"Game Mode: {(this.isTurnBased ? "Turn-Based" : "Simultaneous")}");
            this.DebugLog($"Current Player ID: {this.CurrentPlayerID}");
        }

        #endregion

        #region Helper Methods

        private void DebugLog(string message)
        {
            if (this.enableDebugLogs)
            {
                Debug.Log($"[PlayerManager] {message}");
            }
        }

        #endregion
    }

    // Event Args
    public class PlayerJoinedEventArgs : EventArgs
    {
        public PlayerController player;
    }

    public class PlayerLeftEventArgs : EventArgs
    {
        public PlayerController player;
    }

    public class PlayerTurnEventArgs : EventArgs
    {
        public PlayerController PreviousPlayer { get; set; }
        public PlayerController CurrentPlayer { get; set; }
        public int PlayerIndex { get; set; }
    }
}