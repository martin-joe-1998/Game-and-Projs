using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerInput playerInput;  // PlayerInput ���Լ�����������ϵͳ�����֣�Animations�ļ�������ͬ���ű�
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
    Vector2 currentMovementInput;    // ���ڽ�������
    Vector3 currentMovement;         // ����ִ���ƶ��ļ���ʱ�ο��ķ���
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
        characterController = GetComponent<CharacterController>();   // �Ѿ��ڽ�ɫģ�����ϸ�����Character Controllerģ�飬����ͨ�����ַ�ʽ access ��ɫ���ϵĸ�ģ��
        animator = GetComponent<Animator>();                         // access ��ɫ���ϵ� animator ģ��
        timer = GetComponent<Timer>();

        // setup state
        states = new PlayerStateFactory(this);
        currentState = states.Grounded();
        currentState.EnterState();

        // set the parameter hash references
        isWalkingHash = Animator.StringToHash("isWalking");   // ʹ��Hash����ʹunity������øò���
        isRunningHash = Animator.StringToHash("isRunning");
        isFallingHash = Animator.StringToHash("isFalling");
        isJumpingHash = Animator.StringToHash("isJumping");
        jumpCountHash = Animator.StringToHash("jumpCount");
        isFreeidlingHash = Animator.StringToHash("isFreeidling");
        randomFreeidleHash = Animator.StringToHash("randomFreeidle");

        // callback function
        playerInput.CharacterControls.Move.started += OnMovementInput;    // ��Move.�¼��������ͬʱ������OnMovementInput��ɶ��ƶ���������봦��
        playerInput.CharacterControls.Move.canceled += OnMovementInput;   // �ص�����������������¼��ĸ�������
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
        currentMovementInput = context.ReadValue<Vector2>();  // �����ά�ƶ�����
        currentMovement.x = currentMovementInput.x;           // ӳ��Ϊ��ά�ռ��е�xzƽ���ƶ�����
        currentMovement.z = currentMovementInput.y;
        currentRunMovement.x = currentMovementInput.x * runMultiplier;  // �ܲ��ƶ�ʱ�����ٶȲ���
        currentRunMovement.z = currentMovementInput.y * runMultiplier;

        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;  // ͨ�����������ж��Ƿ�ס�����
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();  // �����ܲ�ʱ��shift��������û��value��������ReadValueAsButton
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
        Vector3 positionToLookAt;   // ��ɫ�����ƶ��ķ���

        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;

        Quaternion currentRotation = transform.rotation;

        // ����ɫ�ƶ�ʱ�������������ƶ��ķ���
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
