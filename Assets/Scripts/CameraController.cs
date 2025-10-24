using UnityEngine;
using UnityEngine.InputSystem; // <<< Added Input System namespace
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public float sensX = 400f;
    public float sensY = 400f;
    public Transform orientation;
    public Transform followTarget;
    // private float yRotation; // Likely not needed directly

    public Transform playerCam; // Might still be useful for initial reference or specific cases
    private bool isThirdPerson = false;
    public Key toggleKey = Key.Q;

    public GameObject GameManager;

    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCam; // First-Person VCam
    private CinemachinePOV pov;

    private void Start()
    {
        if (freeLookCamera == null) freeLookCamera = GetComponentInChildren<CinemachineFreeLook>();
        if (virtualCam == null) virtualCam = GetComponentInChildren<CinemachineVirtualCamera>();

        if (virtualCam != null)
        {
            pov = virtualCam.GetCinemachineComponent<CinemachinePOV>();
            if (pov == null) Debug.LogWarning("FP Virtual Camera is missing CinemachinePOV component!");
        }
        else { Debug.LogError("First Person Virtual Camera (virtualCam) not assigned or found!"); }

        if (freeLookCamera == null) Debug.LogError("Third Person FreeLook Camera (freeLookCamera) not assigned or found!");

        SetSensitivity(sensX, sensY);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (virtualCam != null) virtualCam.Priority = 20;
        if (freeLookCamera != null) freeLookCamera.Priority = 10;
        isThirdPerson = false;
    }

    public void SetSensitivity(float horizontal, float vertical)
    {
        if (freeLookCamera != null)
        {
            freeLookCamera.m_XAxis.m_MaxSpeed = horizontal;
            freeLookCamera.m_YAxis.m_MaxSpeed = vertical / 100;
        }
        if (pov != null)
        {
            pov.m_HorizontalAxis.m_MaxSpeed = horizontal;
            pov.m_VerticalAxis.m_MaxSpeed = vertical;
        }
    }

    void Update()
    {
        // --- NEW INPUT SYSTEM CHECKS ---
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
            Cursor.lockState != CursorLockMode.Locked &&
            GameManager != null && !GameManager.GetComponent<GameManager>().isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleView();
        }
        // --- END NEW INPUT SYSTEM CHECKS ---
    }

    // Corrected LateUpdate
    void LateUpdate()
    {
        if (orientation == null || followTarget == null) return;

        CinemachineBrain brain = CinemachineCore.Instance.GetActiveBrain(0);

        if (brain != null && brain.ActiveVirtualCamera != null)
        {
            float currentYRotation = brain.ActiveVirtualCamera.State.FinalOrientation.eulerAngles.y;
            orientation.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
            followTarget.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }
    }


    void ToggleView()
    {
        if (virtualCam == null || freeLookCamera == null) return;

        isThirdPerson = !isThirdPerson;

        if (isThirdPerson)
        {
            freeLookCamera.Priority = 20;
            virtualCam.Priority = 10;
        }
        else
        {
            virtualCam.Priority = 20;
            freeLookCamera.Priority = 10;
        }
    }

    // Add this function to CameraController.cs
    public bool IsThirdPersonView()
    {
        return isThirdPerson; // Return the current state
    }
}