using UnityEngine;

using Ham.GameControl;

using System;

namespace Ham.GameControl.Input
{

    public enum InputEventType
    {
        Hover, Unhover, Select
    }


    [RequireComponent(typeof(BoxCollider))]
    public class InputController : Controller
    {

        private InputManager manager = Manager.Get<InputManager>();
        public event EventHandler<ControllerInputEventArgs> OnHover;
        public event EventHandler<ControllerInputEventArgs> OnUnhover;
        public event EventHandler<ControllerInputEventArgs> OnSelect;

        protected override void OnEnable()
        {
            this.manager.RegisterController(this);
            this.manager.OnPointerPressed += (sender, e) =>
            {
                Debug.Log("InputController: manager.OnPointerPressed was triggered on position: " + e.Position.x + "-" + e.Position.y);
                Vector2 screenPosition = e.Position;
                Ray ray = Camera.main.ScreenPointToRay(screenPosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Debug.Log("Physics.Raycast is true :eyes:");
                    if (hit.transform == this.transform)
                    {
                        Debug.Log("InputController: " + this.gameObject.name + " was clicked.");
                        this.OnSelect?.Invoke(this, new ControllerInputEventArgs { EventType = InputEventType.Select });
                    }
                }
            };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public virtual void OnMouseDown()
        {
            //Debug.Log("InputController: OnMouseDown!!");
            //this.OnSelect?.Invoke(this, new ControllerInputEventArgs { EventType = InputEventType.Select });
        }

        public virtual void OnMouseEnter()
        {
            Debug.Log("InputController: OnMouseEnter!!");
            this.OnHover?.Invoke(this, new ControllerInputEventArgs { EventType = InputEventType.Hover });
        }

        public virtual void OnMouseExit()
        {
            Debug.Log("InputController: OnMouseExit!!");
            this.OnUnhover?.Invoke(this, new ControllerInputEventArgs { EventType = InputEventType.Unhover });
        }
    }



    public class ControllerInputEventArgs : ControllerActionEventArgs
    {
        public InputEventType EventType;
    }
}