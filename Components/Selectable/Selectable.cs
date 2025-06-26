using UnityEngine;

using Ham.GameControl;
using Ham.GameControl.Input;

using System;
using Ham.GameControl.Player;

namespace Ham.Components.Selectable
{


    [RequireComponent(typeof(BoxCollider))]
    public class Selectable : MonoBehaviour
    {

        private InputManager manager = Manager.Get<InputManager>();
        public event EventHandler<SelectableEventArgs> OnHover;
        public event EventHandler<SelectableEventArgs> OnUnhover;
        public event EventHandler<SelectableEventArgs> OnSelect;

        private SelectableEventArgs eventArgs = new();

        private void Awake()
        {
            this.eventArgs.Target = this.gameObject;   
        }

        public void Select(PlayerController player)
        {
            this.eventArgs.ByPlayer = player;
            Debug.Log("Selectable: Select with non-pointer device: " + player.PlayerName + " selected Selectable on " + this.gameObject.name);
            this.OnSelect?.Invoke(this, this.eventArgs);
        }

        public void Hover(PlayerController player)
        {
            this.eventArgs.ByPlayer = player;
            Debug.Log("Selectable: Hover with non-pointer device");
            this.OnHover?.Invoke(this, this.eventArgs);
        }

        public void Unhover(PlayerController player)
        {
            this.eventArgs.ByPlayer = player;
            Debug.Log("Selectable: Unhover with non-pointer device");
            this.OnUnhover?.Invoke(this, this.eventArgs);
        }
    }



    public class SelectableEventArgs : EventArgs
    {
        public PlayerController ByPlayer;
        public GameObject Target;
    }
}