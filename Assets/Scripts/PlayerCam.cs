using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [Header("Camera Settings")]
    public float sensX = 400f;
    public float sensY = 400f;
    public Transform orientation;

    [Header("View Toggle")]
    public float thirdPersonDistance = 3.5f; // Good distance for TPV
    public float firstPersonDistance = 0.0f; // CRITICAL: This must be 0.0f
    public float switchSpeed = 10f;
    public bool isThirdPerson = false; // Starts in FPV
    public KeyCode toggleKey = KeyCode.Q; // *** NEW: Reliable key bind for toggling ***

    private float targetDistance;
    private float currentDistance;

    private float xRotation;
    private float yRotation;
    public bool canLook = true; // N




    private void Start()
    {
        // Lock cursor on start for FPV movement
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure distance starts at FPV distance (0.0f)
        isThirdPerson = false;
        targetDistance = firstPersonDistance;
        currentDistance = firstPersonDistance;
    }

    //private void Update()
    //{
    //    // 1. Cursor Management for Resuming Control (Click to lock)
    //    if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
    //    {
    //        Cursor.lockState = CursorLockMode.Locked;
    //        Cursor.visible = false;
    //    }

    //    // 2. Guaranteed Toggle Check
    //    if (Input.GetKeyDown(toggleKey))
    //    {
    //        ToggleView();
    //    }


    //    // 3. Get Mouse Input (Only process if cursor is locked for movement)
    //    if (Cursor.lockState == CursorLockMode.Locked)
    //    {
    //        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
    //        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

    //        yRotation += mouseX;
    //        xRotation -= mouseY;
    //        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    //    }

    //    // 4. Rotate Camera and Orientation
    //    transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    //    orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    //}

    private void Update()
    {
        // 1. Cursor Management (Only if camera is active)
        if (canLook && Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 2. Toggle View (Q key)
        if (canLook && Input.GetKeyDown(toggleKey))
        {
            ToggleView();
        }

        // 3. Mouse Input (Only if active)
        if (canLook && Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        }

        // 4. Rotate Camera and Orientation (still runs)
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }


    private void FixedUpdate()
    {
        // Smoothly update current distance
        targetDistance = isThirdPerson ? thirdPersonDistance : firstPersonDistance;
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.fixedDeltaTime * switchSpeed);

        // Snap to target if very close
        if (Mathf.Abs(currentDistance - targetDistance) < 0.01f)
        {
            currentDistance = targetDistance;
        }
    }

    // Public method called by the UI button (or Q key)
    public void ToggleView()
    {
        isThirdPerson = !isThirdPerson;

        if (!isThirdPerson)
        {
            // Returning to FPV: Lock cursor for movement control
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Entering TPV: Unlock cursor so user can interact with UI/menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Public getter for MoveCamera.cs
    public float GetCurrentDistance()
    {
        return currentDistance;
    }
}