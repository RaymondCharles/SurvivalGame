using UnityEngine;

public class VineAttack : MonoBehaviour
{
    // This function runs automatically when the vine's trigger collides with another collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object we hit has the "Player" tag
        if (other.CompareTag("Player"))
        {
            Debug.Log("VINE PUNCH HIT THE PLAYER!");
            // You would put your damage logic here
        }
    }
}