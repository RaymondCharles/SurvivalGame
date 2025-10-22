using UnityEngine;
using Unity.AI.Navigation;

//TODO:
// 1. Add Octaves to create more complex terrain DONE
// 2. Consider if we want to have it generate a full map or more like a chunk system
// 3. Change all fields to properties with getters and setters including validation where necessary and sliders
public class TerrainGenerator : MonoBehaviour
{
    public NavMeshSurface surface;

    // Terrain dimensions
    public int width = 2000;
    public int height = 2000;
    public int depth = 50;
    public float scale = 20f;
    public int octaves = 3; // Number of layers of Perlin noise
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
