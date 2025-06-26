using UnityEngine;

using Ham.GameControl.Sound;
using Ham.GameControl.SceneTransition;
using Ham.GameControl;

namespace Ham.Board
{
    class BoardGameSoundManager : SoundManager
    {
        private BoardGameManager game;
        private SceneTransitionManager sceneTransition;

        protected override void Awake()
        {
            base.Awake();   
        }
        protected override void Start()
        {
            base.Start();
            this.game = Manager.Get<MainMenuGameManager>();
            Debug.Log("BoardGameSoundManager: this.game: " + this.game);
            this.sceneTransition = Manager.Get<SceneTransitionManager>();
            Debug.Log("BoardGameSoundManager: this.sceneTransition: " + this.sceneTransition);
            this.game.OnCellSelected += (object sender, CellSelectedEventArgs e)
            =>
            {
                this.SfxSource?.PlayOneShot((e.IsSelectionValid) ? this.ValidActionSfx : this.InvalidActionSfx);
            };


            this.sceneTransition.OnSceneLoaded +=
            (object sender, SceneLoadedEventArgs e) =>
            {
                this.PlayMusic("forest");
            };
        }
    }
}