using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{

    [Header("References")]
    public GameObject inventory;
    public CinemachineVirtualCamera fpCamera;     // First-person camera
    public CinemachineFreeLook tpCamera;          // Third-person camera
    public Camera mainCamera;                     // The main Unity camera (optional but useful)

    private bool isThirdPerson = false;
    public bool isInventoryOpen = false;

    public GameObject Terrain;
    public GameObject SunflowerNightPrefab;
    private int day = 1;
    private GameObject[] Enemies;
    private int baseEnemyCount = 50;

    //Parameters for Enemy AI Script
    public Transform player;


    //Bake the meshes as soon as the map loads.
    void Start()
    {
        Terrain.GetComponent<TerrainGenerator>().StartWorld();
        Enemies = new GameObject[baseEnemyCount];
        for (int i=0; i < Enemies.Length; i++)
        {
            Vector3 position = Terrain.GetComponent<TerrainGenerator>().GetRandomPointOnTerrain();
            Enemies[i] = SpawnEnemy(SunflowerNightPrefab);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }


    public GameObject SpawnEnemy(GameObject enemyPrefab)
    {
        Vector3 position = Terrain.GetComponent<TerrainGenerator>().GetRandomPointOnTerrain();
        GameObject Enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        Enemy.GetComponent<EnemyAI>().player = player;
        return Enemy;
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventory.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            // 🧭 Unlock cursor + disable camera control
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            DisableCameraInput();
        }
        else
        {
            // 🎮 Restore camera control
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
