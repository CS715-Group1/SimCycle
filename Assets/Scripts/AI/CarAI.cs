using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.Events;

public enum Turning
{
    RIGHT,
    LEFT,
    STRAIGHT,
    BLOCKED
}

public class CarAI : MonoBehaviour
{
    private List<Target> path = null;

    [SerializeField] private float arriveDistance = 1.5f, lastPointArriveDistance = .1f;
    [SerializeField] private float turningAngleOffest = 5;
    [SerializeField] private Target currentTarget;
    [SerializeField] private Transform raycastStart;
    private float maxDistance = 2f;

    private Turning turning;
    public Turning Turning { get; set; }

    private IntersectionLogic intersectionLogic;
    public IntersectionLogic IntersectionLogic { get; set; }


    private int index = 0;

    private bool stop;
    private bool collisionStop;

    public bool Stop
    {
        get { return stop || collisionStop; }
        set { stop = value; }
    }

    [field: SerializeField]
    public UnityEvent<Vector2> OnDrive { get; set; }

    private void Start()
    {
        turning = Turning.STRAIGHT;

        if (path == null || path.Count == 0)
        {
            Stop = true;
        }
        else
        {
            currentTarget = path[index];
        }
    }

    public void SetPath(List<Target> path)
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

        Vector3 relativePoint = transform.InverseTransformPoint(this.path[index + 1].transform.position);

        float angle = Mathf.Atan2(relativePoint.x, relativePoint.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0,angle, 0);
        Stop = false;
    }

    private void Update()
    {
        CheckIfArrived();
        Drive();
        CheckForCollisions();
    }

    private void CheckForCollisions()
    {
        if(Physics.Raycast(raycastStart.position, transform.forward, maxDistance, 1 << gameObject.layer))
        {
            collisionStop = true;
        } else
        {
            collisionStop = false;
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
            OnDrive?.Invoke(Vector2.zero);
        }
        else
        {
            Vector3 relativePoint = transform.InverseTransformPoint(currentTarget.transform.position);
            float angle = Mathf.Atan2(relativePoint.x, relativePoint.z) * Mathf.Rad2Deg;
            //Debug.Log(angle.ToString());

            float speed = 1;

            if (currentTarget.laneEnd && index != path.Count - 1)
            {
                Vector3 nextRelativePoint = transform.InverseTransformPoint(path[index+1].transform.position);

                float nextAngle = Mathf.Atan2(nextRelativePoint.x, nextRelativePoint.z) * Mathf.Rad2Deg;

                if(nextAngle > 20f)
                {
                    Debug.Log("Turning right");
                    turning = Turning.RIGHT;
                    speed = 0.7f;
                } else if(nextAngle < -20f)
                {
                    Debug.Log("Turning left");
                    turning = Turning.LEFT;
                    speed = 0.7f;
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

            OnDrive?.Invoke(new Vector2(rotateCar, speed));
        }
    }



    public void MakeIntersectionDecision()
    {
        if (intersectionLogic.IsAbleToGo(turning))
        {
            Stop = false;
        } else
        {
            Stop = true;
        }
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;

        //Gizmos.DrawLine(raycastStart.position, raycastStart.position + transform.forward*maxDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i].transform.position, path[i + 1].transform.position);
        }

    }

    internal bool IsThisLastPathIndex()
    {
        return index >= path.Count-1;
    }
}
