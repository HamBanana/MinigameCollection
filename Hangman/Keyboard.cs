using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    private List<string> usedLetters = new List<string>();

    public UnityEvent<string> OnKeyPressed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 97; i < 123; i++){
            KeyCode kc = (KeyCode)i;
            //if (this.usedLetters.Contains(kc.ToString())){return;}
            if (Input.GetKeyDown(kc)){
                string pressedkey = kc.ToString();
                this.ProcessLetter(pressedkey);
            }
        }
    }

    public void DisableLetter(string letter, bool exists){
        Button butt = this.transform.Find(letter).GetComponent<Button>();
        Image img = this.transform.Find(letter).GetComponent<Image>();
        if (!butt){return;}
        butt.interactable = false;
        if (!exists){
            img.color = Color.red;
        } else {
            img.color = Color.green;
        }
    }

    public void ProcessLetter(string letter){
        Debug.Log("Processing letter: " + letter);
        this.usedLetters.Add(letter.ToUpper());
        this.OnKeyPressed?.Invoke(letter);
    }
}
