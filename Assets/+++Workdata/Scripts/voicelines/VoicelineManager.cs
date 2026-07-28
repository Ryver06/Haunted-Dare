using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class VoicelineManager : MonoBehaviour
{
    [Serializable]
    public class StoryLineOption
    {
        public string key;
        
        public string dialogPath;
        public string fmodEventId;
    }

    [SerializeField] private InkDialogue inkDialogue;
    [SerializeField]
    private List<StoryLineOption> _storyLineOptions = new List<StoryLineOption>();
    
   

    
    /// <summary>
    /// checks the list if any key matches and plays the appropiate fmod audio and ink dialog
    /// </summary>
    /// <param name="key"></param>
    public void SetStoryline(string key)
    {
        //checks if the key matches with anything in the list
        StoryLineOption foundOption = _storyLineOptions.Find(option => option.key == key); 
        if (foundOption == null)
            return; 

        
        inkDialogue.StartDialogue(foundOption.dialogPath);
        RuntimeManager.PlayOneShot(foundOption.fmodEventId);
    }
    
   

   
    
}
