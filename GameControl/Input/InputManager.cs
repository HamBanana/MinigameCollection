using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ham.GameControl.Input
{
    public class InputManager : Manager
    {
        //public InputActionAsset InputActions;

        public event EventHandler<PointerEventArgs> OnPointerPressed;
        public event EventHandler<PointerEventArgs> OnPointerReleased;
        public event EventHandler<PointerEventArgs> OnPointerMoved;

        //private InputAction clickAction;
        //private InputAction positionAction;

        /*protected override void Awake()
        {
            base.Awake();

            if (InputActions == null){
                Debug.LogError("InputManager: InputActions asset not assigned.");
            }

            this.clickAction = InputActions.FindAction("Interact");
            //this.positionAction = InputActions.FindAction("Point");

            if (this.clickAction != null){
                Debug.Log("InputManager: Click action was found.");
                clickAction.started += (ctx) => {this.RaisePointerEvent(OnPointerPressed, ctx);};
                clickAction.canceled += (ctx) => {this.RaisePointerEvent(OnPointerPressed, ctx);};
            }

            if (positionAction != null){
                positionAction.performed += (ctx) => {this.RaisePointerEvent(OnPointerMoved, ctx);};
            }

            InputActions.Enable();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected void RaisePointerEvent(EventHandler<PointerEventArgs> handler, InputAction.CallbackContext ctx){
            //Vector2 pos = ctx.ReadValue<Vector2>();
            Vector2 pos = Pointer.current?.position.ReadValue() ?? Vector2.zero;
            handler?.Invoke(this, new PointerEventArgs {Position = pos} );
        }

        protected override void OnDestroy(){
            base.OnDestroy();
            InputActions?.Disable();
        }
        */
    }

    public class PointerEventArgs : EventArgs {
        public Vector2 Position;
    }
}
