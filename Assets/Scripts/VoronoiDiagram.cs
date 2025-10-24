using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO:
// 1. Add noise to cell edges for more natural look
// 2. Optimize point search to reduce computation time
public class VoronoiDiagram : MonoBehaviour
{
    [SerializeField] private Color[] cellColors;
    [SerializeField] private int numOfCells = 10;
    private int imgSize;
    private int pixelsPerCell;
    private RawImage image;
    private Vector2Int[,] pointsPosArray; // Array to hold cell point positions
    private Color[,] cellColorsArray; // Array to hold cell colors


    private void Awake()
    {
        // Fetch RawImage component and determine size
        // Just cache the RawImage. The RectTransform size may not be final at Awake
        image = GetComponent<RawImage>();
    }

    // Check that colors are assigned in the inspector
    private void OnValidate()
    {
        if (cellColors == null || cellColors.Length == 0)
        {
            cellColors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
        }
    }

    private void Start()
    {
        // Force layout rebuild so RectTransform.rect has the correct pixel size
        var rt = image.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        imgSize = Mathf.RoundToInt(rt.rect.width);
        if (imgSize <= 0)
        {
            Debug.LogWarning($"VoronoiDiagram: computed imgSize <= 0 (rect.width={rt.rect.width}). Using fallback 256.");
            imgSize = 256; // fallback to a sane default
        }

        GenerateVDiagram();
    }

    private void GenerateVDiagram()
    {
        // Create texture, loop through pixels, assign colour according to Voronoi logic
        // Ensure we have at least one cell and at least one pixel per cell
        int cells = Mathf.Max(1, numOfCells);
        pixelsPerCell = Mathf.Max(1, imgSize / cells);

        // Create texture without mipmaps (keeps sampling predictable) and with RGBA32 format
        Texture2D texture = new Texture2D(imgSize, imgSize, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };


        // Ensure points are generated before use
        GeneratePoints();
        /*** test code to visualize points only
        for (int x = 0; x < imgSize; x++)
        {
            for (int y = 0; y < imgSize; y++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        for (int i = 0; i < numOfCells; i++)
        {
            for (int j = 0; j < numOfCells; j++)
            {
                texture.SetPixel(pointsPosArray[i, j].x, pointsPosArray[i, j].y, Color.black);
            }
        }
        ***/

        // Loop through each pixel to determine its closest point, and assign color accordingly
        for (int x = 0; x < imgSize; x++)
        {
            for (int y = 0; y < imgSize; y++)
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
                texture.SetPixel(x, y, cellColorsArray[closestCell.x, closestCell.y]);
            }
        }
        texture.Apply();
        image.texture = texture;
        // Save a PNG to persistentDataPath
        byte[] png = texture.EncodeToPNG();
        string path = System.IO.Path.Combine(Application.persistentDataPath, $"voronoi_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
        System.IO.File.WriteAllBytes(path, png);
    }

    private void GeneratePoints()
    {
        pointsPosArray = new Vector2Int[numOfCells, numOfCells];
        cellColorsArray = new Color[numOfCells, numOfCells];
        for (int i = 0; i < numOfCells; i++)
        {
            for (int j = 0; j < numOfCells; j++)
            {
                pointsPosArray[i, j] = new Vector2Int(i * pixelsPerCell + Random.Range(0, pixelsPerCell), j * pixelsPerCell + Random.Range(0, pixelsPerCell)); // Each point is a random position within its cell
                cellColorsArray[i, j] = cellColors[Random.Range(0, cellColors.Length)];// Assign a random color from the array
            }
        }
    }
}
