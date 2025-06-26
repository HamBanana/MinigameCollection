using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ham.GameControl.Player
{

    [System.Serializable]
    public class PlayerCustomizationData
    {
        public string playerName = "Player";
        public Color playerColor = Color.white;
        public Sprite playerSymbol;
        public GameObject playerModel;
        public int selectedSymbolIndex = 0;
        public int selectedColorIndex = 0;
        public int selectedModelIndex = 0;
    }

    public class PlayerCustomizationSystem : Manager
    {
        [Header("Available Options")]
        public string[] availableNames = { "Player 1", "Player 2", "Player 3", "Player 4" };
        public Color[] availableColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan, Color.magenta };
        public Sprite[] availableSymbols;
        public GameObject[] availableModels;

        [Header("Default Settings")]
        public string defaultNamePrefix = "Player";
        public Color defaultColor = Color.white;
        public Sprite defaultSymbol;
        public GameObject defaultModel;

        [Header("UI References")]
        public PlayerCustomizationUI customizationUI;

        [Header("Debug")]
        public bool enableDebugLogs = true;

        // Events
        public event EventHandler<PlayerCustomizationEventArgs> OnPlayerCustomizationChanged;
        public event EventHandler OnCustomizationComplete;

        // Player customization data
        private Dictionary<int, PlayerCustomizationData> playerCustomizations = new Dictionary<int, PlayerCustomizationData>();
        private PlayerManager playerManager;

        // State
        public bool IsCustomizationActive { get; private set; } = false;
        public int CurrentCustomizingPlayerID { get; private set; } = -1;

        protected override void Start()
        {
            base.Start();
            this.playerManager = Manager.Get<PlayerManager>();

            // Initialize default customizations for all potential players
            this.InitializeDefaultCustomizations();

            this.DebugLog("PlayerCustomizationSystem initialized");
        }

        #region Initialization

        private void InitializeDefaultCustomizations()
        {
            for (int i = 0; i < this.availableNames.Length; i++)
            {
                var customization = new PlayerCustomizationData
                {
                    playerName = i < this.availableNames.Length ? this.availableNames[i] : $"{this.defaultNamePrefix} {i + 1}",
                    playerColor = i < this.availableColors.Length ? this.availableColors[i] : this.defaultColor,
                    playerSymbol = i < this.availableSymbols.Length ? this.availableSymbols[i] : this.defaultSymbol,
                    playerModel = i < this.availableModels.Length ? this.availableModels[i] : this.defaultModel,
                    selectedColorIndex = i < this.availableColors.Length ? i : 0,
                    selectedSymbolIndex = i < this.availableSymbols.Length ? i : 0,
                    selectedModelIndex = i < this.availableModels.Length ? i : 0
                };

                this.playerCustomizations[i] = customization;
            }

            this.DebugLog($"Initialized default customizations for {this.playerCustomizations.Count} players");
        }

        #endregion

        #region Public Interface

        public void StartCustomization()
        {
            this.IsCustomizationActive = true;
            this.CurrentCustomizingPlayerID = 0;

            if (this.customizationUI != null)
            {
                this.customizationUI.ShowCustomization();
                this.customizationUI.LoadPlayerCustomization(this.CurrentCustomizingPlayerID, this.GetPlayerCustomization(this.CurrentCustomizingPlayerID));
            }

            this.DebugLog("Started player customization");
        }

        public void EndCustomization()
        {
            this.IsCustomizationActive = false;
            this.CurrentCustomizingPlayerID = -1;

            if (this.customizationUI != null)
            {
                this.customizationUI.HideCustomization();
            }

            // Apply customizations to existing players
            this.ApplyCustomizationsToPlayers();

            this.OnCustomizationComplete?.Invoke(this, EventArgs.Empty);
            this.DebugLog("Ended player customization");
        }

        public void NextPlayer()
        {
            if (!this.IsCustomizationActive) return;

            this.CurrentCustomizingPlayerID++;

            // Check if we've customized all players or reached max
            if (this.CurrentCustomizingPlayerID >= this.playerManager.maxPlayers ||
                this.CurrentCustomizingPlayerID >= this.availableNames.Length)
            {
                this.EndCustomization();
                return;
            }

            // Load next player's customization
            if (this.customizationUI != null)
            {
                this.customizationUI.LoadPlayerCustomization(this.CurrentCustomizingPlayerID, this.GetPlayerCustomization(this.CurrentCustomizingPlayerID));
            }

            this.DebugLog($"Moved to customizing player {this.CurrentCustomizingPlayerID}");
        }

        public void PreviousPlayer()
        {
            if (!this.IsCustomizationActive) return;

            this.CurrentCustomizingPlayerID = Mathf.Max(0, this.CurrentCustomizingPlayerID - 1);

            if (this.customizationUI != null)
            {
                this.customizationUI.LoadPlayerCustomization(this.CurrentCustomizingPlayerID, this.GetPlayerCustomization(this.CurrentCustomizingPlayerID));
            }

            this.DebugLog($"Moved to customizing player {this.CurrentCustomizingPlayerID}");
        }

        #endregion

        #region Customization Methods

        public void SetPlayerName(int playerID, string name)
        {
            var customization = this.GetPlayerCustomization(playerID);
            customization.playerName = name;

            this.EmitCustomizationChanged(playerID, customization);
            this.DebugLog($"Player {playerID} name set to: {name}");
        }

        public void SetPlayerColor(int playerID, int colorIndex)
        {
            if (colorIndex < 0 || colorIndex >= this.availableColors.Length) return;

            var customization = this.GetPlayerCustomization(playerID);
            customization.selectedColorIndex = colorIndex;
            customization.playerColor = this.availableColors[colorIndex];

            this.EmitCustomizationChanged(playerID, customization);
            this.DebugLog($"Player {playerID} color set to: {customization.playerColor}");
        }

        public void SetPlayerSymbol(int playerID, int symbolIndex)
        {
            if (symbolIndex < 0 || symbolIndex >= this.availableSymbols.Length) return;

            var customization = this.GetPlayerCustomization(playerID);
            customization.selectedSymbolIndex = symbolIndex;
            customization.playerSymbol = this.availableSymbols[symbolIndex];

            this.EmitCustomizationChanged(playerID, customization);
            this.DebugLog($"Player {playerID} symbol set to index: {symbolIndex}");
        }

        public void SetPlayerModel(int playerID, int modelIndex)
        {
            if (modelIndex < 0 || modelIndex >= this.availableModels.Length) return;

            var customization = this.GetPlayerCustomization(playerID);
            customization.selectedModelIndex = modelIndex;
            customization.playerModel = this.availableModels[modelIndex];

            this.EmitCustomizationChanged(playerID, customization);
            this.DebugLog($"Player {playerID} model set to index: {modelIndex}");
        }

        public void CyclePlayerColor(int playerID, bool forward = true)
        {
            var customization = this.GetPlayerCustomization(playerID);
            int newIndex = forward ?
                (customization.selectedColorIndex + 1) % this.availableColors.Length :
                (customization.selectedColorIndex - 1 + this.availableColors.Length) % this.availableColors.Length;

            this.SetPlayerColor(playerID, newIndex);
        }

        public void CyclePlayerSymbol(int playerID, bool forward = true)
        {
            var customization = this.GetPlayerCustomization(playerID);
            int newIndex = forward ?
                (customization.selectedSymbolIndex + 1) % this.availableSymbols.Length :
                (customization.selectedSymbolIndex - 1 + this.availableSymbols.Length) % this.availableSymbols.Length;

            this.SetPlayerSymbol(playerID, newIndex);
        }

        public void CyclePlayerModel(int playerID, bool forward = true)
        {
            var customization = this.GetPlayerCustomization(playerID);
            int newIndex = forward ?
                (customization.selectedModelIndex + 1) % this.availableModels.Length :
                (customization.selectedModelIndex - 1 + this.availableModels.Length) % this.availableModels.Length;

            this.SetPlayerModel(playerID, newIndex);
        }

        #endregion

        #region Data Access

        public PlayerCustomizationData GetPlayerCustomization(int playerID)
        {
            if (!this.playerCustomizations.ContainsKey(playerID))
            {
                // Create default customization if it doesn't exist
                this.playerCustomizations[playerID] = new PlayerCustomizationData
                {
                    playerName = $"{this.defaultNamePrefix} {playerID + 1}",
                    playerColor = this.defaultColor,
                    playerSymbol = this.defaultSymbol,
                    playerModel = this.defaultModel
                };
            }

            return this.playerCustomizations[playerID];
        }

        public string GetPlayerName(int playerID) => this.GetPlayerCustomization(playerID).playerName;
        public Color GetPlayerColor(int playerID) => this.GetPlayerCustomization(playerID).playerColor;
        public Sprite GetPlayerSymbol(int playerID) => this.GetPlayerCustomization(playerID).playerSymbol;
        public GameObject GetPlayerModel(int playerID) => this.GetPlayerCustomization(playerID).playerModel;

        public bool IsColorTaken(int colorIndex, int excludePlayerID = -1)
        {
            foreach (var kvp in this.playerCustomizations)
            {
                if (kvp.Key != excludePlayerID && kvp.Value.selectedColorIndex == colorIndex)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsSymbolTaken(int symbolIndex, int excludePlayerID = -1)
        {
            foreach (var kvp in this.playerCustomizations)
            {
                if (kvp.Key != excludePlayerID && kvp.Value.selectedSymbolIndex == symbolIndex)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Integration with PlayerManager

        private void ApplyCustomizationsToPlayers()
        {
            foreach (var player in this.playerManager.Players)
            {
                this.ApplyCustomizationToPlayer(player);
            }
        }

        public void ApplyCustomizationToPlayer(PlayerController player)
        {
            var customization = this.GetPlayerCustomization(player.ID);
            player.SetPlayerData(
                customization.playerName,
                customization.playerColor,
                customization.playerSymbol,
                player.ID
            );

            // Apply model if the player has a visual representation
            var playerVisuals = player.GetComponent<PlayerVisuals>();
            if (playerVisuals != null && customization.playerModel != null)
            {
                playerVisuals.SetModel(customization.playerModel);
            }

            this.DebugLog($"Applied customization to player {player.ID}: {customization.playerName}");
        }

        #endregion

        #region Events

        private void EmitCustomizationChanged(int playerID, PlayerCustomizationData customization)
        {
            this.OnPlayerCustomizationChanged?.Invoke(this, new PlayerCustomizationEventArgs
            {
                PlayerID = playerID,
                Customization = customization
            });
        }

        #endregion

        #region Validation

        public bool ValidateCustomizations()
        {
            // Check for duplicate colors (if desired)
            var usedColors = new HashSet<int>();
            var usedSymbols = new HashSet<int>();

            foreach (var customization in this.playerCustomizations.Values)
            {
                if (usedColors.Contains(customization.selectedColorIndex))
                {
                    this.DebugLog($"Warning: Duplicate color detected: {customization.playerColor}");
                    // Could auto-fix or return false
                }
                usedColors.Add(customization.selectedColorIndex);

                if (usedSymbols.Contains(customization.selectedSymbolIndex))
                {
                    this.DebugLog($"Warning: Duplicate symbol detected at index: {customization.selectedSymbolIndex}");
                }
                usedSymbols.Add(customization.selectedSymbolIndex);
            }

            return true;
        }

        #endregion

        #region Helper Methods

        private void DebugLog(string message)
        {
            if (this.enableDebugLogs)
            {
                Debug.Log($"[PlayerCustomizationSystem] {message}");
            }
        }

        #endregion
    }

    // Event Args
    public class PlayerCustomizationEventArgs : EventArgs
    {
        public int PlayerID { get; set; }
        public PlayerCustomizationData Customization { get; set; }
    }
}