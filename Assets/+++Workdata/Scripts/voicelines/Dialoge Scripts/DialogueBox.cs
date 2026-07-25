using System;
using System.Collections;
using System.Collections.Generic;

using Ink.Runtime;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    public static event Action<DialogueBox> DialogueContinued;
    public static event Action<DialogueBox, int> ChoiceSelected;

    #region Inspector

    [SerializeField] private TextMeshProUGUI dialogueSpeaker;

    [SerializeField] private TextMeshProUGUI dialogueText;

    //[SerializeField] private Button continueButton;

    [Header("Choices")]

   // [SerializeField] private Transform choicesContainer;

   // [SerializeField] private Button choiceButtonPrefab;

    #endregion

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    #region Unity Event Functions

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

       
    }

    private void OnEnable()
    {
        dialogueSpeaker.SetText(string.Empty); // ""
        dialogueText.SetText(string.Empty);
    }

    #endregion

    public void DisplayText(DialogueLine line)
    {
        if (line.speaker != null)
        {
            dialogueSpeaker.SetText(line.speaker);
        }

        dialogueText.SetText(line.text);

        // Read out other information such as a speaker image;
        
    }

    

    private IEnumerator DelayedSelect(Selectable newSelection)
    {
        //yield return new WaitForSeconds(0.1f);
        yield return null; // Wait for next Update() / next frame

        newSelection.Select();
    }

    
    

    
}
