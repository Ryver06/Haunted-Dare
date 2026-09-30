using FMODUnity;
using UnityEngine;

public class PuzzleInteraction : MonoBehaviour
{
   public string fmodEvent;
   
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
}
