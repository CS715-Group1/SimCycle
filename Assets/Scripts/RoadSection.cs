using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IntersectionConnection
{
    public Intersection intersection;
    public List<Target> entryPoints;
}

public class RoadSection : MonoBehaviour
{
    [SerializeField] private List<IntersectionConnection> connections;//The entry and exit points for the section of the road

    private int weight = 0;
    [SerializeField] private List<Lane> lanes = new();
    private int sectionNumber = 0;
    private List<Target> targets = new();
    private int opposingConnectionNum = 1;

    private void Start()
    {
        foreach (Transform road in transform)
        {
            sectionNumber++;
            weight++;
            foreach (Transform child in road)
            {
                if(child.gameObject.TryGetComponent(out Target t))
                {
                    targets.Add(t);
                }
            }
        }

        foreach (IntersectionConnection connection in connections)
        {
            foreach (Target entry in connection.entryPoints)
            {
                Lane lane = MakeLane(entry);
                
                lane.AddToStart(connection.intersection.GetNearestTarget(lane.GetLaneStart().position));
                lane.AddTarget(connections[opposingConnectionNum].intersection.GetNearestTarget(lane.lastTarget().position));

                Edge edge = new(lane.GetTargets(), weight, connections[opposingConnectionNum].intersection.transform);
                connection.intersection.AddEdge(edge);

                lanes.Add(lane);
            }           
            opposingConnectionNum--;
        }
    }

    //method makes a lane by getting the nearest target to the entry that isn't part of the same object
    //This will be the next target on the orad in the same lane which will be added to the lane list
    //This process is repeated for the amount of time equal to the number of road prefabs
    private Lane MakeLane(Target entry)
    {
        Lane lane = new Lane();
        lane.AddTarget(entry);
        entry.OpenForConnection = false;

        for (int i = 0; i < sectionNumber - 1; i++)
        {
            float distance = float.MaxValue;
            Transform lastTargetTransform = lane.lastTarget();//The previously added target to continue the road creation
            Target nextTarget = null;

            foreach (var item in targets)
            {
                if (item.OpenForConnection && lastTargetTransform.parent != item.transform.parent)
                {
                    float distanceFromTarget = Vector3.Distance(item.transform.position, lastTargetTransform.position);

                    if (distanceFromTarget < distance)
                    {
                        nextTarget = item;
                        distance = distanceFromTarget;
                    }
                }

            }

            lane.AddTarget(nextTarget);
            nextTarget.OpenForConnection = false;//prevent accidental access in future
        }
        return lane;
        
    }

    public List<Transform> GetInConnections()
    {
        List<Transform> list = new List<Transform>();   

        foreach (var l in lanes) {
            list.Add(l.GetLaneStart());
        }
        return list;
    }

    public List<Transform> GetOutConnections()
    {
        List<Transform> list = new List<Transform>();

        foreach (var l in lanes)
        {
            list.Add(l.lastTarget());
        }
        return list;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        foreach (var item in connections)
        {
            Vector3 intersectionPos = item.intersection.transform.position + new Vector3(3.5f, .5f, 3.5f);
            Gizmos.DrawSphere(intersectionPos, 0.4f);
            foreach (var entry in item.entryPoints)
            {
                Vector3 entryPos = entry.transform.position + new Vector3(0, .5f, 0);
                Gizmos.DrawSphere(entryPos, 0.4f);
                Gizmos.DrawLine(entryPos, intersectionPos);
            }

        }


        Gizmos.color = Color.red;

        foreach (Lane l in lanes)
        {
            List<Target> targetList = l.GetTargets();
            int i = 0;

            foreach (Target t in targetList)
            {
                Gizmos.DrawSphere(t.transform.position, 0.3f);

                if (i < targetList.Count)
                {
                    Gizmos.DrawLine(t.transform.position, targetList[i + 1].transform.position);
                }
            }
        }  
    }
}
