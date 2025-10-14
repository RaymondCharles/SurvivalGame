using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform playerRoot;
    public PlayerCam playerCamScript;

    public float verticalOffset = 1.2f;

    [Header("TPV Collision")]
    public LayerMask collisionMask; // Set this in Inspector to ignore the player object
    public float minDistance = 0.5f; // Closest the camera can get to the player

    private void Update()
    {
        if (playerRoot == null || playerCamScript == null) return;

        float distance = playerCamScript.GetCurrentDistance();

        // 1. Define the true target point (Player's world position + Vertical Offset)
        Vector3 targetCenter = playerRoot.position + Vector3.up * verticalOffset;

        // 2. Get player's direction
        Vector3 playerForwardDirection = playerCamScript.orientation.forward;

        // 3. Calculate the ideal TPV position (without collision check)
        Vector3 idealOffset = -playerForwardDirection * distance;
        Vector3 idealPosition = targetCenter + idealOffset;

        // 4. *** CRITICAL: CAMERA COLLISION CHECK ***
        // Raycast from the target center back toward the ideal position
        Vector3 rayDirection = idealOffset.normalized;
        float rayDistance = idealOffset.magnitude;

        if (Physics.Raycast(targetCenter, rayDirection, out RaycastHit hit, rayDistance, collisionMask))
        {
            // If the ray hits something, shorten the distance
            // Use the distance to the hit point minus a small safety buffer
            float collisionDistance = hit.distance - minDistance;

            // Recalculate the position using the new, shorter distance
            Vector3 collisionOffset = rayDirection * collisionDistance;
            transform.position = targetCenter + collisionOffset;
        }
        else
        {
            // No collision: Use the ideal position
            transform.position = idealPosition;
        }

        // 5. Ensure the camera always looks at the target center
        transform.LookAt(targetCenter);
    }
}