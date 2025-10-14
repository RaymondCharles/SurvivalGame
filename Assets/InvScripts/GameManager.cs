using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject inventory; // The empty GameObject with 2 panels as children

    void Update()
    {
        ToggleUI();
    }

    void ToggleUI()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventory.SetActive(!inventory.activeSelf);

        }
    }
}
