using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============== INSTRUCTION ==============
// Add empty game object to character prefab
// Position the object on the character's head
// Add a "Box Collider" component and set it to "Is Trigger"
// Add this script
// Link the camera in the inspector (needs to be done within the scene)

public class AudioListenerRotation : MonoBehaviour
{
     [Tooltip("Camera to take rotation from")]
     public GameObject Camera;

     void Update()
     {
          // Overwrite orientation of audio listener
          transform.rotation = Camera.transform.rotation; 
     }
}