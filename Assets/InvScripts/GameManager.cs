using UnityEngine;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem; // <<< Added

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GameObject inventory;
    public CinemachineVirtualCamera fpCamera;
    public CinemachineFreeLook tpCamera;
    public Camera mainCamera;

    private bool isThirdPerson = false;
    public bool isInventoryOpen = false;
    [SerializeField] private PlayerMovement2 pmScript;

    public GameObject Terrain;
    // (Keep all your biome prefab arrays)
    [SerializeField] private GameObject[] desertEnemyDayPrefabs;
    [SerializeField] private GameObject[] desertEnemyNightPrefabs;
    [SerializeField] private GameObject[] desertEnvPrefabs;
    [SerializeField] private GameObject[] grassEnemyDayPrefabs;
    [SerializeField] private GameObject[] grassEnemyNightPrefabs;
    [SerializeField] private GameObject[] grassEnvPrefabs;
    [SerializeField] private GameObject[] snowEnemyDayPrefabs;
    [SerializeField] private GameObject[] snowEnemyNightPrefabs;
    [SerializeField] private GameObject[] snowEnvPrefabs;

    public int day = 0;
    private List<GameObject> Enemies = new List<GameObject>();
    private List<GameObject> EnvironmentObjects = new List<GameObject>();
    private int baseEnemyCount = 20;

    public Transform player;
    // public PlayerStats playerStats; // Consider using PlayerVitals if you have it

    public enum dayState { day, night }
    public dayState timeOfDay;

    public struct BiomePrefabs
    { /* ... struct definition ... */
        public GameObject[] enemyDayPrefabs;
        public GameObject[] enemyNightPrefabs;
        public GameObject[] environmentPrefabs;
    }
    private BiomePrefabs[] biomePrefabs = new BiomePrefabs[3];

    // --- New Input System References ---
    private PlayerInputActions playerInputActions;
    private InputAction toggleInventoryAction;
    // ---

    private void Awake() // Changed from Start to Awake for input setup
    {
        // --- Input System Setup ---
        playerInputActions = new PlayerInputActions();
        toggleInventoryAction = playerInputActions.Player.ToggleInventory;
        // ---

        // (Keep your existing biome setup logic from Start)
        Terrain.GetComponent<TerrainGenerator>().StartWorld();
        biomePrefabs[0] = new BiomePrefabs
        { /* ... desert ... */
            enemyDayPrefabs = desertEnemyDayPrefabs,
            enemyNightPrefabs = desertEnemyNightPrefabs,
            environmentPrefabs = desertEnvPrefabs
        };
        biomePrefabs[1] = new BiomePrefabs
        { /* ... grass ... */
            enemyDayPrefabs = grassEnemyDayPrefabs,
            enemyNightPrefabs = grassEnemyNightPrefabs,
            environmentPrefabs = grassEnvPrefabs
        };
        biomePrefabs[2] = new BiomePrefabs
        { /* ... snow ... */
            enemyDayPrefabs = snowEnemyDayPrefabs,
            enemyNightPrefabs = snowEnemyNightPrefabs,
            environmentPrefabs = snowEnvPrefabs
        };
    }

    private void OnEnable()
    {
        playerInputActions?.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputActions?.Player.Disable();
    }
    // Removed Start as logic moved to Awake

    void Update()
    {
        // <<< CHANGE: Check the toggle inventory action >>>
        if (toggleInventoryAction.triggered)
        {
            ToggleInventory();
        }
    }

    // (SwitchTime and SpawnObject methods remain unchanged)
    public void SwitchTime() { /* ... your existing logic ... */ }
    public GameObject SpawnObject(GameObject prefab, dayState timeOfDay) { /* ... your existing logic ... */ return null; } // Added return null to satisfy compiler if needed

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventory != null) inventory.SetActive(isInventoryOpen);
        if (pmScript != null) pmScript.inventoryOpen = isInventoryOpen;

        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            DisableCameraInput();
        }
        else
        {
            EnableCameraInput();
            // Cursor lock state depends on whether player is currently in TP or FP view
            // Assumes CameraController handles switching isThirdPerson flag correctly
            CameraController camController = FindObjectOfType<CameraController>(); // Or use a direct reference if possible
            if (camController != null && !camController.IsThirdPersonView()) // Need a way to check current view
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else // In third person or no CameraController found, keep cursor free
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    // (DisableCameraInput and EnableCameraInput methods remain unchanged)
    private void DisableCameraInput() { /* ... your existing logic ... */ }
    private void EnableCameraInput() { /* ... your existing logic ... */ }

    // (SetThirdPerson method remains unchanged, though might be better in CameraController)
    public void SetThirdPerson(bool value) { isThirdPerson = value; }

    // Helper method for ToggleInventory to check current camera state
    // This assumes your CameraController has a public bool property or method
    // Example: public bool IsThirdPersonView() => isThirdPerson;
    // Add such a method to CameraController.cs if needed.
}