using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using TMPro;
using UnityEngine;

public class VoicelineManager : MonoBehaviour
{
    // in seperates script
    [Serializable]
    public class StoryLineOption
    {
        public string key;
        
        //TODO maybe inky story text object?!
        public string dialogStartText;
        public string fmodEventId;
    }

    [SerializeField]
    private List<StoryLineOption> _storyLineOptions = new List<StoryLineOption>();
    
    [SerializeField] private TMP_Text dialogueTxt;

    
    //das bleibt
    public void SetStoryline(string key)
    {
        //checks if the key matches with anything in the list
        StoryLineOption foundOption = _storyLineOptions.Find(option => option.key == key); 
        if (foundOption == null)
            return; //TODO also log error!
        
        ReplaceSubtitles(foundOption.dialogStartText);
        PlayVoiceline(foundOption.fmodEventId);
    }
    
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
