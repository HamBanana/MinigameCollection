using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Ham.GameControl;

namespace Ham.UI.Elements
{
    public class WinnerLabel : GameElement
    {
        private Sprite labelSprite;

        private Image winnerImage;
        private TextMeshProUGUI labelTMP;

        void Awake()
        {
            this.winnerImage = this.GetComponentInChildren<Image>();
            this.labelTMP = this.GetComponent<TextMeshProUGUI>();
        }

        public void SetText(string text)
        {
            this.labelTMP.text = text;
        }

        public void SetSprite(Sprite sprite)
        {
            Debug.Log("WinnerLabel: this.winnerImage: " + this.winnerImage);
            Debug.Log("WinnerLabel: this.winnerImage.sprite: " + this.winnerImage.sprite);
            Debug.Log("WinnerLabel: sprite: " + sprite);
            this.winnerImage.sprite = sprite;
        }

        public void SetSpriteEnabled(bool enabled)
        {
            this.gameObject.GetComponentInChildren<Image>().gameObject.SetActive(enabled);
        }

        public void SetImageActive(bool active){
            this.winnerImage.enabled = active;
        }

        public void RemoveImageComponent()
        {
            Image image = this.GetComponentInChildren<Image>();
            if (image != null)
            {
                Debug.Log("WinnerLabel: Destroying Image component.");
                Destroy(this.winnerImage.gameObject);
            }
        }

        public void AddImageComponent()
        {
            Image image = this.GetComponentInChildren<Image>();
            if (image == null)
            {
                this.winnerImage = this.gameObject.AddComponent<Image>();
                this.winnerImage.transform.SetParent(this.transform);
            }
        }

    }
}
