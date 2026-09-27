using FMODUnity;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter music;
    
    public void PauseMusic(bool isPaused)
    {
        music.EventInstance.setPaused(isPaused);
    }
}
