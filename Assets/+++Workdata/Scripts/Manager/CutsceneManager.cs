using FMODUnity;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter music;

    [SerializeField] private Transform player;
    [SerializeField] private Transform veronica;
   
    
    
    
    public void PauseMusic(bool isPaused)
    {
        music.EventInstance.setPaused(isPaused);
    }

    public void TeleportPlayer(Transform tp)
    {
        player.position = tp.position;
    }

    public void TeleportVeronica(Transform tp)
    {
        veronica.position = tp.position;
        veronica.rotation = tp.rotation;
    }

    
}
