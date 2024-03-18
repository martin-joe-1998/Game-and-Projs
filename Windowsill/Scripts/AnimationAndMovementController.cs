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
        characterController = GetComponent<CharacterController>();   // �Ѿ��ڽ�ɫģ�����ϸ�����Character Controllerģ�飬����ͨ�����ַ�ʽ access ��ɫ���ϵĸ�ģ��
        animator = GetComponent<Animator>();                         // access ��ɫ���ϵ� animator ģ��

        // set the parameter hash references
        isWalkingHash = Animator.StringToHash("isWalking");   // ʹ��Hash����ʹunity������øò���
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        jumpCountHash = Animator.StringToHash("jumpCount");

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

    private void Update()
    {
        HandleRotation();     // ���ƽ�ɫ�ƶ�ʱ��������ת������
        HandleAnimation();    // ����һ�ж���

        // ���ø����ڽ�ɫ���ϵ� Character Controller �����ִ���ƶ�
        if (isRunPressed) {   // ����ס�ܶ���ʱ
            appliedMovement.x = currentRunMovement.x;
            appliedMovement.z = currentRunMovement.z;
        } else {              // �����߶�ʱ
            appliedMovement.x = currentMovement.x;
            appliedMovement.z = currentMovement.z;
        }

        characterController.Move(appliedMovement * movingSpeed * Time.deltaTime);

        // ����Move���ܸı� characterController �� isGrounded �ж��������������Ҫ��Move�����
        HandleGravity();      // ������������
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

    // ��Input system�����ά������ת��Ϊ(x, 0f, y), ͬʱ�ж��Ƿ����ƶ�
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
        bool isWalking = animator.GetBool(isWalkingHash);  // GetBool ��ֱ�����ǰ�� animator �ﴴ���� bool ����������,����ȡ��ֵ 
        bool isRunning = animator.GetBool(isRunningHash);  // ����ʹ������һ�ַ�ʽ

        if (isMovementPressed && !isWalking) {             // ����ס�������ʵ��û����·ʱ��������·����
            animator.SetBool(isWalkingHash, true);
        } else if (!isMovementPressed && isWalking) {      // ��û�а�ס�������ȴ����·ʱ���ر���·����
            animator.SetBool(isWalkingHash, false);
        }

        if ((isMovementPressed && isRunPressed) && !isRunning) {          // ��ͬʱ��ס��������ܲ���ȴû�����ܲ�ʱ�������ܲ�����
            animator.SetBool(isRunningHash, true);
        } else if ((!isMovementPressed || !isRunPressed) && isRunning) {  // ��û�а�ס��������ܲ���ȴ���ܲ�ʱ���ر��ܲ�����
            animator.SetBool(isRunningHash, false);
        }
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

    private void HandleGravity()
    {
        bool isFalling = currentMovement.y <= 0.0f || !isJumpPressed;
        float fallMultiplier = 2.0f;

        if (characterController.isGrounded) {                  // ��ɫ�ڵ�����ʱ������ϵ��
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
        } else if (isFalling) {                                //  ��ɫ�ڿ������������ϵ��
            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);  // �������������У�����������2��������    
            appliedMovement.y = Mathf.Max((previousYVelocity + currentMovement.y) * .5f, -20.0f);     // ��ֹ�Ӹ߿ռ�����׹����������ϵ��������         
        } else {                                              // ��ɫ�ڿ�������������ϵ��
            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (jumpGravities[jumpCount] * Time.deltaTime);  // ������������� deltaTime �Ͳ��������Ծ(�����Ӿ���)   
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
