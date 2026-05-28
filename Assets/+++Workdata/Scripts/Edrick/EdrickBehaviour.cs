using System;
using UnityEngine;
using UnityEngine.AI;

public class EdrickBehaviour : MonoBehaviour
{
  /*
   * behaviour: unnoticed, noticed, chasing
   * unnoticed: player lost or cant see player
   * noticed: heard or saw player
   * chasing: player is running off -> edrick starts chasing
   */


  [SerializeField] private Transform player;
  [SerializeField] private float WalkSpeed;
  [SerializeField] private float RunSpeed;
    
  [Tooltip("minimum distance of the player when Enemy starts to run")]
  [SerializeField] private float chasingDistance;
  [Tooltip("maximum distance in which enemy is walking")]
  [SerializeField] private float walkDistance;
  
  
  private NavMeshAgent agent;
  private NavMeshPatrol patrol;
  
  private bool isSpotted;

  private void Awake()
  {
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

  public void ChasePlayer()
  {
     patrol.SetPlayerTarget();
     isSpotted = true;
  }
  
  
}
