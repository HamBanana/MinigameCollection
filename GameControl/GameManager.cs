using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Events;

using Ham.GameControl;
using Ham.GameControl.Player;

using System;
using System.Collections.Generic;

public enum GameState {
    Paused, Playing, Stopped
}


namespace Ham.GameControl
{

    public class GameManager : Manager
    {

        public event Action OnGameInputDisabled;
        public event Action OnGameInputEnabled;

        public event Action<GameManager, GameEndedEventArgs> OnGameEnded;
        public event Action<GameManager, GameStartedEventArgs> OnGameStarted;

        public event Action<GameManager, TurnStartedEventArgs> OnTurnStarted;

        public GameState CurrentGameState = GameState.Stopped;

        protected bool processUpdate = true;



        [SerializeField]
        private GameObject closeButton;

        protected override void Awake()
        {
            base.Awake();
            UnityEngine.Debug.Log("GameManager: Awake()");
        }

        protected override void Start()
        {
            base.Start();
        }

        protected void EndGame(GameEndedEventArgs args){
            UnityEngine.Debug.Log("GameManager: args.winner: " + args.winner);
            Debug.Log("GameManager: this.OnGameEnded:" + this.OnGameEnded);
            if (this.OnGameEnded == null){
                Debug.LogError("GameManager: OnGameEnded is null, for some reason.");
                return;
            }
            this.CurrentGameState = GameState.Stopped;
            this.OnGameEnded?.Invoke(this, args);
            this.SetInputActive(false);
        }

        protected void StartGame(GameStartedEventArgs args){
            this.CurrentGameState = GameState.Playing;
            this.OnGameStarted?.Invoke(this, args);
            this.SetInputActive(true);
        }

        protected void StartTurn(TurnStartedEventArgs args){
            this.OnTurnStarted?.Invoke(this, args);
        }

        protected void SetInputActive(bool active){
            this.processUpdate = active;
            if (active){
                this.OnGameInputEnabled?.Invoke();
            } else {
                this.OnGameInputDisabled?.Invoke();
            }
        }

    }

    // EventArgs
    public class GameEndedEventArgs : EventArgs
    {
        public IReadOnlyList<PlayerController> players;
        public PlayerController winner;
        public bool IsDraw = false;
    }

    public class GameStartedEventArgs : EventArgs
    {
        public PlayerController[] players;
    }

    public class TurnStartedEventArgs : EventArgs {
        public IReadOnlyList<PlayerController> players;
        public PlayerController newPlayer;
        public PlayerController prevPlayer;
    }

    public class SelectionEventArgs : EventArgs {
        public GameObject selectedGameObject;
    }
}