using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDMController : MonoBehaviour
{
    [SerializeField] private float desiredSpeed = 20.0f; // Desired speed of the vehicle
    [SerializeField] private float timeGap = 1.5f; // Time gap to the vehicle in front
    [SerializeField] private float minDistance = 4.0f; // Minimum following distance
    [SerializeField] private float acceleration = 2.0f; // Maximum acceleration
    [SerializeField] private float deceleration = 3.0f; // Maximum deceleration
    [SerializeField] private float maxSpeed = 20.0f; // Maximum speed
    [SerializeField] private float torque = .5f;
    [SerializeField] private float power = 1f;

    private Rigidbody rb;
    private float currentMaxSpeed;
    public DriveInfo driveInfo;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentMaxSpeed = maxSpeed;
    }

    public void Move(DriveInfo driveInfo)
    {
        this.driveInfo = driveInfo;
    }

    private void FixedUpdate()
    {
        //switch case to get speed needed to successfully make a turn at an intersection
        switch (driveInfo.turn)
        {
            case Turning.LEFT:
                currentMaxSpeed = 2f;
                break;
            case Turning.RIGHT:
                currentMaxSpeed = 3.5f;
                break;
            case Turning.STRAIGHT:
                currentMaxSpeed = 10f;
                break;
        }

        // Check if stoppingPosition is not null before using it
        if (driveInfo.stoppingPosition != null)
        {
            float carAcceleration = CalculateAcceleration();
            UpdateVelocity(carAcceleration);
        }
        else
        {
            
            // If targetVehicle is null, can implement some default behavior.
            UpdateVelocity(acceleration);
        }

        rb.AddTorque(driveInfo.turnDirection * (rb.velocity.magnitude/currentMaxSpeed) * torque * Vector3.up);
    }

    private float CalculateAcceleration()
    {
        float deltaSpeed = rb.velocity.magnitude - driveInfo.matchingSpeed;
        float desiredDistance = minDistance + Mathf.Max(0, rb.velocity.magnitude * timeGap + (rb.velocity.magnitude * deltaSpeed) / (2 * Mathf.Sqrt(acceleration * deceleration)));
        float distance = Vector3.Distance(transform.position, driveInfo.stoppingPosition); 

        float desiredAcceleration = acceleration * (1 - Mathf.Pow(rb.velocity.magnitude / desiredSpeed, 4) - Mathf.Pow(desiredDistance / distance, 2));
        desiredAcceleration = Mathf.Clamp(desiredAcceleration, -deceleration, acceleration);

        return desiredAcceleration;
    }

    private void UpdateVelocity(float carAcceleration)
    {
        // Update the vehicle's velocity
        rb.velocity += power * transform.forward * carAcceleration * Time.deltaTime;
        // Clamp the velocity to the maximum speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, currentMaxSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(driveInfo.stoppingPosition, 1);
    }
}