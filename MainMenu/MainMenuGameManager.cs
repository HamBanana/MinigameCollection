using UnityEngine;

using Ham.GameControl;
using Ham.Board;

using System;
using Ham.GameControl.SceneTransition;

public class MainMenuGameManager : BoardGameManager
{
    private SceneTransitionManager sceneTransition;
    protected override void Awake()
    {
        base.Awake();
        this.sceneTransition = Manager.Get<SceneTransitionManager>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        this.Board.OnCellSelected += (object sender, CellSelectedEventArgs e) =>
        {
            if (e.cell.Text != null && e.cell.Text != "" && e.cell.Text != "Halløj")
            {
                e.IsSelectionValid = true;
                this.InvokeCellSelected(e);
                this.sceneTransition.LoadScene(e.cell.Text + "Scene");
            }
            else
            {
                e.IsSelectionValid = false;
                this.InvokeCellSelected(e);
            }
        };

        // Setting Cell labels.
        DefaultCell cell = (DefaultCell)this.Board.GetCell(0, 2);
        cell.SetText("TicTacToe");
        //cell.AddController<>

    }
}
