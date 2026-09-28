using FMODUnity;
using UnityEngine;

public class PuzzleInteraction : MonoBehaviour
{
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
}
