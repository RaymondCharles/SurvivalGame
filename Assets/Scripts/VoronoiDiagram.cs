using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoronoiDiagram : MonoBehaviour
{
    [SerializeField] private Color[] cellColors;
    private int imgSize;
    private int numOfCells = 10;
    private int pixelsPerCell;
    private RawImage image;
    private Vector2Int[,] pointsPosArray; // Array to hold cell point positions
    private Color[,] cellColorsArray;


    private void Awake()
    {
        // Fetch RawImage component and determine size
        image = GetComponent<RawImage>();
        imgSize = Mathf.RoundToInt(image.GetComponent<RectTransform>().sizeDelta.x);
    }

    private void Start()
    {
        GenerateVDiagram();
    }

    private void GenerateVDiagram()
    {
        // Create texture, loop through pixels, assign colour according to Voronoi logic
        Texture2D texture = new Texture2D(imgSize, imgSize) { filterMode = FilterMode.Point };
        pixelsPerCell = imgSize / numOfCells;

        // Ensure points are generated before use
        GeneratePoints();

        for (int x = 0; x < imgSize; x++)
        {
            for (int y = 0; y < imgSize; y++)
            {
                texture.SetPixel(x, y, Color.black);
            }
        }
        
        for (int i = 0; i < numOfCells; i++)
        {
            for (int j = 0; j < numOfCells; j++)
            {
                texture.SetPixel(pointsPosArray[i, j].x, pointsPosArray[i, j].y, Color.black);
            }
        }
        texture.Apply();
        image.texture = texture;

        #if UNITY_EDITOR
        // Save PNG into Assets/GeneratedTextures (Editor only)
        string folder = Application.dataPath + "/GeneratedTextures";
        if (!System.IO.Directory.Exists(folder))
            System.IO.Directory.CreateDirectory(folder);

        string fileName = "Voronoi_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string fullPath = System.IO.Path.Combine(folder, fileName);
        System.IO.File.WriteAllBytes(fullPath, texture.EncodeToPNG());

        // Refresh AssetDatabase so the file appears in the Project window
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
    
    private void GeneratePoints()
    {
        pointsPosArray = new Vector2Int[numOfCells, numOfCells];
        cellColorsArray = new Color[numOfCells, numOfCells];
        for (int x = 0; x < numOfCells; x++)
        {
            for (int y = 0; y < numOfCells; y++)
            {
                pointsPosArray[x, y] = new Vector2Int(x * pixelsPerCell + Random.Range(0, pixelsPerCell), y * pixelsPerCell + Random.Range(0, pixelsPerCell)); // Each point is a random position within its cell
                cellColorsArray[x, y] = cellColors[Random.Range(0, cellColors.Length)];// Assign a random color from the array
            }
        }
    }
}
