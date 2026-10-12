using UnityEngine;

public class NPC_Footsteps : MonoBehaviour
{
    private PlayerTerrainChecker _playerTerrainChecker;
    private FMOD.Studio.EventInstance footsteps;

    [Header("Raycast Settings")]
    [SerializeField] private float rayOriginOffset = 0.5f;
    [SerializeField] private float rayLength = 1.5f;

  
    private void Awake()
    {
        _playerTerrainChecker = GetComponent<PlayerTerrainChecker>();
    }
    
    public void SelectAndPlayFootstep()
    {
        PlayFootstep(_playerTerrainChecker.GetTerrainParameter());

        
        //replace with npc stuff
        int state = PlayerController.Instance.GetCurrentState();

        switch (state)
        {
           
            case 0:
                //idle
                break;
           
            case 1:
                //walk
                break;
           
            case 2:
                //running
                break;
           
            case 3:
                //sneaking
                break;
           
        }

    }
    
    private void PlayFootstep(int terrain)
    {
        //replace event
        footsteps = FMODUnity.RuntimeManager.CreateInstance("event:/Edrick/Footsteps_Walk");
        footsteps.setParameterByName("Terrain", terrain);
        footsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        footsteps.start();
        footsteps.release();
    }

}
