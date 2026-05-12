using System;
using System.Collections;
using FMODUnity;
using TMPro;
using UnityEngine;

public class VoicelineManager : MonoBehaviour
{
    [SerializeField] private TMP_Text dialogueTxt;

    private void Awake()
    {
        dialogueTxt.text = "";
    }

    /// <summary>
    /// replaces the subtitles in the UI with new text
    /// </summary>
    public void ReplaceSubtitles(string text)
    {
        //use this method in the Timeline signals
        dialogueTxt.text = text;
        StartCoroutine(EmptyText());
    }

    public void PlayVoiceline(string path)
    {
        RuntimeManager.PlayOneShot(path);
    }
    
    private IEnumerator EmptyText()
    {
        yield return new WaitForSeconds(3f);
        dialogueTxt.text = "";
    }
    
    
}
