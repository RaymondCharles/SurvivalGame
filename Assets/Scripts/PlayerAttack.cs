using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackDamage = 10f;
    public float attackDistance = 100f;
    public Camera playerCamera; // Assign your player's camera here

    void Update()
    {
        // Check if the left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray (laser beam) from the center of the camera forward
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            // Fire the ray. If it hits something...
            if (Physics.Raycast(ray, out hit, attackDistance))
            {
                // Try to get the PassiveAI script from the object we hit
                PassiveAI enemy = hit.collider.GetComponent<PassiveAI>();

                // If the object we hit has the PassiveAI script...
                if (enemy != null)
                {
                    // ...tell it to take damage!
                    enemy.TakeDamage(attackDamage);
                    Debug.Log("Hit the passive enemy!");
                }
            }
        }
    }
}