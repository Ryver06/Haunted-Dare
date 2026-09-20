using System;
using UnityEngine;
using UnityEngine.AI;

public class VeronicaBehaviour : MonoBehaviour
{
    private static readonly int MovementSpeedId = Animator.StringToHash("MovementValue");

    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    
    private Animator animator;
    private NavMeshAgent agent;

    private bool _isRunning;
    
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
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
}
