using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarControl : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] private float power = 5;
    [SerializeField] private float torque = .5f;
    [SerializeField] private float maxSpeed = 5;

    [SerializeField] private Vector2 movementVector;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector2 movementInput)
    {
        this.movementVector = movementInput;
    }

    private void FixedUpdate()
    {
        if (rb.velocity.magnitude < maxSpeed){
            rb.AddForce(movementVector.y * power * transform.forward);
        }
        rb.AddTorque(movementVector.x * movementVector.y * torque * Vector3.up);
      
    }
}
