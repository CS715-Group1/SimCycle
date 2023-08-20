using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentBreakForce;
    private bool isBreaking;

    private Vector3 nextNode;

    [SerializeField] private DriverLogic driverLogic;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteerAngle;

    [SerializeField] private Rigidbody mRigidbody;
    [SerializeField] private float lowerAmount;

    [SerializeField] private WheelCollider backRightWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider backLefttWheelCollider;

    [SerializeField] private Transform backRightWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform backLefttWheelTransform;

    private void Start()
    {
        mRigidbody.centerOfMass -= new Vector3(0, lowerAmount, 0);
        nextNode = driverLogic.getNextNode();
    }

    private void FixedUpdate()
    {   
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        horizontalInput = DirectionToNextNode();//Input.GetAxis(HORIZONTAL);//
        verticalInput = 0.5f;//Input.GetAxis(VERTICAL);//
        DistanceToNode();
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private float DirectionToNextNode()
    {
        Vector3 relativePosition = transform.InverseTransformPoint(nextNode);
        return relativePosition.x / relativePosition.magnitude;

    }

    private void DistanceToNode()
    {
        if(Vector3.Distance(nextNode, transform.position) < 30f)
        {
            if (mRigidbody.velocity.magnitude > 12)
            {
                verticalInput = -0.7f;
            } else if(mRigidbody.velocity.magnitude > 5)
            {
                verticalInput = -1f;
            }
        }
       
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentBreakForce = isBreaking ? breakForce : 0;

        if(isBreaking )
        {
            ApplyBreaking();
        }
    }
    
    private void ApplyBreaking()
    {
        backRightWheelCollider.brakeTorque = currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce;
        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        backLefttWheelCollider.brakeTorque = currentBreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(backRightWheelCollider, backRightWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(backLefttWheelCollider, backLefttWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;

        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.SetPositionAndRotation(pos, rot);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Node"))
        {
            Debug.Log("NEW node");
            nextNode = driverLogic.getNextNode();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(nextNode, new Vector3(2,2,2));
    }
}
