using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackDamage = 10f;
    public float attackDistance = 100f;
    public Camera playerCamera;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, attackDistance))
            {
                // --- THIS IS THE CORRECTED LOGIC ---
                // We don't care what type of AI it is. We just look for the EnemyHealth component.
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();

                // If the object we hit has an EnemyHealth script...
                if (enemyHealth != null)
                {
                    // ...tell that component to take damage!
                    enemyHealth.TakeDamage(attackDamage);
                    Debug.Log("Hit " + hit.collider.name);
                }
            }
        }
    }
}