using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDMController : MonoBehaviour
{
    public float maxSpeed = 10f;
    public float maxAcceleration = 2f;
    public float desiredSpeed = 5f;
    public float minDistance = 2f;
    public float timeHeadway = 1.5f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Calculate acceleration based on IDM
        float acceleration = CalculateIDMAcceleration();

        // Update the vehicle's velocity and position
        UpdateVelocity(acceleration);
    }

    private float CalculateIDMAcceleration()
    {
        // Calculate the distance to the vehicle in front
        // Might replace this with vision
        RaycastHit hit;
        float distanceToVehicleInFront = float.MaxValue;

        if (Physics.Raycast(transform.position, transform.forward, out hit, minDistance * 2))
        {
            distanceToVehicleInFront = hit.distance;
        }

        // IDM acceleration formula
        float acceleration = maxAcceleration * (1 - Mathf.Pow(rb.velocity.magnitude / maxSpeed, 4)) -
                             Mathf.Pow(desiredSpeed / Mathf.Max(distanceToVehicleInFront, 0.1f), 2);

        return acceleration;
    }

    private void UpdateVelocity(float acceleration)
    {
        // Update the vehicle's velocity
        rb.velocity += transform.forward * acceleration * Time.deltaTime;

        // Clamp the velocity to the maximum speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }
}

