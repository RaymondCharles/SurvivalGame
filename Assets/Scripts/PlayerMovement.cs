using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    //Movement  
    private float moveSpeed;
    public float walkSpeed = 7f;
    public float sprintSpeed = 10f;

    public float groundDrag;

    public bool isSliding = false;

    //Jumping
    public float jumpForce = 5f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    bool readyToJump;

    //Crouch
    public float crouchSpeed = 3.5f;
    public float crouchYScale = 0.5f;
    public float startYScale;

    //Accel & Decel
    // Recommended high rates for realistic, snappy feel
    public float acceleration = 40f; // Adjusted for better control responsiveness
    public float deceleration = 35f;
    public float reverseDecelerationMultiplier = 3f;
    private float currentSpeed;


    
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    
    
    public float playerHeight = 2.1f;
    public LayerMask whatIsGround;
    bool grounded;

    
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    
    public Transform orientation;

    // Physics constant (for stability)
    const float GRAVITY = -9.81f;
    const float GROUND_STICK = -15f;

    // ---- CharacterController ----
    CharacterController controller;
    Vector3 velocityCC;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    private Vector3 lastMoveDirection = Vector3.forward;

    public enum MovementState { walking, sprinting, crouching, air }
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
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();

            // Inject the full intended speed + boost
            float jumpMomentumSpeed = moveSpeed + 0.5f;

            velocityCC.x = lastMoveDirection.x * jumpMomentumSpeed;
            velocityCC.z = lastMoveDirection.z * jumpMomentumSpeed;

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
        currentSpeed = Mathf.Min(currentSpeed, moveSpeed);

        // --------------------------------------------------------------------------------

        float horizontalSpeed = grounded ? currentSpeed : currentSpeed * airMultiplier;

        // Calculate desired horizontal velocity explicitly
        Vector3 desiredHorizontalVelocity = moveDirection * horizontalSpeed;

        // Apply horizontal velocity to velocityCC for movement
        velocityCC.x = desiredHorizontalVelocity.x;
        velocityCC.z = desiredHorizontalVelocity.z;


        // --- Slope Stability and Vertical Movement ---
        if (grounded && OnSlope())
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
            // Apply gravity
            if (grounded && velocityCC.y < 0f)
                velocityCC.y = GROUND_STICK;
            velocityCC.y += GRAVITY * Time.deltaTime;
        }

        // Final motion: Use the separate X, Y, Z components of velocityCC
        Vector3 motion = new Vector3(velocityCC.x, velocityCC.y, velocityCC.z) * Time.deltaTime;
        controller.Move(motion);
    }

    private void Jump()
    {
        exitingSlope = true;
        // Apply the pure vertical jump impulse. Horizontal momentum is preserved via velocityCC.x/z.
        velocityCC.y = Mathf.Sqrt(jumpForce * 2f * Mathf.Abs(GRAVITY));
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}