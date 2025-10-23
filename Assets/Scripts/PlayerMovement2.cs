using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement2 : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed = 7f;
    public float sprintSpeed = 10f;

    public float groundDrag;

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

    [Header("Acceleration & Deceleration")]
    // Recommended high rates for realistic, snappy feel
    public float acceleration = 40f; // Adjusted for better control responsiveness
    public float deceleration = 5f;
    public float reverseDecelerationMultiplier = 3f;
    private float currentSpeed;

    [Header("Sliding")]
    private float slideCooldown = 3f;
    private bool startedSlide = false;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode slideKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight = 2.1f;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;

    [Header("References")]
    public Transform orientation;
    public Slidingv2 slidingScript;

    // Physics constant (for stability)
    const float GRAVITY = -20f;
    const float GROUND_STICK = -15f;

    // ---- CharacterController ----
    CharacterController controller;
    public Vector3 velocityCC;


    //Movement variables
    public bool inventoryOpen = false;
    public float horizontalInput;
    public float verticalInput;
    Vector3 moveDirection;
    private Vector3 lastMoveDirection = Vector3.forward;

    public enum MovementState { walking, sprinting, sliding, crouching, air }
    public MovementState state;

    // --- Accessors (REQUIRED BY SLIDING.CS) ---
    public float CurrentMoveSpeed => currentSpeed;
    public void SetCurrentSpeed(float speed) { currentSpeed = speed; }
    public Vector3 GetSlopeNormal() => slopeHit.normal;
    public float GetCurrentSlopeAngle() => slopeHit.normal == Vector3.zero ? 0f : Vector3.Angle(slopeHit.normal, Vector3.up);
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, controller.height * 0.5f + 0.3f, whatIsGround))
        {
            float angle = Vector3.Angle(slopeHit.normal, Vector3.up);
            return angle > 0f && angle <= maxSlopeAngle;
        }
        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 dir) => Vector3.ProjectOnPlane(dir, slopeHit.normal).normalized;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        startYScale = transform.localScale.y;
        readyToJump = true;

        currentSpeed = 0f;
        moveSpeed = walkSpeed;
    }

    private void Update()
    {
        if (!inventoryOpen)
        {
            grounded = controller ? controller.isGrounded : grounded;
            MyInput();
            StateHandler();

            // *** FINAL FIX: Agreesive Safety Check to prevent 0 m/s snap on start ***
            // If the player is giving input but the current speed is very low, snap the speed to walkSpeed.
            if (CurrentMoveSpeed < 0.1f && (horizontalInput != 0f || verticalInput != 0f) && !isSliding)
            {
                currentSpeed = walkSpeed;
            }

            if (!isSliding)
            {
                MovePlayer();
            }
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Capture current movement direction for jump momentum
        if (horizontalInput != 0f || verticalInput != 0f)
        {
            lastMoveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
        }

        if (Input.GetKeyDown(crouchKey))
        {
            if (!controller.isGrounded)
            {
                // Apply stronger gravity when not grounded
                velocityCC.y -= GRAVITY * 2 * Time.deltaTime;
            }

            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        }
        

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

        if (Input.GetKeyDown(slideKey) && !isSliding && !startedSlide)
        {
            startedSlide = true;
            isSliding = true;
            slidingScript.StartSlide(velocityCC); // Pass current momentum
        }
 
        if (Input.GetKey(slideKey) && (horizontalInput != 0f || verticalInput != 0f) && grounded && readyToJump && isSliding)
        {
            slidingScript.SlideMovement();
        }

        if ((Input.GetKeyUp(slideKey) && isSliding) || (!grounded && isSliding))
        {
            isSliding = false;
            slidingScript.StopSlide();
            Invoke(nameof(ResetSlideStart), slideCooldown);
        }

        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            if (isSliding)
            {
                isSliding = false;
                velocityCC = slidingScript.slideVelocity;
                Invoke(nameof(ResetSlideStart), slideCooldown);
            }
            Jump();


            Invoke(nameof(ResetJump), jumpCooldown);

        }
    }

    private void StateHandler()
    {
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
        Vector3 flatVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);

        float inputMagnitude = (new Vector2(horizontalInput, verticalInput)).magnitude;
        float targetSpeed = moveSpeed * inputMagnitude;

        float speedChangeRate;
        float effectiveCurrentSpeed = flatVelocity.magnitude;

        // --- Determine smooth speed transition rate ---

        if (inputMagnitude < 0.01f) // 1. Full Stop (Inertia Deceleration)
        {
            speedChangeRate = deceleration;
        }
        else if (targetSpeed > effectiveCurrentSpeed) // 2. Accelerating or Speeding Up 
        {
            speedChangeRate = acceleration;
        }
        else if (targetSpeed < effectiveCurrentSpeed) // 3. Slowing Down or Transitioning to Slower State
        {
            speedChangeRate = deceleration;
        }
        else // 4. Braking/Reversing
        {
            if (Vector3.Dot(flatVelocity.normalized, moveDirection) < 0 && effectiveCurrentSpeed > 0.1f)
            {
                speedChangeRate = deceleration * reverseDecelerationMultiplier;
            }
            else
            {
                speedChangeRate = acceleration;
            }
        }

        // Apply smoothing universally
        currentSpeed = Mathf.MoveTowards(effectiveCurrentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);

        // Final clamp ensures speed never exceeds the current state's max speed (moveSpeed)
        currentSpeed = Mathf.Max(currentSpeed, moveSpeed);

        // --------------------------------------------------------------------------------

        float horizontalSpeed = currentSpeed;

        // Calculate desired horizontal velocity explicitly
        Vector3 desiredHorizontalVelocity = moveDirection * horizontalSpeed;

        // Apply horizontal velocity to velocityCC for movement
        velocityCC.x = desiredHorizontalVelocity.x;
        velocityCC.z = desiredHorizontalVelocity.z;


        // --- Slope Stability and Vertical Movement ---
        if (grounded && OnSlope() && readyToJump)
        {
            velocityCC.y = GROUND_STICK;

            // Adjust horizontal velocity to hug the slope
            Vector3 slopeDir = GetSlopeMoveDirection(moveDirection);
            Vector3 slopeAlignedHorizontalMovement = slopeDir * horizontalSpeed;

            Vector3 downSlope = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
            float downhillDot = Vector3.Dot(slopeDir, downSlope);
            if (downhillDot > 0f)
            {
                slopeAlignedHorizontalMovement += slopeDir * (downhillDot * 2f * Time.deltaTime);
            }
            // Use slope-aligned horizontal movement for velocityCC
            velocityCC.x = slopeAlignedHorizontalMovement.x;
            velocityCC.z = slopeAlignedHorizontalMovement.z;
        }
        else
        {
            velocityCC.y += GRAVITY * Time.deltaTime;
        }
        // Final motion: Use the separate X, Y, Z components of velocityCC
        Vector3 motion = new Vector3(velocityCC.x, velocityCC.y, velocityCC.z) * Time.deltaTime;
        controller.Move(motion);
    }

    private void Jump()
    {
        // Apply the pure vertical jump impulse. Horizontal momentum is preserved via velocityCC.x/z.
        velocityCC.y = Mathf.Sqrt(jumpForce * 2f * Mathf.Abs(GRAVITY));
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void ResetSlideStart()
    {
        startedSlide = false;
        Debug.Log(startedSlide);
    }
}