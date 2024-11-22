using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class dialogos : MonoBehaviour
{
    [SerializeField] private float TextSpeed;


    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private TextMeshProUGUI nameComponent;
    [SerializeField] private Image characterImage;

    [SerializeField] private List<CustomList> textList = new List<CustomList>();

    public int currentIndex;
    public int currentLineIndex;
    private bool isTyping;
    private bool textStarted;

    public GameObject[] next;
    public GameObject[] turnOff;

    //public NameInput nameInput;
    public string playerName;
    public string SP_world;
    private void Start()
    {
        //playerName = nameInput.getName();
        
        InitializeVariables();
        StartDialogue();
    }

    private void InitializeVariables()
    {
        currentIndex = currentLineIndex = 0;
        isTyping = textStarted = false;
    }

    private void Update()
    {
        
            if (textStarted && Input.GetMouseButtonDown(0))
            {
                if (!isTyping)
                {
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    textComponent.text = textList[currentIndex].lines[currentLineIndex];
                    nameComponent.text = textList[currentIndex].characterName[currentLineIndex];
                    characterImage.sprite = textList[currentIndex].characterSprite[currentLineIndex];
                    isTyping = false;
                }
            }     
        
    }

    private void StartDialogue()
    {
        textComponent.text = string.Empty;
        currentIndex = currentLineIndex = 0;
        textStarted = true;
        StartCoroutine(TypeLine());
        UpdateCharacterInfo();
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        foreach (char c in textList[currentIndex].lines[currentLineIndex].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(TextSpeed);
        }

        isTyping = false;
    }

    private void NextLine()
    {
        if (currentLineIndex >= textList[currentIndex].lines.Length -1)
        {
       
            currentIndex++;
            currentLineIndex = 0;
        }

        currentLineIndex++;
        textComponent.text = string.Empty;
        StartCoroutine(TypeLine());
        UpdateCharacterInfo();
        
        if (currentLineIndex == textList[currentIndex].lines.Length - 1)
        {
            if (textList[currentIndex].EnumKindText == TextKind.gameobjectOn)
            {
                foreach (var ObjToTurnOn in next)
                {
                    ObjToTurnOn.SetActive(true);
                }

                foreach (var objToTurnOff in turnOff)
                {
                    objToTurnOff.SetActive(false);
                }
            }
            else if(textList[currentIndex].EnumKindText == TextKind.startCinametic)
            {
                foreach (var ObjToTurnOn in next)
                {
                    ObjToTurnOn.SetActive(true);
                }

                foreach (var objToTurnOff in turnOff)
                {
                    objToTurnOff.SetActive(false);
                }
            }
            else if (textList[currentIndex].EnumKindText == TextKind.NoMove)
            {
                SceneManager.LoadScene("Map");
            }
        }
    }

    private void UpdateCharacterInfo()
    {
        if (currentLineIndex < textList[currentIndex].characterName.Length && currentLineIndex < textList[currentIndex].characterSprite.Length) 
        { 
            nameComponent.text = textList[currentIndex].characterName[currentLineIndex]; 
            characterImage.sprite = textList[currentIndex].characterSprite[currentLineIndex];
        }
    }
}

[Serializable] public class CustomList
{
    public string[] lines;
    public string[] characterName;
    public Sprite[] characterSprite;
    public TextKind EnumKindText;
}

[Serializable] public enum TextKind
{
    gameobjectOn,
    NoMove,
    startCinametic
}