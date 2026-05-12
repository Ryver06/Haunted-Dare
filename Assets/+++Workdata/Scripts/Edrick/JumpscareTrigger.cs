using System;
using UnityEngine;

public class JumpscareTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Jumpscare();
        }
    }
}
