using UnityEngine;
using Unity.AI.Navigation;
using System.Linq; // used for Sum of array
using System.Collections.Generic;

//TODO:
// 1. Add Octaves to create more complex terrain DONE
// 2. Consider if we want to have it generate a full map or more like a chunk system
// 3. Change all fields to properties with getters and setters including validation where necessary and sliders
public class TerrainGenerator : MonoBehaviour
{
    public NavMeshSurface surface;

    // Terrain dimensions - serialized fields with inspector sliders and validated properties
    [Header("Terrain Dimensions")]
    //[SerializeField, Range(128, 8192), Tooltip("Width (heightmap samples)")]
    private int width = 800;
    //[SerializeField, Range(128, 8192), Tooltip("Height (heightmap samples)")]
    private int height = 800;
    //[SerializeField, Range(1, 512), Tooltip("Max vertical size")]
    private int depth = 70;
    //[SerializeField, Range(0.1f, 500f), Tooltip("Noise scale")]
    private float scale = 5f;
    //[SerializeField, Range(1, 16), Tooltip("Number of Perlin noise octaves")]
    private int octaves = 3;

    // Voronoi diagram fields
    [SerializeField] private int[] biomes;
    private int numOfCells = 3;
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
            biomes = new int[] { 0, 1, 2 }; // Default biome types - 0 = grassland, 1 = desert, 2 = snow
        }
    }
    public float persistance = 0.5f; // Range 0-1: Amplitude multiplier for each octave
    public float lacunarity = 2f; // Frequency multiplier for each octave
    // Offsets for Perlin noise to create random terrain
    public float offsetX = 100f;
    public float offsetY = 100f;
    private Terrain terrain;
    public int[,] biomeMap;

    //Point arrays for each biome
    private List<Vector2Int> grassPoints = new List<Vector2Int>();
    private List<Vector2Int> desertPoints = new List<Vector2Int>();
    private List<Vector2Int> snowPoints = new List<Vector2Int>();



    public void StartWorld()
    {
        // Generate Voronoi diagram for biome map
        biomeMap = GenerateVDiagram();
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
        terrainData.SetHeights(0, 0, GenerateHeights(biomeMap));
        // Assign Splat Map based on biome
        AssignSplatMap(terrain, terrain.terrainData, biomeMap);

        return terrainData;
    }

    float[,] GenerateHeights(int[,] biomeMap)
    {
        // Generate a simple height map using Perlin noise and normalize it based on actual min/max values
        float[,] heights = new float[width, height];

        // Track min and max so we can normalize the whole map afterwards
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        int blendRange = 40;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int currBiome = biomeMap[x, y];
                float baseAmplitude = 1f;
                float baseFrequency = 1f;
                int usedOctaves = Octaves;

                if (currBiome == 0) // Grassland biome
                {
                    grassPoints.Add(new Vector2Int(x,y));
                    baseAmplitude = 2.8f;
                    baseFrequency = 3.2f;
                    lacunarity = 0.25f;
                    depth = 80;
                }
                else if (currBiome == 1) // Desert biome
                {
                    desertPoints.Add(new Vector2Int(x,y));
                    baseAmplitude = 0.5f; // Lower amplitude for flatter terrain
                    baseFrequency = 2.5f;   // Higher frequency for more variation
                    usedOctaves = 4;      // local override for desert
                    depth = 50;
                }
                else if (currBiome == 2) // Snow biome
                {
                    snowPoints.Add(new Vector2Int(x,y));
                    baseAmplitude = 3f; // Lower amplitude for flatter terrain
                    baseFrequency = 3f;   // Lower frequency for more variation
                    usedOctaves = 2;      // local override for snow
                    depth = 100;          // Taller terrain for mountains
                    lacunarity = 0.25f;
                }
                
                /***
                // Blend with neighboring biomes within blendRange - expanded to use smooth interpolation
                float nearestDist = blendRange + 1f;
                int neighborBiome = currBiome;

                for (int dx = -blendRange; dx <= blendRange; dx++)
                {
                    for (int dy = -blendRange; dy <= blendRange; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                            continue;

                        int neighbor = biomeMap[nx, ny];
                        if (neighbor != currBiome)
                        {
                            float dist = Mathf.Sqrt(dx * dx + dy * dy);
                            if (dist < nearestDist)
                            {
                                nearestDist = dist;
                                neighborBiome = neighbor;
                            }
                        }
                    }
                }

                // If near another biome, compute a smooth blend factor (1 = at boundary, 0 = far)
                if (nearestDist <= blendRange && neighborBiome != currBiome)
                {
                    float rawBlend = 1f - Mathf.Clamp01(nearestDist / (float)blendRange);
                    float smooth = Mathf.SmoothStep(0f, 1f, rawBlend);

                    // Neighbor biome properties
                    float otherAmp = (neighborBiome == 0) ? 2f : 0.5f;
                    float otherFreq = (neighborBiome == 0) ? 1f : 2f;
                    int otherOctaves = (neighborBiome == 0) ? Octaves : 2;

                    // Blend amplitudes, frequencies and octave count smoothly toward neighbor values near the boundary
                    baseAmplitude = Mathf.Lerp(baseAmplitude, otherAmp, smooth);
                    baseFrequency = Mathf.Lerp(baseFrequency, otherFreq, smooth);
                    usedOctaves = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(usedOctaves, otherOctaves, smooth)));
                }
                
                ***/
                // Generate height using Perlin noise with multiple octaves
                float noiseHeight = 0f;
                float amplitude = baseAmplitude;
                float frequency = baseFrequency;

                for (int i = 0; i < usedOctaves; i++)
                {
                    noiseHeight += CalculateHeight(x, y, frequency) * amplitude;
                    // Decrease amplitude and increase frequency for next octave
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                heights[x, y] = noiseHeight;

                if (noiseHeight < minHeight) minHeight = noiseHeight;
                if (noiseHeight > maxHeight) maxHeight = noiseHeight;
            }
        
        }
        // Normalize using actual min/max to avoid clamped flat areas at extremes
        if (maxHeight > minHeight)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    heights[x, y] = Mathf.InverseLerp(minHeight, maxHeight, heights[x, y]);
                }
            }
        }
        else
        {
            // Degenerate case: all values equal, set to midpoint (or zero)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    heights[x, y] = 0f;
                }
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

    public Vector3 GetRandomPointOnTerrain(int biome)
    {
        // Returns a random point on the terrain within the specified biome type
        terrain ??= GetComponent<Terrain>();
        if (terrain == null || biomeMap == null) return Vector3.zero;

        float terrainPosX = terrain.transform.position.x;
        float terrainPosZ = terrain.transform.position.z;

        List<Vector2Int> Points;
        if (biome == 0)
        {
            Points = grassPoints;
        }
        else if (biome == 1)
        {
            Points = desertPoints;
        }
        else
        {
            Points = snowPoints;
        }
        Vector2Int randomPoint = Points[Random.Range(0, Points.Count - 1)];
        int randomX = randomPoint.x;
        int randomZ = randomPoint.y;

        float y = terrain.SampleHeight(new Vector3((float)randomX + terrainPosX, 0f, (float)randomZ + terrainPosZ));

        Vector3 worldPos = new Vector3((float)randomX + terrainPosX, y + 7f, (float)randomZ + terrainPosZ); // add slight offset to Y to avoid spawning inside terrain
        if (Physics.Raycast(worldPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
        {
            worldPos.y = hit.point.y + 0.1f;
        }

        Debug.Log("Found point at " + worldPos +  " looking for biome:" + biome + " in biome " + biomeMap[randomX, randomZ]);
        return worldPos;
    }

    // Voronoi Diagram Generation
    private int[,] GenerateVDiagram()
    {
        // Loop through pixels, assign rules according to Voronoi logic
        int[,] biomeMap = new int[width, height]; // Currently simple 0 or 1, can be extended for more biomes, as well as adding blending between biomes by using float values
        int cells = Mathf.Max(1, numOfCells);
        pixelsPerCell = Mathf.Max(1, height / numOfCells); // assuming square plane for Voronoi diagram - could need to modify for non-square

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
    void AssignSplatMap (Terrain terrain, TerrainData terrainData, int[,] biomeMap) {
        // Creates Splat map based on biome type at each point on the terrain
        float[, ,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
            {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
             {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y/(float)terrainData.alphamapHeight;
                float x_01 = (float)x/(float)terrainData.alphamapWidth;
                 
                // Sample the height at this location 
                float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapResolution),Mathf.RoundToInt(x_01 * terrainData.heightmapResolution) );
                 
                // Calculate the normal of the terrain 
                Vector3 normal = terrainData.GetInterpolatedNormal(y_01,x_01);

                // Calculate the steepness of the terrain
                float steepness = terrainData.GetSteepness(y_01, x_01);
                
                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];
              
                // Sample the biome for this point using the precomputed biomeMap
                int mapX = Mathf.Clamp(Mathf.RoundToInt(x_01 * (Width - 1)), 0, Width - 1);
                int mapY = Mathf.Clamp(Mathf.RoundToInt(y_01 * (Height - 1)), 0, Height - 1);
                int biome = biomeMap[mapX, mapY];

                // bias texture weights by biome type (0 = grassland, 1 = desert)
                if (biome == 0) // grassland
                {
                    // More grass (texture 0), less sand (texture 1)
                    splatWeights[0] = 1.0f;
                    splatWeights[1] = 0f;
                    splatWeights[2] = 0f;
                }
                else if (biome == 1) // desert
                {
                    // More sand (texture 1), less grass (texture 0)
                    splatWeights[0] = 0f;
                    splatWeights[1] = 1.0f;
                    splatWeights[2] = 0f;
                }
                else if (biome == 2) // snow
                {
                    // More snow (texture 2), less grass (texture 0)
                    splatWeights[0] = 0f;
                    splatWeights[1] = 0f;
                    splatWeights[2] = 1.0f;
                }
                else
                {
                    // default
                    splatWeights[0] = 0.5f;
                    splatWeights[1] = Mathf.Clamp01(terrainData.heightmapResolution - height);
                }
                 
                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = splatWeights.Sum();
                 
                // Loop through each terrain texture
                for(int i = 0; i<terrainData.alphamapLayers; i++){
                     
                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;
                     
                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }
      
        // assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}