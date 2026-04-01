using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    #region Inspecotr

    [Header("Movement")] 
    [SerializeField] private float walkSpeed; 
    [SerializeField] private float runSpeed;
    
    
    [Header("Jump")]
    [SerializeField] private float jumpPower = 1.5f;
    [SerializeField] private float gravityMulitplier = 3f;
    [SerializeField] private float velocity;
      
    
    
    [Header("Movement Agility")] 
    [SerializeField] private float moveDirAgility = 10f;
    [SerializeField] private float moveDirChangeAgility = 2.5f;
    [SerializeField] private float airAgility = 1.5f;
    
    [Header("Stamina")]
    [SerializeField] private float stamina;
    [SerializeField] private float maxStamina;
    [SerializeField] private float staminaRegen;
    [SerializeField] private float staminaDrain;
    
    [Header("Camera")] 
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float verticalCameraRotationMin = -30f;
    [SerializeField] private float verticalCameraRotationMax = 70f;
    [SerializeField] private float cameraHorizontalSpeed = 200f;
    [SerializeField] private float cameraVerticalSpeed = 130;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float coyoteTime = 0.2f;
    
    [Header("UI")]
    [SerializeField] private Image staminaBar;

    #endregion

    #region private Variables

    #region Input

    private GameInput inputActions;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction runAction;
    
    #endregion

    //references
    private CharacterController controller;
    
    //Vector2
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private Vector2 _cameraRotation; 
    
    //is...
    private bool _isInverted;
    private bool _isGrounded;
    private bool _isRunning;
    public bool _isJumping;
    
    //can...
    private bool _canRun;
    
    
    private float _currentSpeed;
    private float _airTime;
    private Vector3 _playerVelocity;
    
    private float gravityValue = -9.81f;
    #endregion
    
   
    
   

    
    


    private void Awake()
    {
        inputActions = new GameInput();
        moveAction = inputActions.Player.Move;
        lookAction = inputActions.Player.Look;
        jumpAction = inputActions.Player.Jump;
        runAction = inputActions.Player.Sprint;
        
        controller = GetComponent<CharacterController>();

        _currentSpeed = walkSpeed;
        _canRun = true;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        
        moveAction.performed += Move;
        moveAction.canceled += Move;

        lookAction.performed += Look;
        lookAction.canceled += Look;
        
        runAction.performed += Run;
        runAction.canceled += Run;
            
        jumpAction.performed += Jump;
        jumpAction.canceled += Jump;
      

    }

    private void FixedUpdate()
    {
       Moving(); 
        
        
    }

    void Update()
    {
        UpdateStamina();
       CheckGround();
       
      
    }

    private void LateUpdate()
    {
        RotateCamera();
    }

    private void OnDisable()
    {
        inputActions.Disable();
        
        moveAction.performed -= Move;
        moveAction.canceled -= Move;
        
        lookAction.performed -= Look;
        lookAction.canceled -= Look;
        
        runAction.performed -= Run;
        runAction.canceled -= Run;
        
        jumpAction.performed -= Jump;
        jumpAction.canceled -= Jump;
    }

    #region Input Methods

    private void Move(InputAction.CallbackContext context)
    {
       _moveInput = context.ReadValue<Vector2>();
       
    }

    private void Look(InputAction.CallbackContext context)
    {
        _lookInput = lookAction.ReadValue<Vector2>();
    }

    private void Jump(InputAction.CallbackContext context)
    {
        _isJumping = context.ReadValueAsButton();
    }

    private void Run(InputAction.CallbackContext context)
    {
        _isRunning = !_isRunning;

        if (_isRunning && _canRun)
        {
           _currentSpeed =  runSpeed; 
        }
        else
        {
            _currentSpeed = walkSpeed;
        }
    }

    #endregion

    #region Movement

    private void Moving()
    {
        //worldDir effects the player
        Vector3 worldDir = GetWorldInputDir(_moveInput.normalized);
      
        // Jumping
        if (_isGrounded && _isJumping)
        {
            velocity += jumpPower;
        }

        ApplyGravity();
        worldDir.y = velocity;
        
        controller.Move(worldDir * _currentSpeed * Time.fixedDeltaTime); 
        
       
    }

    #endregion
    
    #region camera

    private void RotateCamera()
    {
        if (_lookInput != Vector2.zero)
        {
            bool isMouseActive = CheckHardwareInput();
            float deltaTimeMultiplier = isMouseActive ? 1 : Time.deltaTime;

            _cameraRotation.x += _lookInput.y * cameraVerticalSpeed * deltaTimeMultiplier * (_isInverted ? -1 : 1);
            _cameraRotation.y += _lookInput.x * cameraHorizontalSpeed * deltaTimeMultiplier;

            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, verticalCameraRotationMin, verticalCameraRotationMax);
        }

        //cameraTarget.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
        transform.rotation = Quaternion.Euler(0, _cameraRotation.y, 0);
        cameraTarget.localRotation = Quaternion.Euler(_cameraRotation.x, 0, 0);
    }

    private bool CheckHardwareInput()
    {
        if (lookAction.activeControl == null) return true;

        return lookAction.activeControl.device.name == "Mouse";

    }
    #endregion
    
    private Vector3 GetWorldInputDir(Vector2 moveInput)
    {
        if (moveInput == Vector2.zero) return Vector3.zero;

        Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 worldDir = cameraTarget.TransformDirection(inputDir);

        worldDir.y = 0;

        return worldDir.normalized;
    }

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

    private void ApplyGravity()
    {
        if (_isGrounded && velocity < 0f)
        {
            velocity = -1f;
        }
        
        if (!_isGrounded)
        {
            velocity += gravityValue * gravityMulitplier * Time.deltaTime;
        }
    }
    
    #region Stamina

    
    /// <summary>
    /// drains or regens stamina as well as sets various bools, depending on the situation
    /// </summary>
    private void UpdateStamina()
    {
      
        float targetFillAmount = (float)stamina / maxStamina;
        staminaBar.fillAmount = targetFillAmount;
      
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
            
            
            /*
             * bug: when player holds run, even after stamina is completely drained, and then lets go after stamina
             *      regened, then the player sprints again for a short time
             *
             * canceling the input fixes it
             */
            
            runAction.canceled += Run;
            runAction.canceled -= Run;
            
        }
        
        if (_isRunning)
        {
            stamina -= staminaDrain + Time.deltaTime;
        }
      
    }

    #endregion
    
}
