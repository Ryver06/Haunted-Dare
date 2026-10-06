using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RespawnManager : MonoBehaviour
{
   [Serializable]
   public class RespawnProperties
   {
      public string respawnId; 
      
      public Transform respawnPoint;
      public Transform enemyRespawnPoint;
      
      public List<GameObject> deactiveObjects; //objects that need to be turned off after respawning
      public List<GameObject> activeObjects; //objects that need to be turned on after respawning

      public UnityEvent onRespawn;
   }
   
   public static RespawnManager Instance;
   

   [SerializeField] private Transform player;
   [SerializeField] private Transform edrick;
   
   public List<RespawnProperties> respawnProperties;

   private void Awake()
   {
      Instance = this;
   }

   public void CheckId(string respawnId)
   {
      //goes through the respawn list
      for (int i = 0; i < respawnProperties.Count; i++)
      {
         if (respawnProperties[i].respawnId == respawnId)
         {
            Respawn(i);
         }
      }
   }

   private void Respawn(int respawnId)
   {
      player.position = respawnProperties[respawnId].respawnPoint.position; //puts player on spawn point
      edrick.position = respawnProperties[respawnId].enemyRespawnPoint.position; //puts edrick on spawn point
      
      //matches the id in the list and turns the objects on, which are in the activeObjects list
      foreach (var obj in respawnProperties[respawnId].activeObjects)
      {
         obj.SetActive(true);
      }
      
      //matches the id in the list and turns the objects off, which are in the deactiveObjects list
      foreach (var obj in respawnProperties[respawnId].deactiveObjects)
      {
         obj.SetActive(false);
      }
      
      respawnProperties[respawnId].onRespawn.Invoke();
   }
}
