using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ham.GameControl.Player
{
    public class PlayerVisuals : MonoBehaviour
    {
        [Header("3D Model")]
        public Transform modelParent;
        private GameObject currentModelInstance;

        [Header("UI Elements")]
        public Image playerColorImage;
        public Image playerSymbolImage;
        public TMP_Text playerNameText;
        public TMP_Text playerIDText;

        [Header("Material Overrides")]
        public Material playerColorMaterial; // Material that will be colored
        public string colorPropertyName = "_Color";

        [Header("Settings")]
        public bool autoApplyCustomization = true;

        private PlayerController playerController;
        private PlayerCustomizationSystem customizationSystem;

        private void Start()
        {
            this.playerController = this.GetComponent<PlayerController>();
            this.customizationSystem = Manager.Get<PlayerCustomizationSystem>();

            if (this.autoApplyCustomization && this.playerController != null)
            {
                this.ApplyCustomization();
            }

            // Subscribe to customization changes
            if (this.customizationSystem != null)
            {
                this.customizationSystem.OnPlayerCustomizationChanged += this.OnCustomizationChanged;
            }
        }

        private void OnDestroy()
        {
            if (this.customizationSystem != null)
            {
                this.customizationSystem.OnPlayerCustomizationChanged -= this.OnCustomizationChanged;
            }
        }

        private void OnCustomizationChanged(object sender, PlayerCustomizationEventArgs e)
        {
            // Update visuals if this is our player
            if (this.playerController != null && e.PlayerID == this.playerController.ID)
            {
                this.ApplyCustomization();
            }
        }

        #region Public Methods

        public void ApplyCustomization()
        {
            if (this.playerController == null || this.customizationSystem == null) return;

            var customization = this.customizationSystem.GetPlayerCustomization(this.playerController.ID);
            this.ApplyCustomization(customization);
        }

        public void ApplyCustomization(PlayerCustomizationData customization)
        {
            // Apply name
            this.SetPlayerName(customization.playerName);

            // Apply color
            this.SetPlayerColor(customization.playerColor);

            // Apply symbol
            this.SetPlayerSymbol(customization.playerSymbol);

            // Apply model
            this.SetModel(customization.playerModel);

            // Update ID display
            if (this.playerController != null)
            {
                this.SetPlayerID(this.playerController.ID);
            }
        }

        public void SetPlayerName(string name)
        {
            if (this.playerNameText != null)
            {
                this.playerNameText.text = name;
            }
        }

        public void SetPlayerColor(Color color)
        {
            // Update UI color indicator
            if (this.playerColorImage != null)
            {
                this.playerColorImage.color = color;
            }

            // Update 3D model materials
            this.ApplyColorToModel(color);
        }

        public void SetPlayerSymbol(Sprite symbol)
        {
            if (this.playerSymbolImage != null)
            {
                this.playerSymbolImage.sprite = symbol;
                this.playerSymbolImage.enabled = symbol != null;
            }
        }

        public void SetPlayerID(int id)
        {
            if (this.playerIDText != null)
            {
                this.playerIDText.text = $"P{id + 1}";
            }
        }

        public void SetModel(GameObject modelPrefab)
        {
            // Destroy existing model
            if (this.currentModelInstance != null)
            {
                DestroyImmediate(this.currentModelInstance);
                this.currentModelInstance = null;
            }

            // Instantiate new model
            if (modelPrefab != null && this.modelParent != null)
            {
                this.currentModelInstance = Instantiate(modelPrefab, this.modelParent);

                // Apply current player color to the new model
                if (this.playerController != null && this.customizationSystem != null)
                {
                    var customization = this.customizationSystem.GetPlayerCustomization(this.playerController.ID);
                    this.ApplyColorToModel(customization.playerColor);
                }
            }
        }

        #endregion

        #region Private Methods

        private void ApplyColorToModel(Color color)
        {
            if (this.currentModelInstance == null) return;

            // Method 1: Apply to specific material if provided
            if (this.playerColorMaterial != null)
            {
                this.playerColorMaterial.SetColor(this.colorPropertyName, color);
            }

            // Method 2: Apply to all renderers (more general approach)
            var renderers = this.currentModelInstance.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                // Create material instances to avoid affecting other objects
                var materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].HasProperty(this.colorPropertyName))
                    {
                        // Create a new material instance
                        materials[i] = new Material(materials[i]);
                        materials[i].SetColor(this.colorPropertyName, color);
                    }
                }
                renderer.materials = materials;
            }

            // Method 3: Look for specific components that handle player coloring
            var colorableComponents = this.currentModelInstance.GetComponentsInChildren<IPlayerColorable>();
            foreach (var colorable in colorableComponents)
            {
                colorable.SetPlayerColor(color);
            }
        }

        #endregion
    }

    // Interface for components that can be colored by player
    public interface IPlayerColorable
    {
        void SetPlayerColor(Color color);
    }

    // Example implementation for a simple colorable component
    public class SimplePlayerColorable : MonoBehaviour, IPlayerColorable
    {
        [Header("Colorable Settings")]
        public Renderer targetRenderer;
        public string materialColorProperty = "_Color";
        public int materialIndex = 0;

        private Material originalMaterial;
        private Material instanceMaterial;

        private void Awake()
        {
            if (this.targetRenderer == null)
                this.targetRenderer = this.GetComponent<Renderer>();

            // Store original material
            if (this.targetRenderer != null && this.targetRenderer.materials.Length > this.materialIndex)
            {
                this.originalMaterial = this.targetRenderer.materials[this.materialIndex];
            }
        }

        public void SetPlayerColor(Color color)
        {
            if (this.targetRenderer == null || this.originalMaterial == null) return;

            // Create material instance if we don't have one
            if (this.instanceMaterial == null)
            {
                this.instanceMaterial = new Material(this.originalMaterial);

                // Replace the material in the renderer
                var materials = this.targetRenderer.materials;
                materials[this.materialIndex] = this.instanceMaterial;
                this.targetRenderer.materials = materials;
            }

            // Apply the color
            if (this.instanceMaterial.HasProperty(this.materialColorProperty))
            {
                this.instanceMaterial.SetColor(this.materialColorProperty, color);
            }
        }

        private void OnDestroy()
        {
            // Clean up the material instance
            if (this.instanceMaterial != null)
            {
                DestroyImmediate(this.instanceMaterial);
            }
        }
    }
}