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
    [SerializeField] public bool reckless;
    private float maxDetectionDistance = 20.0f;
    private DriveInfo driveInfo = new();
    public Transform vertex;
    private IntersectionLogic intersectionLogic;
    private Rigidbody rb;
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

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

    //Method to generate a list of turns that the car can iterate through.
    //This turn list will be used to determine what direction the car is going in next so it can inform other agents
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
        if(GameState.Instance.stopMotion)
        {
            rb.velocity = Vector3.zero;
        }
        else
        {
            TryDriveVehicle();
        }
    }

    private void TryDriveVehicle()
    {
        CheckIfArrived();
        //reckless causes drivers going straight through an intersection to maintain their speed
        if (reckless && nextTurn == Turning.STRAIGHT)
        {
            CheckForCollisionsReckless();
        }
        else
        {
            CheckForCollisions();

        }
        Drive();
    }

    private void CheckForCollisions()
    {
        //get the distance between the vehicle and the next intersection to check for need to slow down
        float distanceToIntersection = Vector3.Distance(stoppingPos, transform.position);
        RaycastHit hit;
        if ( Physics.Raycast(raycastStart.position, transform.forward, out hit, maxDetectionDistance))
        {
            if (hit.collider.CompareTag("Car") && hit.distance < distanceToIntersection)
            {
                // The hit.collider is the vehicle in front.
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
    }

    private void CheckForCollisionsReckless()
    {
        RaycastHit hit;
        if (Physics.Raycast(raycastStart.position, transform.forward, out hit, maxDetectionDistance))
        {
            if (hit.collider.CompareTag("Car"))
            {
                // The hit.collider is the vehicle in front.
                driveInfo.matchingSpeed = hit.transform.GetComponent<Rigidbody>().velocity.magnitude;
                driveInfo.stoppingPosition = hit.transform.position;
            }
            else
            {
                driveInfo.stoppingPosition = new Vector3(0,1000,0);
            }

        }
        else
        {
            driveInfo.stoppingPosition = new Vector3(0, 1000, 0);
        }
    }

    public void UpdateVertex()
    {
        if (vertexIndex < vertexPath.Count - 1)
        {
            vertexIndex++;
            vertex = vertexPath[vertexIndex];
        }

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
            driveInfo.stoppingPosition = stoppingPos;
            driveInfo.matchingSpeed = 0;
            
            OnDrive?.Invoke(driveInfo);
        }
        else
        {
            //get the angle from the direction the agent is pointing to the next target
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

            //package infomation about the current state of the car and send it to the IDM controller
            driveInfo.turnDirection = rotateCar;
            OnDrive?.Invoke(driveInfo);
        }
    }

    //Check if vehicle is able to go when it gets to the interection
    public bool MakeIntersectionDecision()
    {
        if (intersectionLogic.IsAbleToGo(nextTurn, carsSeen, GameState.Instance.useVision))
        {
            Debug.Log("SHOULD GO");
            vertex = vertexPath[vertexIndex];
            //stopping position is offset to make it in the center of the intersection
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
        if (path == null) return;

        Gizmos.color = Color.green;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i].transform.position, path[i + 1].transform.position);
        }
    }
}
