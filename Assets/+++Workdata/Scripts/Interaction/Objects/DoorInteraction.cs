using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorInteraction : MonoBehaviour
{
    public static readonly int Hash_DoorOpen = Animator.StringToHash("DoorOpen");

    [SerializeField] private bool specialInteraction;
    [SerializeField] private List<Animator> anim;

    [SerializeField] private UnityEvent SpecialInteraction;
    
    private bool doorOpen;


    private void Update()
    {
        foreach (Animator anim in anim)
        {
            anim.SetBool(Hash_DoorOpen, doorOpen);
        }
    }

    public void Door_Interaction()
    {
        if (specialInteraction)
        {
            SpecialInteraction.Invoke();
            specialInteraction = false;
            return;
        }
        
        doorOpen = !doorOpen;
    }

    /// <summary>
    /// NPC Opens Door
    /// </summary>
    public void NPC_Door_Interaction()
    {
        if (doorOpen) return; //only opens/interacts with door if its closed
        
        doorOpen = true;
        
        Debug.Log("doorOpen");
    }
    
    /// <summary>
    /// NPC Closes Door
    /// </summary>
    public void NPC_Door_Close()
    {
        if (!doorOpen) return;
        
        StartCoroutine(CloseDoor());
    }

    IEnumerator CloseDoor()
    {
        yield return new WaitForSeconds(1.5f);

        doorOpen = false;
        
        Debug.Log("doorClose");
    }
}
