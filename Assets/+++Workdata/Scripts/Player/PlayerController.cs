using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public enum PlayerState
{ Idle, Walking, Running, Jumping, Rolling, Glide, Aiming, Attacking, Interacting, Inventory }
public class PlayerController : MonoBehaviour

{
    
    public static Action InventoryAction; // for synchronising inventory
    public static PlayerController Instance;
    
    #region Animator Hashes

    private static readonly int HashMovementSpeed = Animator.StringToHash("MovementSpeed");
    private static readonly int HashGrounded = Animator.StringToHash("Grounded");
    private static readonly int HashActionTrigger = Animator.StringToHash("ActionTrigger");
    private static readonly int HashActionId = Animator.StringToHash("ActionId");
    private static readonly int HashActionType = Animator.StringToHash("ActionType");

  #endregion

    #region Inspector Variables

    [Header("Movement Setup")]
    
    [Min(0)] //sets the minimum speed, so we cant accidentally put in negative numbers
    [Tooltip("The maximum walking speed of the player in m/s")]
    [SerializeField] private float walkSpeed = 5f;

    [Min(0)]
    [Tooltip("The maximum running speed of the player in m/s")] 
    [SerializeField] private float runSpeed = 8f;
    
    


    [Tooltip("")] 
    [SerializeField] private float rotationSpeed = 10f; //rotation of the player

    [Header("Movement Agility")] 
    
    [SerializeField] private float moveDirAgility = 10f;
    
    [SerializeField] private float moveDirChangeAgility = 2.5f;

    [SerializeField] private float airAgility = 1.5f;

    [Header("Stamina")]
    [SerializeField] private float stamina;
    [SerializeField] private float maxStamina; //this is public for the LevelUp System
    [SerializeField] private float staminaDrain;
    [SerializeField] private float staminaRegen;
    [SerializeField] private Image staminaBar_fixed;
    
    [SerializeField] private float staminaRegen_moving;
    [SerializeField] private float staminaRegen_standing;
    
    
    [Header("Jump")] 
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCoolDown = 0.3f;

    
    
    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;

    [SerializeField] private float groundCheckDistance = 0.3f;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float coyoteTime = 0.2f;

    [Header("Step Climb")] 
    [SerializeField] private Transform stepRayUpper;
    [SerializeField] private Transform stepRayLower;

    
    [SerializeField] private float stepLowerDefaultHeight = 0.02f;
    [Tooltip("Max height the player can step up")]
    [SerializeField] private float stepHeight = 0.3f;
    [Tooltip("the height the player is boosted up when there's a step")]
    [SerializeField] private float stepSmooth = 0.1f;
    
    [Header("Camera")] 
    //[SerializeField] private Transform cameraRig;
    [SerializeField] private Transform playerCameraTarget;

    [SerializeField] private float verticalCameraRotationMin = -30f;
    [SerializeField] private float verticalCameraRotationMax = 70f;
    
    [SerializeField] private float cameraHorizontalSpeed = 200f;
    [SerializeField] private float cameraVerticalSpeed = 130;

    [SerializeField] private CinemachineCamera playerCam;
    
    
    [Header("Animator")] 
    public Animator playerBodyAnim;
    
    
    
    

  #endregion
  
    #region private Variables
   
   public PlayerState PlayerState;

    private PlayerInteraction _playerInteraction;  //interactions
  
    private Rigidbody rb;
    
    private Vector2 _lookInput;     //movement
    private Vector2 _moveInput;

    private Quaternion _characterTargetRotation; 
    private Vector2 _cameraRotation;
    private Vector2 _playerRotation;
    private Transform cameraTarget;

    private bool _isGrounded;
    private float _airTime;
    private float _rollStartTime;
    private float _currentSpeed;

    private bool _canRun;
    private bool _isRunning;
    private bool _isRolling = false;
    private bool _isGliding;
    private bool _isAiming;

    private bool _canJump = true;
    private bool _canAttack;
    private bool _canGlide;   //movement

    private bool hasRegenerated;

    private bool isInverted;

    private Vector3 moveDir;
    
    private bool isInventoryOpen;// INVENTORY
    
 
    #region Input

        //movement
         private GameInput inputActions;
         private InputAction lookAction;
         private InputAction moveAction;
         private InputAction runAction;
         private InputAction jumpAction;
         private InputAction rollAction;
         
         //gliding
         private InputAction glideAction;
         
         //weapons
         private InputAction aimAction;
         private InputAction attackAction;
         
         //potions
         private InputAction potionAction;
         private InputAction switchingAction;
         
         //Interaction
         private InputAction interactAction;
         
         //Inventory
         private InputAction inventoryAction;
         

     #endregion


#endregion

    #region Unity Eventfunctions

    private void Awake()
    {
        Instance = this;

        _playerInteraction = GetComponentInChildren<PlayerInteraction>();

        cameraTarget = playerCameraTarget;
        stepRayUpper.position = new Vector3(stepRayUpper.localPosition.x, stepHeight, stepRayUpper.localPosition.z);
        stepRayLower.position = new Vector3(stepRayLower.localPosition.x, stepLowerDefaultHeight, stepRayLower.localPosition.z);

        
        rb = GetComponent<Rigidbody>();
        
        inputActions = new GameInput();
        lookAction = inputActions.Player.Look;
        moveAction = inputActions.Player.Move;
        runAction = inputActions.Player.Sprint;
        jumpAction = inputActions.Player.Jump;
        attackAction = inputActions.Player.Attack;
        interactAction = inputActions.Player.Interact;
        
        
        staminaRegen = staminaRegen_standing;
    }

    private void OnEnable()
    {
        inputActions.Enable();

        lookAction.performed += Look;
        lookAction.canceled += Look;
        
        moveAction.performed += Move;
        moveAction.canceled += Move;
        
        runAction.performed += Run;
        runAction.canceled += Run;

        jumpAction.performed += Jump;
        
        interactAction.performed += Interaction;

    }

    private void Start()
    {
        _currentSpeed = walkSpeed;
        _characterTargetRotation = Quaternion.identity;
        
        
    }

    private void FixedUpdate()
    {
        CheckGround();
        
        moveDir = GetWorldInputDir(_moveInput);
        
        RotateHandler(moveDir);
        MoveHandler(moveDir);
        
        StepClimbHandler(moveDir);
    }

    private void Update()
    {
        UpdateAnimator();

        UpdateStamina();

        if (_moveInput == Vector2.zero)
        {
            PlayerState = PlayerState.Idle;
            staminaRegen = staminaRegen_standing;
        }
        else
        {
            PlayerState = PlayerState.Walking;
            staminaRegen = staminaRegen_moving;
        }
        
    }

    private void LateUpdate()
    {
        
        RotateCamera();
    }

    private void OnDisable()
    {
        inputActions.Disable();

        lookAction.performed -= Look;
        lookAction.canceled -= Look;
        
        moveAction.performed -= Move;
        moveAction.canceled -= Move;
        
        runAction.performed -= Run;
        runAction.canceled -= Run;

        jumpAction.performed -= Jump;
        
        
        interactAction.performed -= Interaction;
        
       

    }

  #endregion

    #region for UI
  public void SetInput(bool enabled) // for the UI and Screens
  {
      if (enabled) inputActions.Enable();
      else inputActions.Disable();
  }
  #endregion
   
    #region Input


    private void Interaction(InputAction.CallbackContext ctx)
    {
       _playerInteraction.Interact();
    }
   

    private void Look(InputAction.CallbackContext ctx)
    {
        _lookInput = lookAction.ReadValue<Vector2>();
    }
    
    private void Move(InputAction.CallbackContext ctx)
    {
        _moveInput = moveAction.ReadValue<Vector2>();
       
    }
    
    private void Run(InputAction.CallbackContext ctx)
    {
        if (_canRun)
        {
            _isRunning = !_isRunning;
            _currentSpeed = _isRunning ? runSpeed : walkSpeed;
            
        }
    }
    
    private void Jump(InputAction.CallbackContext ctx)
    {
        if (_isGrounded && _canJump)
        {
            if (stamina < 20)
            {
                return;
            }
            
            //_canJump = false;     keep this commented til anim event is added
            
            //playerBodyAnim.SetTrigger(HashJumpTrigger);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            DrainStamina(20);
            
        }
    }
    
    private Vector3 GetWorldInputDir(Vector2 moveInput)
    {
        if (moveInput == Vector2.zero) return Vector3.zero;

        Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 worldDir = cameraTarget.TransformDirection(inputDir);

        worldDir.y = 0;

        return worldDir.normalized;
    }
    
    #endregion

    #region Movement

    private void StepClimbHandler(Vector3 worldDir)
    {
        if (Physics.Raycast(stepRayLower.position, worldDir, out RaycastHit hitLower, 0.3f))
        {
            if (!Physics.Raycast(stepRayUpper.position, worldDir, out RaycastHit hitUpper, 0.4f))
            {
                rb.position += new Vector3(0f, stepSmooth, 0f); //going up until there's no more an edge detected
            }
        }
    }
    
    private void RotateHandler(Vector3 worldDir)
    {
        if (worldDir != Vector3.zero)
        {
            _characterTargetRotation = Quaternion.LookRotation(worldDir);
        }
        
        
//CHECK THIS OUT FOR LATER
        Quaternion newRotation = 
            Quaternion.Slerp(rb.rotation, _characterTargetRotation, rotationSpeed * Time.fixedDeltaTime);
        //slerp causes for a soft rotation
        
        rb.MoveRotation(newRotation);
    }
    
    private void MoveHandler(Vector3 worldDir)
    {
        float targetSpeed = 0f;

        if (_moveInput != Vector2.zero)
        {
            targetSpeed = _currentSpeed;
        }

        Vector3 targetVelocity = worldDir * targetSpeed;
        Vector3 currentVelocity = rb.linearVelocity;

        float finalAgility;

        if (_isGrounded)
        {
            Vector3 currentDir = currentVelocity.normalized;
            float directionDot = Vector3.Dot(currentDir, worldDir);
            float agilityT = (directionDot + 1) / 2;

            float dynamicAgility = Mathf.Lerp(moveDirChangeAgility, moveDirAgility, agilityT);

            finalAgility = currentVelocity.sqrMagnitude < 0.1f ? moveDirAgility : dynamicAgility;
        }
        else
        {
            finalAgility = airAgility;
        }

        Vector3 newHorizontalVelocity = Vector3.Lerp(
            new Vector3(currentVelocity.x, 0, currentVelocity.z),
            new Vector3(targetVelocity.x, 0, targetVelocity.z),
            finalAgility * Time.fixedDeltaTime
        );
        
        rb.linearVelocity = new Vector3(newHorizontalVelocity.x, currentVelocity.y, newHorizontalVelocity.z);

    }

  #endregion

    #region camera

    private void RotateCamera()
    {
        if (_lookInput != Vector2.zero)
        {
            bool isMouseActive = CheckHardwareInput();
            float deltaTimeMultiplier = isMouseActive ? 1 : Time.deltaTime;

            _cameraRotation.x += _lookInput.y * cameraVerticalSpeed * deltaTimeMultiplier * (isInverted ? -1 : 1);
            _cameraRotation.y += _lookInput.x * cameraHorizontalSpeed * deltaTimeMultiplier;
            
            _cameraRotation.y += _lookInput.x * cameraHorizontalSpeed * deltaTimeMultiplier;
            
            //_playerRotation.y += _lookInput.x * cameraVerticalSpeed * deltaTimeMultiplier;

            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, verticalCameraRotationMin, verticalCameraRotationMax);
        }

        cameraTarget.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
        transform.rotation = Quaternion.Euler(0, _cameraRotation.y, 0);
    }

    private bool CheckHardwareInput()
    {
        if (lookAction.activeControl == null) return true;

        return lookAction.activeControl.device.name == "Mouse";

    }

  #endregion

    #region Ground Check
    
    private void CheckGround()
    {
        bool isGrounded = Physics.CheckSphere(
            transform.position + Vector3.down * (groundCheckDistance - groundCheckRadius),
            groundCheckRadius,//groundCheckDistance,
            groundLayer);

        if (isGrounded)
        {
            _airTime = 0;
            _canRun = true;
            
        }
        else
        {
            _airTime += Time.deltaTime;
        }

        _isGrounded = _airTime < coyoteTime;
    }

    

  #endregion

    #region Animations

    public void AnimIsLanding()
    {
        StartCoroutine(EnableJump());
    }

    IEnumerator EnableJump()
    {
        
        yield return new WaitForSeconds(jumpCoolDown);
        _canJump = true;
    }
    
    
    public void AnimAction(int id)
    {
        playerBodyAnim.SetTrigger(HashActionTrigger);
        playerBodyAnim.SetInteger(HashActionId, id);
    }
    
    public void AnimActionType(int actionId)
    {
       // playerBodyAnim.SetTrigger(HashActionTypeTrigger);
        playerBodyAnim.SetInteger(HashActionType, actionId);
    }
    
    private void UpdateAnimator()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0;
        
        playerBodyAnim.SetFloat(HashMovementSpeed, velocity.magnitude);
        playerBodyAnim.SetBool(HashGrounded, _isGrounded);
        
       
    }
    

  #endregion
    
    #region GameInput

         public void DisableInput()
        {
            inputActions.Disable();
         }
  
        public void EnableInput()
        {
            inputActions.Enable();
        }

  #endregion
    
    #region Courotine
    
     public IEnumerator LayerWeight(int layerIndex, float targetWeight, float duration)
    {
         float startWeight = playerBodyAnim.GetLayerWeight(layerIndex);
        float elapsed = 0f;

        while (elapsed < duration)
         {
             elapsed += Time.deltaTime;
            float newWeight = Mathf.Lerp(startWeight, targetWeight, elapsed / duration);
            playerBodyAnim.SetLayerWeight(layerIndex, newWeight);
            yield return null;
        }
    }
     

    private IEnumerator AttackCooldown()
    {
        _canAttack = false;
        yield return new WaitForSeconds(1.5f);
        _canAttack = true;

    }
    
  #endregion
  
  
  
  public int GetCurrentState ()
  {
      return (int)PlayerState;
  }

  #region Stamina
  
    
    //Drain Stamina instantly, instead of slowly
  private void DrainStamina(int cost)
  {
      if (stamina >= 1)
      {
          stamina -= cost;
      }
  }

  private void UpdateStamina()
  {
      
      float targetFillAmount = (float)stamina / maxStamina;
      staminaBar_fixed.fillAmount = targetFillAmount;
      
      //activates everything that you need stamina for
      if (stamina >= 1)
      {
          _canRun = true;
          _canGlide = true;
      }
      
      
      //Regens Stamina
      if (!_isRunning && !_isGliding && _isGrounded)
      {
          if (stamina <= maxStamina - 0.01f)
          {
              stamina += staminaRegen * Time.deltaTime;

              if (stamina >= maxStamina)
              {
                  stamina = maxStamina;
              }
          }
      }

       

      //deactivating everything that you need stamina for
      if (stamina <= 0)
      {
          stamina = 0;
          _canRun = false;
          _isRunning = false;
          _currentSpeed = walkSpeed;
      }
        
      if (_isRunning)
      {
          stamina -= staminaDrain + Time.deltaTime;
      }
      
  }
  
  
  private AnimationState _currentState = AnimationState.Base;
  enum AnimationState
  {
      Base,
      Locomotion,
      Jump,
      Fall,
      Crouch
  }
  
  
  #endregion  
 
}
