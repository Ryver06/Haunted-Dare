using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class LoadScene : MonoBehaviour
{
   [SerializeField] private List<StudioEventEmitter> mainMenuSound;
   
   public GameObject fadePanel;
   public GameObject deathUi;
   public Animator anim_fadepanel;

   public string respawnId;
   
   private void Awake()
   {
      anim_fadepanel.Play("FadePanel_fade_out");
   }

   public void ChangeScene(int scene)
   {
      StartCoroutine(FadeAndLoadScene(scene));
   }

   
   private IEnumerator FadeAndLoadScene(int scene)
   {
      anim_fadepanel.Play("FadePanel_fade_in");
      yield return new WaitForSeconds(1f);

      if (mainMenuSound != null)
      {
         foreach (StudioEventEmitter emitter in mainMenuSound)
         {
            emitter.Stop();
         }
      }
      
      SceneManager.LoadScene(scene);
   }

   public void QuitGame()
   {
      Application.Quit();
   }


   #region respawn

   //Set Respawn ID inb Trigger boxes
   public void SetRespawnId(string respawnId)
   {
      this.respawnId = respawnId;
   }

   public void Respawn()
   {
      StartCoroutine(FadeAndRespawn());
   }

   private IEnumerator FadeAndRespawn()
   {
      anim_fadepanel.Play("FadePanel_fade_in");
      yield return new WaitForSeconds(1f);
      
      RespawnManager.Instance.CheckId(respawnId);
      deathUi.SetActive(false);
      
      yield return new WaitForSeconds(1f);
      anim_fadepanel.Play("FadePanel_fade_out");
      
      PlayerController.Instance.EnableInput();
   }

   #endregion
   
}
