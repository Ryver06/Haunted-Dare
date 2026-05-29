using System;
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
  
  
  private NavMeshAgent agent;
  private NavMeshPatrol patrol;
  
  public bool isSpotted;

  private void Awake()
  {
      instance = this;
      
      agent = GetComponent<NavMeshAgent>();
      patrol = GetComponent<NavMeshPatrol>();
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

  public void TargetPlayer()
  {
     patrol.SetPlayerTarget();
     isSpotted = true;

     agent.stoppingDistance = chasingStoppingDistance;
  }
  
 
  
  
  
}
