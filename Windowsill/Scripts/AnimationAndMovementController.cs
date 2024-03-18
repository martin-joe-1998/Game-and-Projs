using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    private PlayerInput playerInput;
    private CharacterController characterController;
    private Animator animator;

    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;
    int jumpCountHash;

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
    bool isJumpAnimating = false;
    int jumpCount = 0;
    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>();

    [SerializeField] private float rotationFactorPerFrame = 15.0f;
    [SerializeField] private float movingSpeed = 1.0f;
    [SerializeField] private float runMultiplier = 4.0f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float groundedGravity = -.05f;

    Coroutine currentJumpResetRoutine = null;

    private void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();   // 已经在角色模型身上附加了Character Controller模组，所以通过这种方式 access 角色身上的该模组
        animator = GetComponent<Animator>();                         // access 角色身上的 animator 模组

        // set the parameter hash references
        isWalkingHash = Animator.StringToHash("isWalking");   // 使用Hash可以使unity更快调用该参数
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        jumpCountHash = Animator.StringToHash("jumpCount");

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

    private void Update()
    {
        HandleRotation();     // 控制角色移动时的身体旋转（朝向）
        HandleAnimation();    // 控制一切动画

        // 利用附加在角色身上的 Character Controller 组件来执行移动
        if (isRunPressed) {   // 当按住跑动键时
            appliedMovement.x = currentRunMovement.x;
            appliedMovement.z = currentRunMovement.z;
        } else {              // 正常走动时
            appliedMovement.x = currentMovement.x;
            appliedMovement.z = currentMovement.z;
        }

        characterController.Move(appliedMovement * movingSpeed * Time.deltaTime);

        // 由于Move可能改变 characterController 的 isGrounded 判定，因此重力机制要在Move后调用
        HandleGravity();      // 控制重力机制
        HandleJump();
    }

    private void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }

    // 从Input system读入二维变量并转化为(x, 0f, y), 同时判断是否在移动
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
    }

    // set the initial velocity and gravity using jump heights and durations
    private void SetupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
        float secondJumpGravity = (-2 * (maxJumpHeight + 2)) / Mathf.Pow((timeToApex * 1.25f), 2);
        float secondJumpInitialVelocity = (2 * (maxJumpHeight + 2)) / (timeToApex * 1.25f);
        float thirdJumpGravity = (-2 * (maxJumpHeight + 4)) / Mathf.Pow((timeToApex * 1.5f), 2);
        float thirdJumpInitialVelocity = (2 * (maxJumpHeight + 4)) / (timeToApex * 1.5f);

        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        jumpGravities.Add(0, gravity);
        jumpGravities.Add(1, gravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);
    }

    private void HandleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);  // GetBool 里直接填此前在 animator 里创建的 bool 变量的名字,来读取此值 
        bool isRunning = animator.GetBool(isRunningHash);  // 这里使用了另一种方式

        if (isMovementPressed && !isWalking) {             // 当按住方向键但实际没有走路时，激活走路参数
            animator.SetBool(isWalkingHash, true);
        } else if (!isMovementPressed && isWalking) {      // 当没有按住方向键但却在走路时，关闭走路参数
            animator.SetBool(isWalkingHash, false);
        }

        if ((isMovementPressed && isRunPressed) && !isRunning) {          // 当同时按住方向键和跑步键却没有在跑步时，激活跑步参数
            animator.SetBool(isRunningHash, true);
        } else if ((!isMovementPressed || !isRunPressed) && isRunning) {  // 当没有按住方向键或跑步键却在跑步时，关闭跑步参数
            animator.SetBool(isRunningHash, false);
        }
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

    private void HandleGravity()
    {
        bool isFalling = currentMovement.y <= 0.0f || !isJumpPressed;
        float fallMultiplier = 2.0f;

        if (characterController.isGrounded) {                  // 角色在地面上时的重力系数
            if (isJumpAnimating) {
                animator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;
                currentJumpResetRoutine = StartCoroutine(JumpResetRoutine());
                if (jumpCount == 3) {
                    jumpCount = 0;
                    animator.SetInteger(jumpCountHash, jumpCount);
                }
            }
            currentMovement.y = groundedGravity;
            appliedMovement.y = groundedGravity;
        } else if (isFalling) {                                //  角色在空中下落的重力系数
            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);  // 如果是下落过程中，则重力乘以2加速下落    
            appliedMovement.y = Mathf.Max((previousYVelocity + currentMovement.y) * .5f, -20.0f);     // 防止从高空急速下坠，设置重力系数的下限         
        } else {                                              // 角色在空中上升的重力系数
            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (jumpGravities[jumpCount] * Time.deltaTime);  // 这里如果不乘以 deltaTime 就不会进行跳跃(至少视觉上)   
            appliedMovement.y = (previousYVelocity + currentMovement.y) * .5f;
        }
    }

    private void HandleJump()
    {
        if (!isJumping && characterController.isGrounded && isJumpPressed) {
            if (jumpCount < 3 && currentJumpResetRoutine != null) {
                StopCoroutine(currentJumpResetRoutine);
            }
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJumping = true;
            jumpCount += 1;
            animator.SetInteger(jumpCountHash, jumpCount);
            currentMovement.y = initialJumpVelocities[jumpCount];
            appliedMovement.y = initialJumpVelocities[jumpCount];
        } else if (!isJumpPressed && characterController.isGrounded && isJumping) {
            isJumping = false;
        }
    }

    private IEnumerator JumpResetRoutine() 
    {
        yield return new WaitForSeconds(.5f);
        jumpCount = 0;
    }
}
