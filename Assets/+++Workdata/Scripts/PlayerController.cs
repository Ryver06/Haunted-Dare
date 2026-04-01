using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
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
    private static readonly int HashActionTypeTrigger = Animator.StringToHash("HolsterTrigger");
    private static readonly int HashAttackTrigger = Animator.StringToHash("AttackTrigger");
    private static readonly int HashAttackID = Animator.StringToHash("AttackId"); //0 is tail attack, 1 is fire attack
    private static readonly int HashJumpTrigger = Animator.StringToHash("Jump");
    private static readonly int HashIsGliding = Animator.StringToHash("isGliding");

  #endregion

    #region Inspector Variables

    [Header("Movement Setup")]
    
    [Min(0)] //sets the minimum speed, so we cant accidentally put in negative numbers
    [Tooltip("The maximum walking speed of the player in m/s")]
    [SerializeField] private float walkSpeed = 5f;

    [Min(0)]
    [Tooltip("The maximum running speed of the player in m/s")] 
    [SerializeField] private float runSpeed = 8f;

    [SerializeField] private float glideSpeed;
    [SerializeField] private float glideDamping;
    


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
    
    [Header("Mana")]
    [SerializeField] private float mana;
    [SerializeField] private float maxMana;  //this is public for the LevelUp System
    [SerializeField] private float manaRegen;
    [SerializeField] private Image manaBar_fixed;
    [SerializeField] private Image manaBar_smooth;
    [SerializeField] private float manaBarSpeed = .3f;
    
    [Header("Jump")] 
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCoolDown = 0.3f;

    [Header("Dodge")]
    [SerializeField] private float rollPower;
    [SerializeField] private float rollDuration;
    [SerializeField] private AnimationCurve rollCurve;
    [SerializeField] private GameObject particles;

    [Header("Fire Attack")] 
    [SerializeField] private Transform firePosition;
    [SerializeField] private GameObject crosshair;
    
    [Header("Potions")] 
    [SerializeField] private float manaPotionAmount;
    [SerializeField] private float healthPotionAmount;
    [SerializeField] private Image potionImage;
    [SerializeField] private Sprite healthPotion;
    [SerializeField] private Sprite manaPotion;
    
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
    [SerializeField] private Transform cameraRig;
    [SerializeField] private Transform playerCameraTarget;
    [SerializeField] private Transform aimCameraTarget;

    [SerializeField] private float verticalCameraRotationMin = -30f;
    [SerializeField] private float verticalCameraRotationMax = 70f;
    
    [SerializeField] private float cameraHorizontalSpeed = 200f;
    [SerializeField] private float cameraVerticalSpeed = 130;

    [SerializeField] private CinemachineCamera playerCam;
    [SerializeField] private CinemachineCamera aimCam;
    
    
    [Header("Animator")] 
    public Animator playerBodyAnim;
    
    [Header("Inventory")]//--------------------INVENTORY
    
   // public GameObject inventoryContainer;
    
    
    

  #endregion
  
    #region private Variables
   
   public PlayerState PlayerState;

    private PlayerInteraction _playerInteraction;  //interactions
  
    private Rigidbody rb;
    
    private Vector2 _lookInput;     //movement
    private Vector2 _moveInput;

    private Quaternion _characterTargetRotation; 
    private Vector2 _cameraRotation;
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

    public bool _canUseHealthPotion;
    public bool _canUseManaPotion;
    public bool canSwitch;
 
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

        _canAttack = true;
        _playerInteraction = GetComponent<PlayerInteraction>();

        cameraTarget = playerCameraTarget;
        stepRayUpper.position = new Vector3(stepRayUpper.localPosition.x, stepHeight, stepRayUpper.localPosition.z);
        stepRayLower.position = new Vector3(stepRayLower.localPosition.x, stepLowerDefaultHeight, stepRayLower.localPosition.z);

        
        rb = GetComponent<Rigidbody>();
        
        inputActions = new GameInput();
        lookAction = inputActions.Player.Look;
        moveAction = inputActions.Player.Move;
        runAction = inputActions.Player.Run;
        jumpAction = inputActions.Player.Jump;
        rollAction = inputActions.Player.Roll;
        attackAction = inputActions.Player.Attack;
        interactAction = inputActions.Player.Interact;
        glideAction = inputActions.Player.Glide;
        aimAction = inputActions.Player.Aim;
        potionAction = inputActions.Player.Potion;
        switchingAction = inputActions.Player.Switching;
        

        mana = maxMana;
        staminaRegen = staminaRegen_standing;
        
        //INVENTORY
        inventoryAction = inputActions.Player.Inventory;
        
        //potions
        _canUseHealthPotion = true;
        _canUseManaPotion = false;
        canSwitch = true;

        potionImage.sprite = healthPotion;

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

        rollAction.performed += Roll;
        

        attackAction.performed += Attack;
        
        interactAction.performed += Interaction;

        glideAction.performed += GlideStart;
        glideAction.canceled += GlideEnd;

        aimAction.performed += Aim;
       
        interactAction.performed += Interaction;
        
        inventoryAction.performed += InventoryInput;

        potionAction.performed += UsePotion;
        switchingAction.performed += SelectPotion;


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

        if (_isRolling)
        {
            RollHandler();
            return;
        }
        
        RotateHandler(moveDir);
        MoveHandler(moveDir);
        
        StepClimbHandler(moveDir);
    }

    private void Update()
    {
        UpdateAnimator();

        if (_isGliding && _isGrounded)
        {
            _isGliding = false;

            rb.linearDamping = 1;
            _currentSpeed = walkSpeed;
        }

        if (_isGliding)
        {
            //bug: when sprinting while gliding and then stop sprinting, speed gets set back to walk speed
            _currentSpeed = glideSpeed; //this is the fix
        }

        UpdateStamina();
        UpdateMana();

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

        if (_isRolling)
        {
            particles.SetActive(true);
        }
        else if (!_isRolling)
        {
            particles.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        cameraRig.position = transform.position;
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
        
        rollAction.performed -= Roll;
        
        attackAction.performed -= Attack;
       
        interactAction.performed -= Interaction;
        
        glideAction.performed -= GlideStart;
        glideAction.canceled -= GlideEnd;
        
        aimAction.performed -= Aim;
        
        interactAction.performed -= Interaction;
        
        inventoryAction.performed -= InventoryInput;
        
        potionAction.performed -= UsePotion;
        switchingAction.performed -= SelectPotion;

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
            
            playerBodyAnim.SetTrigger(HashJumpTrigger);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            DrainStamina(20);
            
        }
    }
    
    private void Roll(InputAction.CallbackContext ctx)
    {
        if (_isGliding || !_isGrounded) return;
        if (stamina < 30)
        {
            return;
        }
        
        _rollStartTime = Time.time;
        _isRolling = true;
        
        AnimAction(1);
        StartCoroutine(StoppingRoll());
        DrainStamina(30);
    }

    

    

    private void Attack(InputAction.CallbackContext ctx)
    {
        
        //fire attack
        if (_isAiming && _canAttack)
        {
            GameObject fireball = FireballPool.instance.GetFireball();

            if (fireball != null &&  mana >= 10)
            {
                playerBodyAnim.SetTrigger(HashAttackTrigger);
                playerBodyAnim.SetInteger(HashAttackID, 1);
                
                //if fireballs are availible, they get shot forwards in the camera targets direction
                fireball.transform.position = firePosition.position; //sets position
                Vector3 shootDir = cameraTarget.forward; //shooting direction

                fireball.transform.rotation = Quaternion.LookRotation(shootDir);

                fireball.SetActive(true);
                Fireball fb = fireball.GetComponent<Fireball>();
                fb.Shooting(shootDir);
                DrainMana(10); 
                StartCoroutine(AttackCooldown()); //small cooldown to avoid spamming
                
                
            }
        }

        //close/tail Attack
        if (!_isAiming && _canAttack)
        {
            //check if player has enough stamina to attack
            if (stamina < 50)
            {
                return;
            }
            
            playerBodyAnim.SetTrigger(HashAttackTrigger);
            playerBodyAnim.SetInteger(HashAttackID, 0);

            StartCoroutine(AttackCooldown());
            DrainStamina(50);
        }
        
    }
    
    private void Aim(InputAction.CallbackContext ctx)
    {
        _isAiming = !_isAiming;

        if (_isAiming)
        {
            aimCam.Priority = 10;
            playerCam.Priority = 0;

            cameraTarget = aimCameraTarget;
            crosshair.SetActive(true);
        }
        else
        {
            aimCam.Priority = 0;
            playerCam.Priority = 10;

            cameraTarget = playerCameraTarget;
            crosshair.SetActive(false);
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

    private void GlideStart(InputAction.CallbackContext ctx)
    {
        
        if (!_isGrounded && _canGlide)
        {
            _isGliding = true;
            _canRun = false;
            
            rb.linearDamping = glideDamping;
            _currentSpeed = glideSpeed;
        }
    }
    
    private void GlideEnd(InputAction.CallbackContext ctx)
    {
        if (!_isGrounded && _canGlide)
        {
            StopGliding();  
        }
    }

    private void StopGliding()
    {
            _isGliding = false;

            rb.linearDamping = 1;
            _currentSpeed = walkSpeed;
        
    }

    private void UsePotion(InputAction.CallbackContext ctx)
    {
        if (_canUseHealthPotion) //health potion is equipped
        {
            if (GameState.Instance.HasHealthPotion() == false) return; // if no health potion -> no healing
            
            PlayerInfo.Instance.UseHealthPotion(healthPotionAmount); //healing
            
            GameState.Instance.Add("item_healthPotion", -1); //1 health potion gets removed from the inventory
        }

        if (_canUseManaPotion) //mana potion is equipped
        {
            if (GameState.Instance.HasManaPotion() == false) return; // if no mana potion -> no more mana
            
            UseManaPotion(manaPotionAmount);
            GameState.Instance.Add("item_manaPotion", -1); //1 health potion gets removed from the inventory
        }
       
    }

    private void SelectPotion(InputAction.CallbackContext ctx)
    {
        if (!canSwitch) return;
        
        
        if (_canUseHealthPotion)
        {
            _canUseHealthPotion = false;
            _canUseManaPotion = true;
            
            potionImage.sprite = manaPotion;

            StartCoroutine(SwitchTimer());
            return;
        }
        
        if (_canUseManaPotion)
        {
            _canUseHealthPotion = true;
            _canUseManaPotion = false;
            
            potionImage.sprite = healthPotion;
            
            StartCoroutine(SwitchTimer());
            return;
        }
        
        
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

    private void RollHandler()
    {
        float elapsedTime = Time.time - _rollStartTime;
        float timeRatio = Mathf.Clamp01(elapsedTime / rollDuration);

        float curveValue = rollCurve.Evaluate(timeRatio);
        float currentRollForce = rollPower * curveValue;

        rb.linearVelocity = transform.forward * currentRollForce;

        if (timeRatio >= 1f)
        {
            _isRolling = false;
        }
    }
    
    private void RotateHandler(Vector3 worldDir)
    {
        if (worldDir != Vector3.zero)
        {
            _characterTargetRotation = Quaternion.LookRotation(worldDir);
        }

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

            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, verticalCameraRotationMin, verticalCameraRotationMax);
        }

        cameraTarget.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
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
        playerBodyAnim.SetTrigger(HashActionTypeTrigger);
        playerBodyAnim.SetInteger(HashActionType, actionId);
    }
    
    private void UpdateAnimator()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0;
        
        playerBodyAnim.SetFloat(HashMovementSpeed, velocity.magnitude);
        playerBodyAnim.SetBool(HashGrounded, _isGrounded);
        
        playerBodyAnim.SetBool(HashIsGliding, _isGliding);
    }


    // stops the player from rolling twice
    IEnumerator StoppingRoll()
    {
        yield return new WaitForSeconds(0.1f);
        playerBodyAnim.SetInteger(HashActionId, 0);
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

    private IEnumerator SwitchTimer()
    {
        canSwitch = false;
        yield return new WaitForSeconds(0.5f);
        canSwitch = true;
    }
  #endregion
  
    #region ENABLE-DISABLE for INVENTORY
  public void OnDisable_InventoryOpen() //  to block look & move without blocking INVENTORY
  {
      lookAction.performed -= Look;
      lookAction.canceled -= Look;

      moveAction.performed -= Move;
      moveAction.canceled -= Move;
        
      runAction.performed -= Run;
      runAction.canceled -= Run;

      jumpAction.performed -= Jump;
        
      rollAction.performed -= Roll;
        
      attackAction.performed -= Attack;
  }
    
  public void OnEnable_InventoryClose() //  to block look & move without blocking INVENTORY
  {
      inputActions.Enable();

      lookAction.performed += Look;
      lookAction.canceled += Look;

      moveAction.performed += Move;
      moveAction.canceled += Move;
        
      runAction.performed += Run;
      runAction.canceled += Run;

      jumpAction.performed += Jump;
        
      rollAction.performed += Roll;

      attackAction.performed += Attack;
  }
  #endregion
  
    #region Inventory
  void InventoryInput(InputAction.CallbackContext context)
  {
      InventoryAction?.Invoke(); // InventoryManager handles the rest
  }
    
  public void SetInventoryOpen(bool open)
  {
      isInventoryOpen = open;

      if (open)
      {
          OnDisable_InventoryOpen();   // inventory open → lock player
          _moveInput = Vector2.zero;
      }
      else
      {
          OnEnable_InventoryClose();   // inventory closed → enable movement
      }
  }
  #endregion
  
  public int GetCurrentState ()
  {
      return (int)PlayerState;
  }

  #region Stamina and Mana


    #region UI

    private IEnumerator UpdateManabar(float targetFillAmount) 
    {
        float elapsed = 0;
        float currentFillAmount = manaBar_smooth.fillAmount;
    
        while (elapsed < manaBarSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / manaBarSpeed;
    
            float currentFill = Mathf.Lerp(currentFillAmount, targetFillAmount, t); 
            manaBar_smooth.fillAmount = currentFill;
            yield return null;
        }
    
        manaBar_smooth.fillAmount = targetFillAmount;
    }

  #endregion
    
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
          _isGliding = false;
          _canGlide = false;
          _currentSpeed = walkSpeed;
          StopGliding();
      }
        
      if (_isRunning || _isGliding)
      {
          stamina -= staminaDrain + Time.deltaTime;
      }
      
  }

  /// <summary>
  /// Drains mana in the amount put into the cost
  /// </summary>
  private void DrainMana(int cost)
  {
      if (mana >= 1)
      {
          mana -= cost;
          
          float targetFillAmount = (float)mana / maxMana;
          manaBar_fixed.fillAmount = targetFillAmount;

          StartCoroutine(UpdateManabar(targetFillAmount));
      }
  }
  
  /// <summary>
  /// Regenerates Mana. Keep manaRegen on a low number
  /// </summary>
  private void UpdateMana()
  {
      if (mana <= maxMana - 0.01f)
      {
          mana += manaRegen * Time.deltaTime;
          
          float targetFillAmount = (float)mana / maxMana;
          manaBar_fixed.fillAmount = targetFillAmount;
      }
  }

  public void LevelUpStamina(float amount)
  {
      maxStamina += amount;
      stamina += amount;
  }
  
  public void LevelUpMana(float amount)
  {
      maxMana += amount;
      mana += amount;
  }

  public void UseManaPotion(float add)
  {
      if (mana == maxMana) return;

      mana += add;

      if (mana >= maxMana)
      {
          mana = maxMana;
      }
      
      float targetFillAmount = (float)mana / maxMana;
      manaBar_fixed.fillAmount = targetFillAmount;

      StartCoroutine(UpdateManabar(targetFillAmount));

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
