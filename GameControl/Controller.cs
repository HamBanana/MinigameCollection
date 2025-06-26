using UnityEngine;

using Ham.GameControl.Input;

using System;

namespace Ham.GameControl
{

    public class Controller : MonoBehaviour
    {

        public event EventHandler<ControllerActionEventArgs> OnAction;

        public AudioClip TestSound;

        protected virtual void Awake(){
        }

        protected virtual void OnEnable(){
            Debug.Log("Controller: Enable: " + this.gameObject.name);
            Manager manager = Manager.Get<Manager>();
            manager.RegisterController(this);
        }

        protected virtual void OnDestroy()
        {
            
        }

        protected virtual void Start()
        {
            //this.OnAction?.Invoke(this, new ControllerActionEventArgs(){ControllerGameObject = this.gameObject});
        }
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        
        [ContextMenu("Perform Controller action test")]
        public void PerformTestControllerAction(){
            Debug.Log("Performing test controller action.");
            this.OnAction?.Invoke(this, new ControllerActionEventArgs {ControllerGameObject = this.gameObject});
        }
    }

    public class ControllerActionEventArgs : EventArgs { 
        public string TestString = "ControllerAction Invoked.";
        public GameObject ControllerGameObject;
    }
}
