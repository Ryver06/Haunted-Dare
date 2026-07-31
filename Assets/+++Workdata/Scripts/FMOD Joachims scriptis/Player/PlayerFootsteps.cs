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

    [Header("Noise Settings")]
    [SerializeField] private float walkingNoise;
    [SerializeField] private float runningNoise;
    [SerializeField] private float sneakingNoise;
    private void Awake()
    {
        _playerTerrainChecker = GetComponent<PlayerTerrainChecker>();
    }
    
    public void SelectAndPlayFootstep()
    {
        PlayFootstep(_playerTerrainChecker.GetTerrainParameter());

       int state = PlayerController.Instance.GetCurrentState();

       switch (state)
       {
           
           case 0:
               //idle
               break;
           
           case 1:
               //walk
               NoiseMeter.Instance.MakeNoise(walkingNoise);
               break;
           
           case 2:
               //running
               NoiseMeter.Instance.MakeNoise(runningNoise);
               break;
           
           case 3:
               //sneaking
               NoiseMeter.Instance.MakeNoise(sneakingNoise);
               break;
           
       }

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