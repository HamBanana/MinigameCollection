using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

using Ham.Board;
using Ham.GameControl;

using System;
using Ham.GameControl.UI;
using Ham.UI;
using Unity.VisualScripting;

namespace Ham.Board
{
    public class BoardGameManager : GameManager
    {
        public event EventHandler<CellSelectedEventArgs> OnCellSelected;

        public Board.BoardController Board; // Reference to the board

        protected override void Awake(){
            base.Awake();
        }

        protected override void Start()
        {
            this.Board.OnCellSelected += (object sender, CellSelectedEventArgs e) 
            => {this.OnCellSelected?.Invoke(this, e);};
        }

        protected void InvokeCellSelected(CellSelectedEventArgs e){
            this.OnCellSelected?.Invoke(this, e);
        }
    }
}
