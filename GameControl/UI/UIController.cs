using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

using System.Diagnostics;
using System;

using Ham.GameControl.Player;
using Ham.GameControl;
using Ham.UI.Elements;

namespace Ham.GameControl.UI
{

    public class UIController : MonoBehaviour
    {

        public event EventHandler OnRequestSceneRestart;
        public event EventHandler OnRequestGameQuit;
        public event EventHandler OnRequestSceneQuit;

        private OverlayController overlay;

        private Button restartButton;

        private Canvas canvas;

        [SerializeField]
        private GameObject winnerLabelPrefab;
        private WinnerLabel winnerLabel;
        
        void Awake()
        {
            this.canvas = this.GetComponentInChildren<Canvas>();
            this.overlay = this.GetComponentInChildren<OverlayController>();
            this.winnerLabel = this.GetComponentInChildren<WinnerLabel>();
            this.overlay.Hide();
        }

        void Start()
        {
        }

        void OnDestroy()
        {
            Destroy(this.winnerLabel.gameObject);
        }

        public void RequestSceneRestart(){
            this.OnRequestSceneRestart?.Invoke(this, EventArgs.Empty);
        }
        public void RequestGameQuit(){
            this.OnRequestGameQuit?.Invoke(this, EventArgs.Empty);
        }
        public void RequestSceneQuit(){
            this.OnRequestSceneQuit?.Invoke(this, EventArgs.Empty);
        }

        public void ShowWinScreen(GameEndedEventArgs e)
        {
            UnityEngine.Debug.Log($"UIController: ShowWinScreen EventArgs: \n IsDraw: {e.IsDraw}\nWinner name: {e.winner?.name}");
            //this.winnerLabel = Instantiate(this.winnerLabelPrefab).GetComponent<WinnerLabel>();
            //this.winnerLabel.transform.SetParent(this.canvas.transform);
            if (e.winner == null || e.IsDraw)
            {
                this.winnerLabel.SetImageActive(false);
                this.winnerLabel.SetText("Draw");
            }
            else
            {
                this.winnerLabel.SetImageActive(true);
                this.winnerLabel.SetText("Winner: ");
                this.winnerLabel.SetSprite(e.winner.PlayerSymbol);
            }
            this.overlay.Show();
            this.winnerLabel.Show();
        }

        public void SetOverlay(bool active)
        {
            UnityEngine.Debug.Log("UIController: SetOverlay: " + active + ": " + this.overlay);
            //(active) ? this.overlay.Show() : this.overlay.Hide();
            if (active){
                this.overlay.Show();
            } else {
                this.overlay.Hide();
            }
        }

        public Canvas GetCanvas(){
            return this.canvas;
        }
    }
}
