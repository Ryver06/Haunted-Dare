using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerFootsteps_Jump : MonoBehaviour
{
    private PlayerTerrainChecker _playerTerrainChecker;
    
    private FMOD.Studio.EventInstance footsteps_jump;

    [Header("Raycast Settings")]
    [SerializeField] private float rayOriginOffset = 0.5f;
    [SerializeField] private float rayLength = 3.0f;

    private void Awake()
    {
        _playerTerrainChecker = GetComponent<PlayerTerrainChecker>();
    }

    public void SelectAndPlayFootstep_Jump()
    {
        PlayFootstep_Jump(_playerTerrainChecker.GetTerrainParameter());
    }

    private void PlayFootstep_Jump(int terrain)
    {
        footsteps_jump = FMODUnity.RuntimeManager.CreateInstance("event:/Player/Player_Jump");
        footsteps_jump.setParameterByName("Terrain", terrain);
        footsteps_jump.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        footsteps_jump.start();
        footsteps_jump.release();
    }
}