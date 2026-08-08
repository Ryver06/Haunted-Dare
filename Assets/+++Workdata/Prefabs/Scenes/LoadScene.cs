using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
   public GameObject fadePanel;
   public Animator anim_fadepanel;

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
      SceneManager.LoadScene(scene);
   }

   public void QuitGame()
   {
      Application.Quit();
   }
   
   
}
