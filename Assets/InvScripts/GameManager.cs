using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject inventory;
    public PlayerCam playerCam;
    

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool newState = !inventory.activeSelf;
            inventory.SetActive(newState);

            if (newState)
            {
                // Inventory opened → unlock cursor + disable camera look
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                playerCam.canLook = false;
            }
            else
            {
                // Inventory closed → restore camera control
                playerCam.canLook = true;

                if (!playerCam.isThirdPerson)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
    }
}
