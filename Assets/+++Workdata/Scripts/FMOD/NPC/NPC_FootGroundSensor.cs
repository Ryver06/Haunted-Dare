using UnityEngine;
using FMODUnity;


public class NPC_FootGroundSensor : MonoBehaviour
{
    //[SerializeField] private PlayerController  playerController;
    [SerializeField] private PlayerTerrainChecker _playerTerrainChecker;

    [SerializeField] private string footId = "Left";
    [SerializeField] private float cooldown = 0.1f;
    
    private FMOD.Studio.EventInstance footsteps;
    
    private float _lastTime;
    private Collider _col;

    void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        
            
        if (Time.time - _lastTime < cooldown) return;

        PlayFootstep(_playerTerrainChecker.GetTerrainParameter());
        _lastTime = Time.time;
            
            
        
        
       // if (playerController.GetCurrentState() == 0) return;
        
        if (Time.time - _lastTime < cooldown) return;

        PlayFootstep(_playerTerrainChecker.GetTerrainParameter());
        _lastTime = Time.time;
    }
    

    private void PlayFootstep(int terrain)
    {
        Debug.Log("PlayFootstep");
        
        footsteps = RuntimeManager.CreateInstance("event:/Edrick/Footsteps_Walk");
        
        
        footsteps.setParameterByName("Terrain", terrain);
        footsteps.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        footsteps.start();
        footsteps.release();
    }
}
