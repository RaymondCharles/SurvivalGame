using UnityEngine;

public class Sliding : MonoBehaviour
{
    // ... (All variable declarations remain the same as the previous response) ...
    // NOTE: Copy all variable declarations from the previous successful Sliding.cs file.

    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private CharacterController controller;
    private PlayerMovement pm;

    [Header("Sliding Settings")]
    public float maxSlideTime = 2f;
    public float slideForce = 15f;
    public float slideFriction = 3f;
    public float maxSlideSpeedMultiplier = 3.5f;
    public float downhillAccelMultiplier = 0.5f;
    public float gravityDownhillFactor = 10f;

    [Header("Scale")]
    public float slideYScale = 0.5f;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;
    private bool sliding;
    private float slideSpeed;
    private float slideTimer;
    private Vector3 lastMoveDir;

    [Header("Coast Settings")]
    public float coastDuration = 1.8f;
    public float coastSlowdownRate = 4f; // Base rate
    public float maxCoastDuration = 3.0f; // NEW: Cap the max time inertia can last
    private bool coasting;
    private float coastTimer;
    private float coastSpeed;
    private Vector3 coastDir;

    private const float GROUND_STICK = -15f;

    [Header("Debug")]
    public bool showDebugSpeed = true;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        pm = GetComponent<PlayerMovement>();
        if (playerObj) startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0f || verticalInput != 0f))
            StartSlide();

        if (Input.GetKeyUp(slideKey) && sliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (sliding)
            SlidingMovement();
        else if (coasting)
            CoastMovement();
    }

    private void StartSlide()
    {
        if (!controller || !pm) return;
        coasting = false;
        sliding = true;

        pm.isSliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);

        float currentHorizontalSpeed = pm.CurrentMoveSpeed;
        slideSpeed = Mathf.Max(currentHorizontalSpeed, pm.sprintSpeed) + slideForce;

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        // ... (Logic remains the same) ...

        if (!controller || !pm) return;

        Vector3 inputDir = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
        Vector3 moveDir = inputDir.sqrMagnitude > 0 ? inputDir : (lastMoveDir.sqrMagnitude > 0 ? lastMoveDir : orientation.forward);

        float currentFlatSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

        bool onSlope = pm.OnSlope();
        float maxAllowed = pm.sprintSpeed * maxSlideSpeedMultiplier;

        if (onSlope)
        {
            Vector3 slopeNormal = pm.GetSlopeNormal();
            Vector3 gravityProjection = Vector3.ProjectOnPlane(Physics.gravity, slopeNormal);
            float downhillDot = Vector3.Dot(moveDir, gravityProjection.normalized);

            float gravityPull = gravityProjection.magnitude * gravityDownhillFactor;
            float speedSquaredAccel = currentFlatSpeed * currentFlatSpeed * downhillAccelMultiplier;

            float effectiveAccel = (gravityPull + speedSquaredAccel) * downhillDot;
            float frictionAmount = slideFriction;

            slideSpeed += (effectiveAccel * Time.fixedDeltaTime) - (frictionAmount * Time.fixedDeltaTime);

            moveDir = pm.GetSlopeMoveDirection(moveDir);
        }
        else
        {
            slideSpeed = Mathf.MoveTowards(slideSpeed, pm.sprintSpeed, slideFriction * Time.fixedDeltaTime);
            slideSpeed = Mathf.Min(slideSpeed, maxAllowed);
        }

        Vector3 vel = moveDir * slideSpeed + Vector3.up * GROUND_STICK;
        controller.Move(vel * Time.fixedDeltaTime);

        lastMoveDir = moveDir;

        slideTimer -= Time.fixedDeltaTime;
        if (slideTimer <= 0f && !onSlope)
            StopSlide();
    }

    private void CoastMovement()
    {
        if (!controller || !pm) return;
        if (coastTimer <= 0f) { coasting = false; return; }

        float sprint = pm.sprintSpeed;

        // Slowdown gradually to sprint speed
        coastSpeed = Mathf.MoveTowards(coastSpeed, sprint, coastSlowdownRate * Time.fixedDeltaTime);

        // Pass coast speed to PlayerMovement to transition inertia correctly
        if (pm) pm.SetCurrentSpeed(coastSpeed);

        Vector3 vel = coastDir * coastSpeed + Vector3.up * GROUND_STICK;
        controller.Move(vel * Time.fixedDeltaTime);

        coastTimer -= Time.fixedDeltaTime;

        if (coastSpeed <= sprint + 0.1f)
            coasting = false;
    }

    private void StopSlide()
    {
        if (!sliding) return;

        sliding = false;

        // CRUCIAL: Transfer the high momentum speed back to the PlayerMovement script
        if (pm) pm.SetCurrentSpeed(slideSpeed);

        pm.isSliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);

        // *** FIX 3: Adjust coast duration based on speed for smoother transition ***
        float excessSpeed = slideSpeed - pm.sprintSpeed;

        // Calculate a dynamic duration that scales with excess speed, capped by maxCoastDuration
        coastDuration = Mathf.Clamp(excessSpeed / (pm.sprintSpeed * 0.5f), 0.5f, maxCoastDuration);

        coasting = true;
        coastDir = (lastMoveDir.sqrMagnitude > 0.0001f) ? lastMoveDir : orientation.forward;
        coastSpeed = slideSpeed;
        coastTimer = coastDuration;
    }

    private void OnGUI()
    {
        if (showDebugSpeed && pm != null)
        {
            GUI.skin.label.fontSize = 20;
            string status = pm.isSliding ? (sliding ? "SLIDING" : "COASTING") : "IDLE";

            float currentSpeed = pm.CurrentMoveSpeed;

            GUI.Label(new Rect(10, 10, 300, 30), $"Status: {status}");
            GUI.Label(new Rect(10, 40, 300, 30), $"Speed: {currentSpeed:F2} m/s");
        }
    }
}