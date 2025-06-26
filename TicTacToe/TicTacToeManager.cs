using UnityEngine;

using Ham.Board;
using Ham.GameControl;
using Ham.GameControl.SceneTransition;
using Ham.GameControl.Input;
using Ham.GameControl.UI;
using Ham.GameControl.Player;

using System;
using System.Collections;


namespace Ham.TicTacToe
{
    [DefaultExecutionOrder(-99)]
    public class TicTacToeManager : BoardGameManager
    {

        public int CurrentPlayerIndex = 0; // The current player
        public PlayerController CurrentPlayer; // The current player

        private SceneTransitionManager sceneTransitionManager = Manager.Get<SceneTransitionManager>();
        private PlayerManager playerManager = Manager.Get<PlayerManager>();
        private InputManager inputManager = Manager.Get<InputManager>();
        private UIManager ui = Manager.Get<UIManager>();

        [SerializeField]
        private Camera mainCamera;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            if (this.playerManager == null)
            {
                Debug.LogWarning("TicTacToeManager: Start: PlayerManager not ready, delaying...");
                StartCoroutine(this.WaitForPlayerManager());
                return;
            }
            this.mainCamera.enabled = true;

            // Subscribe to Board events
            this.Board.OnCellOwnerChanged += (object sender, CellOwnerChangedEventArgs e) =>
            {
                Debug.Log("TicTacToeManager: OnCellOwnerChanged");
                Debug.Log("TicTacToeManager: " + e.cell.name + "\n" + e.newOwner.name);
                if (this.CheckWinner()) { return; }
                this.NextPlayer();
            };

            this.Board.OnCellSelected += (object sender, CellSelectedEventArgs e) =>
            {
                Cell cell = e.cell;
                Ownable ownable = cell.GetComponent<Ownable>();
                if (!ownable || ownable == null){ return; }
                Debug.Log("TicTacToeManager: OnCellSelected");
                if (this.CurrentGameState != GameState.Playing)
                {
                    Debug.Log("TicTacToeManager: Cell selected, but game not in progress.");
                    return;
                }
                ownable.Owner = this.CurrentPlayer;
            };

            // Subscribing own events.

            this.OnTurnStarted += (GameManager sender, TurnStartedEventArgs e) => { };
            this.OnGameStarted += (GameManager sender, GameStartedEventArgs e) =>
            {
            };
            this.OnGameEnded += (GameManager sender, GameEndedEventArgs e) => { };

            if (this.playerManager != null)
            {
                // Subscribing to PlayerManager events.
                this.playerManager.OnPlayerJoined += (sender, e) =>
                {
                    Debug.Log("TicTacToeManager: Player joined!!!");
                };
            }
            else
            {
                Debug.LogWarning("TicTacToeManager: playerManager null, for some reason.");
            }

            if (this.inputManager != null)
            {
                this.inputManager.OnPointerPressed += (object sender, PointerEventArgs e) =>
                {

                };
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        protected override void OnSetup()
        {
            base.OnSetup();
            Debug.Log("TicTacToeManager: Resetting board");
            if (!this.Board)
            {
                Debug.Log("TicTacToeManager: Board needs to be set in TicTacToeManager.");
                return;
            }

            this.Board.Reset(); // Reset the board
        }

        private IEnumerator WaitForPlayerManager()
        {
            while ((this.playerManager = Manager.Get<PlayerManager>()) == null)
            {
                yield return null;
            }

            Debug.Log("TicTacToeManager: TicTacToePlayerManager found after delay");

            if (this.playerManager.Players.Count >= 2)
            {
                Debug.Log("TicTacToeManager: Importing Players from PlayerManager.");
                //this.Players[0] = this.playerManager.Players[0] as TicTacToePlayer;
                //this.Players[1] = this.playerManager.Players[1] as TicTacToePlayer;

                Debug.Log("TicTacToeManager: this.playerManager.Players[0]: " + this.playerManager.Players[0]);
                Debug.Log("TicTacToeManager: this.Players[0]: " + this.playerManager.Players[0]);

                Debug.Log("TicTacToeManager: Setting CurrentPlayerIndex.");
                this.CurrentPlayerIndex = 0;
                Debug.Log("TicTacToeManager: Setting CurrentPlayer.");
                this.CurrentPlayer = this.playerManager.Players[this.CurrentPlayerIndex];

                // Trigger start event
                this.StartGame(new GameStartedEventArgs());

            }
        }

        public void NextPlayer()
        {
            TurnStartedEventArgs e = new TurnStartedEventArgs() { prevPlayer = this.CurrentPlayer };
            this.CurrentPlayerIndex = (this.CurrentPlayerIndex == this.playerManager.Players.Count - 1) ? 0 : CurrentPlayerIndex + 1;
            this.CurrentPlayer = this.playerManager.Players[this.CurrentPlayerIndex];
            e.newPlayer = this.CurrentPlayer;
            e.players = this.playerManager.Players;
            this.StartTurn(e);
        }
        public bool CheckWinner()
        {
            Debug.Log("TicTacToeManager CheckWinner called");
            // Check for any winning condition first.
            for (int i = 0; i < this.Board.Width; i++)
            {
                for (int j = 0; j < this.Board.Height; j++)
                {
                    Cell cell = this.Board.Cells[i][j];
                    PlayerController owner = cell.GetComponent<Ownable>().Owner;

                    PlayerController northOwner = cell.neighbours["north"].GetComponent<Ownable>().Owner;
                    PlayerController northnorthOwner = cell.neighbours["north"].neighbours["north"].GetComponent<Ownable>().Owner;
                    PlayerController eastOwner = cell.neighbours["east"].GetComponent<Ownable>().Owner;
                    PlayerController easteastOwner = cell.neighbours["east"].neighbours["east"].GetComponent<Ownable>().Owner;
                    PlayerController neOwner = cell.neighbours["north"].neighbours["east"].GetComponent<Ownable>().Owner;
                    PlayerController neneOwner = cell.neighbours["north"].neighbours["east"].neighbours["north"].neighbours["east"].GetComponent<Ownable>().Owner;
                    PlayerController nwOwner = cell.neighbours["north"].neighbours["west"].GetComponent<Ownable>().Owner;
                    PlayerController nwnwOwner = cell.neighbours["north"].neighbours["west"].neighbours["north"].neighbours["west"].GetComponent<Ownable>().Owner;
                    if (owner == null)
                    {
                        continue;
                    }
                    // Check horizontal
                    if (i + 2 < this.Board.Width && owner == eastOwner && owner == easteastOwner)
                    {
                        Debug.Log("Winner: " + owner.PlayerName);
                        this.EndGame(new GameEndedEventArgs() { winner = CurrentPlayer, players = this.playerManager.Players });
                        return true;
                    }
                    // Check vertical
                    if (j + 2 < this.Board.Height && owner == northOwner && owner == northnorthOwner)
                    {
                        Debug.Log("Winner: " + owner.PlayerName);
                        this.EndGame(new GameEndedEventArgs() { winner = this.CurrentPlayer, players = this.playerManager.Players });
                        return true;
                    }
                    // Check diagonal
                    if (i + 2 < this.Board.Width && j + 2 < this.Board.Height && owner == neOwner && owner == neneOwner)
                    {
                        Debug.Log("Winner: " + owner.PlayerName);
                        this.EndGame(new GameEndedEventArgs() { winner = this.CurrentPlayer, players = this.playerManager.Players });
                        return true;
                    }
                    // Check anti-diagonal
                    if (i - 2 >= 0 && j + 2 < this.Board.Height && owner == nwOwner && owner == nwnwOwner)
                    {
                        Debug.Log("Winner: " + owner.PlayerName);
                        this.EndGame(new GameEndedEventArgs() { winner = this.CurrentPlayer, players = this.playerManager.Players });
                        return true;
                    }
                }
            }

            // No winner found, now check for a draw.
            bool boardFull = true;
            for (int i = 0; i < this.Board.Width; i++)
            {
                for (int j = 0; j < this.Board.Height; j++)
                {
                    Cell cell = this.Board.Cells[i][j];
                    PlayerController owner = cell.GetComponent<Ownable>().Owner;
                    if (owner == null)
                    {
                        boardFull = false;
                        break; // Exit inner loop early if an empty cell is found.
                    }
                }
                if (!boardFull)
                {
                    break; // Exit outer loop early if board is not full.
                }
            }

            if (boardFull)
            {
                Debug.Log("Draw!");
                this.EndGame(new GameEndedEventArgs() { winner = null, players = this.playerManager.Players, IsDraw = true });
                return true;
            }

            // Game is still in progress if no win and the board is not full.
            return false;
        }

        public void OnBoardReset()
        {
            Debug.Log("OnBoardReset called");
            this.mainCamera.transform.localPosition = new Vector3(this.Board.Width / 2, this.Board.Height / 2, -this.Board.Width);
        }

        public void GameStarted()
        {

        }

        public override void RegisterController(Controller controller)
        {
            base.RegisterController(controller);
            InputController input = controller as InputController;
        }
    }
}
