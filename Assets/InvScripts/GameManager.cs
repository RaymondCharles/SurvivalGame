using UnityEngine;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    [Header("References")]
    public GameObject inventory;
    public CinemachineVirtualCamera fpCamera;     // First-person camera
    public CinemachineFreeLook tpCamera;          // Third-person camera
    public Camera mainCamera;                     // The main Unity camera (optional but useful)

    private bool isThirdPerson = false;
    public bool isInventoryOpen = false;
    [SerializeField] private PlayerMovement2 pmScript;

    public GameObject Terrain;
    [SerializeField] private List<GameObject> enemyDayPrefabs;


    private string[] biomes = {"Desert","Grass","Snow"};

    [SerializeField] private GameObject[] desertEnemyDayPrefabs;
    [SerializeField] private GameObject[] desertEnemyNightPrefabs;
    [SerializeField] private GameObject[] desertEnvPrefabs;

    [SerializeField] private GameObject[] grassEnemyDayPrefabs;
    [SerializeField] private GameObject[] grassEnemyNightPrefabs;
    [SerializeField] private GameObject[] grassEnvPrefabs;

    [SerializeField] private GameObject[] snowEnemyDayPrefabs;
    [SerializeField] private GameObject[] snowEnemyNightPrefabs;
    [SerializeField] private GameObject[] snowEnvPrefabs;


    public int day = 1;
    private List<GameObject> Enemies = new List<GameObject>();
    private int baseEnemyCount = 40;

    //Parameters for Enemy AI Script
    public Transform player;
    //public PlayerStats playerStats;
    public PlayerVitals playerVitals;

    public enum dayState { day, night }
    public dayState timeOfDay = dayState.night;

    public struct BiomePrefabs
    {
        public GameObject[] enemyDayPrefabs;
        public GameObject[] enemyNightPrefabs;
        public GameObject[] environmentPrefabs;
        public List<GameObject> environmentObjects;
    }

    private BiomePrefabs[] biomePrefabs = new BiomePrefabs[3];

    //Input System References
    private PlayerInputActions playerInputActions;
    private InputAction InventoryAction;


    private void Awake() // Changed from Update to Awake for setup
    {
        playerInputActions = new PlayerInputActions();
        InventoryAction = playerInputActions.Player.ToggleInventory;
    }

    private void OnEnable()
    {
        playerInputActions?.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputActions?.Player.Disable();
    }




    //Bake the meshes as soon as the map loads.
    void Start()
    {
        Terrain.GetComponent<TerrainGenerator>().StartWorld();

        biomePrefabs[0] = new BiomePrefabs
        {
            enemyDayPrefabs = grassEnemyDayPrefabs,
            enemyNightPrefabs = grassEnemyNightPrefabs,
            environmentPrefabs = grassEnvPrefabs,
            environmentObjects = new List<GameObject>()
        };

        biomePrefabs[1] = new BiomePrefabs
        {
            enemyDayPrefabs = desertEnemyDayPrefabs,
            enemyNightPrefabs = desertEnemyNightPrefabs,
            environmentPrefabs = desertEnvPrefabs,
            environmentObjects = new List<GameObject>()
        };
        biomePrefabs[2] = new BiomePrefabs
        {
            enemyDayPrefabs = snowEnemyDayPrefabs,
            enemyNightPrefabs = snowEnemyNightPrefabs,
            environmentPrefabs = snowEnvPrefabs,
            environmentObjects = new List<GameObject>()
        };
    }

    void Update()
    {
        if (InventoryAction.triggered)
        {
            ToggleInventory();
        }
    }

    public void SwitchTime()
    {
        timeOfDay = (timeOfDay == dayState.day) ? dayState.night : dayState.day;
        int enemyCount;
        if (timeOfDay == dayState.night)
        {
            //40% more enemies every night
            enemyCount = Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(1.4f, day));
        }
        else
        {
            //10% more enemies every daytime.
            enemyCount = Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(1.1f, day));
        }
        Debug.Log(enemyCount+ "Total num of enemies per biome");

        while (Enemies.Count > 0)
        {
            Destroy(Enemies[0]);
            Enemies.RemoveAt(0);
        }

        int count = 0; // count to track biome index
        foreach (BiomePrefabs biome in biomePrefabs)
        {
            Debug.Log("Is running" + count);
            GameObject[] enemyPrefabs;
            enemyPrefabs = (timeOfDay == dayState.night) ? biome.enemyNightPrefabs : biome.enemyDayPrefabs;

            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 position = Terrain.GetComponent<TerrainGenerator>().GetRandomPointOnTerrain(count);
                int index = Random.Range(0, (enemyPrefabs.Length - 1));
                Debug.Log(enemyPrefabs);
                Debug.Log(index);
                Enemies.Add(SpawnObject(enemyPrefabs[index], timeOfDay, count));
            }

            GameObject[] environmentPrefabs = biome.environmentPrefabs;
            int environmentObjectCount = Random.Range(40, 60); // 4 to test biome spawning quickly
            environmentObjectCount -= biome.environmentObjects.Count;
            if (environmentObjectCount > 0)
            {
                for (int i = 0; i < environmentObjectCount; i++)
                {
                    Debug.Log("Spawning environment object in biome index: " + count);
                    Vector3 position = Terrain.GetComponent<TerrainGenerator>().GetRandomPointOnTerrain(count);
                    int index = Random.Range(0, (environmentPrefabs.Length - 1));
                    biome.environmentObjects.Add(SpawnObject(environmentPrefabs[index], timeOfDay, count));
                    Debug.Log("Spawned environment object: " + environmentPrefabs[index].name);
                }
            }
            count++;
        }
       
    }


    public GameObject SpawnObject(GameObject prefab, dayState timeOfDay, int biome)
    {
        Vector3 position = Terrain.GetComponent<TerrainGenerator>().GetRandomPointOnTerrain(biome);
        GameObject objectSpawn = Instantiate(prefab, position, Quaternion.identity);
        if (objectSpawn.CompareTag("Enemy"))
        {
            if (timeOfDay == dayState.day)
            {
                objectSpawn.GetComponent<PassiveAI>().player = player;
            }
            else
            {
                objectSpawn.GetComponent<EnemyAI>().player = player;
                objectSpawn.GetComponent<EnemyAI>().playerVitals = playerVitals;
            }
        }
        return objectSpawn;
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventory.SetActive(isInventoryOpen);
        pmScript.inventoryOpen = isInventoryOpen;

        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            DisableCameraInput();
        }
        else
        {
            EnableCameraInput();

            if (!isThirdPerson)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    private void DisableCameraInput()
    {
        if (fpCamera != null)
        {
            var pov = fpCamera.GetCinemachineComponent<CinemachinePOV>();
            if (pov != null)
            {
                pov.m_HorizontalAxis.m_MaxSpeed = 0;
                pov.m_VerticalAxis.m_MaxSpeed = 0;
            }
        }

        if (tpCamera != null)
        {
            tpCamera.m_XAxis.m_MaxSpeed = 0;
            tpCamera.m_YAxis.m_MaxSpeed = 0;
        }
    }

    private void EnableCameraInput()
    {
        if (fpCamera != null)
        {
            var pov = fpCamera.GetCinemachineComponent<CinemachinePOV>();
            if (pov != null)
            {
                pov.m_HorizontalAxis.m_MaxSpeed = 400f; // Restore your sensitivity
                pov.m_VerticalAxis.m_MaxSpeed = 400f;
            }
        }

        if (tpCamera != null)
        {
            tpCamera.m_XAxis.m_MaxSpeed = 400f;
            tpCamera.m_YAxis.m_MaxSpeed = 4f; // Cinemachine FreeLook vertical is usually much lower
        }
    }

    public void SetThirdPerson(bool value)
    {
        isThirdPerson = value;
    }
}