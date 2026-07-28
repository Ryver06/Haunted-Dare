using System;
using UnityEngine;

public class InkDialogue : MonoBehaviour
{
    public static InkDialogue Instance;
    
    #region Inspector

    [Tooltip("Path to a specified knot.stitch in the ink file.")]
    [SerializeField] private string dialoguePath;
    
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    public void StartDialogue()
    {
        if (string.IsNullOrWhiteSpace(dialoguePath))
        {
            Debug.LogWarning("No dialogue path defined", this);
            return;
        }
        
        FindObjectOfType<GameController>().StartDialogue(dialoguePath);
    }
    
    public void StartDialogue(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.LogWarning("No dialogue path defined", this);
            return;
        }
        
        FindObjectOfType<GameController>().StartDialogue(path);
    }
}