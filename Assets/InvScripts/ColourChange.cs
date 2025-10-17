using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourChange : MonoBehaviour
{
    public Material blueMaterial;
    public Material greenMaterial;

    public Renderer characterRenderer;
    private bool isBlue = true;

    void Start()
    {
        characterRenderer = GetComponent<Renderer>();
        characterRenderer.material = blueMaterial;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isBlue)
            {
                characterRenderer.material = greenMaterial;
            }
            else
            {
                characterRenderer.material = blueMaterial;
            }
            isBlue = !isBlue;
        }
    }
}
