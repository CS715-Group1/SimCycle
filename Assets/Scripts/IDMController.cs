using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDMController : MonoBehaviour
{
    public float desiredSpeed = 20.0f; // Desired speed of the vehicle
    public float timeGap = 1.5f; // Time gap to the vehicle in front
    public float minDistance = 2.0f; // Minimum following distance
    public float acceleration = 2.0f; // Maximum acceleration
    public float deceleration = 3.0f; // Maximum deceleration
    public float maxSpeed = 20.0f; // Maximum speed

    private Transform targetVehicle; // The vehicle in front
    private float maxDetectionDistance = 50.0f;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDetectionDistance))
        {
            if (hit.collider.CompareTag("Car"))
            {
                // The hit.collider is the vehicle in front.
                targetVehicle = hit.collider.transform;
            }
        }

        // Check if targetVehicle is not null before using it
        if (targetVehicle != null)
        {
            float carAcceleration = CalculateAcceleration();
            UpdateVelocity(carAcceleration);
        }
        else
        {
            // If targetVehicle is null, can implement some default behavior.
            UpdateVelocity(acceleration);
            // rb.velocity = Vector3.zero;
        }
    }

    private float CalculateAcceleration()
    {
        float deltaSpeed = rb.velocity.magnitude - targetVehicle.GetComponent<Rigidbody>().velocity.magnitude;
        float desiredDistance = minDistance + Mathf.Max(0, rb.velocity.magnitude * timeGap + (rb.velocity.magnitude * deltaSpeed) / (2 * Mathf.Sqrt(acceleration * deceleration)));
        float distance = Vector3.Distance(transform.position, targetVehicle.position);

        float desiredAcceleration = acceleration * (1 - Mathf.Pow(rb.velocity.magnitude / desiredSpeed, 4) - Mathf.Pow(desiredDistance / distance, 2));
        desiredAcceleration = Mathf.Clamp(desiredAcceleration, -deceleration, acceleration);

        return desiredAcceleration;
    }

    private void UpdateVelocity(float carAcceleration)
    {
        // Update the vehicle's velocity
        rb.velocity += transform.forward * carAcceleration * Time.deltaTime;

        // Clamp the velocity to the maximum speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }

    // public void SetTargetVehicle(Transform target)
    // {
    //     targetVehicle = target;
    // }
}
