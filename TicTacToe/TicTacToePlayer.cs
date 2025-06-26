using Ham.GameControl.Player;
using UnityEngine;

using UnityEngine.InputSystem;

namespace Ham.TicTacToe
{
    public class TicTacToePlayer : PlayerController
    {

        private void OnEnable()
        {
            Debug.Log("TicTacToePlayer: Enabled");

            var pointAction = GetComponent<PlayerInput>()?.actions["Point"];
            var clickAction = GetComponent<PlayerInput>()?.actions["Click"];
            if (pointAction != null)
            {
                pointAction.performed += ctx => Debug.Log("TicTacToePlayer: pointAction.performed");
                //pointAction.Enable(); // Just in case
            }
            if (clickAction != null)
            {
                clickAction.performed += ctx => Debug.Log("TicTacToePlayer: clickAction.performed");
                //pointAction.Enable(); // Just in case
            }
        }
    }
}
