using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class PuzzleInteraction : MonoBehaviour
{
   public string fmodEvent;

   public List<Interactable> pictures;


   private int _count; //use to only set pictures interactable once
   
   public void WrongFrame()
   {
      RuntimeManager.PlayOneShot("event:/Player/Puzzle");
   }

   public void SetInteractable()
   {
      GetComponent<Interactable>().canInteract = true;
   }

   public void SetNotInteractable()
   {
      GetComponent<Interactable>().canInteract = false;
   }

   //used for anything that should play an audio
   public void PlayAudio()
   {
      RuntimeManager.PlayOneShot(fmodEvent);
   }

   public void SetPicturesInteractable()
   {
      if (_count >= 1) return;
      
      foreach (Interactable interactable in pictures)
      {
         interactable.canInteract = true;
      }

      _count = 1;
   }
}
