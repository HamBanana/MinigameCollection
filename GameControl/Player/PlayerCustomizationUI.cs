using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ham.GameControl.Player
{

    public class PlayerCustomizationUI : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject customizationPanel;
        public GameObject playerPreviewPanel;

        [Header("Player Info")]
        public TMP_Text playerNumberText;
        public TMP_InputField nameInputField;

        [Header("Color Selection")]
        public Button colorLeftButton;
        public Button colorRightButton;
        public Image colorPreview;
        public TMP_Text colorIndexText;

        [Header("Symbol Selection")]
        public Button symbolLeftButton;
        public Button symbolRightButton;
        public Image symbolPreview;
        public TMP_Text symbolIndexText;

        [Header("Model Selection")]
        public Button modelLeftButton;
        public Button modelRightButton;
        public Image modelPreview; // Or could be a 3D preview
        public TMP_Text modelNameText;

        [Header("Navigation")]
        public Button previousPlayerButton;
        public Button nextPlayerButton;
        public Button finishButton;

        [Header("Preview")]
        public PlayerPreview playerPreview;

        [Header("Settings")]
        public bool allowDuplicateColors = false;
        public bool allowDuplicateSymbols = false;

        // References
        private PlayerCustomizationSystem customizationSystem;
        private int currentPlayerID = 0;
        private PlayerCustomizationData currentCustomization;

        private void Awake()
        {
            this.SetupButtonListeners();
        }

        private void Start()
        {
            this.customizationSystem = Manager.Get<PlayerCustomizationSystem>();
            this.HideCustomization();
        }

        private void SetupButtonListeners()
        {
            // Color buttons
            if (this.colorLeftButton != null)
                this.colorLeftButton.onClick.AddListener(() => this.CycleColor(false));
            if (this.colorRightButton != null)
                this.colorRightButton.onClick.AddListener(() => this.CycleColor(true));

            // Symbol buttons
            if (this.symbolLeftButton != null)
                this.symbolLeftButton.onClick.AddListener(() => this.CycleSymbol(false));
            if (this.symbolRightButton != null)
                this.symbolRightButton.onClick.AddListener(() => this.CycleSymbol(true));

            // Model buttons
            if (this.modelLeftButton != null)
                this.modelLeftButton.onClick.AddListener(() => this.CycleModel(false));
            if (this.modelRightButton != null)
                this.modelRightButton.onClick.AddListener(() => this.CycleModel(true));

            // Navigation buttons
            if (this.previousPlayerButton != null)
                this.previousPlayerButton.onClick.AddListener(this.PreviousPlayer);
            if (this.nextPlayerButton != null)
                this.nextPlayerButton.onClick.AddListener(this.NextPlayer);
            if (this.finishButton != null)
                this.finishButton.onClick.AddListener(this.FinishCustomization);

            // Name input
            if (this.nameInputField != null)
                this.nameInputField.onEndEdit.AddListener(this.OnNameChanged);
        }

        #region Public Interface

        public void ShowCustomization()
        {
            if (this.customizationPanel != null)
                this.customizationPanel.SetActive(true);
        }

        public void HideCustomization()
        {
            if (this.customizationPanel != null)
                this.customizationPanel.SetActive(false);
        }

        public void LoadPlayerCustomization(int playerID, PlayerCustomizationData customization)
        {
            this.currentPlayerID = playerID;
            this.currentCustomization = customization;

            this.UpdateUI();
            this.UpdatePreview();
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            // Player number
            if (this.playerNumberText != null)
                this.playerNumberText.text = $"Player {this.currentPlayerID + 1}";

            // Name
            if (this.nameInputField != null)
                this.nameInputField.text = this.currentCustomization.playerName;

            // Color
            this.UpdateColorUI();

            // Symbol
            this.UpdateSymbolUI();

            // Model
            this.UpdateModelUI();

            // Navigation buttons
            this.UpdateNavigationButtons();
        }

        private void UpdateColorUI()
        {
            if (this.colorPreview != null)
                this.colorPreview.color = this.currentCustomization.playerColor;

            if (this.colorIndexText != null)
                this.colorIndexText.text = $"{this.currentCustomization.selectedColorIndex + 1}/{this.customizationSystem.availableColors.Length}";

            // Update button interactability based on duplicate settings
            if (!this.allowDuplicateColors)
            {
                this.UpdateColorButtonStates();
            }
        }

        private void UpdateSymbolUI()
        {
            if (this.symbolPreview != null && this.currentCustomization.playerSymbol != null)
                this.symbolPreview.sprite = this.currentCustomization.playerSymbol;

            if (this.symbolIndexText != null)
                this.symbolIndexText.text = $"{this.currentCustomization.selectedSymbolIndex + 1}/{this.customizationSystem.availableSymbols.Length}";

            // Update button interactability based on duplicate settings
            if (!this.allowDuplicateSymbols)
            {
                this.UpdateSymbolButtonStates();
            }
        }

        private void UpdateModelUI()
        {
            if (this.modelNameText != null)
            {
                string modelName = this.currentCustomization.playerModel != null ?
                    this.currentCustomization.playerModel.name : "None";
                this.modelNameText.text = modelName;
            }

            // Update model preview if you have one
            if (this.modelPreview != null)
            {
                // Could show a 2D representation of the 3D model
                // Or take a screenshot of the model
            }
        }

        private void UpdateNavigationButtons()
        {
            if (this.previousPlayerButton != null)
                this.previousPlayerButton.interactable = this.currentPlayerID > 0;

            // Update finish button text based on whether this is the last player
            if (this.finishButton != null)
            {
                var buttonText = this.finishButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    bool isLastPlayer = this.currentPlayerID >= this.customizationSystem.availableNames.Length - 1;
                    buttonText.text = isLastPlayer ? "Finish" : "Next Player";
                }
            }
        }

        private void UpdateColorButtonStates()
        {
            // Check if left/right colors are available
            int leftIndex = (this.currentCustomization.selectedColorIndex - 1 + this.customizationSystem.availableColors.Length) % this.customizationSystem.availableColors.Length;
            int rightIndex = (this.currentCustomization.selectedColorIndex + 1) % this.customizationSystem.availableColors.Length;

            if (this.colorLeftButton != null)
                this.colorLeftButton.interactable = !this.customizationSystem.IsColorTaken(leftIndex, this.currentPlayerID);

            if (this.colorRightButton != null)
                this.colorRightButton.interactable = !this.customizationSystem.IsColorTaken(rightIndex, this.currentPlayerID);
        }

        private void UpdateSymbolButtonStates()
        {
            // Similar logic for symbols
            int leftIndex = (this.currentCustomization.selectedSymbolIndex - 1 + this.customizationSystem.availableSymbols.Length) % this.customizationSystem.availableSymbols.Length;
            int rightIndex = (this.currentCustomization.selectedSymbolIndex + 1) % this.customizationSystem.availableSymbols.Length;

            if (this.symbolLeftButton != null)
                this.symbolLeftButton.interactable = !this.customizationSystem.IsSymbolTaken(leftIndex, this.currentPlayerID);

            if (this.symbolRightButton != null)
                this.symbolRightButton.interactable = !this.customizationSystem.IsSymbolTaken(rightIndex, this.currentPlayerID);
        }

        private void UpdatePreview()
        {
            if (this.playerPreview != null)
            {
                this.playerPreview.UpdatePreview(this.currentCustomization);
            }
        }

        #endregion

        #region Button Callbacks

        private void CycleColor(bool forward)
        {
            this.customizationSystem.CyclePlayerColor(this.currentPlayerID, forward);
            this.currentCustomization = this.customizationSystem.GetPlayerCustomization(this.currentPlayerID);
            this.UpdateColorUI();
            this.UpdatePreview();
        }

        private void CycleSymbol(bool forward)
        {
            this.customizationSystem.CyclePlayerSymbol(this.currentPlayerID, forward);
            this.currentCustomization = this.customizationSystem.GetPlayerCustomization(this.currentPlayerID);
            this.UpdateSymbolUI();
            this.UpdatePreview();
        }

        private void CycleModel(bool forward)
        {
            this.customizationSystem.CyclePlayerModel(this.currentPlayerID, forward);
            this.currentCustomization = this.customizationSystem.GetPlayerCustomization(this.currentPlayerID);
            this.UpdateModelUI();
            this.UpdatePreview();
        }

        private void OnNameChanged(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                newName = $"Player {this.currentPlayerID + 1}";
                this.nameInputField.text = newName;
            }

            this.customizationSystem.SetPlayerName(this.currentPlayerID, newName);
            this.currentCustomization = this.customizationSystem.GetPlayerCustomization(this.currentPlayerID);
        }

        private void PreviousPlayer()
        {
            this.customizationSystem.PreviousPlayer();
        }

        private void NextPlayer()
        {
            this.customizationSystem.NextPlayer();
        }

        private void FinishCustomization()
        {
            this.customizationSystem.EndCustomization();
        }

        #endregion
    }

    // Helper component for 3D player preview
    public class PlayerPreview : MonoBehaviour
    {
        [Header("Preview Settings")]
        public Transform modelParent;
        public Camera previewCamera;
        public Light previewLight;

        private GameObject currentModelInstance;

        public void UpdatePreview(PlayerCustomizationData customization)
        {
            // Destroy old model
            if (this.currentModelInstance != null)
            {
                DestroyImmediate(this.currentModelInstance);
            }

            // Instantiate new model
            if (customization.playerModel != null && this.modelParent != null)
            {
                this.currentModelInstance = Instantiate(customization.playerModel, this.modelParent);

                // Apply color to model (if it has a renderer)
                var renderers = this.currentModelInstance.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    // You might want to only color specific materials
                    foreach (var material in renderer.materials)
                    {
                        if (material.HasProperty("_Color"))
                        {
                            material.color = customization.playerColor;
                        }
                    }
                }
            }
        }
    }
}