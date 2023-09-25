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

    private void Start()
    {
        //for each of the IntersectionEntry givng a FourWayLogic that has the approachingLane of the lane opposite and to the right using modulus  

        for (int i = 0; i < approachingLanes.Count; i++)
        {
            IntersectionLogic intersectionLogic = new FourWayLogic(approachingLanes[(i-1)%4].handler, approachingLanes[(i + 1) % 4].handler);
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



}
