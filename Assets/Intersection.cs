using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Intersection : MonoBehaviour
{

    [SerializeField] private List<Target> targets = new();
    [SerializeField] private List<IntersectionEntry> approachingLanes = new();
    public List<Edge> edges = new();
    public Transform center;

    public List<CarAI> carsInIntersection = new();

    private void Start()
    {
        //for each of the IntersectionEntry givng a FourWayLogic that has the approachingLane of the lane opposite and to the right using modulus  

        for (int i = 0; i < approachingLanes.Count; i++)
        {
            IntersectionLogic intersectionLogic = new FourWayLogic(approachingLanes[mod(i - 1, 4)].handler, approachingLanes[mod(i + 2, 4)].handler, approachingLanes[mod(i+1,4)].handler);
            approachingLanes[i].handler.IntersectionLogic = intersectionLogic;
        }
    }

    internal Target GetNearestTarget(Vector3 start)
    {
        float distance = float.MaxValue;
        Target target = null;

        foreach (Target item in targets)
        {
            float distanceFromTarget = Vector3.Distance(item.transform.position, start);

            if (distanceFromTarget < distance){
                distance = distanceFromTarget;
                target = item;
            }        
        }
        return target;
    }

    public void AddEdge(Edge edge)
    {
        edges.Add(edge);
    }

    public List<Edge> GetEdges()
    {
        return edges;
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            if (other.TryGetComponent<CarAI>(out CarAI car))
            {
                if (!car.IsThisLastPathIndex() && !carsInIntersection.Contains(car))
                {
                    carsInIntersection.Add(car);
                    RefreshApproachingLanes();
                }
            }
        }
    }

    private void RefreshApproachingLanes()
    {
        foreach (CarAI car in carsInIntersection)
        {
            if (!car.IsTakingIntersection())
            {
                car.MakeIntersectionDecision();
            }
            
        }

        //foreach (var approach in approachingLanes)
        //{
        //    approach.handler.CheckCanGo();
        //}
    }

    //private void Update()
    //{
    //    if (currentCar == null)
    //    {
    //        if (trafficQueue.Count > 0)
    //        {
    //            currentCar = trafficQueue.Dequeue();
    //            currentCar.Stop = false;
    //        }
    //    }
    //}

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            if (other.TryGetComponent<CarAI>(out CarAI car))
            {
                carsInIntersection.Remove(car);
                car.OutOfIntersection();
                RemoveApproachingCar(car);
                RefreshApproachingLanes();

            }
        }
    }

    private void RemoveApproachingCar(CarAI car)
    {
        foreach (var approach in approachingLanes)
        {

            approach.handler.RemoveCar(car);
        }
    }

}
