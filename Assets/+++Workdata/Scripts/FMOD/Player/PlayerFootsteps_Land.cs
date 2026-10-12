using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerFootsteps_Land : MonoBehaviour
{
    private PlayerTerrainChecker _playerTerrainChecker;
    
    private FMOD.Studio.EventInstance footsteps_land;

    [Header("Raycast Settings")]
    [SerializeField] private float rayOriginOffset = 0.5f;
    [SerializeField] private float rayLength = 3.0f;

    private void Awake()
    {
        _playerTerrainChecker = GetComponent<PlayerTerrainChecker>();
    }

    public void SelectAndPlayFootstep_Land()
    {
        PlayFootstep_Land(_playerTerrainChecker.GetTerrainParameter());
    }

    private void PlayFootstep_Land(int terrain)
    {
        footsteps_land = FMODUnity.RuntimeManager.CreateInstance("event:/Player/Player_Land");
        footsteps_land.setParameterByName("Terrain", terrain);
        footsteps_land.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        footsteps_land.start();
        footsteps_land.release();
    }
}