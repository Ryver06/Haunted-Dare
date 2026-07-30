using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerFootsteps : MonoBehaviour
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
    }
    
    private void PlayFootstep(int terrain)
    {
        footsteps = FMODUnity.RuntimeManager.CreateInstance("event:/Player/Player_Footsteps");
        footsteps.setParameterByName("Terrain", terrain);
        footsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        footsteps.start();
        footsteps.release();
    }
    

}