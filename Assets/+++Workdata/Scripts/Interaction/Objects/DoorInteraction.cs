using System.Collections.Generic;
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public static readonly int Hash_Interact = Animator.StringToHash("Interact");
    
    [SerializeField] private List<Animator> anim;

    private bool doorOpen;

    public void Door_Interaction()
    {
        foreach (Animator anim in anim)
        {
            anim.SetTrigger(Hash_Interact);
            
            Debug.Log("Door_Interaction");
        }
    }
}
