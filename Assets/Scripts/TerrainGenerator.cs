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

    // Voronoi diagram fields
    [SerializeField] private int[] biomes;
    [SerializeField] private int numOfCells = 10;
    private int imgSize;
    private int pixelsPerCell;
    private Vector2Int[,] pointsPosArray; // Array to hold cell point positions
    private int[,] cellBiomesArray; // Array to hold cell biomes


    public int Width { get => width; set => width = Mathf.Max(16, value); }
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
        octaves = Mathf.Clamp(octaves, 1, 32);
    
        if (biomes == null || biomes.Length == 0)
        {
            biomes = new int[] { 0, 1 }; // Default biome types - 0 = grassland, 1 = desert
        }
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

        terrainData.SetHeights(0, 0, GenerateHeights(GenerateVDiagram()));

        return terrainData;
    }

    float[,] GenerateHeights(int[,] biomeMap)
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
                heights[x, y] += biomeMap[x, y]; // Add exaggerated height offset based on biome type
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

    // Voronoi Diagram Generation
    private int[,] GenerateVDiagram()
    {
        // Loop through pixels, assign rules according to Voronoi logic
        int[,] biomeMap = new int[width, height]; // Currently simple 0 or 1, can be extended for more biomes, as well as adding blending between biomes by using float values
        // Ensure we have at least one cell and at least one pixel per cell
        int cells = Mathf.Max(1, numOfCells);
        pixelsPerCell = Mathf.Max(1, height / cells); // assuming square plane for Voronoi diagram

        // Ensure points are generated before use
        GeneratePoints();

        // Loop through each pixel to determine its closest point, and assign color accordingly
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Get the grid position of the current pixel
                int gridX = x / pixelsPerCell;
                int gridY = y / pixelsPerCell;

                float closestDist = Mathf.Infinity;
                Vector2Int closestCell = new Vector2Int();

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        // Calculate the pixel coordinates
                        int X = gridX + i;
                        int Y = gridY + j;
                        // Check if the pixel is within bounds
                        if (X < 0 || Y < 0 || X >= numOfCells || Y >= numOfCells) continue;

                        // Create Vector for distance calculation
                        float distance = Vector2Int.Distance(new Vector2Int(x, y), pointsPosArray[X, Y]);

                        // Once loop exits, we have the closest cell
                        if (distance < closestDist)
                        {
                            closestDist = distance;
                            closestCell = new Vector2Int(X, Y);
                        }
                    }
                }
                // Once looped through all nearby points, assign color of closest cell
                biomeMap[x, y] = cellBiomesArray[closestCell.x, closestCell.y];
            }
        }
        return biomeMap;
    }

    private void GeneratePoints()
    {
        // Generate Array of random points within each cell, and assign each cell a random biome from the array
        pointsPosArray = new Vector2Int[numOfCells, numOfCells];
        cellBiomesArray = new int[numOfCells, numOfCells];
        for (int i = 0; i < numOfCells; i++)
        {
            for (int j = 0; j < numOfCells; j++)
            {
                pointsPosArray[i, j] = new Vector2Int(i * pixelsPerCell + Random.Range(0, pixelsPerCell), j * pixelsPerCell + Random.Range(0, pixelsPerCell)); // Each point is a random position within its cell
                cellBiomesArray[i, j] = biomes[Random.Range(0, biomes.Length)];// Assign a random biome from the array
            }
        }
    }

}
