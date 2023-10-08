using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.Events;

public enum Turning
{
    RIGHT,
    LEFT,
    STRAIGHT,
    BLOCKED,
    NONE
}


[Serializable]
public class DriveInfo
{
    public Vector3 stoppingPosition;
    public float matchingSpeed;
    public Vector2 turning;
    public float turnDirection;
    public Turning turn;
}

[Serializable]
public class CarAI : MonoBehaviour
{
    private List<Target> path = null;
    private int index = 0;

    private List<Transform> vertexPath = null;
    private int vertexIndex = 0;
    private Vector3 stoppingPos;


    public Queue<Turning> turnQueue = new();
    public Turning nextTurn;

    [SerializeField] private float arriveDistance = 1.5f, lastPointArriveDistance = 2f;
    [SerializeField] private float turningAngleOffest = 5;
    [SerializeField] private Target currentTarget;
    [SerializeField] private Transform raycastStart;
    [SerializeField] private bool useVision;
    private float maxDistance = 2f;
    private float maxDetectionDistance = 20.0f;
    private DriveInfo driveInfo = new();
    public Transform vertex;
    private bool approaching;
    private IntersectionLogic intersectionLogic;
    public IntersectionLogic IntersectionLogic 
    { 
        get { return intersectionLogic; }
        set { intersectionLogic = value; } 
    }

    public bool takingIntersection = false;
    public bool blocked = false;


    public List<CarAI> carsSeen = new();

    private bool stop;
    private bool collisionStop;

    public bool Stop
    {
        get { return stop || collisionStop; }
        set { stop = value; }
    }

    [field: SerializeField]
    public UnityEvent<DriveInfo> OnDrive { get; set; }

    private void Start()
    {
        if (path == null || path.Count == 0)
        {
            Debug.Log("No intial path");
            Stop = true;
        }
        else
        {
            currentTarget = path[index];
        }
    }

    public void SetPath(List<Target> path, List<Transform> vertexPath)
    {
        if(path.Count == 0)
        {
            Debug.Log("No Path");
            Destroy(gameObject);
            return;
        }

        this.path = path;
        index = 0;
        currentTarget = this.path[index];   

        this.vertexPath = vertexPath;
        vertexIndex = 1;
        stoppingPos = vertexPath[vertexIndex].position + new Vector3(3.5f, 0, 3.5f);
        vertex = vertexPath[vertexIndex];

        Vector3 relativePoint = transform.InverseTransformPoint(this.path[index + 1].transform.position);

        float angle = Mathf.Atan2(relativePoint.x, relativePoint.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0,angle, 0);
        Stop = false;
        MakeTurnList();
        nextTurn = turnQueue.Dequeue();
    }

    private void MakeTurnList()
    {
        for (int i = 0; i < path.Count; i++)
        {
            Target target = path[i];

            if (target.laneEnd && i != path.Count - 1 && i != 0 && !path[i-1].laneEnd)
            {
                Vector3 approachingVector = target.transform.position - path[i - 1].transform.position;
                Vector3 leavingVector = path[i + 1].transform.position - target.transform.position;

                float nextAngle = Vector3.SignedAngle(approachingVector, leavingVector, Vector3.up);

                if (nextAngle > 20f)
                {
                    turnQueue.Enqueue(Turning.RIGHT);
                }
                else if (nextAngle < -20f)
                {
                    turnQueue.Enqueue(Turning.LEFT);
                }
                else
                {
                    turnQueue.Enqueue(Turning.STRAIGHT);
                }
            }
        }
    }

    private void Update()
    {
        CheckIfArrived();
        CheckForCollisions(); 
        Drive();
    }

    private void CheckForCollisions()
    {
        //get the distance between the vehicle and the next intersection to check for need to slow down
        float distanceToIntersection = Vector3.Distance(stoppingPos, transform.position);
        RaycastHit hit;
        if ( !(approaching) && Physics.Raycast(raycastStart.position, transform.forward, out hit, maxDetectionDistance))
        {
            if (hit.collider.CompareTag("Car") && hit.distance < distanceToIntersection)
            {
                // The hit.collider is the vehicle in front.
                Debug.Log("Struck car");
                driveInfo.matchingSpeed = hit.transform.GetComponent<Rigidbody>().velocity.magnitude;   
                driveInfo.stoppingPosition = hit.transform.position;
            } else
            {
                driveInfo.stoppingPosition = stoppingPos;
                driveInfo.matchingSpeed = 0;
            }
        }
        else
        {
            driveInfo.stoppingPosition = stoppingPos;
            driveInfo.matchingSpeed = 0;
        }

        //if (!takingIntersection && Physics.Raycast(raycastStart.position, transform.forward, maxDistance, layerMask))
        //{
        //    collisionStop = true;
        //} else
        //{
        //    collisionStop = false;
        //}
    }

    private void CheckIfArrived()
    {
        if (Stop == false)
        {
            var distanceToCheck = arriveDistance;
            Vector3 currentTargetPosition = currentTarget.transform.position;
            if (index == path.Count - 1)
            {
                distanceToCheck = lastPointArriveDistance;
            } 
            if(Vector3.Distance(currentTargetPosition, transform.position) < distanceToCheck)
            {
                
                SetNextTargetIndex();
            }
        }
    }

    private void SetNextTargetIndex()
    {
        index++;
        if(index >= path.Count)
        {
            Stop = true;
            
            Destroy(gameObject);
        }
        else
        {
            currentTarget = path[index];
        }
    }

    private void Drive()
    {
        if (Stop)
        {
            //Only call when stopping at an intersection
            driveInfo.stoppingPosition = stoppingPos;
            driveInfo.matchingSpeed = 0;
            
            OnDrive?.Invoke(driveInfo);
        }
        else
        {
            Vector3 relativePoint = transform.InverseTransformPoint(currentTarget.transform.position);
            float angle = Mathf.Atan2(relativePoint.x, relativePoint.z) * Mathf.Rad2Deg;

            if (index != 0 && path[index-1].laneEnd && index != path.Count - 1)
            {
                if(turnQueue.Count > 0)
                {
                    nextTurn = turnQueue.Dequeue();
                }
            }


            var rotateCar = 0;
            if (angle > turningAngleOffest)
            {
                rotateCar = 1;
            } else if (angle < -turningAngleOffest)
            {
                rotateCar = -1;
            }

            driveInfo.turnDirection = rotateCar;

            OnDrive?.Invoke(driveInfo);
        }
    }

    public void Approaching()
    {
        if(vertexIndex < vertexPath.Count-1)
        {
            vertexIndex++;
            vertex = vertexPath[vertexIndex];
            approaching = true;
        }

    }

    //Check if vehicle is able to go when I get to the interection
    public bool MakeIntersectionDecision()
    {
        if (intersectionLogic.IsAbleToGo(nextTurn, carsSeen, useVision))
        {
            Debug.Log(vertexIndex);
            vertex = vertexPath[vertexIndex];
            stoppingPos = vertexPath[vertexIndex].position + new Vector3(3.5f, 0, 3.5f);
            driveInfo.turn = nextTurn;
            
            blocked = false;

            return true;
        } else
        {
            blocked = true;
            return false;
        }
    }

    //This method is run when a vehicle reaches the intersection to decide if it can take the turn
    public void EnterIntesction()
    {
        takingIntersection = true;
        approaching = false;
    }

    public bool IsTakingIntersection()
    {
        return takingIntersection;
    }

    public void OutOfIntersection()
    {
        blocked = false;
        takingIntersection = false;
        driveInfo.turn = Turning.STRAIGHT;
    }

    public void UpdateSeenCars(List<CarAI> identifiableObjects)
    {
        this.carsSeen = identifiableObjects;
        //Debug.Log(this.carsSeen[0]);
    }

    public void SetStoppingPoint(Vector3 stoppingPoint)
    {
        stoppingPos = stoppingPoint;
    }

    internal bool IsThisLastPathIndex()
    {
        return index >= path.Count-1;
    }

    internal Turning GetNextTurn()
    {
        if(blocked)
        {
            return Turning.BLOCKED;
        }
        else
        {
            return nextTurn;
        }
        
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;

        //Gizmos.DrawLine(raycastStart.position, raycastStart.position + transform.forward*maxDetectionDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i].transform.position, path[i + 1].transform.position);
        }
    }
}
