using UnityEngine;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

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
    public GameObject SunflowerNightPrefab;
    [SerializeField] private List<GameObject> enemyDayPrefabs;
    [SerializeField] private List<GameObject> enemyNightPrefabs;
    private int day = 1;
    private List<GameObject> Enemies = new List<GameObject>();
    private int baseEnemyCount = 50;

    //Parameters for Enemy AI Script
    public Transform player;

    public enum dayState { day, night }
    public dayState timeOfDay;


    //Bake the meshes as soon as the map loads.
    void Start()
    {
        Terrain.GetComponent<TerrainGenerator>().StartWorld();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    public void SwitchTime()
    {
        while (Enemies.Count > 0)
        {
            Destroy(Enemies[0]);
            Enemies.RemoveAt(0);
        }


        timeOfDay = (timeOfDay == dayState.day) ? dayState.night : dayState.day;
        List<GameObject> enemyPrefabs;
        int enemyCount;
        if (timeOfDay == dayState.night)
        {
            //50% more enemies every night
            enemyCount = Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(1.5f, day));
            enemyPrefabs = enemyNightPrefabs;
        }
        else
        {
            //20% more enemies every daytime.
            enemyCount = Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(1.2f, day));
            enemyPrefabs = enemyDayPrefabs;
        }

        for (int i=0; i < enemyCount; i++)
        {
            Vector3 position = Terrain.GetComponent<TerrainGenerator>().GetRandomPointOnTerrain();
            int index = Random.Range(0, enemyPrefabs.Count);
            Enemies.Add(SpawnEnemy(enemyPrefabs[index], timeOfDay));
        }
    }


    public GameObject SpawnEnemy(GameObject enemyPrefab, dayState timeOfDay)
    {
        Vector3 position = Terrain.GetComponent<TerrainGenerator>().GetRandomPointOnTerrain();
        GameObject Enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        if (timeOfDay == dayState.day)
        {
            Enemy.GetComponent<PassiveAI>().player = player;
        }
        else
        {
            Enemy.GetComponent<EnemyAI>().player = player;
        }
        return Enemy;
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
        isThirdPerson = value;
    }
}
