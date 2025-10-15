using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    // Terrain dimensions
    public int width = 256;
    public int height = 256;
    public int depth = 20;
    public float scale = 20f;

    public void Start()
    {
        // Generate Terrain by calling terrain function
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
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
                heights[x, y] = CalculateHeight(x, y);
            }
        }
        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        // Use Perlin noise to create a height value for the inputted coordinates
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
