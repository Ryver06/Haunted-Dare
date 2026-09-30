using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class VeronicaBehaviour : MonoBehaviour
{
    private static readonly int MovementSpeedId = Animator.StringToHash("MovementValue");
    private static readonly int Hash_EnterState = Animator.StringToHash("EnterState");
    private static readonly int Hash_StateId = Animator.StringToHash("StateID");

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    
    [Header("Reference")]
    [SerializeField] private Animator animator;
    
    [Header("Gameplay Settings")]
    [Header("Start")]
    [SerializeField] private GameObject atDoorInteraction;
    [SerializeField] private GameObject LivingRoomInteraction;
    
    [Header("At Door")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerTp;
    [SerializeField] private Transform kitchenTp;
    
    
    
    private NavMeshAgent agent;

    private bool _isRunning;
    
    private void Awake()
    {
        
        agent = GetComponent<NavMeshAgent>();
        
        
    }

    private void Update()
    {
        // Update the MovementSpeed in the animator with the speed of the navMeshAgent.
        animator.SetFloat(MovementSpeedId, agent.velocity.magnitude);
        
        //set speed depending on if Veronica is running or not
        agent.speed = _isRunning  ? runSpeed : walkSpeed;
    }

    public void SetTarget(Transform target)
    {
        agent.SetDestination(target.position);
    }

    //use to set Veronica's speed during or after cutscenes
    public void IsRunning(bool isRunning)
    {
        _isRunning = isRunning;
    }

    #region Cutscene Events

    public void SetState(bool enterState)
    {
        animator.SetBool(Hash_EnterState, enterState);
    }
    private void SetState(bool enterState, int id)
    {
        animator.SetBool(Hash_EnterState, enterState);
        animator.SetInteger(Hash_StateId, id);
    }

    public void AtDoor()
    {
        SetState(true, 1);
        atDoorInteraction.SetActive(true);
    }

    public void Inside()
    {
        agent.ResetPath(); //veronica clears her target
        
        player.position = playerTp.position;
    }

    /// <summary>
    /// gets called when veronica heads into the dining room
    /// </summary>
    public void VeronicaGoesToKitchen()
    {
        agent.ResetPath();
        transform.position = kitchenTp.position;
        
        gameObject.SetActive(false);
    }

    public void LivingRoom()
    {
        SetState(true,2);
        LivingRoomInteraction.SetActive(true);
    }
    

    #endregion
}
