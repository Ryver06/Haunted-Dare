using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;

public enum PlayerHiddenState {Visible, Crouched, Hidden} //standing, crouched, in locker
public enum PlayerState { Idle, Walking, Running, Sneaking}
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public static readonly int Hash_MovementValue = Animator.StringToHash("MovementValue");
    public static readonly int Hash_IsCrouched = Animator.StringToHash("isCrouched");
    public static readonly int Hash_Jump = Animator.StringToHash("Jump");
    public static readonly int Hash_isGrounded = Animator.StringToHash("isGrounded");

    #region Inspector

    [SerializeField] private CharacterController controller;
    
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeed = 3f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 5f;
    [SerializeField] private float stamina = 5f;
    [SerializeField] private float staminaRegen = 1.6f;
    [SerializeField] private float staminaDrain = 1.6f;
    
    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Jumpscare")]
    [SerializeField] private PlayableDirector jumpscare;
    [SerializeField] private float jumpscareTimer;
    
    [Header("Flashlight")]
    [SerializeField] private GameObject flashlight;
    
    [Header("UI")]
    [SerializeField] private List<Image> staminaBars;

    [SerializeField] private GameObject gameOverUI;
    
    [Header("Cam settings")]
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform camFollow;
    
    [SerializeField] private float verticalCameraRotationMin = -30f;
    [SerializeField] private float verticalCameraRotationMax = 70f;
    
    [SerializeField] private float cameraHorizontalSpeed = 200f;
    [SerializeField] private float cameraVerticalSpeed = 130;
    
    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    #endregion

    #region Private Variables

    #region Input

    private GameInput inputActions;
    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction flashlightAction;
    private InputAction runAction;
    private InputAction crouchAction;
    private InputAction pauseAction;

    #endregion

    //states
    private PlayerHiddenState playerHiddenState;
    public PlayerState playerState;
    
    //movement
    private Vector2 _moveInput;
    private Vector3 _moveDir;
    private float _currentSpeed;
    private bool _isRunning;
    private bool _isCrouched;
    private bool _canRun;
    private bool _canJump;
    private bool _sprintHeld;
    
    //player 1st person cam
    private Vector2 _lookInput;
    public Vector2 _cameraRotation;
    private Transform cameraTarget;
    private bool isInverted;
    
    //gravity
    private Vector3 velocity;
    private bool _isGrounded;

    //jump
    private bool _isjumping;

    //flashlight
    private bool _flashlightOn;
    
    //locker
    private bool _inLocker;
    
    //references
    private Animator anim;
    private PlayerInteraction _playerInteraction;
    
    //UI
    private bool _canPause;
    
    
    #endregion

    #region Event Functions

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //hides cursor
    }

    private void Awake()
    {
        Instance = this;
        
        inputActions = new GameInput();
        lookAction = inputActions.Player.Look;
        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
        interactAction = inputActions.Player.Interact;
        flashlightAction = inputActions.Player.FlashLight;
        runAction = inputActions.Player.Sprint;
        crouchAction = inputActions.Player.Crouch;
        pauseAction = inputActions.Player.Pause;
        
        cameraTarget = playerCam;

        _currentSpeed = walkSpeed;
        
        _playerInteraction = GetComponentInChildren<PlayerInteraction>();
        anim = GetComponentInChildren<Animator>();
        
        _isCrouched = false;
        _canJump = true;
        _canPause = true;
        
        stamina = maxStamina;
        
        playerHiddenState = PlayerHiddenState.Visible;
       
    }

    private void OnEnable()
    {
        EnableInput();

        lookAction.performed += Look;
        lookAction.canceled += Look;
        
        moveAction.performed += Move;
        moveAction.canceled += Move;

        jumpAction.performed += Jump;
        jumpAction.canceled += Jump;

        interactAction.performed += Interaction;
        
        flashlightAction.performed += Flashlight;
        
        runAction.performed += Run;
        runAction.canceled += Run;

        crouchAction.performed += Crouch;

        pauseAction.performed += Pause;

    }

    private void FixedUpdate()
    {
        CheckGround();
        
        _moveDir = GetWorldInputDir(_moveInput);
    }

    private void Update()
    {
        Movement();
        Gravity();
        UpdateAnimator();
        UpdateStamina();
    }
    
    private void LateUpdate()
    {
        
        RotateCamera();
    }

    private void OnDisable()
    {
        DisableInput();

        lookAction.performed -= Look;
        lookAction.canceled -= Look;
        
        moveAction.performed -= Move;
        moveAction.canceled -= Move;
        
        jumpAction.performed -= Jump;
        jumpAction.canceled -= Jump;
        
        interactAction.performed -= Interaction;
        
        flashlightAction.performed -= Flashlight;
        
        runAction.performed -= Run;
        runAction.canceled -= Run;

        crouchAction.performed -= Crouch;

        pauseAction.performed -= Pause;
    }
    
    #endregion

    #region Input Methods
    
    private void Interaction(InputAction.CallbackContext ctx)
    {
        if (_inLocker)
        {
            ExitLockerMode();
            return;
        }
        
        _playerInteraction.Interact();
    }
    
    private void Move(InputAction.CallbackContext ctx)
    {
        _moveInput = moveAction.ReadValue<Vector2>();
       
    }
    
    private void Run(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (_canRun && !_isCrouched)
                _isRunning = true;
            _sprintHeld = true;
        }

        if (ctx.canceled)
        {
            _isRunning = false;
            _sprintHeld = false;
        }

        _currentSpeed = _isRunning ? runSpeed : walkSpeed;
    }
    
    private void Look(InputAction.CallbackContext ctx)
    {
        
        _lookInput = ctx.ReadValue<Vector2>();
    }
    
    private void Jump(InputAction.CallbackContext ctx)
    {
        if (_isGrounded && _canJump)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            anim.SetTrigger(Hash_Jump);
        }
    }
    
    private void Flashlight(InputAction.CallbackContext ctx)
    {
        _flashlightOn = !_flashlightOn;
        
        flashlight.SetActive(_flashlightOn);
    }
    
    private void Crouch(InputAction.CallbackContext ctx)
    {
        if(_inLocker) return;
        
        _isCrouched = !_isCrouched;

        if (_isCrouched)
        {
          Crouching();  
        }
        else if (!_isCrouched)
        {
            Uncrouching();
        }
    }

    private void Pause(InputAction.CallbackContext ctx)
    {
        if (!_canPause) return;
        
        PauseManager.Instance.Pause();
        Cursor.lockState = CursorLockMode.None;
    }
    
    #endregion

    #region Crouch

    private void Crouching()
    {
        _canRun = false;
        _canJump = false;
        _currentSpeed = crouchSpeed;
            
        controller.height = 0.9f;
        controller.center = new Vector3(0, 0.45f, 0);
        camFollow.localPosition = new Vector3(0, 1.15f, 0);
        playerHiddenState = PlayerHiddenState.Crouched;
        playerState = PlayerState.Sneaking;
    }

    private void Uncrouching()
    {
        _canRun = true;
        _canJump = true;
        _currentSpeed = walkSpeed;
            
        controller.height = 1.7f;
        controller.center = new Vector3(0, 0.925f, 0);
        camFollow.localPosition = new Vector3(0, 1.6f, 0);
        playerHiddenState = PlayerHiddenState.Visible;
    }

    #endregion

    #region GameInput

    public void EnableInput()
    {
        inputActions.Enable();
    }
    
    public void DisableInput()
    {
        inputActions.Disable();
    }

    #endregion
    
    #region Camera
    
    private void RotateCamera()
    {
        if (_inLocker)
        {
            return;
        }
        
        if (_lookInput != Vector2.zero)
        {
            bool isMouseActive = CheckHardwareInput();
            float deltaTimeMultiplier = isMouseActive ? 1 : Time.deltaTime;

            _cameraRotation.x += _lookInput.y * cameraVerticalSpeed * deltaTimeMultiplier * (isInverted ? -1 : 1);
            _cameraRotation.y += _lookInput.x * cameraHorizontalSpeed * deltaTimeMultiplier;
            
            //_cameraRotation.y += _lookInput.x * cameraHorizontalSpeed * deltaTimeMultiplier;

            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, verticalCameraRotationMin, verticalCameraRotationMax);
        }

        cameraTarget.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
        transform.rotation = Quaternion.Euler(0, _cameraRotation.y, 0); //turn player model same as cam but only on y Axis
    }
    
    private bool CheckHardwareInput()
    {
        if (lookAction.activeControl == null) return true;

        return lookAction.activeControl.device.name == "Mouse";

    }

    public void ExitLocker(Quaternion rot)
    {
        _cameraRotation.y -= 180;
        cameraTarget.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
        transform.rotation = Quaternion.Euler(0, _cameraRotation.y, 0); //turn player model same as cam but only on y Axis
        
        print(cameraTarget.eulerAngles);

    }
    
    #endregion

    private void Movement()
    {
        if(_inLocker) return;
        
        Vector3 finalMove = (_moveDir * _currentSpeed) + velocity;

        controller.Move(finalMove * Time.deltaTime);
        
        if (_moveInput == Vector2.zero && !_isCrouched)
        {
            playerState = PlayerState.Idle;
        }
        else if (!_isRunning && !_isCrouched && _moveInput != Vector2.zero) //no running, no crouching but moving
        {
            playerState = PlayerState.Walking;
        }

        if (_isRunning)
        {
            playerState = PlayerState.Running;
        }
        
        _currentSpeed = _isRunning ? runSpeed : walkSpeed;
    }
    
    
    private Vector3 GetWorldInputDir(Vector2 moveInput)
    {
        if (moveInput == Vector2.zero) return Vector3.zero;

        Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 worldDir = cameraTarget.TransformDirection(inputDir);

        worldDir.y = 0;

        return worldDir.normalized;
    }

    private void Gravity()
    {
        if (_isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        velocity.y += gravity * Time.deltaTime;
    }

    public void Jumpscare()
    {
        StartCoroutine(JumpscareRoutine());
        jumpscare.Play();
        RuntimeManager.PlayOneShot("event:/Edrick/Jumpscare");
    }

    private IEnumerator JumpscareRoutine()
    {
        _canPause = false;
        
       DisableInput();
       UIManager.Instance.DisableForDeathUI();
       
       yield return new WaitForSeconds(jumpscareTimer);
       
       gameOverUI.SetActive(true);
       
       yield return new WaitForSeconds(2f);
       Cursor.lockState = CursorLockMode.None;
    }

    #region Ground Check
    
    private void CheckGround()
    {
        _isGrounded = Physics.CheckSphere(
            transform.position + Vector3.down * (groundCheckDistance - groundCheckRadius),
            groundCheckRadius,//groundCheckDistance,
            groundLayer);
        
    }

    

    #endregion

    #region Animation

    private void UpdateAnimator()
    {
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0;

        float speed = horizontalVelocity.magnitude;

        anim.SetFloat(Hash_MovementValue, speed);
        anim.SetBool(Hash_IsCrouched, _isCrouched);
        anim.SetBool(Hash_isGrounded, _isGrounded);
    }

    #endregion

    #region Locker

    public void EnterLockerMode()
    {
        Uncrouching();
        _isCrouched = false;
        _inLocker = true;
        _canJump = false;
        playerHiddenState = PlayerHiddenState.Hidden;
    }
    
    public void ExitLockerMode()
    {
        LockerManager.instance.ExitLocker();
        StartCoroutine(LockerRoutine());
        
        
    }

    private IEnumerator LockerRoutine()
    {
        yield return new WaitForSeconds(2f);
        
        _inLocker = false;
        _canJump = true;
        playerHiddenState = PlayerHiddenState.Visible;
        
    }

    #endregion

    #region Stamina

    //Drain Stamina instantly, instead of slowly
    private void DrainStamina(int cost)
    {
        if (stamina >= 1 )
        {
            stamina -= cost;
        }
    }

    private void UpdateStamina()
    {
        foreach (Image bar in staminaBars)
        {
            float targetFillAmount = (float)stamina / maxStamina;
            bar.fillAmount = targetFillAmount;
        }
        
      
        //activates everything that you need stamina for
        if (stamina >= 1)
        {
            _canRun = true;
        }
      
      
        //Regens Stamina
        if (!_isRunning && _isGrounded)
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
        
        //drain stamina slowly
        if (_isRunning && _moveInput != Vector2.zero)
        {
            stamina -= staminaDrain + Time.deltaTime;
        }
      
    }

    #endregion
    
    public int GetCurrentHiddenState ()
    {
        return (int)playerHiddenState;
    }
    
    public int GetCurrentState ()
    {
        return (int)playerState;
    }
}

