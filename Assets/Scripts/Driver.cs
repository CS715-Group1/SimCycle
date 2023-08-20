using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Driver : MonoBehaviour
{
    Rigidbody rigidbody;
    [SerializeField] private float speed = 10;
    private float forwardAmount;
    private float turnAmount;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }


    private void FixedUpdate() 
    {

        transform.position += transform.forward * speed * forwardAmount * Time.deltaTime;
        transform.eulerAngles += new Vector3(0, turnAmount, 0);
    }

    public void SetInputs(float forwardAmount, float turnAmount)
    {
        this.forwardAmount = forwardAmount;
        this.turnAmount = turnAmount;
    }
}
