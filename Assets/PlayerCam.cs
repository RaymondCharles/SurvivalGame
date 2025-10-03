using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX; //x sensitivity
    public float sensY; //y sesitivity

    public Transform orientation; //player orientation

    float xRotation; //x rotation of the camera
    float yRotation; //y rotation of the camera

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //Locks the cursor to the middle 
        Cursor.visible = false; //Makes the cursor invisible
    }

    private void Update()
    {
        //get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        //Add x input to the y rotation
        yRotation += mouseX;

        //Subtract y input from the x rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); //So player can look up/down beyond 90 degrees

        //rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);


    }
}
