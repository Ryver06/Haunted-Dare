using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Inspector

    [SerializeField] private CharacterController controller;
    
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    
    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Jumpscare")]
    [SerializeField] private GameObject jumpscare;
    
    [Header("Cam settings")]
    [SerializeField] private Transform playerCam;
    
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

    #endregion

    //movement
    private Vector2 _moveInput;
    private Vector3 _moveDir;
    private float _currentSpeed;
    
    //player 1st person cam
    private Vector2 _lookInput;
    private Vector2 _cameraRotation;
    private Transform cameraTarget;
    private bool isInverted;
    
    //gravity
    private Vector3 velocity;
    private bool _isGrounded;

    //jump
    private bool _isjumping;
    
    
    public PlayerInteraction _playerInteraction;
    
    #endregion

    #region Event Functions

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //hides cursor
    }

    private void Awake()
    {
        inputActions = new GameInput();
        lookAction = inputActions.Player.Look;
        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
        interactAction = inputActions.Player.Interact;
        
        cameraTarget = playerCam;

        _currentSpeed = walkSpeed;
        
        _playerInteraction = GetComponentInChildren<PlayerInteraction>();
    }

    private void OnEnable()
    {
        EnabelInput();

        lookAction.performed += Look;
        lookAction.canceled += Look;
        
        moveAction.performed += Move;
        moveAction.canceled += Move;

        jumpAction.performed += Jump;
        jumpAction.canceled += Jump;

        interactAction.performed += Interaction;

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
        

    }
    
    #endregion

    #region Input Methods
    
    private void Interaction(InputAction.CallbackContext ctx)
    {
        _playerInteraction.Interact();
    }
    
    private void Move(InputAction.CallbackContext ctx)
    {
        _moveInput = moveAction.ReadValue<Vector2>();
       
    }
    
    private void Look(InputAction.CallbackContext ctx)
    {
        _lookInput = ctx.ReadValue<Vector2>();
    }
    
    private void Jump(InputAction.CallbackContext ctx)
    {
        if (_isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }
    
    #endregion

    #region GameInput

    public void EnabelInput()
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
        if (_lookInput != Vector2.zero)
        {
            bool isMouseActive = CheckHardwareInput();
            float deltaTimeMultiplier = isMouseActive ? 1 : Time.deltaTime;

            _cameraRotation.x += _lookInput.y * cameraVerticalSpeed * deltaTimeMultiplier * (isInverted ? -1 : 1);
            _cameraRotation.y += _lookInput.x * cameraHorizontalSpeed * deltaTimeMultiplier;
            
            _cameraRotation.y += _lookInput.x * cameraHorizontalSpeed * deltaTimeMultiplier;

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
    
    #endregion

    private void Movement()
    {
        //time.deltatime causes it to be framerate dependend 
        controller.Move(_moveDir * _currentSpeed * Time.deltaTime);
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
        controller.Move(velocity * Time.deltaTime);
    }

    public void Jumpscare()
    {
        StartCoroutine(JumpscareRoutine());
    }

    private IEnumerator JumpscareRoutine()
    {
       DisableInput();
       jumpscare.SetActive(true);
       
       yield return new WaitForSeconds(2f);
       
       jumpscare.SetActive(false);
       EnabelInput(); //TODO replace with GameOver UI
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
    
}
