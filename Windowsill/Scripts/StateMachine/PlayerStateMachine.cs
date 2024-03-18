using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerInput playerInput;  // PlayerInput 是自己命名的输入系统的名字，Animations文件夹里有同名脚本
    private CharacterController characterController;
    private Animator animator;
    private Timer timer;

    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;
    int jumpCountHash;
    int isFallingHash;
    int isFreeidlingHash;
    int randomFreeidleHash;

    // moving variables
    Vector2 currentMovementInput;    // 用于接受输入
    Vector3 currentMovement;         // 最终执行移动的计算时参考的方向
    Vector3 currentRunMovement;
    Vector3 appliedMovement;
    bool isMovementPressed;
    bool isRunPressed;

    // jumping variables
    bool isJumpPressed = false;
    float initialJumpVelocity;
    float maxJumpHeight = 2.0f;
    float maxJumpTime = 0.75f;

    bool isJumping = false;
    bool requireNewJumpPress = false;
    int jumpCount = 0;
    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>();

    [SerializeField] private float rotationFactorPerFrame = 15.0f;
    [SerializeField] private float movingSpeed = 1.0f;
    [SerializeField] private float runMultiplier = 4.0f;
    float gravity = -9.8f;

    Coroutine currentJumpResetRoutine = null;

    // set variables
    PlayerBaseState currentState;
    PlayerStateFactory states;

    // getter and setter
    public PlayerBaseState CurrentState { get { return currentState; } set { currentState = value; } }
    public CharacterController CharacterController {  get { return characterController; } }
    public Animator Animator { get { return animator; } }
    public Coroutine CurrentJumpResetRoutine { get { return currentJumpResetRoutine; } set { currentJumpResetRoutine = value; } }
    public Dictionary<int, float> InitialJumpVelocities { get { return initialJumpVelocities; } }
    public Dictionary<int, float> JumpGravities {  get { return jumpGravities; } }
    public Timer Timer { get { return timer; } }
    public int JumpCount { get { return jumpCount; } set { jumpCount = value; } }
    public int IsWalkingHash { get { return isWalkingHash; } }
    public int IsRunningHash { get { return isRunningHash; } }
    public int IsFallingHash { get { return isFallingHash; } }
    public int IsFreeidlingHash { get { return isFreeidlingHash; } }
    public int RandomFreeidle { get { return randomFreeidleHash; } }
    public int IsJumpingHash { get { return isJumpingHash; } }
    public int JumpCountHash { get { return jumpCountHash; } }
    public bool IsMovementPressed { get { return isMovementPressed; } }
    public bool IsRunPressed { get { return isRunPressed; } }
    public bool RequireNewJumpPress { get { return requireNewJumpPress; } set { requireNewJumpPress = value; } }
    public bool IsJumping {  set { isJumping = value; } }
    public bool IsJumpPressed { get { return isJumpPressed; } }
    public float Gravity { get { return gravity; } }
    public float CurrentMovementY { get { return currentMovement.y; } set { currentMovement.y = value; } }
    public float AppliedMovementY { get { return appliedMovement.y; } set { appliedMovement.y = value; } }
    public float AppliedMovementX { get { return appliedMovement.x; } set { appliedMovement.x = value; } }
    public float AppliedMovementZ { get { return appliedMovement.z; } set { appliedMovement.z = value; } }
    public float RunMultiplier { get { return runMultiplier; } }
    public Vector2 CurrentMovementInput { get { return currentMovementInput; } }

    private void Awake()
    {
        // initially set reference variables
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();   // 已经在角色模型身上附加了Character Controller模组，所以通过这种方式 access 角色身上的该模组
        animator = GetComponent<Animator>();                         // access 角色身上的 animator 模组
        timer = GetComponent<Timer>();

        // setup state
        states = new PlayerStateFactory(this);
        currentState = states.Grounded();
        currentState.EnterState();

        // set the parameter hash references
        isWalkingHash = Animator.StringToHash("isWalking");   // 使用Hash可以使unity更快调用该参数
        isRunningHash = Animator.StringToHash("isRunning");
        isFallingHash = Animator.StringToHash("isFalling");
        isJumpingHash = Animator.StringToHash("isJumping");
        jumpCountHash = Animator.StringToHash("jumpCount");
        isFreeidlingHash = Animator.StringToHash("isFreeidling");
        randomFreeidleHash = Animator.StringToHash("randomFreeidle");

        // callback function
        playerInput.CharacterControls.Move.started += OnMovementInput;    // 当Move.事件被唤起的同时，调用OnMovementInput完成对移动方向的输入处理
        playerInput.CharacterControls.Move.canceled += OnMovementInput;   // 回调函数还会接收来自事件的各种数据
        playerInput.CharacterControls.Move.performed += OnMovementInput;  // performed : An Interaction with the Action has been completed.
        playerInput.CharacterControls.Run.started += OnRun;
        playerInput.CharacterControls.Run.canceled += OnRun;
        playerInput.CharacterControls.Jump.started += OnJump;
        playerInput.CharacterControls.Jump.canceled += OnJump;

        SetupJumpVariables();
    }

    // Start is called before the first frame update
    void Start()
    {
        characterController.Move(appliedMovement * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        currentState.UpdateStates();
        characterController.Move(appliedMovement * movingSpeed * Time.deltaTime);
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();  // 读入二维移动方向
        currentMovement.x = currentMovementInput.x;           // 映射为三维空间中的xz平面移动方向
        currentMovement.z = currentMovementInput.y;
        currentRunMovement.x = currentMovementInput.x * runMultiplier;  // 跑步移动时乘以速度参数
        currentRunMovement.z = currentMovementInput.y * runMultiplier;

        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;  // 通过输入向量判断是否按住方向键
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();  // 由于跑步时左shift按键控制没有value，所以是ReadValueAsButton
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
        requireNewJumpPress = false;
    }

    private void SetupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        float initialGravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
        float secondJumpGravity = (-2 * (maxJumpHeight + 2)) / Mathf.Pow((timeToApex * 1.25f), 2);
        float secondJumpInitialVelocity = (2 * (maxJumpHeight + 2)) / (timeToApex * 1.25f);
        float thirdJumpGravity = (-2 * (maxJumpHeight + 4)) / Mathf.Pow((timeToApex * 1.5f), 2);
        float thirdJumpInitialVelocity = (2 * (maxJumpHeight + 4)) / (timeToApex * 1.5f);

        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        jumpGravities.Add(0, initialGravity);
        jumpGravities.Add(1, initialGravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);
    }

    private void HandleRotation()
    {
        Vector3 positionToLookAt;   // 角色正在移动的方向

        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;

        Quaternion currentRotation = transform.rotation;

        // 当角色移动时，将看向正在移动的方向
        if (isMovementPressed) {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
    }

    private void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }
}
