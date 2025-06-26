using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using Ham.TicTacToe;
using Ham.GameControl;

using System;
using System.Diagnostics;

using Ham.GameControl.UI;
using Ham.UI;


namespace Ham.GameControl.SceneTransition
{

    public class SceneTransitionManager : Manager
    {
        public event EventHandler<SceneLoadedEventArgs> OnSceneLoaded;
        public event EventHandler<SceneEndedEventArgs> OnSceneEnded;

        public event EventHandler<SceneLoadedEventArgs> OnSceneLoadRequested;
        public event EventHandler<SceneLoadedEventArgs> OnSceneLoadBegin;
        public event EventHandler<SceneLoadedEventArgs> OnSceneLoadEnd;

        public event EventHandler OnSceneRestartRequested;
        public event EventHandler OnGameQuitRequested;

        public string CurrentSceneName;

        protected override void Awake()
        {
            base.Awake();
            this.CurrentSceneName = SceneManager.GetActiveScene().name;
        }

        protected override void Start()
        {
            base.Awake();
            this.OnSceneLoaded?.Invoke(this, new SceneLoadedEventArgs() { loadedSceneName = this.CurrentSceneName });
        }

        protected void CloseCurrentScene()
        {
            UnityEngine.Debug.Log("Closing scene...");
            if (SceneManager.GetActiveScene().name != "MainMenuScene")
            {
                UnityEngine.Debug.Log("Closing scene...");
                this.LoadScene("MainMenuScene");
            }
            else
            {
                UnityEngine.Debug.Log("Closing game...");
                Application.Quit();
            }
        }

        public void RestartScene()
        {
            UnityEngine.Debug.Log("Restarting scene");
            this.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadScene(string sceneName)
        {
            Manager.ClearRegistry();
            this.OnSceneEnded?.Invoke(this, new SceneEndedEventArgs() { endedSceneName = this.CurrentSceneName });
            this.OnSceneLoadBegin?.Invoke(this, new SceneLoadedEventArgs(){loadedSceneName = sceneName});
            SceneManager.LoadScene(sceneName);
            this.OnSceneLoadEnd?.Invoke(this, new SceneLoadedEventArgs(){loadedSceneName = sceneName});
        }

        public void QuitScene(){
            if (SceneManager.GetActiveScene().name == "MainMenuScene"){
                this.QuitGame();
            } else {
                this.LoadScene("MainMenuScene");
            }
        }

        public void QuitGame()
        {
            this.OnGameQuitRequested?.Invoke(this, EventArgs.Empty);
        }

    }

    public class SceneTransitionEventArgs : EventArgs
    {
        public string newSceneName;
    }

    public class SceneLoadedEventArgs : EventArgs
    {
        public string loadedSceneName;
    }

    public class SceneEndedEventArgs : EventArgs
    {
        public string endedSceneName;
    }
}