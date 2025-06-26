using System.Net.Http.Headers;
using Ham.GameControl;
using TMPro;
using UnityEditor;
using UnityEngine;

public class HangmanManager : Manager
{

    protected override void Awake(){
        base.Awake();
    }

    public enum Language {
        English,
        Danish
    }

    [SerializeField]
    private GameObject letterBoxPrefab;

    [SerializeField]
    private GameObject letterBoxContainer;

    private GameObject[] letterBoxList;

    [SerializeField]
    private Keyboard keyboard;

    //private Vector3 initialLetterBoxPosition = new Vector3(-8, -3, 0);

    public Language language = Language.English;
    public TextAsset wordlist;

    private string targetWord = "Sesquipedalianism";
    private string targetWordClue = "Technically refers to the use of long or obscure words, often with the implication that it's excessive or pretentious.";

    [SerializeField]
    private GameObject targetWordClueObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        this.LoadWord();
    }

    void Update()
    {
    }

    public void TestLetter(string letter){
        Debug.Log(letter);
        bool found = false;
        foreach (GameObject letterbox in this.letterBoxList){
            //Debug.Log(letterbox.transform.GetComponentInChildren<TextMeshProUGUI>().text);
            if (letterbox.transform.GetComponentInChildren<TextMeshProUGUI>().text.ToUpper() == letter){
                Debug.Log(letterbox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
                letterbox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().enabled = true;
                found = true;
            }
        }
        if (found == true){
                this.keyboard.DisableLetter(letter, true);
        } else {
                this.keyboard.DisableLetter(letter, false);
        }
    }

    private void LoadWord(){
        var datalines = this.wordlist.text.Split("\n");
        int rand = Random.Range(1, datalines.Length);
        string[] word = datalines[rand].Split("\t");

        this.targetWord = word[0];

        this.letterBoxList = new GameObject[this.targetWord.Length];

        Debug.Log(this.targetWord);
        
        string tw = this.targetWord;
        for (int i = 0; i < tw.Length; i++){
            Debug.Log(tw[i]);
            //Vector3 pos = this.initialLetterBoxPosition;
            //pos.x += i;
            this.letterBoxList[i] = Instantiate(this.letterBoxPrefab, new Vector3(i, 0, 0), Quaternion.identity);
            this.letterBoxList[i].GetComponent<LetterBox>().text.text = tw[i].ToString();
            this.letterBoxList[i].transform.SetParent(this.letterBoxContainer.transform);
        }
        
        this.targetWordClueObject.GetComponent<TextMeshProUGUI>().text = word[1];
    }
}
