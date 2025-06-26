using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LetterButton : MonoBehaviour
{

    [SerializeField]
    private Image image;

    [SerializeField]
    private TextMeshProUGUI text;

    void Awake()
    {
        transform.localScale = Vector3.one;   
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleClick()
    {
        string letter = this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        Debug.Log("Letterbutton pressed: " + letter);
        this.gameObject.GetComponentInParent<Keyboard>().ProcessLetter(letter);
    }
}
