using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement2 : MonoBehaviour
{
    // --- (Keep all your existing Header variables: Movement, Jumping, Crouching, etc.) ---
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed = 7f;
    public float sprintSpeed = 10f;
    public bool isSliding = false;
    [Header("Jumping")]
    public float jumpForce = 5f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    public bool readyToJump;
    [Header("Crouching")]
    public float crouchSpeed = 3.5f;
    public float crouchYScale = 0.5f;
    public float startYScale;
    private float startControllerHeight;
    [Header("Acceleration & Deceleration")]
    public float acceleration = 40f;
    public float deceleration = 5f;
    public float reverseDecelerationMultiplier = 3f;
    private float currentSpeed;
    [Header("Sliding")]
    private float slideCooldown = 3f;
    private bool startedSlide = false;
    [Header("Ground Check")]
    public LayerMask whatIsGround;
    bool grounded;
    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    [Header("References")]
    public Transform orientation;
    public Slidingv2 slidingScript; // Your reference to the sliding script


    // Physics constant (for stability)
    const float GRAVITY = -20f;
    const float STICK_TO_GROUND_FORCE = -5f;

    // ---- CharacterController ----
    CharacterController controller;
    public Vector3 velocityCC; // Holds current velocity

    // ---- New Input System ----
    private PlayerInputActions playerInputActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchSlideAction;
    private Vector2 moveInput;

    //Movement variables
    public bool inventoryOpen = false;
    public float horizontalInput;
    public float verticalInput;
    Vector3 moveDirection;
    private Vector3 lastMoveDirection = Vector3.forward;

    public enum MovementState { walking, sprinting, sliding, crouching, air }
    public MovementState state;

    // --- Accessors ---
    public float CurrentMoveSpeed => currentSpeed;
    public void SetCurrentSpeed(float speed) { currentSpeed = speed; } // This seems unused now but keep if needed elsewhere
    public Vector3 GetSlopeNormal() => slopeHit.normal;
    public float GetCurrentSlopeAngle() => slopeHit.normal == Vector3.zero ? 0f : Vector3.Angle(slopeHit.normal, Vector3.up);
    public bool OnSlope()
    { /* ... unchanged OnSlope logic ... */
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out slopeHit, controller.height * 0.5f + 0.3f, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < controller.slopeLimit && angle != 0;
        }
        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 dir) => Vector3.ProjectOnPlane(dir, slopeHit.normal).normalized;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        startYScale = transform.localScale.y;
        startControllerHeight = controller.height;
        readyToJump = true;
        currentSpeed = 0f;
        moveSpeed = walkSpeed;

        playerInputActions = new PlayerInputActions();
        moveAction = playerInputActions.Player.Move;
        jumpAction = playerInputActions.Player.Jump;
        sprintAction = playerInputActions.Player.Sprint;
        crouchSlideAction = playerInputActions.Player.CrouchSlide;
    }

    private void OnEnable() { playerInputActions?.Player.Enable(); }
    private void OnDisable() { playerInputActions?.Player.Disable(); }

    private void Update()
    {
        if (!inventoryOpen)
        {
            grounded = controller.isGrounded;
            MyInput();
            StateHandler();
            ApplyGravityAndStickiness();

            if (isSliding && grounded)
            {
                slidingScript.SlideMovement(); // Let sliding script handle movement
                velocityCC.x = slidingScript.slideVelocity.x; // Update velocity based on slide script's results
                velocityCC.z = slidingScript.slideVelocity.z; // Keep slide's horizontal velocity
                                                              // Note: Gravity/Stickiness is applied separately in ApplyGravityAndStickiness
            }
            else
            {
                if (isSliding && !grounded) StopSlideLogic(false);
                MovePlayer(); // Handle regular or air movement
            }

            // Final Move call
            controller.Move(velocityCC * Time.deltaTime);
        }
    }

    private void ApplyGravityAndStickiness()
    { /* ... unchanged gravity logic ... */
        if (grounded && velocityCC.y < 0)
        {
            velocityCC.y = STICK_TO_GROUND_FORCE;
        }
        else
        {
            velocityCC.y += GRAVITY * Time.deltaTime;
        }
    }

    private void MyInput()
    { /* ... unchanged MyInput logic using playerInputActions ... */
        moveInput = moveAction.ReadValue<Vector2>();
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;
        if (horizontalInput != 0f || verticalInput != 0f)
        {
            lastMoveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
        }
        if (jumpAction.triggered && readyToJump && grounded)
        {
            readyToJump = false;
            if (isSliding) StopSlideLogic(false);
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (crouchSlideAction.triggered)
        {
            StartCrouch();
            if ((horizontalInput != 0f || verticalInput != 0f) && grounded && !isSliding && !startedSlide && state == MovementState.sprinting)
            {
                StartSlideLogic();
            }
        }
        if (crouchSlideAction.WasReleasedThisFrame())
        {
            StopCrouch();
            if (isSliding) StopSlideLogic(true);
        }
    }
    private void StartCrouch()
    { /* ... unchanged StartCrouch logic ... */
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        controller.height = startControllerHeight * crouchYScale;
        controller.center = new Vector3(controller.center.x, startControllerHeight * crouchYScale / 2f, controller.center.z);
    }
    private void StopCrouch()
    { /* ... unchanged StopCrouch logic with ceiling check ... */
        float checkDistance = startControllerHeight - (startControllerHeight * crouchYScale);
        Vector3 checkStart = transform.position + Vector3.up * (startControllerHeight * crouchYScale);
        if (!Physics.Raycast(checkStart, Vector3.up, checkDistance, whatIsGround))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            controller.height = startControllerHeight;
            controller.center = new Vector3(controller.center.x, startControllerHeight / 2f, controller.center.z);
        }
    }
    private void StartSlideLogic()
    { /* ... unchanged StartSlideLogic ... */
        if (startedSlide) return;
        startedSlide = true;
        isSliding = true;
        StartCrouch();
        slidingScript.StartSlide(new Vector3(velocityCC.x, 0, velocityCC.z));
    }
    private void StopSlideLogic(bool forceStandUp)
    { /* ... unchanged StopSlideLogic ... */
        if (!isSliding) return;
        isSliding = false;
        slidingScript.StopSlide();
        if (forceStandUp || !crouchSlideAction.IsPressed())
        {
            StopCrouch();
        }
        Invoke(nameof(ResetSlideStart), slideCooldown);
    }


    private void StateHandler()
    {
        // Determine target speed based on state
        float targetMaxSpeed;

        if (isSliding && grounded)
        {
            state = MovementState.sliding;
            // <<< CHANGE: Calculate speed from the slideVelocity vector in Slidingv2
            float currentSlideSpeed = new Vector3(slidingScript.slideVelocity.x, 0, slidingScript.slideVelocity.z).magnitude;
            targetMaxSpeed = currentSlideSpeed; // Target speed IS the current slide speed
            currentSpeed = currentSlideSpeed; // Update currentSpeed directly
        }
        else if (crouchSlideAction.IsPressed() && grounded)
        {
            state = MovementState.crouching;
            targetMaxSpeed = crouchSpeed;
            if (transform.localScale.y != crouchYScale) StartCrouch();
        }
        else if (grounded && sprintAction.IsPressed())
        {
            state = MovementState.sprinting;
            targetMaxSpeed = sprintSpeed;
            if (transform.localScale.y != startYScale) StopCrouch();
        }
        else if (grounded)
        {
            state = MovementState.walking;
            targetMaxSpeed = walkSpeed;
            if (transform.localScale.y != startYScale) StopCrouch();
        }
        else // In Air
        {
            state = MovementState.air;
            targetMaxSpeed = moveSpeed; // Use the speed from the last grounded state
        }

        // Set moveSpeed for acceleration logic if NOT sliding
        if (state != MovementState.sliding)
        {
            moveSpeed = targetMaxSpeed;
        }
        // No else needed, currentSpeed is set directly in the sliding check above
    }

    private void MovePlayer()
    {
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
        Vector3 flatVelocity = new Vector3(velocityCC.x, 0, velocityCC.z);
        float effectiveCurrentSpeed = flatVelocity.magnitude;
        float inputMagnitude = moveInput.magnitude;
        float targetSpeed = moveSpeed * inputMagnitude; // moveSpeed is set by StateHandler

        // --- Acceleration / Deceleration (Only if NOT sliding) ---
        // <<< CHANGE: Wrapped accel/decel logic in this check >>>
        if (state != MovementState.sliding)
        {
            float speedChangeRate;
            if (targetSpeed > effectiveCurrentSpeed + 0.1f) speedChangeRate = acceleration;
            else if (targetSpeed < effectiveCurrentSpeed - 0.1f)
            {
                speedChangeRate = (Vector3.Dot(flatVelocity.normalized, moveDirection) < 0 && effectiveCurrentSpeed > 0.1f)
                                  ? deceleration * reverseDecelerationMultiplier : deceleration;
            }
            else
            {
                speedChangeRate = deceleration;
                if (inputMagnitude < 0.01f && effectiveCurrentSpeed < 0.1f) targetSpeed = 0f;
            }
            currentSpeed = Mathf.MoveTowards(effectiveCurrentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);
            currentSpeed = Mathf.Min(currentSpeed, moveSpeed); // Clamp to max state speed
        }
        // else: currentSpeed is being handled by StateHandler/Sliding script


        // --- Apply Movement ---
        Vector3 targetHorizontalVelocity;
        if (grounded && OnSlope() && !exitingSlope)
        {
            targetHorizontalVelocity = GetSlopeMoveDirection(moveDirection) * currentSpeed;
        }
        else
        {
            targetHorizontalVelocity = moveDirection * currentSpeed;
        }

        // Apply calculated horizontal velocity (vertical is handled by ApplyGravityAndStickiness)
        velocityCC.x = targetHorizontalVelocity.x;
        velocityCC.z = targetHorizontalVelocity.z;
    }


    private void Jump()
    { /* ... unchanged Jump logic ... */
        exitingSlope = true;
        velocityCC.y = Mathf.Sqrt(jumpForce * -2f * GRAVITY);
    }
    private void ResetJump()
    { /* ... unchanged ResetJump logic ... */
        readyToJump = true;
        exitingSlope = false;
    }
    private void ResetSlideStart()
    { /* ... unchanged ResetSlideStart logic ... */
        startedSlide = false;
    }
}