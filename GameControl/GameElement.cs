using UnityEngine;

namespace Ham.GameControl
{
    public class GameElement : MonoBehaviour {

        public bool Hide(){
            this.gameObject.SetActive(false);
            return true;
        }

        public bool Show(){
            this.gameObject.SetActive(true);
            return true;
        }

        public string Test(){
            return "GameElement test";
        }
    }
}