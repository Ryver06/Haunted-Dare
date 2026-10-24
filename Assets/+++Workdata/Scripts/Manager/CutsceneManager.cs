using FMODUnity;
using UnityEngine;
using UnityEngine.AI;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter music;
    [SerializeField] private StudioEventEmitter chaseMusic;

    [SerializeField] private Transform player;
    [SerializeField] private Transform veronica;
   
    [Header("office")]
    [SerializeField] private GameObject edrick;
    [SerializeField] private NavMeshAgent agentVeronica;
    
    
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

    
    /// Veronica Disappears after Office cutscene due to a glitch with the agent.
    /// Setting agent component on and off refreshes it and makes veronica stop teleporting
    public void SetAgentActive(bool active)
    {
       agentVeronica.enabled = active; 
    }

    public void AfterOfficeCutscene()
    {
        edrick.SetActive(true);
        LoadScene.Instance.SetRespawnId("HideNSeek");
        NoiseMeter.Instance.ActivateNoiseMeter();
    }
}
