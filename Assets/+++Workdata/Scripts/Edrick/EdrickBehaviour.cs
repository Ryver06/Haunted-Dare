using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EdrickBehaviour : MonoBehaviour
{
    public static EdrickBehaviour instance;
    
  [SerializeField] private Transform player;
  [SerializeField] private float WalkSpeed;
  [SerializeField] private float RunSpeed;
    
  [Tooltip("minimum distance of the player when Enemy starts to run")]
  [SerializeField] private float chasingDistance;
  [Tooltip("maximum distance in which enemy is walking")]
  [SerializeField] private float walkDistance;
  
  
  [SerializeField] private float chasingStoppingDistance;
  [SerializeField] private float spottingTimer;
  
  [Header("Triggers")]
  [SerializeField] private GameObject jumpscareTrigger;


  [SerializeField] private Transform raycastPos;
  
  private NavMeshAgent agent;
  private NavMeshPatrol patrol;
  
  
  public bool isSpotted;

  private void Awake()
  {
      instance = this;
      
      agent = GetComponent<NavMeshAgent>();
      patrol = GetComponent<NavMeshPatrol>();
      
      jumpscareTrigger.SetActive(false);
     
  }

  private void Update()
  {
      UpdateEnemySpeed();
  }

  /// <summary>
  /// checks distance between player and enemy and updates the enemies speed
  /// </summary>
  private void UpdateEnemySpeed()
  {
      Vector3 playerPos =  player.position;
      Vector3 enemyPos = gameObject.transform.position;

      float distance = Vector3.Distance(playerPos, enemyPos);
      
       
      if (distance > chasingDistance && isSpotted)
      {
          agent.speed = RunSpeed;
      }

      if (distance < walkDistance)
      {
          agent.speed = WalkSpeed;
      }
  }

 


  public void CheckPlayerStatus()
  {
     int playerState = PlayerController.Instance.GetCurrentHiddenState();

     switch (playerState)
     {
        case 0:
            //Visible
            Debug.Log("Player is Visible");
            //todo sound effect "i see you"
            TargetPlayer();
        break;
        
        case 1:
            //crouched
            Debug.Log("Player is crouched");
            StartCoroutine(SpottingTimer());
        break;
        
        case 2:
            //Hidden -> InLocker
            Debug.Log("Player is in Locker");
            DisableJumpscare();
        break;
     }
  }

  #region Player Chasing Methods
  
  private void TargetPlayer()
  {
      if (!RaycastCheck()) return;
      
      
      jumpscareTrigger.SetActive(true);
      patrol.SetPlayerTarget();
      isSpotted = true;

      agent.stoppingDistance = chasingStoppingDistance;
      
  }

  private IEnumerator SpottingTimer()
  {
      if (RaycastCheck())
      {
          yield return new WaitForSeconds(spottingTimer);
          TargetPlayer(); 
      }
  }

  private void DisableJumpscare()
  {
      if (isSpotted)
      {
          StartCoroutine(PlayerHidden());
      }
      else
      {
          jumpscareTrigger.SetActive(false); 
      }
  }

  IEnumerator PlayerHidden()
  {
      yield return new WaitForSeconds(0.5f); //if player was already spotted, cant immeadiatly hide in locker and be immortal
      jumpscareTrigger.SetActive(false);  //just in case
      
      yield return new WaitForSeconds(3f);
      LoosePlayer();
  }

  private bool RaycastCheck()
  {
      RaycastHit hit;
      
      Vector3 direction = (player.position + Vector3.up * 0.5f) - raycastPos.position;
      direction.Normalize();

      float distance = Vector3.Distance(raycastPos.position, player.position);
      
      if (Physics.Raycast(raycastPos.position, direction,  out hit, distance))
      {
          
          
          if (hit.collider.CompareTag("Player"))
          {
              return true;
          }
      }
      
      return false;
  }

  /// <summary>
  /// Raycasting only once often fails to hit something, so this is a backup
  /// </summary>
  public void CheckPlayerConstantly()
  {
      if (RaycastCheck() && PlayerController.Instance.GetCurrentHiddenState() == 0)
      {
          TargetPlayer();
      }
  }

  public void LoosePlayer()
  {
     StopCoroutine(SpottingTimer());
     isSpotted = false;
     jumpscareTrigger.SetActive(false); 
  }

  public void HeardPlayer()
  {
      //if player is too loud, save the transform and make it the target
  }
  

  #endregion
}
