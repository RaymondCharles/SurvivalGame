using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float sensX = 400f;
    public float sensY = 400f;
    public Transform orientation;
    public Transform followTarget;
    private float yRotation;

    public Transform playerCam;
    public GameObject FPCamera;
    public GameObject TPCamera;
    private bool isThirdPerson = false;
    public KeyCode toggleKey = KeyCode.Q; // *** NEW: Reliable key bind for toggling ***

    public GameObject GameManager;
    


    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    private CinemachinePOV pov;

    private void Start()
    {
        if (freeLookCamera == null)
            freeLookCamera = GetComponent<CinemachineFreeLook>();

        if (virtualCam == null)
            virtualCam = GetComponent<CinemachineVirtualCamera>();

        pov = virtualCam.GetCinemachineComponent<CinemachinePOV>();

        SetSensitivity(sensX, sensY);

        // Lock cursor on start for FPV movement
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //Changes cinemachine sensitivity
    public void SetSensitivity(float horizontal, float vertical)
    {
        freeLookCamera.m_XAxis.m_MaxSpeed = horizontal; // horizontal rotation speed
        freeLookCamera.m_YAxis.m_MaxSpeed = vertical / 100;   // vertical rotation speed
        pov.m_HorizontalAxis.m_MaxSpeed = horizontal;
        pov.m_VerticalAxis.m_MaxSpeed = vertical;
    }

    // Update is called once per frame
    void Update()
    {
        // 1. Cursor Management for Resuming Control (Click to lock)
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked && !GameManager.GetComponent<GameManager>().isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 2. Guaranteed Toggle Check
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleView();
        }
    }

    void LateUpdate()
    {
        Vector3 yawOnly = new Vector3(0f, playerCam.eulerAngles.y, 0f);
        orientation.rotation = Quaternion.Euler(yawOnly);
        followTarget.rotation = Quaternion.Euler(yawOnly);

    }


    // Public method called by the UI button (or Q key)
    void ToggleView()
    {
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
}
