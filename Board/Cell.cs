using UnityEngine;

using UnityEngine.UIElements;

using System;
using System.Collections.Generic;

using Ham.GameControl;
using Ham.GameControl.Player;
using Ham.TicTacToe;
using TMPro;

namespace Ham.Board
{
    public class Cell : MonoBehaviour
    {
        public Dictionary<string, Cell> neighbours = new Dictionary<string, Cell>();


        public event EventHandler<CellSelectedEventArgs> OnSelected;
        public event EventHandler<CellSelectedInvalidEventArgs> OnSelectedInvalid;

        public string Text = "Halløj";

        /*[field: SerializeField]public string Text { get { return Text; } set {
            Text = value; 
            } }*/


        private void Awake()
        {
            this.neighbours.Add("north", null);
            this.neighbours.Add("south", null);
            this.neighbours.Add("east", null);
            this.neighbours.Add("west", null);
            //this.playerSpriteRenderer = this.transform.GetComponentInChildren<SpriteRenderer>();
        }

        public Cell GetRelative(Vector2 targetcell)
        {
            Debug.Log("Cell.GetRelative on: " + this.name);
            if (targetcell == Vector2.zero)
            {
                Debug.Log("Cell.GetRelative found: " + this.name);
                return this;
            }
            if (targetcell.x > 0) { return this.neighbours["east"]?.GetRelative(new Vector2(targetcell.x - 1, targetcell.y)); }
            else if (targetcell.x < 0) { return this.neighbours["west"]?.GetRelative(new Vector2(targetcell.x + 1, targetcell.y)); }
            else if (targetcell.y > 0) { return this.neighbours["north"]?.GetRelative(new Vector2(targetcell.x, targetcell.y - 1)); }
            else if (targetcell.y < 0) { return this.neighbours["south"]?.GetRelative(new Vector2(targetcell.x, targetcell.y + 1)); }
            else
            {
                Debug.Log("Error while getting cell relative to: " + this.name);
                return null;
            }

        }

        /*public virtual void OnMouseDown()
        {
            Debug.Log("Cell: " + this.gameObject.name);
            this.OnSelected?.Invoke(this, new CellSelectedEventArgs() { cell = this });
        }*/
    }
    public class CellSelectedEventArgs : EventArgs
    {
        public Cell cell;
        public PlayerController player;
        public bool IsSelectionValid = true;
    }
    public class CellSelectedInvalidEventArgs : EventArgs
    {
        public Cell cell;
        public string Reason;
    }

    public class CellHoveredEventArgs : EventArgs
    {
        public Cell cell;
    }

    public class CellUnhoveredEventArgs : EventArgs
    {
        public Cell cell;
    }
}
