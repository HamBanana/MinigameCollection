using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Ham.GameControl;

namespace Ham.UI.Elements
{
    public class NotificationController : Controller
    {

        public void SetText(string text){
            this.GetComponentInChildren<TextMeshProUGUI>().text = text;
        }

    }
}
