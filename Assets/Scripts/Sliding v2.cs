using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Slidingv2 : MonoBehaviour
{
    [Header("Sliding Settings")]
    public float slideAcceleration = 10f;   // Downhill acceleration
    public float slideDeceleration = 5f;    // Uphill deceleration
    public float gravity = 20f;             // Gravity force
    public float slopeThreshold = 0.1f;     // Minimum slope to count as slide

    private CharacterController controller;
    public Vector3 slideVelocity;
    public Transform orientation;
    public PlayerMovement2 pm;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void StartSlide(Vector3 currentVelocity)
    {
        slideVelocity = currentVelocity;
        Vector3 facingDirection = orientation.forward;
        facingDirection.y = 0f;
        facingDirection.Normalize();

        slideVelocity += (10 * facingDirection);
    }
    
    public void SlideMovement()
    {

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            Vector3 slopeNormal = hit.normal;
            Vector3 slopeDirection = Vector3.Cross(Vector3.Cross(Vector3.up, slopeNormal), slopeNormal).normalized;
            float slopeAngle = Vector3.Angle(Vector3.up, slopeNormal);


            // Flatten the orientation forward vector (ignore vertical tilt)
            Vector3 facingDirection = orientation.forward;
            facingDirection.y = 0f;
            facingDirection.Normalize();



            // Determine alignment of facing vs slope
            float alignment = Vector3.Dot(facingDirection, slopeDirection);

            // Keep only the part of velocity aligned with player facing
            Vector3 projectedVelocity = Vector3.Project(slideVelocity, facingDirection);
            slideVelocity.x = projectedVelocity.x;
            slideVelocity.z = projectedVelocity.z;

            if (slopeAngle >= slopeThreshold)
            {
                // Accelerate along the slope
                slideVelocity += slopeDirection * slideAcceleration * Time.deltaTime * Mathf.Abs(alignment);
            }

            // Apply gravity
            slideVelocity.y -= gravity * Time.deltaTime;

            // Move once per frame
            controller.Move(slideVelocity * Time.deltaTime);
        }
    }



    public void StopSlide()
    {
        pm.velocityCC = slideVelocity;
        slideVelocity = Vector3.zero;
    }
}
