using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Ham.GameControl.Player;

namespace Ham.GameControl
{

    public class GameSetupMenu : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject menuPanel;
        public Button customizePlayersButton;
        public Button startGameButton;
        public Button addPlayerButton;
        public Button removePlayerButton;
        public TMP_Text playerCountText;
        public Slider playerCountSlider;

        [Header("Game Settings")]
        public Toggle turnBasedToggle;
        public TMP_Dropdown gameModeDropdown;

        [Header("Current Players Display")]
        public Transform playerListParent;
        public GameObject playerListItemPrefab;

        // References
        private Player.PlayerManager playerManager;
        private Player.PlayerCustomizationSystem customizationSystem;
        private GameManager gameManager;

        private void Start()
        {
            this.playerManager = Manager.Get<Player.PlayerManager>();
            this.customizationSystem = Manager.Get<Player.PlayerCustomizationSystem>();
            this.gameManager = Manager.Get<GameManager>();

            this.SetupUI();
            this.UpdatePlayerDisplay();

            // Subscribe to events
            if (this.playerManager != null)
            {
                this.playerManager.OnPlayerJoined += (s, e) => this.UpdatePlayerDisplay();
                this.playerManager.OnPlayerLeft += (s, e) => this.UpdatePlayerDisplay();
            }

            if (this.customizationSystem != null)
            {
                this.customizationSystem.OnCustomizationComplete += (s, e) => this.UpdatePlayerDisplay();
            }
        }

        private void SetupUI()
        {
            // Button listeners
            if (this.customizePlayersButton != null)
                this.customizePlayersButton.onClick.AddListener(this.OpenPlayerCustomization);

            if (this.startGameButton != null)
                this.startGameButton.onClick.AddListener(this.StartGame);

            if (this.addPlayerButton != null)
                this.addPlayerButton.onClick.AddListener(this.AddPlayer);

            if (this.removePlayerButton != null)
                this.removePlayerButton.onClick.AddListener(this.RemovePlayer);

            // Slider listener
            if (this.playerCountSlider != null)
            {
                this.playerCountSlider.onValueChanged.AddListener(this.OnPlayerCountChanged);
                this.playerCountSlider.maxValue = this.playerManager?.maxPlayers ?? 4;
                this.playerCountSlider.value = this.playerManager?.PlayerCount ?? 2;
            }

            // Toggle listener
            if (this.turnBasedToggle != null)
            {
                this.turnBasedToggle.onValueChanged.AddListener(this.OnTurnBasedToggled);
                this.turnBasedToggle.isOn = this.playerManager?.isTurnBased ?? true;
            }
        }

        #region UI Callbacks

        private void OpenPlayerCustomization()
        {
            if (this.customizationSystem != null)
            {
                this.customizationSystem.StartCustomization();
                this.HideMenu();
            }
            else
            {
                Debug.LogWarning("PlayerCustomizationSystem not found!");
            }
        }

        private void StartGame()
        {
            if (this.playerManager != null)
            {
                this.playerManager.StartGame();
                this.HideMenu();
            }

            // Could also trigger specific game start
            if (this.gameManager != null)
            {
                // Start specific game based on selection
                // gameManager.StartTicTacToe();
            }
        }

        private void AddPlayer()
        {
            if (this.playerManager != null)
            {
                var newPlayer = this.playerManager.CreatePlayer();
                if (newPlayer != null)
                {
                    this.UpdatePlayerDisplay();
                }
            }
        }

        private void RemovePlayer()
        {
            if (this.playerManager != null && this.playerManager.PlayerCount > 0)
            {
                // Remove the last player
                var lastPlayer = this.playerManager.Players[this.playerManager.PlayerCount - 1];
                this.playerManager.RemovePlayer(lastPlayer.ID);
                this.UpdatePlayerDisplay();
            }
        }

        private void OnPlayerCountChanged(float value)
        {
            int targetCount = Mathf.RoundToInt(value);
            int currentCount = this.playerManager?.PlayerCount ?? 0;

            // Add or remove players to match target
            while (currentCount < targetCount && currentCount < (this.playerManager?.maxPlayers ?? 4))
            {
                this.AddPlayer();
                currentCount++;
            }

            while (currentCount > targetCount && currentCount > 1)
            {
                this.RemovePlayer();
                currentCount--;
            }
        }

        private void OnTurnBasedToggled(bool isTurnBased)
        {
            if (this.playerManager != null)
            {
                this.playerManager.SetGameMode(isTurnBased);
            }
        }

        #endregion

        #region Display Updates

        private void UpdatePlayerDisplay()
        {
            // Update player count text
            if (this.playerCountText != null && this.playerManager != null)
            {
                this.playerCountText.text = $"Players: {this.playerManager.PlayerCount}";
            }

            // Update slider without triggering callback
            if (this.playerCountSlider != null && this.playerManager != null)
            {
                this.playerCountSlider.SetValueWithoutNotify(this.playerManager.PlayerCount);
            }

            // Update player list
            this.UpdatePlayerList();

            // Update start button availability
            if (this.startGameButton != null && this.playerManager != null)
            {
                this.startGameButton.interactable = this.playerManager.PlayerCount >= this.playerManager.minPlayersToStart;
            }
        }

        private void UpdatePlayerList()
        {
            if (this.playerListParent == null || this.playerManager == null) return;

            // Clear existing items
            foreach (Transform child in this.playerListParent)
            {
                Destroy(child.gameObject);
            }

            // Create new items
            foreach (var player in this.playerManager.Players)
            {
                this.CreatePlayerListItem(player);
            }
        }

        private void CreatePlayerListItem(PlayerController player)
        {
            GameObject item;

            if (this.playerListItemPrefab != null)
            {
                item = Instantiate(this.playerListItemPrefab, this.playerListParent);
            }
            else
            {
                // Create a simple text item
                item = new GameObject($"Player_{player.ID}");
                item.transform.SetParent(this.playerListParent, false);
                var text = item.AddComponent<TMP_Text>();
                text.text = $"{player.Name} (ID: {player.ID})";
                text.color = player.Color;
            }

            // Try to set up the player list item with customization data
            var listItem = item.GetComponent<PlayerListItem>();
            if (listItem != null)
            {
                listItem.SetupPlayer(player, this.customizationSystem?.GetPlayerCustomization(player.ID));
            }
        }

        #endregion

        #region Menu Control

        public void ShowMenu()
        {
            if (this.menuPanel != null)
                this.menuPanel.SetActive(true);
        }

        public void HideMenu()
        {
            if (this.menuPanel != null)
                this.menuPanel.SetActive(false);
        }

        #endregion
    }

    // Helper component for player list items
    public class PlayerListItem : MonoBehaviour
    {
        [Header("UI Components")]
        public TMP_Text playerNameText;
        public Image playerColorImage;
        public Image playerSymbolImage;
        public TMP_Text playerIDText;

        public void SetupPlayer(PlayerController player, PlayerCustomizationData customization)
        {
            if (this.playerNameText != null)
                this.playerNameText.text = player.Name;

            if (this.playerColorImage != null)
                this.playerColorImage.color = player.Color;

            if (this.playerSymbolImage != null && player.Symbol != null)
            {
                this.playerSymbolImage.sprite = player.Symbol;
                this.playerSymbolImage.enabled = true;
            }
            else if (this.playerSymbolImage != null)
            {
                this.playerSymbolImage.enabled = false;
            }

            if (this.playerIDText != null)
                this.playerIDText.text = $"P{player.ID + 1}";
        }
    }
}