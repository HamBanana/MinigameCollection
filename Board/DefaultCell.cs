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
    public class DefaultCell : Cell {


        public PlayerController Owner; // The current owner of the cell

        [SerializeField]
        private SpriteRenderer playerSpriteRenderer; // Reference to the SpriteRenderer component


        public event EventHandler<CellOwnerChangedEventArgs> OnOwnerChanged;

        public void SetText(string text){
            this.GetComponentInChildren<TextMesh>().text = text;
            this.Text = text;
        }
        
        public void SetOwner(PlayerController newOwner)
        {
            CellOwnerChangedEventArgs e = new CellOwnerChangedEventArgs();
            //Debug.Log("Setting owner of " + this.gameObject.name + " to " + newowner.name);
            if (this.Owner != null)
            {
                Debug.Log("Cell already owned by " + this.Owner.PlayerName);
                return;
            }
            e.prevOwner = this.Owner;
            e.newOwner = newOwner;
            this.Owner = e.newOwner;
            Debug.Log("DefaultCell: this.Owner: " + this.Owner);
            Debug.Log("DefaultCell: Previous Owner: " + e.prevOwner);
            Debug.Log("DefaultCell: New Owner: " + e.newOwner.name);
            Debug.Log("Setting ownership of " + this.gameObject.name + " to " + this.Owner);
            this.playerSpriteRenderer.sprite = this.Owner.PlayerSymbol; // Set the cell's sprite to the player's symbol
            Debug.Log("New cell owner: " + this.Owner.PlayerName);
            e.newOwner = this.Owner;
            e.cell = this;
            this.OnOwnerChanged?.Invoke(this, e);
            //this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = this.Owner.Symbol; // Set the cell's sprite to the player's symbol

        }

    }

    public class CellOwnerChangedEventArgs : EventArgs
    {
        public PlayerController prevOwner;
        public PlayerController newOwner;
        public Cell cell;
    }
}
