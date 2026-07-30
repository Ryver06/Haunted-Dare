using UnityEngine;
using FMODUnity;

public class FMODAnimationEvent : MonoBehaviour
{
   

    public void Play(string eventPath)
    {
        if (string.IsNullOrEmpty(eventPath))
        {
            Debug.LogError("FMOD Event path not set");
            return;
        }
        
        RuntimeManager.PlayOneShot(eventPath, transform.position);
    }
}

