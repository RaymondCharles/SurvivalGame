using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM2 : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed = 7f;
    public float sprintSpeed = 10f;

    // ---- CharacterController ----
    CharacterController controller;
    public Transform orientation;
    Vector3 velocityCC;

    [Header("Ground Check")]
    public float playerHeight = 2.1f;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode slideKey = KeyCode.LeftControl;

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
    public float deceleration = 35f;
    public float reverseDecelerationMultiplier = 3f;
    private float currentSpeed;
    // Physics constant
    const float GRAVITY = -9.81f;


    public float horizontalInput;
    public float verticalInput;
    Vector3 moveDirection;


    public enum MovementState { walking, sprinting, crouching, air }
    public MovementState state;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        startYScale = transform.localScale.y;
        readyToJump = true;

        currentSpeed = 0f;
        moveSpeed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        grounded = controller ? controller.isGrounded : grounded;
        MyInput();
        StateHandler();
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Capture current movement direction for jump momentum
        if (horizontalInput != 0f || verticalInput != 0f)
        {
            moveDirection= (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
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

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void Jump()
    {

        // Apply the pure vertical jump impulse. Horizontal momentum is preserved via velocityCC.x/z.
        velocityCC.y = Mathf.Sqrt(jumpForce * 2f);
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

        float horizontalSpeed = grounded ? currentSpeed : currentSpeed * airMultiplier;

        // Calculate desired horizontal velocity explicitly
        Vector3 desiredHorizontalVelocity = moveDirection * horizontalSpeed;
        velocityCC.x = desiredHorizontalVelocity.x;
        velocityCC.z = desiredHorizontalVelocity.z;


        if (!grounded)
        {
            velocityCC.y += GRAVITY * Time.deltaTime;
        }
        else
        {
            velocityCC.y = 0;
        }

        // Final motion: Use the separate X, Y, Z components of velocityCC
        Vector3 motion = new Vector3(velocityCC.x, velocityCC.y, velocityCC.z) * Time.deltaTime;
        controller.Move(motion);
    }









    private void ResetJump()
    {
        readyToJump = true;
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
}
