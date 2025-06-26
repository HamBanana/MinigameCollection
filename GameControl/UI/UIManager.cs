using System;
using System.Reflection;
using System.Collections;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Ham.GameControl;
using Ham.GameControl.UI;
using Ham.GameControl.SceneTransition;

using Unity.VisualScripting;
using Ham.UI.Elements;
using Ham.GameControl.Player;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Ham.GameControl.UI
{

    public class UIManager : Manager
    {
        private GameManager game;
        private SceneTransitionManager sceneTransition;
        private PlayerManager playerManager;

        public event Action OnUIEvent;

        public event Action OnOverlayToggled;
        public event EventHandler OnSceneRestartRequested;
        public event EventHandler OnGameQuitRequested;


        [SerializeField]
        private GameObject uiControllerPrefab;
        private GameObject waitForInputPrefab;
        protected UIController uiController;

        private Canvas mainCanvas;
        
        [SerializeField]
        private GameObject notificationPanelPrefab;

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("UIManager: Awake()");
            if (this.uiControllerPrefab != null)
            {
                Debug.Log("UIManager: Instantiating UIController");
                GameObject uiGo = Instantiate(this.uiControllerPrefab);
                this.uiController = uiGo.GetComponent<UIController>();
            }
            else
            {
                Debug.LogError("UIManager: UIController prefab not found.");
            }
            /*}*/

            this.mainCanvas = this.uiController.GetCanvas();


        }

        protected override void Start()
        {
            Debug.Log("UIManager: Start()");
            // Connect to hub
            this.game = Manager.Get<GameManager>();
            this.sceneTransition = Manager.Get<SceneTransitionManager>();
            this.playerManager = Manager.Get<PlayerManager>();

            this.sceneTransition.OnSceneLoaded += (object sender, SceneLoadedEventArgs e) =>
            {
                Debug.Log("UIManager: OnSceneLoaded received");
                this.uiController.SetOverlay(false);
            };

            this.game.OnGameEnded += (GameManager manager, GameEndedEventArgs e) =>
            {
                Debug.Log("UIManager: OnGameEnded received: args: " + e);
                this.uiController.SetOverlay(true);

                // If game is a draw.
                this.uiController.ShowWinScreen(e);
            };

            this.sceneTransition.OnSceneLoadEnd += (object sender, SceneLoadedEventArgs e) =>
            {
            };

            this.game.OnGameStarted += (GameManager manager, GameStartedEventArgs e) =>
            {
                Debug.Log("UIManager: OnGameStarted received");
                this.uiController.SetOverlay(false);
            };

            this.game.OnTurnStarted += (GameManager manager, TurnStartedEventArgs e) =>
            {
                this.uiController.SetOverlay(false);
            };
            
            /*this.playerManager.OnWaitForPairingStarted += (sender, e) => {
                this.Notify("To pair with " + e.GameObjectToAttachToDevice.GetComponent<PlayerController>().PlayerName + ", press button on input device.");
            };*/



            this.uiController.OnRequestSceneRestart += (object sender, EventArgs e) =>
            {
                Debug.Log("UIManager: Requesting scene restart.");
                this.sceneTransition.RestartScene();
            };
            this.uiController.OnRequestGameQuit += (object sender, EventArgs e) => { this.sceneTransition.QuitGame(); };
            this.uiController.OnRequestSceneQuit += (object sender, EventArgs e) => { this.sceneTransition.QuitScene(); };

            this.onControllerAction += (sender, e) => {
                this.Notify("UIManager: " + e.TestString);
                Debug.Log("Controller gameobject name: " + e.ControllerGameObject.name);
            };
        }

        public void Notify(string message)
        {
            GameObject notifGo = Instantiate(this.notificationPanelPrefab, this.mainCanvas.transform);
            RectTransform rt = notifGo.GetComponent<RectTransform>();

            float offScreenY = Screen.height * 0.5f;
            float onScreenY = -50f;

            rt.anchoredPosition = new Vector2(0, offScreenY);

            notifGo.GetComponent<NotificationController>()?.SetText(message);

            StartCoroutine(this.showNotification(rt, onScreenY, offScreenY, 0.5f, 3f));
        }

        [ContextMenu("Test notification")]
        public void TestNotification(){
            this.Notify("TESTtest");
        }

        private IEnumerator showNotification(RectTransform rt, float enterY, float exitY, float duration, float delay)
        {
            // Slide in
            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = new Vector2(startPos.x, enterY);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, this.EaseOutCubic(t));
                yield return null;
            }

            rt.anchoredPosition = endPos;

            yield return new WaitForSeconds(delay);

            // Slide out
            t = 0f;
            Vector2 exitPos = new Vector2(startPos.x, exitY);
            while (t < 1f){
                t += Time.deltaTime / duration;
                rt.anchoredPosition = Vector2.Lerp(endPos, exitPos, this.EaseInCubic(t));
                yield return null;
            }

            rt.anchoredPosition = exitPos;

            Destroy(rt.gameObject);


        }

        private float EaseOutCubic(float t)
        {
            return 1 - Mathf.Pow(1 - t, 3);
        }

        private float EaseInCubic(float t)
        {
            return t * t * t;
        }
    }
}
