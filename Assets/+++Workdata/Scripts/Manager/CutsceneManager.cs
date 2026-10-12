using FMODUnity;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter music;
    [SerializeField] private StudioEventEmitter chaseMusic;

    [SerializeField] private Transform player;
    [SerializeField] private Transform veronica;
   
    [Header("office")]
    [SerializeField] private GameObject edrick;
    
    
    public void PauseMusic(bool isPaused)
    {
        music.EventInstance.setPaused(isPaused);
    }

    public void StopChaseMusic()
    {
        chaseMusic.Stop();
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

    public void AfterOfficeCutscene()
    {
        edrick.SetActive(true);
        LoadScene.Instance.SetRespawnId("HideNSeek");
        NoiseMeter.Instance.ActivateNoiseMeter();
    }
}
