using UnityEngine;
using Unity.AI.Navigation;

//TODO:
// 1. Add Octaves to create more complex terrain DONE
// 2. Consider if we want to have it generate a full map or more like a chunk system
// 3. Change all fields to properties with getters and setters including validation where necessary and sliders
public class TerrainGenerator : MonoBehaviour
{
    public NavMeshSurface surface;

    // Terrain dimensions - serialized fields with inspector sliders and validated properties
    [Header("Terrain Dimensions")]
    [SerializeField, Range(128, 8192), Tooltip("Width (heightmap samples)")]
    private int width = 2000;
    [SerializeField, Range(128, 8192), Tooltip("Height (heightmap samples)")]
    private int height = 2000;
    [SerializeField, Range(1, 512), Tooltip("Max vertical size")]
    private int depth = 50;
    [SerializeField, Range(0.1f, 500f), Tooltip("Noise scale")]
    private float scale = 20f;
    [SerializeField, Range(1, 16), Tooltip("Number of Perlin noise octaves")]
    private int octaves = 3;

    public int Width  { get => width;  set => width  = Mathf.Max(16, value); }
    public int Height { get => height; set => height = Mathf.Max(16, value); }
    public int Depth  { get => depth;  set => depth  = Mathf.Clamp(value, 1, 10000); }
    public float Scale{ get => scale;  set => scale  = Mathf.Clamp(value, 0.0001f, 10000f); }
    public int Octaves{ get => octaves; set => octaves = Mathf.Clamp(value, 1, 32); }

    // Ensures inspector edits are validated and persisted in the editor
    private void OnValidate()
    {
        width  = Mathf.Max(16, width);
        height = Mathf.Max(16, height);
        depth  = Mathf.Clamp(depth, 1, 10000);
        scale  = Mathf.Clamp(scale, 0.0001f, 10000f);
        octaves= Mathf.Clamp(octaves, 1, 32);
    }
    public float persistance = 0.5f; // Range 0-1: Amplitude multiplier for each octave
    public float lacunarity = 2f; // Frequency multiplier for each octave
    // Offsets for Perlin noise to create random terrain
    public float offsetX = 100f;
    public float offsetY = 100f;
    private Terrain terrain;

    public void StartWorld()
    {
        // Set random offsets for Perlin noise
        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
        // Generate Terrain by calling terrain function
        terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        //Generate meshes for terrain
        surface.BuildNavMesh();
    }

    // Function to generate terrain data based on dimensions
    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);

        // Set heights using a height map
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        // Generate a simple height map using Perlin noise
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float amplitude = 1;
                float frequency = 1;

                for (int i = 0; i < octaves; i++)
                {
                    heights[x, y] += CalculateHeight(x, y, frequency) * amplitude;
                    // Decrease amplitude and increase frequency for next octave
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                heights[x, y] = Mathf.InverseLerp(-1f, 1f, heights[x, y]); // Normalize to 0-1 range
            }
        }
        return heights;
    }

    float CalculateHeight(int x, int y, float freq)
    {
        // Use Perlin noise to create a height value for the inputted coordinates
        float xCoord = (float)x / width * scale * freq + offsetX;
        float yCoord = (float)y / height * scale * freq + offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord) * 2f - 1f;
    }




    public Vector3 GetRandomPointOnTerrain()
    {
        float terrainPosX = terrain.transform.position.x;
        float terrainPosZ = terrain.transform.position.z;

        // Pick a random point in terrain space
        float randomX = Random.Range(0, width);
        float randomZ = Random.Range(0, height);

        // Get height (Y) at that point
        float y = terrain.SampleHeight(new Vector3(randomX + terrainPosX, 0, randomZ + terrainPosZ));

        // Convert to world coordinates
        Vector3 worldPos = new Vector3(randomX + terrainPosX, y, randomZ + terrainPosZ);
        return worldPos;
    }

}
