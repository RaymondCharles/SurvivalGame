using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed; //Changed inside the script
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag; // not used with CC but kept for compatibility

    [Header("Jumping")]
    public float jumpForce;       // <-- Now treated as "jump height in meters"
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    public float startYScale;

    [Header("Acceleration")]
    public float acceleration = 5f;   // how quickly you speed up
    public float deceleration = 5f;   // how quickly you slow down
    private float currentSpeed;       // stores current speed


    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    // ---- CharacterController replaces Rigidbody ----
    CharacterController controller;
    Vector3 velocityCC;

    Rigidbody rb; // kept so Inspector doesn’t break

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    // Gravity constant (default Unity physics gravity)
    const float GRAVITY = -9.81f;
    const float GROUND_STICK = -2f; // small downward force to stay grounded

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (!controller)
        {
            Debug.LogError("CharacterController missing on Player!");
        }

        rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true; // if Rigidbody exists, disable physics

        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // Grounding check
        grounded = controller ? controller.isGrounded : grounded;

        // Debug ray for visualization
        Debug.DrawRay(
            transform.position,
            Vector3.down * (playerHeight * 0.5f + 0.05f),
            grounded ? Color.green : Color.red
        );

        MyInput();
        SpeedControl();
        StateHandler();

        MovePlayer();
    }

    private void FixedUpdate()
    {
        // Empty when using CharacterController
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump input
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Start crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        }

        // Stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
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
        // Direction
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        // Target speed (0 if no input, otherwise moveSpeed)
        float targetSpeed = (moveDirection.magnitude > 0) ? moveSpeed : 0f;

        // Smoothly accelerate / decelerate
        if (currentSpeed < targetSpeed)
            currentSpeed += acceleration * Time.deltaTime;
        else if (currentSpeed > targetSpeed)
            currentSpeed -= deceleration * Time.deltaTime;

        // Clamp so it doesn’t overshoot
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, moveSpeed);

        // Horizontal movement
        float horizontalSpeed = grounded ? currentSpeed : currentSpeed * airMultiplier;
        Vector3 horizontal = moveDirection * horizontalSpeed;

        // Apply gravity
        if (grounded && velocityCC.y < 0f)
            velocityCC.y = GROUND_STICK;
        velocityCC.y += GRAVITY * Time.deltaTime;

        // Final motion
        Vector3 motion = (horizontal + new Vector3(0f, velocityCC.y, 0f)) * Time.deltaTime;
        controller.Move(motion);
    }


    private void SpeedControl()
    {
        // Not needed for CC, kept for compatibility
    }

    private void Jump()
    {
        exitingSlope = true;

        // Treat jumpForce as "desired jump height in meters"
        velocityCC.y = Mathf.Sqrt(2f * Mathf.Abs(GRAVITY) * Mathf.Max(0.01f, jumpForce));
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (((1 << hit.gameObject.layer) & whatIsGround) != 0)
        {
            grounded = true;
            if (velocityCC.y < 0f) velocityCC.y = GROUND_STICK;
        }
    }

    // Slope functions left as stubs to not break structure
    private bool OnSlope()
    {
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return moveDirection;
    }
}
